using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public enum TrainUpgrade { SteamEfficiency, Brakes, Comfort, Reinforcement }
    public sealed class TrainMaintenance:MonoBehaviour
    {
        private TrainController train;private TrainDayEconomy economy;private TrainPart[] parts;
        private readonly int[] levels=new int[4];
        public bool HasStorageCar {get;private set;}
        public int Level(TrainUpgrade upgrade)=>levels[(int)upgrade];
        public float ComfortFactor=>Mathf.Pow(.8f,Level(TrainUpgrade.Comfort));
        public float WearFactor=>Mathf.Pow(.75f,Level(TrainUpgrade.Reinforcement));
        public float BoilerPerformance=>parts==null?1f:Mathf.Lerp(.25f,1f,parts.First(p=>p.Product==RailwayProduct.BoilerPart).Condition.Health/100f);
        public float BrakePerformance=>parts==null?1f:Mathf.Lerp(.3f,1f,parts.First(p=>p.Product==RailwayProduct.BrakePart).Condition.Health/100f)*(1f+.15f*Level(TrainUpgrade.Brakes));
        public int Price(TrainUpgrade upgrade)=>GetComponent<RailwayInventory>().Config.upgradeBasePrices[(int)upgrade]*(Level(upgrade)+1);
        public int[] Snapshot()=>levels.ToArray();
        public void Restore(int[] saved){for(int i=0;i<levels.Length;i++)levels[i]=saved!=null&&i<saved.Length?Mathf.Clamp(saved[i],0,3):0;Apply();}
        public void Configure(TrainDayEconomy owner)
        {
            train=GetComponent<TrainController>();economy=owner;
            var m=owner.Palette;if(m==null)return;
            if(train.Cars[0].transform.Find("Boiler Service Panel")==null)
            {
                Install(train.Cars[0],"Boiler Service Panel",new Vector3(-1.13f,2.05f,5.9f),RailwayProduct.BoilerPart,m);
                Install(train.Cars[0],"Brake Service Panel",new Vector3(2.12f,1.65f,-1.9f),RailwayProduct.BrakePart,m);
                foreach(var car in train.Cars.Where(c=>c.Kind==CarKind.Passenger))Install(car,"Coach Service Panel",new Vector3(2.13f,1.55f,0),RailwayProduct.CoachPart,m);
                var wrench=MaintenanceWrench.Create(m);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wrench,gameObject.scene);wrench.transform.SetParent(train.Cars[1].transform,false);wrench.transform.localPosition=new Vector3(1.4f,1.9f,-2.6f);wrench.GetComponent<Rigidbody>().isKinematic=true;wrench.AddComponent<StoredRailTool>();
                RouteProps.Box(train.Cars[1].transform,"Wrench Rack",new Vector3(1.4f,1.6f,-2.6f),new Vector3(.5f,.16f,.75f),m[2],true);
            }
            parts=train.Cars.SelectMany(c=>c.GetComponentsInChildren<TrainPart>()).ToArray();Apply();
        }
        private static void Install(TrainCar car,string name,Vector3 position,RailwayProduct product,Material[] m)
        {
            var panel=new GameObject(name);panel.transform.SetParent(car.transform,false);panel.transform.localPosition=position;var body=RouteProps.Box(panel.transform,"Service Access Plate",Vector3.zero,new Vector3(.12f,.48f,.62f),m[3],true);var part=panel.AddComponent<TrainPart>();part.Configure(product,car.Owner.GetComponent<RailwayInventory>(),m);
            panel.AddComponent<ControlHighlight>().Configure(m[9],new[]{body.GetComponent<Renderer>()});
            var label=RouteProps.Board(panel.transform,product==RailwayProduct.BoilerPart?"BOILER":product==RailwayProduct.BrakePart?"BRAKES":"COACH",new Vector3(product==RailwayProduct.BoilerPart?-.085f:.085f,.34f,0),new Vector3(.65f,.18f,.05f),m,Quaternion.Euler(0,product==RailwayProduct.BoilerPart?90:-90,0));
        }
        private void Apply(){train=GetComponent<TrainController>();if(train!=null)train.Resources.EfficiencyMultiplier=1f-.08f*Level(TrainUpgrade.SteamEfficiency);}
        public bool BuyUpgrade(TrainUpgrade upgrade)
        {
            if(economy==null||!economy.AtSettlement("Redfort")||Level(upgrade)>=3||!economy.Wallet.SpendBlumt(Price(upgrade)))return false;
            levels[(int)upgrade]++;Apply();economy.World.SavePreparation();return true;
        }
        public bool BuyStorageCar()
        {
            if(economy==null||HasStorageCar||!economy.AtSettlement("Redfort")||train.ConnectedCount!=train.Cars.Count||!economy.Wallet.Spend(economy.Wallet.Config.storageCarriagePrice))return false;
            var source=train.Cars[1];var copy=Instantiate(source.gameObject,source.transform.parent);copy.name="Temporary Storage Car";
            foreach(var item in copy.GetComponentsInChildren<PickupObject>()){item.gameObject.SetActive(false);if(Application.isPlaying)Destroy(item.gameObject);else DestroyImmediate(item.gameObject);}
            foreach(string name in new[]{"Rail Repair Locker","Onboard Service Ledger","Wrench Rack"}){var unwanted=copy.transform.Find(name);if(unwanted!=null){unwanted.gameObject.SetActive(false);if(Application.isPlaying)Destroy(unwanted.gameObject);else DestroyImmediate(unwanted.gameObject);}}
            var cars=train.Cars.Concat(new[]{copy.GetComponent<TrainCar>()}).ToArray();train.Configure(train.Path,cars,train.HeadDistance);train.Braking=true;HasStorageCar=true;
            RouteProps.Board(copy.transform,"FREIGHT STORAGE",new Vector3(2.03f,1.55f,0),new Vector3(2.6f,.35f,.08f),economy.Palette,Quaternion.Euler(0,-90,0));return true;
        }
        public void Step(float dt)
        {
            if(train==null||parts==null||dt<=0f||train.IsDerailed||economy.World.DayEnded)return;
            foreach(var part in parts)
            {
                float wear=part.Product==RailwayProduct.BoilerPart?(train.Resources.Pressure>90f?.08f*train.Throttle:0f):part.Product==RailwayProduct.BrakePart?(Mathf.Abs(train.Speed)>3f&&train.BrakeAmount>.85f?.04f:0f):0f;
                part.Condition.Damage(wear*dt*WearFactor*(part.Improvised?2f:1f));
            }
        }
        private void FixedUpdate()=>Step(Time.fixedDeltaTime);
        public void TrackImpact(float amount){if(parts==null)return;foreach(var part in parts)part.Condition.Damage(amount*WearFactor);}
    }
}
