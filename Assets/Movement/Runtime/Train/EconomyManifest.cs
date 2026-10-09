using System.Linq;
using AllAboard.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AllAboard.Train
{
    public sealed class EconomyManifest:MonoBehaviour
    {
        private TrainDayEconomy economy;private PlayerInputReader input;
        private int tab;private Vector2 scroll;private GUIStyle panel,heading,text,button,small;private Texture2D backdrop,brass,enamel;
        public bool Visible {get;private set;}
        public void Configure(TrainDayEconomy owner){economy=owner;input=owner.World.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<PlayerInputReader>()).FirstOrDefault();}
        public void Open(){Visible=true;input?.SetMenuOpen(true);}
        public void Close(){Visible=false;input?.SetMenuOpen(false);}
        private void OnDisable(){if(Visible)Close();}
        private void Update(){var keyboard=Keyboard.current;if(keyboard==null)return;if(keyboard.tabKey.wasPressedThisFrame){if(Visible)Close();else if(input!=null&&(input.HasControl||economy.World.DayEnded))Open();}else if(Visible&&keyboard.escapeKey.wasPressedThisFrame)Close();}
        private static Texture2D Solid(Color color){var tex=new Texture2D(1,1);tex.SetPixel(0,0,color);tex.Apply();return tex;}
        private void Styles()
        {
            if(panel!=null)return;backdrop=Solid(new Color(.045f,.07f,.1f,.98f));brass=Solid(new Color(.72f,.51f,.2f));enamel=Solid(new Color(.11f,.2f,.27f));
            panel=new GUIStyle(GUI.skin.box){padding=new RectOffset(24,24,18,18)};panel.normal.background=backdrop;
            heading=new GUIStyle(GUI.skin.label){fontSize=24,fontStyle=FontStyle.Bold};heading.normal.textColor=new Color(1f,.84f,.5f);
            text=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true};text.normal.textColor=new Color(.93f,.92f,.83f);
            small=new GUIStyle(text){fontSize=13};
            button=new GUIStyle(GUI.skin.button){fontSize=15,fontStyle=FontStyle.Bold,padding=new RectOffset(14,14,10,10),margin=new RectOffset(4,4,4,4)};button.normal.background=enamel;button.normal.textColor=new Color(1f,.89f,.66f);button.hover.background=brass;button.active.background=brass;
        }
        private void OnGUI()
        {
            if(economy==null)return;Styles();var bag=economy.Wallet;
            if(!Visible)
            {GUI.Label(new Rect(18,Screen.height-66,420,56),"Gold "+bag.Gold+"  |  Quota "+bag.Quota+"  |  Credit "+bag.CreditDebt+"\nTab: service ledger   •   Passengers "+economy.ActiveCount+" / "+bag.Config.passengerCapacity,small);if(economy.GateBlocked)GUI.Box(new Rect(Screen.width/2-220,60,440,54),"Terminum entry: "+bag.Config.entryFee+" Gold\nReverse to choose the bypass.",panel);return;}
            float width=Mathf.Min(1040,Screen.width-40),height=Mathf.Min(720,Screen.height-40);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height),panel);
            GUILayout.BeginHorizontal();GUILayout.Label("RAILWAY SERVICE LEDGER",heading);if(GUILayout.Button("Close · Tab",button,GUILayout.Width(150)))Close();GUILayout.EndHorizontal();
            GUILayout.Label("DAY "+bag.Day+"  •  "+economy.Difficulty+"  •  GOLD "+bag.Gold+"  •  CREDIT "+bag.CreditDebt+" / "+bag.Config.creditLimit+"  •  BLUMT "+bag.Blumt,text);
            GUILayout.BeginHorizontal();string[] tabs={"Passengers","Freight","Route","Accounts","Run"};for(int i=0;i<tabs.Length;i++)if(GUILayout.Button((tab==i?"◆ ":"")+tabs[i],button)){tab=i;scroll=Vector2.zero;}GUILayout.EndHorizontal();
            scroll=GUILayout.BeginScrollView(scroll);
            if(tab==0)Passengers();else if(tab==1)Freight();else if(tab==2)Route();else if(tab==3)Accounts();else Progress();
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
        private void Passengers()
        {
            GUILayout.Label("ONBOARD · "+economy.ActiveCount+" / "+economy.Wallet.Config.passengerCapacity,heading);
            foreach(var group in economy.Passengers.Where(p=>p!=null&&!p.Resolved).GroupBy(p=>p.Destination))
            {GUILayout.Label(economy.DestinationTitle(group.Key)+" — "+group.Count()+" travellers — expected "+group.Sum(p=>p.ExpectedFare)+" Gold",text);foreach(var p in group)GUILayout.Label("    "+p.Name+"  |  "+p.Happiness.ToString("0")+"% happy  |  "+(p.WantsToLeave?"will leave at next stop":p.MissedStop?"missed stop; Terminum fallback 100 Gold":p.TravelSeconds>p.Deadline?"late":"on schedule"),small);}
            GUILayout.Space(12);GUILayout.Label("PLATFORM DEPARTURES",heading);
            foreach(var pair in economy.Queues)
            {
                var waiting=pair.Value.Where(p=>p!=null&&!p.Aboard&&!p.Resolved).ToArray();if(waiting.Length==0)continue;
                GUILayout.Label(economy.DestinationTitle(pair.Key)+" — "+waiting.Length+" waiting",text);
                GUI.enabled=economy.AtSettlement(pair.Key)&&economy.ActiveCount<economy.Wallet.Config.passengerCapacity;
                if(GUILayout.Button("Board available travellers",button))economy.BoardWaiting(pair.Key);GUI.enabled=true;
                foreach(var p in waiting)GUILayout.Label("    "+p.Name+" → "+economy.DestinationTitle(p.Destination)+"  |  expected "+p.ExpectedFare+" Gold",small);
            }
            GUILayout.Label("Correct stops pay job + destination + happiness tip. Missed destinations pay only 100 Gold at Terminum. Isolated stations are drop-off only.",small);
        }
        private void Freight()
        {
            GUILayout.Label("PHYSICAL FREIGHT · carry crates to the delivery desk",heading);
            foreach(var offer in economy.Cargo.Offers.Where(o=>o.Accepted||economy.AtSettlement(o.origin)))
            {
                GUILayout.Label(offer.title+"  |  "+economy.DestinationTitle(offer.origin)+" → "+economy.DestinationTitle(offer.destination)+"  |  "+offer.reward+" Gold",text);
                GUILayout.Label(offer.Resolved?offer.Outcome.ToString():offer.Accepted?"Accepted · crate condition "+(offer.crate!=null?offer.crate.Health.ToString("0")+"%":"lost"):"Available · crate appears at the station desk",small);
                if(!offer.Accepted&&!offer.Resolved){GUI.enabled=economy.AtSettlement(offer.origin);if(GUILayout.Button("Accept freight contract",button))economy.Cargo.Accept(offer);GUI.enabled=true;}
            }
            GUILayout.Label("Intact: 100% payout. Damaged: 75%. Failed: "+(economy.Wallet.Config.contractPenaltyFraction*100).ToString("0")+"% penalty. Unfinished contracts fail at day end. A crate aboard the train is not delivered until it reaches the correct desk.",small);
        }
        private void Route()
        {
            GUILayout.Label("DESTINATIONS & RISK",heading);
            foreach(var node in economy.World.Network.nodes.Where(n=>n.place==RoutePlace.Town||n.place==RoutePlace.Village||n.place==RoutePlace.Station||n.place==RoutePlace.Bluefort||n.place==RoutePlace.Terminum))
                GUILayout.Label(node.title+"  |  "+node.family+"  |  "+node.place+"  |  destination reward "+economy.Wallet.Config.Destination(node.place)+"  |  "+(economy.World.NodeDistance(node.id)>=0?"on selected route":"off selected route"),text);
            GUILayout.Label("Stop and leave the train to change trackside switches. Hard routes offer more demand and higher-value jobs, with damaged rails to repair. Brittle and strained rails require 3 m/s or less.",small);
            var combat=economy.World.GetComponent<CombatWorld>();if(combat!=null)foreach(var encounter in combat.Encounters)GUILayout.Label("Bandit camp - "+encounter.EdgeId+" - "+encounter.Toll+" Gold toll - "+encounter.State,text);
        }
        private void Accounts()
        {
            var bag=economy.Wallet;GUILayout.Label("TODAY'S ACCOUNTS",heading);
            GUILayout.Label("Cash: "+bag.Gold+" Gold\nOutstanding credit: "+bag.CreditDebt+" Gold\nOther obligations: "+bag.Obligations+" Gold\nQuota: "+bag.Quota+" Gold (includes carried repairs "+bag.RepairDebt+")\nTerminum entry: "+(bag.EntryPaid?"PAID":bag.Config.entryFee+" Gold due before arrival")+"\nSteel: "+bag.SteelMetres.ToString("0.0")+" m",text);
            foreach(var product in new[]{RailwayProduct.Scrap,RailwayProduct.Screws,RailwayProduct.Wood,RailwayProduct.BoilerPart,RailwayProduct.BrakePart,RailwayProduct.CoachPart})GUILayout.Label(product+": "+bag.Stock(product),text);
            GUILayout.Space(10);GUILayout.Label("TRAIN CONDITION � wrench repairs",heading);foreach(var part in economy.World.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<TrainPart>()))GUILayout.Label(part.GetComponentInParent<TrainCar>().name+" � "+part.Product+" � "+part.Condition.Health.ToString("0")+"%"+(part.Improvised?" � temporary scrap repair":""),text);GUILayout.Label("Carry the starting wrench to a service panel. E mounts it; hold M1 and move forward/back eight times. A matching part fully repairs; scrap + screws restore 35 points. Hovering previews the patch before you spend supplies.",small);
            if(bag.Settled)GUILayout.Label(bag.DayReport,text);
            if(economy.ReadyToSettle&&!bag.Settled){GUILayout.Label("Deliver your remaining freight before settling. Finishing now applies penalties to unfinished contracts.",small);if(GUILayout.Button("Finish the day and settle accounts",button))economy.Finish(true);}
            GUILayout.Label("Settlement order: credit and obligations → quota → damage bill → Blumt. Entry is paid separately. A failed quota ends the run.",small);
        }
        private void Progress()
        {
            GUILayout.Label("RUN PROGRESSION",heading);GUILayout.Label("Blumt bank: "+economy.Wallet.Blumt+"\nProgress saves at completed-day boundaries. Quitting during a journey restarts that day from preparation; it does not restore loose objects mid-journey.",text);
            if(economy.World.DayEnded){GUI.enabled=economy.Wallet.Settled&&economy.Wallet.DayReport.StartsWith("Day complete");if(GUILayout.Button("Prepare the next generated day",button))economy.World.PrepareNextDay();GUI.enabled=true;}
            var maintenance=economy.GetComponent<TrainMaintenance>();if(maintenance!=null){GUI.enabled=economy.AtSettlement("Redfort")&&!maintenance.HasStorageCar&&economy.Wallet.Gold>=economy.Wallet.Config.storageCarriagePrice;if(GUILayout.Button("Hire storage carriage for today � "+economy.Wallet.Config.storageCarriagePrice+" Gold",button))maintenance.BuyStorageCar();GUI.enabled=true;GUILayout.Space(12);GUILayout.Label("REDFORT WORKSHOP � permanent upgrades for this run",heading);foreach(TrainUpgrade upgrade in System.Enum.GetValues(typeof(TrainUpgrade))){GUILayout.Label(upgrade+" � level "+maintenance.Level(upgrade)+" / 3",text);GUI.enabled=economy.AtSettlement("Redfort")&&maintenance.Level(upgrade)<3&&economy.Wallet.Blumt>=maintenance.Price(upgrade);if(GUILayout.Button("Fit "+upgrade+" � "+maintenance.Price(upgrade)+" Blumt",button))maintenance.BuyUpgrade(upgrade);GUI.enabled=true;}}
            GUILayout.Space(16);if(GUILayout.Button("Start a fresh run",button))economy.World.StartFreshRun();
        }
        private void OnDestroy(){foreach(var texture in new[]{backdrop,brass,enamel})if(texture!=null)Destroy(texture);}
    }
}
