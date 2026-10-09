using UnityEngine;
namespace AllAboard.Train
{
    [CreateAssetMenu(menuName="All Aboard/Economy Configuration")]
    public sealed class EconomyConfig:ScriptableObject
    {
        public int startingGold=500,startingCoal=10,startingSteelMetres=20;
        public int baseQuota=1500,entryFee=1000,creditLimit=100,steelPrice=50,coalPrice=20,sawPrice=25,connectorPrice=15;
        public float quotaGrowth=1.18f,villageMultiplier=1.2f,merchantDiscount=.15f;
        public int scrapPrice=30,screwsPrice=15,woodPrice=20,medicalPrice=60,recoveryPrice=150,firearmPrice=300,ammoPrice=5;
        public int componentPrice=150,locomotiveComponentPrice=275,storageCarriagePrice=500;
        public int terminumReward=100,townReward=200,villageReward=300,stationReward=500,forcedExitReward=50;
        public int minimumTip=0,maximumTip=100,passengerCapacity=16,normalStartingPassengers=14,mediumStartingPassengers=12,hardStartingPassengers=10;
        public Vector2Int lowJob=new Vector2Int(25,50),normalJob=new Vector2Int(50,80),valuableJob=new Vector2Int(80,120);
        public int cosmeticRepairMin=20,cosmeticRepairMax=75,componentRepairMin=75,componentRepairMax=200,majorRepairMin=200,majorRepairMax=400;
        public Vector2Int minorFine=new Vector2Int(50,150),severeFine=new Vector2Int(200,400),banditToll=new Vector2Int(100,300),cargoReward=new Vector2Int(300,700);
        public float damagedCargoMultiplier=.75f,contractPenaltyFraction=.35f,relevantSaleMultiplier=.6f,irrelevantSaleMultiplier=.3f;
        public float normalBlumt=1f,mediumBlumt=1.25f,hardBlumt=1.5f;
        public bool freePrototypeCoal=true;
        [Header("Passenger service")]
        public float startingHappiness=55f,delayLossPerSecond=.025f,roughDrivingLoss=.8f,poorConditionLoss=.06f,comfortRecovery=.008f;
        public float repairGraceSeconds=180f,stationGraceSeconds=90f,expectedTravelSpeed=8f,missedStopPenalty=20f;
        public int safeDemand=1,mixedDemand=3,hardDemand=5,bluefortDemand=1,maxContracts=3;
        public float bankerChance=.04f,bankerTolerance=25f,cargoDamageSpeed=4f,cargoDamageScale=3f;
        public int bankerMultiplier=10;
        public int wrenchPrice=25;
        public int[] upgradeBasePrices={500,400,400,600};
        public int Quota(int day)=>Mathf.RoundToInt(baseQuota*Mathf.Pow(quotaGrowth,Mathf.Max(0,day-1))/50f)*50;
        public int Destination(RoutePlace place)=>place==RoutePlace.Terminum?terminumReward:place==RoutePlace.Station?stationReward:place==RoutePlace.Village?villageReward:townReward;
        public int Tip(float happiness)=>Mathf.RoundToInt(Mathf.Lerp(minimumTip,maximumTip,Mathf.Clamp01(happiness/100f)));
        public int ProductPrice(RailwayProduct product)=>product==RailwayProduct.Steel?steelPrice:product==RailwayProduct.Coal?coalPrice:product==RailwayProduct.Saw?sawPrice:product==RailwayProduct.Connector?connectorPrice:product==RailwayProduct.Scrap?scrapPrice:product==RailwayProduct.Screws?screwsPrice:product==RailwayProduct.Wrench?wrenchPrice:product==RailwayProduct.BoilerPart?locomotiveComponentPrice:product==RailwayProduct.BrakePart||product==RailwayProduct.CoachPart?componentPrice:woodPrice;
        public float BlumtMultiplier(EconomyDifficulty difficulty)=>difficulty==EconomyDifficulty.Hard?hardBlumt:difficulty==EconomyDifficulty.Medium?mediumBlumt:normalBlumt;
        private static EconomyConfig fallback;
        public static EconomyConfig Default {get{var asset=Resources.Load<EconomyConfig>("EconomyDefaults");if(asset!=null)return asset;if(fallback==null){fallback=CreateInstance<EconomyConfig>();fallback.hideFlags=HideFlags.HideAndDontSave;}return fallback;}}
    }
    public enum EconomyDifficulty{Normal,Medium,Hard}
    public enum PassengerOutcome{Delivered,ForcedExit,HappinessLost,Dead,Kidnapped,Abandoned}
    public enum CargoOutcome{Intact,Damaged,Failed}
}
