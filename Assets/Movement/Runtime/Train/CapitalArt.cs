using UnityEngine;
namespace AllAboard.Train
{
    public static class CapitalArt
    {
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m,bool solid=false)=>RouteProps.Box(p,n,at,size,m,solid);
        public static void Dress(Transform station,Material[] m)
        {
            var assets=station.parent.GetComponent<WorldArtAssets>();
            var stone=assets.Paint(m[8],"Capital Limestone",new Color(.78f,.75f,.65f));
            var brick=assets.Paint(m[2],"Capital Brick",new Color(.52f,.28f,.21f));
            var slate=assets.Paint(m[5],"Capital Slate",new Color(.19f,.23f,.25f));
            var root=new GameObject("Victorian Capital Skyline").transform;root.SetParent(station,false);
            // Terraces flank the railway before the entry gate and continue into the city.
            foreach(float side in new[]{-1f,1f})for(int row=0;row<(side<0f?7:9);row++)
            {
                var building=new GameObject("Capital Terrace").transform;building.SetParent(root,false);building.localPosition=new Vector3(side*(side<0f?58f+(row%2)*17f:72f+(row%3)*12f),0f,(side<0f?-410f:-485f)+row*55f+(row*13)%17);building.localRotation=Quaternion.Euler(0f,side>0f?90f:-90f,0f);
                float height=10.5f+(row%3)*2.8f;
                Box(building,"Masonry Terrace",new Vector3(0f,height*.5f,0f),new Vector3(32f,height,16f),row%3==0?stone:brick,true);
                Box(building,"Limestone Foundation",new Vector3(0f,.4f,-8.13f),new Vector3(32.5f,.8f,.3f),stone);
                Box(building,"Roof Cornice",new Vector3(0f,height+.2f,-8.15f),new Vector3(33f,.45f,.5f),stone);
                foreach(float x in new[]{-12f,-4f,4f,12f})
                {
                    Box(building,"Capital Front Door",new Vector3(x,1.4f,-8.13f),new Vector3(1.3f,2.7f,.08f),m[5]);
                    Box(building,"Door Pediment",new Vector3(x,2.95f,-8.22f),new Vector3(1.9f,.18f,.35f),stone);
                    for(float y=4.5f;y<height-1f;y+=3.1f)
                    {
                        Box(building,"Sash Window Recess",new Vector3(x,y,-8.12f),new Vector3(1.8f,2f,.08f),m[5]);
                        Box(building,"Sash Glass",new Vector3(x,y,-8.18f),new Vector3(1.52f,1.76f,.03f),m[3]);
                        Box(building,"Window Sill",new Vector3(x,y-1f,-8.23f),new Vector3(2.1f,.16f,.3f),stone);
                        Box(building,"Window Header",new Vector3(x,y+1f,-8.23f),new Vector3(2.1f,.16f,.2f),stone);
                        Box(building,"Sash Divider",new Vector3(x,y,-8.23f),new Vector3(1.65f,.06f,.03f),stone);
                        Box(building,"Sash Mullion",new Vector3(x,y,-8.23f),new Vector3(.06f,1.9f,.03f),stone);
                    }
                    Box(building,"Chimney Stack",new Vector3(x,height+1.5f,3f),new Vector3(.75f,3f,.75f),brick);
                    Box(building,"Chimney Pots",new Vector3(x,height+3.15f,3f),new Vector3(.95f,.35f,.95f),stone);
                }
                foreach(float x in new[]{-16f,-8f,0f,8f,16f})Box(building,"Stone Pilaster",new Vector3(x,height*.5f,-8.21f),new Vector3(.25f,height,.22f),stone);
                foreach(float sideRoof in new[]{-1f,1f}){var roof=Box(building,"Slate Roof",new Vector3(0f,height+1.2f,sideRoof*4f),new Vector3(33f,.25f,9f),slate);roof.transform.localRotation=Quaternion.Euler(sideRoof*-18f,0f,0f);}
                var rear=Object.Instantiate(building.gameObject,root).transform;rear.name="Capital Rear Terrace";rear.localPosition=new Vector3(side*(145f+(row%3)*14f),0f,(side<0f?-440f:-515f)+row*57f);rear.localScale=new Vector3(1f,1.2f,1f);
                WorldArtAssets.BatchBuilding(building);WorldArtAssets.BatchBuilding(rear);
            }
            var paving=assets.Paint(m[3],"Capital Paving",new Color(.42f,.43f,.40f));
            foreach(float side in new[]{-1f,1f})
            {
                Box(root,"Capital Paved Street",new Vector3(side*23f,.025f,-200f),new Vector3(10f,.05f,450f),slate);
                Box(root,"Capital Pavement",new Vector3(side*33f,.10f,-200f),new Vector3(10f,.20f,450f),paving,true);
                Box(root,"Capital Pavement Curb",new Vector3(side*28f,.16f,-200f),new Vector3(.15f,.16f,450f),stone);
                Box(root,"Capital Promenade",new Vector3(side*52f,.025f,-215f),new Vector3(25f,.05f,540f),paving);
                Box(root,"Capital Outer Street",new Vector3(side*115f,.025f,-215f),new Vector3(12f,.05f,540f),slate);
                for(float z=-400f;z<25f;z+=38f)
                {
                    Box(root,"Capital Lamp Post",new Vector3(side*29f,2.4f,z),new Vector3(.12f,4.8f,.12f),m[5]);
                    Box(root,"Capital Lantern",new Vector3(side*29f,4.9f,z),new Vector3(.52f,.65f,.52f),m[5]);
                    Box(root,"Capital Lantern Glass",new Vector3(side*29f,4.9f,z),new Vector3(.43f,.47f,.43f),m[8]);
                    Box(root,"Capital Lamp Cap",new Vector3(side*29f,5.3f,z),new Vector3(.64f,.12f,.64f),m[5]);
                }
            }
            Box(root,"Capital Station Square",new Vector3(80f,.025f,45f),new Vector3(100f,.05f,95f),paving);
            Box(root,"Square Fountain Basin",new Vector3(105f,.35f,45f),new Vector3(4f,.7f,4f),stone,true);
            Box(root,"Square Fountain Centre",new Vector3(105f,1.3f,45f),new Vector3(.45f,2f,.45f),m[6]);
            foreach(float z in new[]{20f,67f})Box(root,"City Cross Street",new Vector3(45f,.02f,z),new Vector3(140f,.04f,8f),slate);
            var hall=new GameObject("Capital Station Hall").transform;hall.SetParent(root,false);hall.localPosition=new Vector3(25f,0f,-4f);
            Box(hall,"Grand Station Wing",new Vector3(18f,6f,0f),new Vector3(22f,12f,68f),brick,true);
            foreach(float z in new[]{-31f,-22f,-13f,13f,22f,31f})
            {
                Box(hall,"Station Stone Pier",new Vector3(6.7f,6.2f,z),new Vector3(.7f,12.4f,.6f),stone);
                Box(hall,"Grand Hall Window",new Vector3(6.8f,7f,z+3f),new Vector3(.12f,6f,4.5f),m[3]);
                Box(hall,"Hall Window Mullion",new Vector3(6.68f,7f,z+3f),new Vector3(.15f,6.2f,.16f),stone);
            }
            Box(hall,"Station Roofline",new Vector3(17.7f,12.3f,0f),new Vector3(23f,.5f,69f),stone);
            Box(hall,"Station Clock Tower",new Vector3(18f,13f,0f),new Vector3(9f,26f,9f),stone,true);
            Box(hall,"Tower Face",new Vector3(13.42f,22f,0f),new Vector3(.1f,3.4f,3.4f),m[8]);
            Box(hall,"Clock Hour Hand",new Vector3(13.33f,22f,.47f),new Vector3(.03f,.14f,1.05f),m[5]);
            Box(hall,"Clock Minute Hand",new Vector3(13.33f,22.57f,0f),new Vector3(.03f,1.3f,.12f),m[5]);
            Box(hall,"Tower Cornice",new Vector3(18f,26f,0f),new Vector3(10f,.65f,10f),stone);
            var spire=GameObject.CreatePrimitive(PrimitiveType.Cylinder);spire.name="Tower Crown";spire.transform.SetParent(hall,false);spire.transform.localPosition=new Vector3(18f,28f,0f);spire.transform.localScale=new Vector3(6.5f,2f,6.5f);spire.GetComponent<Renderer>().sharedMaterial=slate;Object.DestroyImmediate(spire.GetComponent<Collider>());
            for(float z=-40f;z<35f;z+=12f)
            {
                Box(station,"Capital Canopy Column",new Vector3(6.7f,3.7f,z),new Vector3(.16f,5.1f,.16f),m[5]);
                Box(station,"Grand Canopy Iron Beam",new Vector3(5f,6.2f,z),new Vector3(6f,.20f,.20f),m[5]);
            }
            Box(station,"Capital Canopy Roof",new Vector3(5f,6.35f,-4f),new Vector3(6.5f,.16f,80f),slate);
            RouteProps.Board(station,"TERMINUM\nGRAND CENTRAL STATION",new Vector3(8.8f,5.2f,-2f),new Vector3(5.5f,1.2f,.12f),m,Quaternion.Euler(0f,-90f,0f));
        }
    }
}
