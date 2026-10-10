using System.Linq;
using AllAboard.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
namespace AllAboard.Train
{
    public sealed class JourneyUI:MonoBehaviour
    {
        private ProceduralRailway world;private TrainDayEconomy economy;private RailwayInventory bag;private PlayerInteraction actor;private PlayerInputReader input;
        private UIDocument document;private PanelSettings settings;private VisualElement root,shade,body;private Label status,prompt,held,title,balance,notice;private ProgressBar charge;
        private ShopVendor shop;private string page;private float refreshAt;private VisualElement navigation;private Button closeButton;
        public bool Visible=>shade!=null&&shade.style.display!=DisplayStyle.None;
        public UIDocument Document=>document;
        private static readonly Color Navy=new Color(.055f,.10f,.14f),Card=new Color(.10f,.17f,.21f),Cream=new Color(.95f,.91f,.80f),Gold=new Color(.94f,.72f,.33f);
        public void Configure(ProceduralRailway owner)
        {
            Close();world=owner;economy=world.Train.GetComponent<TrainDayEconomy>();bag=economy.Wallet;
            actor=world.gameObject.scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PlayerInteraction>()).FirstOrDefault();input=actor!=null?actor.GetComponent<PlayerInputReader>():null;
            if(actor!=null){actor.ShowInteractionPrompts(false);var old=actor.GetComponent<CombatPlayerStatus>();if(old!=null)old.enabled=false;}
            if(document==null){settings=ScriptableObject.CreateInstance<PanelSettings>();settings.name="Frontier UI Panel";settings.themeStyleSheet=Resources.Load<ThemeStyleSheet>("FrontierUI/FrontierTheme");settings.scaleMode=PanelScaleMode.ScaleWithScreenSize;settings.referenceResolution=new Vector2Int(1280,720);settings.match=.5f;document=gameObject.AddComponent<UIDocument>();document.panelSettings=settings;document.sortingOrder=50;}
            BuildFrame();ShopVendor.Install(this,world);
            if(Application.isPlaying&&EventSystem.current==null)
            {
                var events=new GameObject("Frontier UI Events");events.transform.SetParent(transform,false);
                events.AddComponent<EventSystem>();events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }
        private void BuildFrame()
        {
            root=document.rootVisualElement;root.Clear();root.style.flexGrow=1;root.style.color=Cream;root.style.fontSize=16;root.style.unityFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");root.pickingMode=PickingMode.Ignore;
            status=Text(root,"");status.name="journey-status";status.style.position=Position.Absolute;status.style.left=20;status.style.top=18;Panel(status);status.style.paddingLeft=16;status.style.paddingRight=16;
            var bottom=Box(root);bottom.pickingMode=PickingMode.Ignore;bottom.style.position=Position.Absolute;bottom.style.bottom=24;bottom.style.alignSelf=Align.Center;bottom.style.width=650;
            prompt=Text(bottom,"");prompt.style.unityTextAlign=TextAnchor.MiddleCenter;prompt.style.color=Gold;
            held=Text(bottom,"");held.style.unityTextAlign=TextAnchor.MiddleCenter;
            charge=new ProgressBar{lowValue=0,highValue=100,title="Throw strength"};bottom.Add(charge);charge.style.height=18;
            var dot=Text(root,"•");dot.name="aim-dot";dot.style.position=Position.Absolute;dot.style.left=Length.Percent(50);dot.style.top=Length.Percent(50);dot.style.marginLeft=-4;dot.style.marginTop=-12;dot.style.fontSize=22;
            shade=Box(root);shade.style.position=Position.Absolute;shade.style.left=0;shade.style.right=0;shade.style.top=0;shade.style.bottom=0;shade.style.backgroundColor=new Color(0,0,0,.72f);shade.style.alignItems=Align.Center;shade.style.justifyContent=Justify.Center;
            var panel=Box(shade);Panel(panel);panel.style.width=940;panel.style.maxWidth=Length.Percent(94);panel.style.height=640;panel.style.maxHeight=Length.Percent(94);panel.style.paddingLeft=24;panel.style.paddingRight=24;panel.style.paddingTop=20;panel.style.paddingBottom=20;
            var heading=Row(panel);title=Text(heading,"FRONTIER RAILWAY");title.style.fontSize=26;title.style.unityFontStyleAndWeight=FontStyle.Bold;title.style.flexGrow=1;closeButton=Button(heading,"Close · Esc",Close);
            balance=Text(panel,"");balance.style.color=Gold;balance.style.marginBottom=14;
            navigation=Row(panel);foreach(string tab in new[]{"Journey","Passengers","Freight","Supplies","Accounts","Workshop"}){string choice=tab;Button(navigation,tab,()=>SelectPage(choice));}
            notice=Text(panel,"");notice.style.color=Gold;notice.style.marginTop=10;notice.style.marginBottom=8;
            var scroll=new ScrollView();scroll.style.flexGrow=1;panel.Add(scroll);body=scroll.contentContainer;
            shade.style.display=DisplayStyle.None;
        }
        public bool OpenShop(ShopVendor vendor,PlayerInteraction player)
        {
            if(world.DayEnded||bag.Settled||vendor==null||player!=actor||actor.IsUsingHandControl)return false;
            shop=vendor;page="Shop";Open();RenderPage();return true;
        }
        public void OpenLedger(){if(world==null)return;shop=null;page=world.DayEnded?"Day report":"Journey";Open();RenderPage();}
        public void SelectPage(string value){if(world==null||world.DayEnded||!new[]{"Journey","Passengers","Freight","Supplies","Accounts","Workshop"}.Contains(value))return;shop=null;page=value;RenderPage();}
        private void Open(){shade.style.display=DisplayStyle.Flex;input?.SetMenuOpen(true);actor?.ReleaseThrottle();notice.text="";}
        public void Close(){if(shade!=null)shade.style.display=DisplayStyle.None;shop=null;page=null;if(input!=null)input.SetMenuOpen(false);}
        private void Update()
        {
            if(world==null||actor==null||document==null)return;
            var keys=Keyboard.current;
            if(world.DayEnded&&page!="Day report"){shop=null;page="Day report";Open();RenderPage();}
            else if(keys!=null&&keys.escapeKey.wasPressedThisFrame&&Visible&&!world.DayEnded)Close();
            else if(keys!=null&&keys.tabKey.wasPressedThisFrame&&!world.DayEnded){if(Visible)Close();else if(input.HasControl)OpenLedger();}
            if(shop!=null&&(Vector3.Distance(actor.transform.position,shop.transform.position)>6f)){Close();}
            if(Time.unscaledTime<refreshAt)return;refreshAt=Time.unscaledTime+.15f;
            var health=actor.GetComponent<CombatHealth>();var resources=world.Train.Resources;
            bool speedBroken=world.Train.Cars[0].GetComponentsInChildren<TrainInstrument>().Any(i=>i.Kind==InstrumentKind.Speed&&i.Condition!=null&&i.Condition.IsBroken);
            status.text="DAY "+bag.Day+"   ·   "+bag.Gold+" GOLD   ·   "+(health!=null?health.Health.ToString("0")+" HEALTH":"")+"\nSteam "+resources.Steam.ToString("0")+"   |   Speed "+(speedBroken?"BROKEN":Mathf.Abs(world.Train.Speed).ToString("0.0")+" m/s")+"   |   Tab: ledger";
            status.style.display=Visible?DisplayStyle.None:DisplayStyle.Flex;
            prompt.text=Visible?"":actor.ToolProgress>=0?actor.ToolPrompt:actor.HoverPrompt;var item=actor.HeldItem;var weapon=item!=null?item.GetComponent<WeaponItem>():null;
            held.text=Visible?"":item!=null?ItemDetail(item):("E: interact   ·   C: crouch   ·   Shift: sprint");
            charge.title=actor.ToolProgress>=0?"Repair progress":"Throw strength";charge.value=(actor.ToolProgress>=0?actor.ToolProgress:actor.Charge)*100;charge.style.display=!Visible&&(actor.Charge>0||actor.ToolProgress>=0)?DisplayStyle.Flex:DisplayStyle.None;
            root.Q<Label>("aim-dot").style.display=!Visible&&input.HasControl&&(!actor.IsHolding||weapon!=null)?DisplayStyle.Flex:DisplayStyle.None;
            if(Visible){Balance();if(shop!=null)UpdateShopButtons();}
        }
        private void Balance()=>balance.text="DAY "+bag.Day+"     GOLD "+bag.Gold+"     CREDIT "+bag.CreditDebt+" / "+bag.Config.creditLimit+"     STEEL "+bag.SteelMetres.ToString("0.#")+" m     BLUMT "+bag.Blumt;
        private void RenderPage()
        {
            if(body==null)return;body.Clear();title.text=shop!=null?shop.ShopTitle.ToUpperInvariant():page=="Day report"?"DAY "+bag.Day+" · JOURNEY REPORT":"FRONTIER RAILWAY · "+page.ToUpperInvariant();Balance();
            navigation.style.display=world.DayEnded?DisplayStyle.None:DisplayStyle.Flex;closeButton.style.display=world.DayEnded?DisplayStyle.None:DisplayStyle.Flex;
            if(shop!=null){Shop();return;}
            if(page=="Supplies"){Supplies();return;}if(page=="Accounts"){Accounts();return;}if(page=="Day report"){Report();return;}if(page=="Passengers"){Passengers();return;}if(page=="Freight"){Freight();return;}if(page=="Workshop"){Workshop();return;}Journey();
        }
        private void Shop()
        {
            Text(body,"Browse supplies below. Physical items go on the collection tray; repair stock goes into your ledger. Prices use Gold first, then available credit.");
            int sale=shop.SalePrice(actor);if(sale>0)Button(body,"Sell held salvage · "+sale+" Gold",()=>{if(shop.Sell(actor)){notice.text="Salvage sold.";RenderPage();}});
            for(int i=0;i<shop.Offers.Count;i++)
            {
                int index=i;var offer=shop.Offers[i];var card=Box(body);Panel(card);card.style.backgroundColor=Card;card.style.marginBottom=8;card.style.paddingLeft=14;card.style.paddingRight=14;
                var row=Row(card);var name=Text(row,offer.Title);name.style.fontSize=19;name.style.flexGrow=1;
                var buy=Button(row,"Buy · "+offer.Price+" Gold",()=>{shop.Buy(index,actor,out string message);notice.text=message;Balance();UpdateShopButtons();});buy.name="buy-"+i;
                Text(card,offer.Description).style.fontSize=14;
            }
            UpdateShopButtons();
        }
        private void UpdateShopButtons(){if(shop==null)return;for(int i=0;i<shop.Offers.Count;i++){var b=body.Q<Button>("buy-"+i);if(b!=null){bool available=!bag.Settled&&bag.CanPurchase(shop.Offers[i].Price)&&(!shop.Offers[i].Physical||shop.Pending<1);b.SetEnabled(available);b.style.opacity=available?1:.45f;}}}
        private void Supplies()
        {Text(body,"REPAIR STEEL   "+bag.SteelMetres.ToString("0.#")+" m");foreach(RailwayProduct product in System.Enum.GetValues(typeof(RailwayProduct)))if(product>=RailwayProduct.Scrap&&product!=RailwayProduct.Wrench)Text(body,product+"   "+bag.Stock(product));Text(body,"Tools, coal, weapons and valuables are physical objects. Carry them, drop them with Q, or hold Q to throw. Use the saw to cut strained rail and the connector to preview missing-rail repairs.");}
        private void Passengers()
        {
            Text(body,economy.ActiveCount+" / "+bag.Config.passengerCapacity+" passengers aboard");
            foreach(var group in economy.Passengers.Where(p=>p!=null&&!p.Resolved).GroupBy(p=>p.Destination)){Text(body,economy.DestinationTitle(group.Key)+" · "+group.Count()+" travellers · expected "+group.Sum(p=>p.ExpectedFare)+" Gold");foreach(var p in group)Text(body,p.Name+" · "+p.Happiness.ToString("0")+"% happy · "+(p.WantsToLeave?"leaving at the next stop":p.MissedStop?"missed destination":p.TravelSeconds>p.Deadline?"late":"on schedule")).style.fontSize=14;}
            foreach(var pair in economy.Queues){var waiting=pair.Value.Count(p=>!p.Aboard&&!p.Resolved);if(waiting==0)continue;string id=pair.Key;var button=Button(body,"Board "+waiting+" at "+economy.DestinationTitle(id),()=>{notice.text="Boarded "+economy.BoardWaiting(id)+" passengers.";RenderPage();});button.SetEnabled(economy.AtSettlement(id));}
            Text(body,"Stop at the destination to let passengers off. Isolated-stop fares are 5× Terminum fares. Smooth driving protects happiness and tips.");
        }
        private void Freight()
        {
            foreach(var offer in economy.Cargo.Offers.Where(o=>!o.Resolved))
            {
                Text(body,offer.title+" → "+economy.DestinationTitle(offer.destination)+" · "+offer.reward+" Gold");
                if(!offer.Accepted){var captured=offer;var button=Button(body,"Accept at "+economy.DestinationTitle(offer.origin),()=>{notice.text=economy.Cargo.Accept(captured)?"Contract accepted. Load the crate onto your train.":"Unable to accept.";RenderPage();});button.SetEnabled(economy.AtSettlement(offer.origin));}
                else Text(body,"Accepted · crate condition "+(offer.crate!=null?offer.crate.Health.ToString("0")+"%":"lost")+". Carry it to the destination’s delivery control.");
            }
        }
        private void Journey()
        {
            Text(body,"REDFORT → TERMINUM").style.fontSize=24;Text(body,"Prepare fuel and repair supplies, choose your route, and deliver passengers and freight. The direct journey takes about 15 minutes before stops.");
            Text(body,"Trackside switches: stop, get out, press E. Switches stay usable until the locomotive reaches the split. Brittle and strained rail: 3 m/s maximum. Missing rail must be repaired before crossing.");
            Text(body,"High-risk routes include bandit encounters. Weapons cannot hurt passengers or the crew. Check the route signs before choosing your branch.");
            Text(body,"Quota "+bag.Quota+" Gold   ·   Terminum entry "+bag.Config.entryFee+" Gold   ·   Outstanding obligations "+bag.Obligations+" Gold");
            foreach(var node in world.Network.nodes.Where(n=>n.place==RoutePlace.Town||n.place==RoutePlace.Village||n.place==RoutePlace.Station||n.place==RoutePlace.Bluefort||n.place==RoutePlace.Terminum))Text(body,node.title+" · "+node.family+" · "+(world.NodeDistance(node.id)>=0?"selected route":"off route")+" · destination fare "+bag.Config.Destination(node.place));
            foreach(var encounter in world.GetComponent<CombatWorld>().Encounters)Text(body,"Bandit camp · "+encounter.EdgeId+" · toll "+encounter.Toll+" Gold · "+encounter.State);
            if(economy.ReadyToSettle)Button(body,"Finish and settle this day",()=>economy.Finish(true));
        }
        private void Workshop()
        {
            var maintenance=world.Train.GetComponent<TrainMaintenance>();Text(body,"Redfort workshop · upgrades use saved Blumt. You must be stopped at Redfort.");
            foreach(TrainUpgrade upgrade in System.Enum.GetValues(typeof(TrainUpgrade))){var choice=upgrade;var button=Button(body,upgrade+" · level "+maintenance.Level(upgrade)+" / 3 · "+maintenance.Price(upgrade)+" Blumt",()=>{maintenance.BuyUpgrade(choice);RenderPage();});button.SetEnabled(economy.AtSettlement("Redfort")&&maintenance.Level(upgrade)<3&&bag.Blumt>=maintenance.Price(upgrade));}
            var storage=Button(body,"Hire storage carriage · "+bag.Config.storageCarriagePrice+" Gold",()=>{maintenance.BuyStorageCar();RenderPage();});storage.SetEnabled(economy.AtSettlement("Redfort")&&!maintenance.HasStorageCar&&bag.Gold>=bag.Config.storageCarriagePrice);
        }
        private void Accounts()
        {
            Text(body,"Cash "+bag.Gold+" Gold · Credit "+bag.CreditDebt+" Gold · Obligations "+bag.Obligations+" Gold\nQuota "+bag.Quota+" Gold · Carried repair debt "+bag.RepairDebt+" Gold\nTerminum entry "+(bag.EntryPaid?"paid":bag.Config.entryFee+" Gold due"));
            Text(body,"TRAIN CONDITION").style.fontSize=22;
            foreach(var part in world.Train.Cars.SelectMany(c=>c.GetComponentsInChildren<TrainPart>()))Text(body,part.GetComponentInParent<TrainCar>().name+" · "+part.Product+" · "+part.Condition.Health.ToString("0")+"%"+(part.Improvised?" · temporary scrap repair":""));
            Text(body,"Use the starting wrench at a damaged service panel. E mounts it; hold M1 and move the mouse forward/back. A matching part fully repairs the component. Scrap and screws provide a temporary repair. Hovering shows the repair preview before supplies are consumed.");
        }
        private string ItemDetail(PickupObject item)
        {
            var weapon=item.GetComponent<WeaponItem>();if(weapon!=null)return weapon.Status;
            string detail=item.name.Replace("(Clone)","");var cargo=item.GetComponent<CargoCrate>();
            if(cargo!=null&&cargo.Contract!=null)detail=cargo.Contract.title+" → "+economy.DestinationTitle(cargo.Contract.destination)+" · "+cargo.Health.ToString("0")+"% condition";
            else if(item.GetComponent<MedicalItem>()!=null)detail+=" · M1: use";
            else if(item.GetComponent<CoalItem>()!=null)detail="Coal · 40 steam · hold or throw into the furnace";
            else if(item.GetComponent<RailTool>()!=null||item.GetComponent<MaintenanceWrench>()!=null)detail+=" · look at a repair target and press E";
            else if(item.GetComponent<LootCargo>()!=null)detail+=" · sell through a vendor’s menu";
            return detail+"\nQ: drop / hold to throw · "+input.WeldKeyLabel+": attach";
        }
        private void Report()
        {
            Text(body,world.DayEndReason??bag.DayReport).style.fontSize=22;
            Text(body,"Starting funds   "+bag.Config.startingGold+" Gold\nIncome earned   "+bag.TotalEarned+" Gold\nSupplies purchased   "+bag.TotalPurchased+" Gold\nEntry fee   "+(bag.EntryPaid?bag.Config.entryFee:0)+" Gold\nOther cash spending   "+Mathf.Max(0,bag.TotalCashSpent-(bag.EntryPaid?bag.Config.entryFee:0))+" Gold\nCredit due   "+bag.CreditDebt+" Gold\nContract obligations   "+bag.Obligations+" Gold\nQuota   "+bag.SettlementQuota+" Gold\nRepairs paid   "+bag.LastRepairsPaid+" Gold\nNext-day repair debt   "+bag.RepairDebt+" Gold\nSaved Blumt   "+bag.Blumt);
            Text(body,"Passengers resolved   "+economy.Passengers.Count(p=>p!=null&&p.Resolved)+"   ·   Freight resolved   "+economy.Cargo.Offers.Count(o=>o.Resolved));
            if(bag.DayReport!=null&&bag.DayReport.StartsWith("Day complete"))Button(body,"Prepare next day",()=>{Close();world.PrepareNextDay();});
            Button(body,"Start a fresh run",()=>{Close();world.StartFreshRun();});
        }
        private static VisualElement Box(VisualElement parent){var v=new VisualElement();parent.Add(v);return v;}
        private static VisualElement Row(VisualElement parent){var v=Box(parent);v.style.flexDirection=FlexDirection.Row;v.style.alignItems=Align.Center;v.style.flexWrap=Wrap.Wrap;return v;}
        private static Label Text(VisualElement parent,string text){var v=new Label(text);parent.Add(v);v.style.whiteSpace=WhiteSpace.Normal;v.style.marginBottom=8;v.style.marginTop=4;return v;}
        private static void Panel(VisualElement v){v.style.backgroundColor=Navy;v.style.borderBottomLeftRadius=10;v.style.borderBottomRightRadius=10;v.style.borderTopLeftRadius=10;v.style.borderTopRightRadius=10;v.style.paddingTop=10;v.style.paddingBottom=10;}
        private static Button Button(VisualElement parent,string caption,System.Action action)
        {var b=new Button(action){text=caption};parent.Add(b);b.style.backgroundColor=Gold;b.style.color=Navy;b.style.unityFontStyleAndWeight=FontStyle.Bold;b.style.paddingLeft=14;b.style.paddingRight=14;b.style.paddingTop=10;b.style.paddingBottom=10;b.style.marginRight=8;b.style.marginBottom=6;b.style.borderBottomLeftRadius=6;b.style.borderBottomRightRadius=6;b.style.borderTopLeftRadius=6;b.style.borderTopRightRadius=6;b.RegisterCallback<MouseEnterEvent>(_=>b.style.backgroundColor=Cream);b.RegisterCallback<MouseLeaveEvent>(_=>b.style.backgroundColor=Gold);return b;}
        private void OnDisable(){Close();}
        private void OnDestroy(){if(settings!=null)CraftGeometry.Dispose(settings);}
    }
}
