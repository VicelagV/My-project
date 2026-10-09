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
        private Material[] palette;
        private Transform serviceRoot;
        private readonly List<PassengerJourney> passengers=new List<PassengerJourney>();
        private readonly Dictionary<string,List<PassengerJourney>> queues=new Dictionary<string,List<PassengerJourney>>();
        private readonly HashSet<string> visited=new HashSet<string>();
        private float lastSpeed;private Quaternion lastHeading;private bool sampled;
        public IReadOnlyList<PassengerJourney> Passengers=>passengers;
        public IReadOnlyDictionary<string,List<PassengerJourney>> Queues=>queues;
        public ProceduralRailway World=>world;
        public RailwayInventory Wallet=>wallet;
        public Material[] Palette=>palette;
        public EconomyDifficulty Difficulty=>difficulty;
        public void SetDifficulty(EconomyDifficulty value)=>difficulty=value;
        public int ActiveCount=>passengers.Count(p=>p!=null&&p.Aboard&&!p.Resolved);
        public bool GateBlocked {get;private set;}
        public bool ReadyToSettle=>AtSettlement("Terminum")&&wallet.EntryPaid;
        public CargoService Cargo {get;private set;}
        public string DestinationTitle(string id)=>world.Network.Node(id).title;
        public int EarlyExitFare=>wallet.Config.forcedExitReward;
        public void Configure(ProceduralRailway owner,Material[] materials=null)
        {
            world=owner;wallet=GetComponent<RailwayInventory>();palette=materials;visited.Clear();GateBlocked=false;sampled=false;
            foreach(var old in passengers)if(old!=null)Dispose(old.gameObject);
            passengers.Clear();queues.Clear();
            if(serviceRoot!=null)Dispose(serviceRoot.gameObject);
            serviceRoot=new GameObject("Passenger And Cargo Service").transform;serviceRoot.SetParent(world.transform,false);
            if(!Application.isPlaying)serviceRoot.gameObject.hideFlags=HideFlags.DontSaveInEditor;
            var config=wallet.Config;
            int count=Mathf.Min(config.passengerCapacity,difficulty==EconomyDifficulty.Hard?config.hardStartingPassengers:difficulty==EconomyDifficulty.Medium?config.mediumStartingPassengers:config.normalStartingPassengers);
            var rng=new System.Random(world.DailySeed^2781);
            for(int i=0;i<count;i++)
            {
                var passenger=CreatePassenger("Redfort",PassengerSchedule.InitialDestination(world.Network,i),rng,i,false);
                if(!Board(passenger,true)){Dispose(passenger.gameObject);break;}
            }
            foreach(var node in world.Network.nodes.Where(PassengerSchedule.BoardsAt))
            {
                var queue=new List<PassengerJourney>();queues.Add(node.id,queue);
                var destinations=PassengerSchedule.Destinations(world.Network,node.id);if(destinations.Length==0)continue;
                int demand=PassengerSchedule.Demand(node,config);
                for(int i=0;i<demand;i++)
                {
                    var same=destinations.Where(d=>d.family==node.family&&d.position.z>node.position.z).ToArray();
                    var choices=same.Length>0&&rng.NextDouble()<.6?same:destinations;
                    var target=choices[rng.Next(choices.Length)];
                    bool banker=node.family==RouteFamily.Hard&&rng.NextDouble()<config.bankerChance;
                    if(banker)target=world.Network.Node("Terminum");
                    var passenger=CreatePassenger(node.id,target.id,rng,i,banker);
                    passenger.transform.position=node.position+new Vector3(3.9f,1.18f,-12f+i*1.15f);queue.Add(passenger);
                }
            }
            Cargo=GetComponent<CargoService>();if(Cargo==null)Cargo=gameObject.AddComponent<CargoService>();Cargo.Configure(this,serviceRoot);
            var manifest=GetComponent<EconomyManifest>();if(manifest==null)manifest=gameObject.AddComponent<EconomyManifest>();manifest.Configure(this);
            if(palette!=null){EconomyServiceArt.Build(this,serviceRoot);var maintenance=GetComponent<TrainMaintenance>();if(maintenance==null)maintenance=gameObject.AddComponent<TrainMaintenance>();maintenance.Configure(this);}
        }
        private PassengerJourney CreatePassenger(string origin,string destination,System.Random rng,int index,bool banker)
        {
            var obj=new GameObject("Waiting Passenger");obj.transform.SetParent(serviceRoot,false);
            var shape=obj.AddComponent<BoxCollider>();shape.center=new Vector3(0f,.85f,0f);shape.size=new Vector3(.55f,1.7f,.45f);
            var journey=obj.AddComponent<PassengerJourney>();var node=world.Network.Node(origin);
            var family=origin=="Redfort"?world.Network.Node(destination).family:node.family;
            var band=family==RouteFamily.Hard?wallet.Config.valuableJob:family==RouteFamily.Mixed?wallet.Config.normalJob:wallet.Config.lowJob;
            string[] names={"Ada","Eli","Mabel","Finn","Nell","Otis","Rose","Jules","Kit","Wren","Alfie","Dot"};
            journey.Configure(this,destination,rng.Next(band.x,band.y+1),wallet.Config.startingHappiness,(banker?"Banker ":"")+names[rng.Next(names.Length)],banker,origin);
            string model=index%3==0?"BrightCharacterYellow":index%3==1?"BrightCharacter":"BrightCharacterGreen";
            var prefab=Resources.Load<GameObject>(model);if(prefab!=null){var visual=Instantiate(prefab,obj.transform,false);visual.name="Passenger Visual";}
            if(palette!=null)obj.AddComponent<ControlHighlight>().Configure(palette[9],obj.GetComponentsInChildren<Renderer>());
            return journey;
        }
        public bool Board(PassengerJourney passenger,bool initial=false)
        {
            if(passenger==null||passenger.Aboard||passenger.Resolved||wallet.Settled||ActiveCount>=wallet.Config.passengerCapacity)return false;
            if(!initial&&(!AtSettlement()||Vector3.Distance(passenger.transform.position,world.Train.Cars[0].transform.position)>60f))return false;
            var occupied=new HashSet<Transform>(passengers.Where(p=>p!=null&&p.Aboard&&!p.Resolved).Select(p=>p.transform.parent));
            var seat=world.Train.Cars.Where(world.Train.Contains).SelectMany(c=>c.GetComponentsInChildren<Transform>()).FirstOrDefault(t=>t.name=="Passenger Seat Pair"&&!occupied.Contains(t));
            if(seat==null)return false;passenger.Board(seat);passengers.Add(passenger);return true;
        }
        public int BoardWaiting(string node)
        {if(!AtSettlement(node)||!queues.TryGetValue(node,out var queue))return 0;int count=0;foreach(var passenger in queue)if(Board(passenger))count++;return count;}
        public bool AtSettlement(string destination=null)
        {
            if(world==null||world.DayEnded||Mathf.Abs(world.Train.Speed)>.1f)return false;
            return world.Network.nodes.Any(n=>RouteGeometry.IsStop(n.place)&&(destination==null||n.id==destination)&&world.NodeDistance(n.id)>=0f&&Mathf.Abs(world.Train.HeadDistance-world.NodeDistance(n.id))<35f);
        }
        public int Resolve(PassengerJourney passenger,PassengerOutcome outcome,bool capitalFallback=false)
        {
            if(passenger.Resolved)return 0;
            var destination=world.Network.Node(passenger.Destination);
            int value=capitalFallback?(passenger.Happiness>=0f?wallet.PayPassenger(0,RoutePlace.Terminum,0f,PassengerOutcome.Delivered):0):wallet.PayPassenger(passenger.JobValue,destination.place,passenger.Happiness,outcome,passenger.IsBanker);
            passenger.MarkResolved(value);return value;
        }
        private void FixedUpdate()=>Step(Time.fixedDeltaTime);
        public void Step(float dt)
        {
            if(world==null||world.DayEnded||wallet.Settled)return;
            float speed=world.Train.Speed;var heading=world.Train.Cars[0].transform.rotation;
            float braking=sampled&&Mathf.Abs(lastSpeed)>3f?Mathf.Max(0f,(Mathf.Abs(lastSpeed)-Mathf.Abs(speed))/Mathf.Max(.01f,dt)-3f):0f;
            float turning=sampled&&Mathf.Abs(speed)>3f?Mathf.Max(0f,Quaternion.Angle(heading,lastHeading)/Mathf.Max(.01f,dt)-8f)/20f:0f;
            float condition=world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<DamageableComponent>()).Sum(c=>c.DamageFraction);
            foreach(var passenger in passengers.Where(p=>p!=null&&!p.Resolved))
            {
                passenger.Step(dt,Mathf.Min(3f,braking+turning)*(GetComponent<TrainMaintenance>()?.ComfortFactor??1f),condition,Mathf.Abs(speed)>1f&&Mathf.Abs(speed)<9f);
                if(world.Train.Cars[0].transform.position.z>world.Network.Node(passenger.Destination).position.z+45f)passenger.MarkMissed();
            }
            lastSpeed=speed;lastHeading=heading;sampled=true;
            bool entryRoute=world.NodeDistance("Terminum")>=0f;float gate=world.NodeDistance("Gate");
            GateBlocked=entryRoute&&!wallet.EntryPaid&&gate>=0f&&world.Train.HeadDistance>=gate;
            if(GateBlocked){if(wallet.PayEntry())GateBlocked=false;else if(!world.Train.Reversing)world.Train.HoldAtCheckpoint();}
            if(Mathf.Abs(speed)>.1f||GateBlocked)return;
            foreach(var node in world.Network.nodes)
            {
                if(!AtSettlement(node.id))continue;
                foreach(var passenger in passengers.Where(p=>p!=null&&!p.Resolved).ToArray())
                {
                    if(passenger.WantsToLeave)Resolve(passenger,PassengerOutcome.HappinessLost);
                    else if(passenger.Destination==node.id)Resolve(passenger,PassengerOutcome.Delivered);
                    else if(node.place==RoutePlace.Terminum)Resolve(passenger,PassengerOutcome.Delivered,true);
                }
                visited.Add(node.id);
                if(node.place==RoutePlace.Terminum&&wallet.EntryPaid)
                {if(!Cargo.Offers.Any(o=>o.Accepted&&!o.Resolved))Finish(true);else world.Train.HoldAtCheckpoint();break;}
            }
        }
        public void Finish(bool capital)
        {
            if(world.DayEnded)return;
            foreach(var passenger in passengers.Where(p=>p!=null&&!p.Resolved))Resolve(passenger,PassengerOutcome.Abandoned);
            Cargo?.FailOutstanding();
            int repairBill=0;
            foreach(var component in world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<DamageableComponent>()))
                if(component.DamageFraction>0f)repairBill+=component.IsBroken?wallet.Config.majorRepairMax:Mathf.RoundToInt(Mathf.Lerp(wallet.Config.componentRepairMin,wallet.Config.componentRepairMax,component.DamageFraction));
            bool success=wallet.FinishDay(repairBill,difficulty,capital);world.FinishEconomicDay(wallet.DayReport);world.SaveProgress(success,difficulty);
        }
        public void Abort(string reason){foreach(var p in passengers.Where(p=>p!=null&&!p.Resolved))Resolve(p,PassengerOutcome.Abandoned);Cargo?.FailOutstanding();wallet.FailDay(reason);world.SaveProgress(false,difficulty);}
        private static void Dispose(GameObject obj){obj.SetActive(false);if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
    }
}
