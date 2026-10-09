using System.Collections.Generic;
using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainDayEconomy:MonoBehaviour
    {
        [SerializeField] private EconomyDifficulty difficulty=EconomyDifficulty.Normal;
        private ProceduralRailway world;
        private RailwayInventory wallet;
        private readonly List<PassengerJourney> passengers=new List<PassengerJourney>();
        private readonly HashSet<string> visited=new HashSet<string>();
        public IReadOnlyList<PassengerJourney> Passengers=>passengers;
        public bool GateBlocked {get;private set;}
        public string DestinationTitle(string id)=>world.Network.Node(id).title;
        public int EarlyExitFare=>wallet.Config.forcedExitReward;
        public void Configure(ProceduralRailway owner)
        {
            world=owner;wallet=GetComponent<RailwayInventory>();visited.Clear();GateBlocked=false;
            foreach(var old in passengers)if(old!=null){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
            passengers.Clear();var config=wallet.Config;
            int count=Mathf.Min(config.passengerCapacity,difficulty==EconomyDifficulty.Hard?config.hardStartingPassengers:difficulty==EconomyDifficulty.Medium?config.mediumStartingPassengers:config.normalStartingPassengers);
            var seats=world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<Transform>()).Where(t=>t.name=="Passenger Seat Pair").ToArray();
            var optional=world.Network.nodes.FirstOrDefault(n=>n.family==RouteFamily.Safe&&n.place==RoutePlace.Town);
            var rng=new System.Random(world.DailySeed^2781);
            for(int i=0;i<Mathf.Min(count,seats.Length);i++)
            {
                string destination=i<4&&optional!=null?optional.id:i<8?"Bluefort":"Terminum";
                var obj=new GameObject("Passenger "+(i+1));obj.transform.SetParent(seats[i],false);obj.transform.localPosition=new Vector3(0f,1.76f,.05f);
                var collider=obj.AddComponent<BoxCollider>();collider.center=new Vector3(0f,.55f,.0f);collider.size=new Vector3(.5f,1.1f,.45f);
                var journey=obj.AddComponent<PassengerJourney>();journey.Configure(this,destination,rng.Next(config.normalJob.x,config.normalJob.y+1),20f);
                string model=i%3==0?"BrightCharacterYellow":i%3==1?"BrightCharacter":"BrightCharacterGreen";
                var prefab=Resources.Load<GameObject>(model);if(prefab!=null){var visual=Instantiate(prefab,obj.transform,false);visual.transform.localScale=Vector3.one*.65f;}
                passengers.Add(journey);
            }
        }
        public bool AtSettlement(string destination=null)
        {
            if(world==null||world.DayEnded||Mathf.Abs(world.Train.Speed)>.1f)return false;
            return world.Network.nodes.Any(n=>RouteGeometry.IsStop(n.place)&&(destination==null||n.id==destination)&&world.NodeDistance(n.id)>=0f&&Mathf.Abs(world.Train.HeadDistance-world.NodeDistance(n.id))<35f);
        }
        public int Resolve(PassengerJourney passenger,PassengerOutcome outcome)
        {
            if(passenger.Resolved)return 0;
            var destination=world.Network.Node(passenger.Destination);
            int value=wallet.PayPassenger(passenger.JobValue,destination.place,passenger.Happiness,outcome);
            passenger.MarkResolved(value);return value;
        }
        private void FixedUpdate()
        {
            if(world==null||world.DayEnded||wallet.Settled)return;
            bool entryRoute=world.NodeDistance("Terminum")>=0f;
            float gate=world.NodeDistance("Gate");
            GateBlocked=entryRoute&&!wallet.EntryPaid&&gate>=0f&&world.Train.HeadDistance>=gate;
            if(GateBlocked)
            {
                if(wallet.PayEntry())GateBlocked=false;
                else if(!world.Train.Reversing)world.Train.HoldAtCheckpoint();
            }
            if(Mathf.Abs(world.Train.Speed)>.1f||GateBlocked)return;
            foreach(var node in world.Network.nodes)
            {
                if(node.id=="Redfort"||!AtSettlement(node.id)||!visited.Add(node.id))continue;
                foreach(var passenger in passengers.Where(p=>p!=null&&!p.Resolved&&p.Destination==node.id))Resolve(passenger,PassengerOutcome.Delivered);
                if(node.place==RoutePlace.Terminum&&wallet.EntryPaid){Finish(true);break;}
            }
        }
        public void Finish(bool capital)
        {
            if(world.DayEnded)return;
            foreach(var passenger in passengers.Where(p=>p!=null&&!p.Resolved))Resolve(passenger,PassengerOutcome.Abandoned);
            int repairBill=0;
            foreach(var component in world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<DamageableComponent>()))
                if(component.DamageFraction>0f)repairBill+=component.IsBroken?wallet.Config.majorRepairMax:Mathf.RoundToInt(Mathf.Lerp(wallet.Config.componentRepairMin,wallet.Config.componentRepairMax,component.DamageFraction));
            wallet.FinishDay(repairBill,difficulty,capital);world.FinishEconomicDay(wallet.DayReport);
        }
        private void OnGUI()
        {
            if(wallet==null)return;
            GUI.Box(new Rect(16f,Screen.height-90f,330f,72f),"Gold "+wallet.Gold+"   Credit "+wallet.CreditDebt+" / "+wallet.Config.creditLimit+"\nQuota "+wallet.Quota+"   Blumt "+wallet.Blumt+"\nPassengers "+passengers.Count(p=>p!=null&&!p.Resolved)+" / "+wallet.Config.passengerCapacity);
            if(GateBlocked)GUI.Box(new Rect(Screen.width/2f-210f,65f,420f,45f),"Terminum entry requires "+wallet.Config.entryFee+" Gold. Reverse to choose the bypass.");
        }
    }
    public sealed class PassengerJourney:PrototypeInteractable
    {
        private TrainDayEconomy economy;
        public string Destination {get;private set;}
        public int JobValue {get;private set;}
        public float Happiness {get;private set;}
        public bool Resolved {get;private set;}
        public int Paid {get;private set;}
        public override string Prompt=>"TO: "+economy.DestinationTitle(Destination)+" | "+Happiness.ToString("0")+"% happy\nE: early drop-off ("+economy.EarlyExitFare+" Gold)";
        public void Configure(TrainDayEconomy owner,string destination,int job,float happiness){economy=owner;Destination=destination;JobValue=job;Happiness=happiness;}
        public void MarkResolved(int paid){Resolved=true;Paid=paid;gameObject.SetActive(false);}
        public override bool Interact(PlayerInteraction actor){if(Resolved||economy==null||!economy.AtSettlement())return false;economy.Resolve(this,PassengerOutcome.ForcedExit);return true;}
    }
}
