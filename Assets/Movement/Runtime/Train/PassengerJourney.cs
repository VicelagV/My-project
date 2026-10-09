using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class PassengerJourney:PrototypeInteractable
    {
        private TrainDayEconomy economy;
        public string Destination {get;private set;}
        public string Name {get;private set;}
        public int JobValue {get;private set;}
        public float Happiness {get;private set;}
        public float TravelSeconds {get;private set;}
        public float Deadline {get;private set;}
        public bool IsBanker {get;private set;}
        public bool Aboard {get;private set;}
        public bool MissedStop {get;private set;}
        public bool Resolved {get;private set;}
        public int Paid {get;private set;}
        public int ExpectedFare=>economy.Wallet.Config.Destination(economy.World.Network.Node(Destination).place)+JobValue+economy.Wallet.Config.Tip(Happiness)+(IsBanker?economy.Wallet.Config.terminumReward*(economy.Wallet.Config.bankerMultiplier-1):0);
        public override string Prompt=>Name+" — "+economy.DestinationTitle(Destination)+"\n"+(Aboard?Happiness.ToString("0")+"% happy | E: early exit ("+economy.EarlyExitFare+" Gold)":"E: board | expected fare "+ExpectedFare+" Gold");
        public void Configure(TrainDayEconomy owner,string destination,int job,float happiness,string name="Traveller",bool banker=false,string origin="Redfort")
        {economy=owner;Destination=destination;JobValue=job;Happiness=happiness;Name=name;IsBanker=banker;Deadline=PassengerSchedule.Deadline(owner.World.Network,origin,destination,owner.Wallet.Config);}
        public void Board(Transform seat)
        {Aboard=true;transform.SetParent(seat,false);transform.localPosition=new Vector3(0f,1.76f,.05f);var shape=GetComponent<BoxCollider>();shape.center=new Vector3(0f,.55f,0f);shape.size=new Vector3(.5f,1.1f,.45f);foreach(Transform child in transform)if(child.name=="Passenger Visual")child.localScale=Vector3.one*.65f;}
        public void ChangeHappiness(float amount){if(!Resolved)Happiness=Mathf.Clamp(Happiness+amount,-100f,100f);}
        public void Step(float dt,float roughness,float conditionLoss,bool comfortable)
        {
            if(!Aboard||Resolved||dt<=0f)return;TravelSeconds+=dt;var config=economy.Wallet.Config;
            float loss=roughness*config.roughDrivingLoss+conditionLoss*config.poorConditionLoss+(TravelSeconds>Deadline?config.delayLossPerSecond:0f);
            ChangeHappiness((comfortable&&loss<.001f?config.comfortRecovery:-loss)*dt*(IsBanker?2f:1f));
        }
        public bool WantsToLeave=>Happiness<0f||(IsBanker&&Happiness<economy.Wallet.Config.bankerTolerance);
        public void MarkMissed(){if(MissedStop)return;MissedStop=true;ChangeHappiness(-economy.Wallet.Config.missedStopPenalty);}
        public void MarkResolved(int paid){Resolved=true;Paid=paid;gameObject.SetActive(false);}
        public override bool Interact(PlayerInteraction actor)
        {if(Resolved||economy==null)return false;if(!Aboard)return economy.Board(this);if(!economy.AtSettlement())return false;economy.Resolve(this,PassengerOutcome.ForcedExit);return true;}
    }
}
