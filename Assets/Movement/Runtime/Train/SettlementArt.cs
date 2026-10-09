using System;
using System.Linq;
using UnityEngine;
namespace AllAboard.Train
{
    public static class SettlementArt
    {
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m,bool solid=false)=>RouteProps.Box(p,n,at,size,m,solid);
        private static void Sign(Transform p,string text,Vector3 at,Material[] m,float width=3f)=>RouteProps.Board(p,text,at,new Vector3(width,.45f,.08f),m,Quaternion.Euler(0f,-90f,0f));
        public static void Dress(Transform station,RouteNode node,Material[] m,TrainController train,GameObject coal)
        {
            var rng=new System.Random(WorldArtAssets.Hash(node.id+node.title));
            var assets=station.parent.GetComponent<WorldArtAssets>();if(assets==null)assets=station.parent.gameObject.AddComponent<WorldArtAssets>();
            var cream=assets.Paint(m[8],"Station Cream",new Color(.78f,.69f,.49f));
            var roof=assets.Paint(m[7],"Station Roof",new Color(.27f,.16f,.13f));
            foreach(var part in station.GetComponentsInChildren<Renderer>().Where(x=>x.name.Contains("Station")&&x.name.Contains("Wall")))part.sharedMaterial=cream;
            var flatRoof=station.Find("Station Roof");if(flatRoof!=null)flatRoof.GetComponent<Renderer>().enabled=false;
            BuildingRoof.Build(station,"Station Pitched Roof",new Vector3(14f,0f,-2f),10.2f,14.2f,4.35f,17f,roof,cream,.45f);
            Box(station,"Station Roof Ridge",new Vector3(14f,4.35f+BuildingRoof.Rise(10.2f,17f)+.025f,-2f),new Vector3(.22f,.12f,15.2f),m[6]);
            Box(station,"Station Chimney",new Vector3(16f,5.7f,1f),new Vector3(.65f,2.1f,.65f),m[7]);
            for(float z=-25f;z<=19f;z+=11f)
            {
                Box(station,"Canopy Column",new Vector3(6.9f,2.62f,z),new Vector3(.18f,3f,.18f),m[2],true);
                Box(station,"Column Base",new Vector3(6.9f,1.3f,z),new Vector3(.3f,.3f,.3f),m[6]);
                Box(station,"Canopy Bracket",new Vector3(5f,4.02f,z),new Vector3(5.3f,.16f,.16f),m[6]);
                var brace=Box(station,"Canopy Knee Brace",new Vector3(6.25f,3.62f,z),new Vector3(1.6f,.12f,.14f),m[2]);brace.transform.localRotation=Quaternion.Euler(0f,0f,-35f);
            }
            Box(station,"Platform Canopy",new Vector3(5f,4.15f,-3f),new Vector3(5.5f,.18f,49f),roof);
            Box(station,"Canopy Fascia",new Vector3(2.3f,4.1f,-3f),new Vector3(.12f,.3f,49f),cream);
            for(float z=-24f;z<19f;z+=3f)Box(station,"Canopy Decorative Tooth",new Vector3(2.25f,3.89f,z),new Vector3(.12f,.18f,.3f),cream);
            foreach(float z in new[]{-6.4f,2.4f})
            {
                Box(station,"Station Window",new Vector3(8.87f,2.8f,z),new Vector3(.04f,1.3f,1.5f),m[3]);
                foreach(float y in new[]{2.13f,3.47f})Box(station,"Window Frame",new Vector3(8.81f,y,z),new Vector3(.12f,.08f,1.65f),cream);
                foreach(float dz in new[]{-.78f,0f,.78f})Box(station,"Window Mullion",new Vector3(8.8f,2.8f,z+dz),new Vector3(.1f,1.38f,.07f),cream);
            }
            Box(station,"Ticket Counter",new Vector3(16.5f,1.68f,-2f),new Vector3(1.1f,1.05f,5f),m[2],true);
            Box(station,"Ticket Counter Top",new Vector3(16.5f,2.25f,-2f),new Vector3(1.3f,.13f,5.3f),m[6]);
            Sign(station,"TICKETS",new Vector3(16f,3f,-2f),m,2f);
            foreach(float z in new[]{-6f,2f})
            {Box(station,"Waiting Bench",new Vector3(12.8f,1.58f,z),new Vector3(3f,.18f,1f),m[2],true);Box(station,"Bench Back",new Vector3(12.8f,1.98f,z+.48f),new Vector3(3f,.8f,.1f),m[2],true);foreach(float x in new[]{11.6f,14f})Box(station,"Waiting Bench Support",new Vector3(x,1.32f,z),new Vector3(.14f,.34f,.85f),m[3]);}
            RouteProps.Board(station,node.title,new Vector3(8.7f,3.9f,-2f),new Vector3(4.7f,.45f,.08f),m,Quaternion.Euler(0f,-90f,0f));
            float clockY=node.place==RoutePlace.Terminum?5.6f:3.55f;
            Box(station,"Station Clock Hanger",new Vector3(6.77f,node.place==RoutePlace.Terminum?6.12f:3.99f,-2f),new Vector3(.08f,.4f,.08f),m[5]);
            Box(station,"Station Clock Case",new Vector3(6.77f,clockY,-2f),new Vector3(.18f,.75f,.75f),m[6]);
            Box(station,"Clock Face",new Vector3(6.66f,clockY,-2f),new Vector3(.035f,.64f,.64f),m[8]);
            Box(station,"Clock Minute Hand",new Vector3(6.62f,clockY+.11f,-2f),new Vector3(.02f,.25f,.035f),m[5]);
            Box(station,"Clock Hour Hand",new Vector3(6.62f,clockY,-1.91f),new Vector3(.02f,.035f,.2f),m[5]);
            foreach(float z in new[]{-28f,8f,26f})Lamp(station,new Vector3(6f,1.15f,z),m);
            if(node.place==RoutePlace.Station)
            {Sign(station,"PASSENGER DROP-OFF\nFARE: TERMINUM x5",new Vector3(5.8f,2.6f,-12f),m,3.6f);return;}
            bool city=node.place==RoutePlace.Redfort||node.place==RoutePlace.Bluefort||node.place==RoutePlace.Terminum;
            if(node.place==RoutePlace.SupplyStop)return;
            int houses=city?10:node.place==RoutePlace.Town?7:4;
            int leftCount=houses>=7?(houses+1)/2+1:(houses+1)/2;
            var dirt=assets.Paint(m[0],"Town Road",new Color(.45f,.33f,.20f));
            if(node.place!=RoutePlace.Terminum)
            {
                Box(station,"Town Main Street",new Vector3(45f,.025f,-2f),new Vector3(17f,.05f,105f),dirt);
                foreach(float side in new[]{-1f,1f})Box(station,"Wooden Boardwalk",new Vector3(45f+side*11f,.12f,-2f),new Vector3(5f,.24f,105f),m[2],true);
            }
            if(node.place==RoutePlace.Terminum)CapitalArt.Dress(station,m);
            for(int i=0;i<(node.place==RoutePlace.Terminum?0:houses);i++)
            {
                float side=i<leftCount?-1f:1f;
                int row=i<leftCount?i:i-leftCount;
                float z=(side<0f?-43f:-32f)+row*18f+(float)(rng.NextDouble()-.5f)*6f;
                float setback=18f+(float)(rng.NextDouble()-.5f)*5f;
                var paint=assets.Paint(m[0],"House Paint "+(i%5),new[]{new Color(.53f,.29f,.18f),new Color(.65f,.56f,.39f),new Color(.34f,.43f,.33f),new Color(.57f,.40f,.29f),new Color(.44f,.35f,.28f)}[i%5]);
                House(station,new Vector3(45f+side*setback,.0f,z),side<0f?-90f:90f,5.8f+(float)rng.NextDouble()*2.3f,4f+(float)rng.NextDouble()*2.3f,paint,roof,m,i,node.settlement==SettlementKind.Abandoned);
            }
            int shops=city?4:node.shops;
            var market=new GameObject("Marketplace").transform;market.SetParent(station,false);market.localPosition=new Vector3(28f,0f,58f);
            Box(market,"Market Boardwalk",new Vector3(0f,.2f,0f),new Vector3(18f,.4f,14f),m[2],true);
            Box(market,"Market Entry Step",new Vector3(0f,.1f,-7.3f),new Vector3(6f,.2f,.6f),m[2],true);
            for(int i=0;i<shops;i++)Shop(market,new Vector3(-6f+i*4f,.4f,1f),i,m,train,coal,node.settlement==SettlementKind.Abandoned);
            if(node.settlement==SettlementKind.Mining)
            {Box(station,"Ore Bunker",new Vector3(82f,1.4f,28f),new Vector3(10f,2.8f,8f),m[2]);Box(station,"Mine Headframe",new Vector3(87f,6f,38f),new Vector3(1f,12f,1f),m[2]);}
            if(node.settlement==SettlementKind.Lumber)for(int i=0;i<5;i++)Box(station,"Timber Stack",new Vector3(85f,.5f+i*.22f,25f),new Vector3(11f,.2f,3f),m[2]);
        }
        private static void House(Transform parent,Vector3 p,float yaw,float width,float height,Material paint,Material roof,Material[] m,int variant,bool abandoned)
        {
            int style=variant%6;string[] names={"False Front House","Gabled Cottage","Balcony Shopfront","Log Cabin","Frontier Workshop","Stone Bay Cottage"};
            switch(style)
            {
                case 1:width*=.82f;height=3.5f;roof=m[5];break;
                case 2:width*=1.08f;height=6.8f;roof=m[5];break;
                case 3:width*=.9f;height=3.1f;paint=m[2];break;
                case 4:width*=1.25f;height=5.5f;roof=m[7];break;
                case 5:width*=.88f;height=4.4f;paint=m[8];roof=m[5];break;
            }
            var house=new GameObject((abandoned?"Abandoned ":"")+names[style]).transform;house.SetParent(parent,false);house.localPosition=p;house.localRotation=Quaternion.Euler(0f,yaw,0f);
            Box(house,"House Body",new Vector3(0f,height*.5f,0f),new Vector3(width,height,7f),paint,true);
            for(int i=0;i<5;i++)Box(house,"Facade Clapboard",new Vector3(0f,.8f+i*height/6f,-3.53f),new Vector3(width,.035f,.045f),m[2]);
            if(style==0){Box(house,"Western False Front",new Vector3(0f,height+.4f,-3.6f),new Vector3(width+.25f,1.1f,.22f),paint);Box(house,"Facade Cornice",new Vector3(0f,height+1f,-3.63f),new Vector3(width+.65f,.18f,.4f),m[8]);}
            float pitch=style==1?35f:style==3?16f:style==4?30f:20f;
            float rise=BuildingRoof.Rise(width,pitch);
            BuildingRoof.Build(house,"House Roof",Vector3.zero,width,7f,height,pitch,roof,paint,.35f);
            Box(house,"House Roof Ridge",new Vector3(0f,height+rise+.025f,0f),new Vector3(.2f,.12f,7.8f),roof);
            foreach(float end in new[]{-1f,1f})Box(house,"Gable Tie Beam",new Vector3(0f,height-.035f,end*3.57f),new Vector3(width+.13f,.12f,.15f),m[8]);
            Box(house,"Porch Floor",new Vector3(0f,.24f,-4.7f),new Vector3(width+.4f,.48f,2.1f),m[2],true);
            float porchTop=style==2?3.49f:style==4?3.48f:style==5?3.12f:3.08f;
            if(style!=2)Box(house,"Porch Awning",new Vector3(0f,porchTop-.09f,-4.5f),new Vector3(width+.7f,.18f,2.7f),roof);
            foreach(float side in new[]{-1f,1f})Box(house,"Porch Post",new Vector3(side*(width*.5f-.2f),(.48f+porchTop)*.5f,-5.45f),new Vector3(.14f,porchTop-.48f,.14f),m[2]);
            Box(house,"Porch Step",new Vector3(0f,.12f,-5.92f),new Vector3(1.8f,.24f,.55f),m[2],true);
            if(style!=4)
            {
                Box(house,"Closed Decorative Door",new Vector3(0f,1.65f,-3.56f),new Vector3(1.05f,2.35f,.07f),m[2]);
                Box(house,"Door Handle",new Vector3(.32f,1.65f,-3.63f),new Vector3(.05f,.22f,.05f),m[6]);
            }
            foreach(float side in new[]{-1f,1f})
            {Box(house,"House Window",new Vector3(side*width*.30f,1.9f,-3.57f),new Vector3(1.2f,1.4f,.045f),m[5]);foreach(float y in new[]{1.16f,2.64f})Box(house,"Window Trim",new Vector3(side*width*.30f,y,-3.62f),new Vector3(1.35f,.08f,.06f),m[8]);Box(house,abandoned?"Boarded Window":"Window Crossbar",new Vector3(side*width*.30f,1.9f,-3.65f),new Vector3(1.3f,abandoned?.22f:.06f,.06f),abandoned?m[2]:m[8]);}
            float chimneyBase=height+rise*.5f-.2f;
            Box(house,"House Chimney",new Vector3(width*.25f,chimneyBase+.85f,1.3f),new Vector3(.55f,1.7f,.55f),m[7]);
            Box(house,"Chimney Cap",new Vector3(width*.25f,chimneyBase+1.72f,1.3f),new Vector3(.7f,.15f,.7f),m[1]);
            foreach(float side in new[]{-1f,1f})Box(house,"House Corner Trim",new Vector3(side*(width*.5f-.05f),height*.5f,-3.57f),new Vector3(.12f,height,.08f),m[8]);
            if(style==2)
            {
                Box(house,"Second Floor Balcony",new Vector3(0f,3.6f,-4.65f),new Vector3(width+.4f,.22f,2f),m[2]);
                Box(house,"Balcony Handrail",new Vector3(0f,4.5f,-5.55f),new Vector3(width+.4f,.12f,.12f),m[8]);
                for(float x=-width*.5f;x<=width*.5f;x+=.45f)Box(house,"Balcony Spindle",new Vector3(x,4.05f,-5.55f),new Vector3(.055f,.85f,.055f),m[8]);
                foreach(float side in new[]{-1f,1f}){Box(house,"Balcony Side Rail",new Vector3(side*(width*.5f+.12f),4.5f,-4.65f),new Vector3(.12f,.12f,1.9f),m[8]);for(float z=-5.4f;z<-3.7f;z+=.4f)Box(house,"Balcony Side Spindle",new Vector3(side*(width*.5f+.12f),4.05f,z),new Vector3(.055f,.85f,.055f),m[8]);}
                foreach(float x in new[]{-width*.28f,0f,width*.28f})Box(house,"Upper Shop Window",new Vector3(x,5.5f,-3.59f),new Vector3(1.05f,1.5f,.07f),m[5]);
                RouteProps.Board(house,"GENERAL STORE",new Vector3(0f,3.02f,-5.6f),new Vector3(2.4f,.35f,.06f),m,Quaternion.identity);
            }
            if(style==3)
            {
                for(float y=.25f;y<height;y+=.24f)
                {Box(house,"Cabin Front Log",new Vector3(0f,y,-3.59f),new Vector3(width+.2f,.14f,.14f),m[2]);foreach(float side in new[]{-1f,1f})Box(house,"Cabin Corner Log End",new Vector3(side*(width*.5f-.12f),y,-3.72f),new Vector3(.26f,.2f,.2f),m[8]);}
                foreach(Transform part in house)
                    if(part.name=="House Window"||part.name=="Window Trim"||part.name=="Window Crossbar"||part.name=="Boarded Window"||part.name=="Closed Decorative Door"||part.name=="Door Handle")part.localPosition+=Vector3.back*.18f;
            }
            if(style==4)
            {
                foreach(float side in new[]{-1f,1f})
                {Box(house,"Workshop Double Door",new Vector3(side*.75f,1.87f,-3.66f),new Vector3(1.45f,2.8f,.13f),m[2]);var brace=Box(house,"Workshop Door Brace",new Vector3(side*.75f,1.87f,-3.75f),new Vector3(.1f,2.8f,.05f),m[8]);brace.transform.localRotation=Quaternion.Euler(0f,0f,side*23f);}
                Box(house,"Workshop Loft Window",new Vector3(0f,4.5f,-3.61f),new Vector3(1.4f,1.1f,.08f),m[5]);
            }
            if(style==5)
            {
                Box(house,"Stone Foundation",new Vector3(0f,.32f,0f),new Vector3(width+.15f,.64f,7.15f),m[1]);
                Box(house,"Projecting Bay Window",new Vector3(width*.3f,1.9f,-3.85f),new Vector3(1.7f,1.45f,.65f),m[5]);
                Box(house,"Bay Window Hood",new Vector3(width*.3f,2.7f,-3.85f),new Vector3(1.95f,.18f,.85f),roof);
                for(int i=0;i<5;i++)Box(house,"Stone Course",new Vector3(0f,.85f+i*.65f,-3.61f),new Vector3(width,.025f,.03f),m[1]);
            }
            WorldArtAssets.BatchBuilding(house);
        }
        private static void Shop(Transform p,Vector3 at,int index,Material[] m,TrainController train,GameObject coal,bool closed)
        {
            var shop=new GameObject("Vendor Shop").transform;shop.SetParent(p,false);shop.localPosition=at;
            Box(shop,"Shop Back",new Vector3(0f,1.7f,2f),new Vector3(3.5f,3.4f,.15f),m[2]);
            foreach(float x in new[]{-1.7f,1.7f})Box(shop,"Shop Post",new Vector3(x,1.6f,0f),new Vector3(.13f,3.2f,.13f),m[2]);
            Box(shop,"Canvas Shop Awning",new Vector3(0f,3.3f,.8f),new Vector3(3.9f,.18f,3f),index%2==0?m[7]:m[4]);
            var counter=Box(shop,"Shop Counter",new Vector3(0f,.62f,0f),new Vector3(3.3f,1.24f,1f),m[2],true);
            RouteProps.Board(shop,"SALVAGE BOUGHT HERE",new Vector3(0f,1.9f,1.7f),new Vector3(2.9f,.28f,.04f),m,Quaternion.identity);
            RouteProps.Board(shop,closed?"CLOSED":index%2==0?"RAIL & REPAIRS":"COAL & SUPPLIES",new Vector3(0f,2.5f,1.7f),new Vector3(3.2f,.5f,.08f),m,Quaternion.identity);
            for(int i=0;i<3;i++)Box(shop,index%2==0?"Steel Stock":"Coal Stock",new Vector3(-.9f+i*.8f,1.5f,0f),new Vector3(.55f,.3f,.65f),index%2==0?m[3]:m[5]);
            if(!closed)
                for(int i=0;i<4;i++)
                {
                    var product=(RailwayProduct)i;
                    string label=i==0?"STEEL 5m\n50 GOLD":i==1?"COAL\n20 GOLD":i==2?"SAW\n25 GOLD":"CONNECTOR\n15 GOLD";
                    var cap=WorldButtonArt.Create(shop,new Vector3(-1.2f+i*.8f,.86f,-.56f),label,m);
                    cap.AddComponent<RailwayVendor>().Configure(train,product,coal,m);
                }
        }
        private static void Lamp(Transform p,Vector3 at,Material[] m)
        {ModelDetails.Lantern(p,at,m,"Station",2.9f);}
    }
}
