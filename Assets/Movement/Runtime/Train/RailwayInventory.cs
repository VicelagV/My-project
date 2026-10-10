using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailwayInventory : MonoBehaviour
    {
        public const int SteelPrice = 50;
        [SerializeField] private EconomyConfig configuration;
        public EconomyConfig Config=>configuration!=null?configuration:EconomyConfig.Default;
        public int Day {get;private set;}=1;
        public int CreditDebt {get;private set;}
        public int Obligations {get;private set;}
        public int RepairDebt {get;private set;}
        public int Blumt {get;private set;}
        public int Quota=>Config.Quota(Day)+RepairDebt;
        public bool EntryPaid {get;private set;}
        public bool Settled {get;private set;}
        public string DayReport {get;private set;}
        public int LastRepairCarry {get;private set;}
        public int TotalEarned {get;private set;}
        public int TotalPurchased {get;private set;}
        public int LastRepairsPaid {get;private set;}
        public int TotalCashSpent {get;private set;}
        public int SettlementQuota {get;private set;}
        private readonly System.Collections.Generic.Dictionary<RailwayProduct,int> stock=new System.Collections.Generic.Dictionary<RailwayProduct,int>();
        public void Configure(EconomyConfig data)=>configuration=data;
        public int Gold { get; private set; } = 500;
        public float SteelMetres { get; private set; } = 20f;
        public void BeginDay()=>BeginDay(1);
        public void BeginDay(int day) { Day=Mathf.Max(1,day);Gold=Config.startingGold;SteelMetres=Config.startingSteelMetres;CreditDebt=Obligations=0;EntryPaid=Settled=false;DayReport=null;TotalEarned=TotalPurchased=LastRepairsPaid=TotalCashSpent=SettlementQuota=0;stock.Clear();AddStock(RailwayProduct.Scrap,2);AddStock(RailwayProduct.Screws,4);AddStock(RailwayProduct.Wood,2);AddStock(RailwayProduct.BoilerPart);AddStock(RailwayProduct.BrakePart);AddStock(RailwayProduct.CoachPart,2); }
        public bool Spend(int amount) { if (Settled || amount < 0 || Gold < amount) return false; Gold -= amount;TotalCashSpent+=amount; return true; }
        public void Earn(int amount) { if (!Settled && amount > 0) {Gold += amount;TotalEarned+=amount;} }
        public bool BuySteel() { if (!Spend(Config.steelPrice)) return false; SteelMetres += 5f; return true; }
        public bool CanPurchase(int amount)=>!Settled&&amount>=0&&amount<=Gold+Mathf.Max(0,Config.creditLimit-CreditDebt);
        public bool Purchase(int amount)
        {if(!CanPurchase(amount))return false;int borrowed=Mathf.Max(0,amount-Gold);CreditDebt+=borrowed;Gold=Mathf.Max(0,Gold-amount);TotalPurchased+=amount;return true;}
        public bool PurchaseSteel(int price){if(!Purchase(price))return false;SteelMetres+=5f;return true;}
        public bool SpendBlumt(int amount){if(Settled||amount<0||Blumt<amount)return false;Blumt-=amount;return true;}
        public bool ConsumeStock(RailwayProduct product,int quantity=1){if(quantity<=0||Stock(product)<quantity)return false;stock[product]-=quantity;return true;}
        public void AddStock(RailwayProduct product,int quantity=1){stock.TryGetValue(product,out var old);stock[product]=old+Mathf.Max(0,quantity);}
        public int Stock(RailwayProduct product)=>stock.TryGetValue(product,out int count)?count:0;
        public void Charge(int amount){if(!Settled)Obligations+=Mathf.Max(0,amount);}
        public bool PayEntry(){if(EntryPaid)return true;if(Settled||!Spend(Config.entryFee))return false;EntryPaid=true;return true;}
        public int PayPassenger(int job,RoutePlace destination,float happiness,PassengerOutcome outcome,bool special=false)
        {
            if(Settled||happiness<0f)return 0;
            int value=outcome==PassengerOutcome.ForcedExit?Config.forcedExitReward:outcome==PassengerOutcome.Delivered&&happiness>=0f?Mathf.Max(0,job)+(special?Config.terminumReward*Config.bankerMultiplier:Config.Destination(destination))+Config.Tip(happiness):0;
            Earn(value);return value;
        }
        public void ResolveCargo(int reward,CargoOutcome outcome)
        {if(Settled)return;reward=Mathf.Max(0,reward);if(outcome==CargoOutcome.Failed)Charge(Mathf.RoundToInt(reward*Config.contractPenaltyFraction));else Earn(Mathf.RoundToInt(reward*(outcome==CargoOutcome.Damaged?Config.damagedCargoMultiplier:1f)));}
        public bool FinishDay(int damageBill,EconomyDifficulty difficulty,bool enteredCapital)
        {
            damageBill=Mathf.Max(0,damageBill);
            if(Settled)return DayReport!=null&&DayReport.StartsWith("Day complete");Settled=true;
            int due=Obligations+CreditDebt,quota=Quota,available=Gold;
            SettlementQuota=quota;
            bool success=available>=due+quota&&(!enteredCapital||EntryPaid);
            if(!success){Gold=Mathf.Max(0,available-due);DayReport="Quota failed — Gold "+available+", obligations "+due+", quota "+quota;return false;}
            available-=due+quota;RepairDebt=Mathf.Max(0,damageBill-available);LastRepairCarry=RepairDebt;
            int paidRepairs=Mathf.Min(Mathf.Max(0,damageBill),available);available-=paidRepairs;LastRepairsPaid=paidRepairs;
            int gain=Mathf.RoundToInt(available*Config.BlumtMultiplier(difficulty));Blumt+=gain;Gold=0;
            DayReport="Day complete — quota "+quota+", repairs "+paidRepairs+", +"+gain+" Blumt"+(RepairDebt>0?", next-day repair debt "+RepairDebt:"");return true;
        }
        public bool UseSteel(float metres) { if (metres <= 0f || metres > 5.001f || SteelMetres + .001f < metres) return false; SteelMetres = Mathf.Max(0f, SteelMetres - metres); return true; }
        public void RestoreProgress(int blumt,int repairDebt){Blumt=Mathf.Max(0,blumt);RepairDebt=Mathf.Max(0,repairDebt);}
        public void FailDay(string reason){Settled=true;DayReport=reason;}
    }
}
