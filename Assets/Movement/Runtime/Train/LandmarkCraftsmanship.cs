using UnityEngine;
namespace AllAboard.Train
{
    public static class LandmarkCraftsmanship
    {
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m)=>CraftGeometry.Box(p,n,at,size,m);
        public static void Bridge(Transform bridge,Material[] m)
        {
            var root=CraftGeometry.Group(bridge,"Detailed Bridge Ironwork");
            foreach(float side in new[]{-1f,1f})for(float z=-27;z<30;z+=6)
            {
                Box(root,"Girder Splice Plate",new Vector3(side*3.04f,-.73f,z),new Vector3(.045f,1.06f,.38f),m[3]);
                foreach(float y in new[]{-1.13f,-.72f,-.31f})foreach(float dz in new[]{-.12f,.12f})
                    CraftGeometry.Bolt(root,new Vector3(side*3.076f,y,z+dz),.065f,m[6],side*Vector3.right);
                CraftGeometry.Rod(root,"Bridge Diagonal Tie",new Vector3(side*3.25f,.15f,z),new Vector3(side*3.25f,.65f,z+3f),.06f,m[3]);
                CraftGeometry.Rod(root,"Bridge Diagonal Tie",new Vector3(side*3.25f,.65f,z+3),new Vector3(side*3.25f,.15f,z+6f),.06f,m[3]);
            }
            for(float z=-29;z<30;z+=.65f)foreach(float side in new[]{-1f,1f})
                Box(root,"Bridge Deck Plank End",new Vector3(side*2.91f,.006f,z),new Vector3(.47f,.012f,.02f),m[5]);
            CraftGeometry.Bake(root);
        }
        public static void Tunnel(Transform tunnel,Material[] m)
        {
            var root=CraftGeometry.Group(tunnel,"Detailed Tunnel Masonry");
            foreach(float side in new[]{-1f,1f})
            {
                for(float z=-58;z<60;z+=12)
                {
                    Box(root,"Tunnel Rib Pier",new Vector3(side*3.82f,3f,z),new Vector3(.09f,6f,.18f),m[8]);
                    Box(root,"Tunnel Rib Cap",new Vector3(side*3.72f,5.97f,z),new Vector3(.3f,.15f,.38f),m[8]);
                    Box(root,"Tunnel Lamp Bracket",new Vector3(side*3.64f,3.8f,z),new Vector3(.36f,.065f,.065f),m[3]);
                    Box(root,"Tunnel Lamp Case",new Vector3(side*3.45f,3.84f,z),new Vector3(.2f,.28f,.22f),m[5]);
                    Box(root,"Tunnel Lamp Lens",new Vector3(side*3.34f,3.84f,z),new Vector3(.035f,.19f,.15f),m[8]);
                }
                Box(root,"Tunnel Drainage Channel",new Vector3(side*3.55f,.045f,0),new Vector3(.16f,.035f,119f),m[5]);
                for(float y=.7f;y<5.8f;y+=.65f)Box(root,"Tunnel Masonry Course",new Vector3(side*3.837f,y,0),new Vector3(.024f,.026f,119f),m[2]);
            }
            CraftGeometry.Bake(root);
        }
        public static void Shed(Transform shed,float width,float height,Material[] m)
        {
            var root=CraftGeometry.Group(shed,"Detailed Shed Joinery");
            foreach(float side in new[]{-1f,1f})
            {
                for(float z=-2.8f;z<3;z+=.36f)Box(root,"Shed Board Lap",new Vector3(side*(width*.5f+.102f),height*.5f,z),new Vector3(.025f,height,.025f),m[5]);
                foreach(float y in new[]{.5f,height-.45f})foreach(float z in new[]{-2.6f,2.6f})CraftGeometry.Bolt(root,new Vector3(side*(width*.5f+.12f),y,z),.04f,m[3],side*Vector3.right);
            }
            BuildingCraftsmanship.Roof(root,Vector3.zero,width+.18f,6.18f,height,18,m[3]);
            Box(root,"Shed Entrance Threshold",new Vector3(0,.175f,-3),new Vector3(2.5f,.025f,.25f),m[3]);
            CraftGeometry.Bake(root);
        }
        public static void WorkSite(Transform site,LandmarkKind kind,Material[] m)
        {
            var root=CraftGeometry.Group(site,"Detailed Landmark Equipment");
            if(kind==LandmarkKind.Mine)
            {
                foreach(float x in new[]{-3f,3f})foreach(float z in new[]{-1.8f,1.8f})
                {Box(root,"Headframe Footing",new Vector3(x,.13f,z),new Vector3(.85f,.26f,.85f),m[1]);foreach(float y in new[]{.4f,5.5f,7.7f})CraftGeometry.Bolt(root,new Vector3(x,y,z-.3f),.12f,m[3],Vector3.back);}
                // Short mine rails and a detailed stationary ore tub.
                foreach(float x in new[]{-.55f,.55f})Box(root,"Mine Cart Rail",new Vector3(x,.045f,1.3f),new Vector3(.075f,.07f,4f),m[3]);
                Box(root,"Ore Tub Body",new Vector3(0,.65f,1.3f),new Vector3(1.15f,.75f,1.2f),m[3]);
                Box(root,"Ore Tub Load",new Vector3(0,1.05f,1.3f),new Vector3(1f,.15f,1.05f),m[5]);
                foreach(float x in new[]{-.62f,.62f})foreach(float z in new[]{.94f,1.66f})CraftGeometry.Round(root,"Ore Tub Wheel",new Vector3(x,.25f,z),new Vector3(.37f,.07f,.37f),m[5],Quaternion.Euler(0,0,90));
            }
            if(kind==LandmarkKind.LoggingCamp)
            {
                foreach(float z in new[]{.1f,1.45f})foreach(float y in new[]{.4f,1f})
                {CraftGeometry.Round(root,"Timber End Grain",new Vector3(6.48f,y,z),new Vector3(.44f,.012f,.44f),m[8],Quaternion.Euler(0,0,90));}
                CraftGeometry.Rod(root,"Crane Backstay",new Vector3(16,5.9f,5),new Vector3(18,.25f,5),.05f,m[5]);
                foreach(float y in new[]{1f,3f,5f})Box(root,"Crane Mast Collar",new Vector3(16,y,5),new Vector3(.39f,.13f,.39f),m[3]);
            }
            if(kind==LandmarkKind.Farm)
            {
                foreach(float x in new[]{13f,19f,25f})
                {var bale=Box(root,"Bound Hay Bale",new Vector3(x,.6f,13f),new Vector3(1.8f,1.2f,1f),m[8]);bale.transform.localRotation=Quaternion.Euler(0,x*7,0);}
                foreach(float x in new[]{13f,19f,25f})foreach(float side in new[]{-1f,1f})Box(root,"Hay Binding",new Vector3(x+side*.5f,1.23f,13f),new Vector3(.045f,.04f,1.1f),m[2]);
            }
            if(kind==LandmarkKind.Ruins)
            {
                for(int i=0;i<12;i++){var rubble=Box(root,"Fallen Masonry",new Vector3(-4.5f+i*.74f,.11f,3.1f-(i%3)*.45f),new Vector3(.52f,.22f,.37f),m[i%2==0?1:2]);rubble.transform.localRotation=Quaternion.Euler(0,i*37,0);}
            }
            if(kind==LandmarkKind.Checkpoint||kind==LandmarkKind.HostileCamp)
            {
                foreach(float x in new[]{-4f,4f})
                {Box(root,"Field Supply Chest",new Vector3(x,.35f,5),new Vector3(1.1f,.7f,.75f),m[2]);foreach(float dx in new[]{-.35f,.35f})Box(root,"Chest Iron Binding",new Vector3(x+dx,.72f,5),new Vector3(.045f,.035f,.8f),m[3]);}
            }
            CraftGeometry.Bake(root);
        }
        public static void Outfitter(Transform shop,Material[] m)
        {
            var root=CraftGeometry.Group(shop,"Detailed Outfitter Joinery");
            foreach(float x in new[]{-2f,2f})
            {
                Box(root,"Outfitter Canopy Post",new Vector3(x,1.88f,1.8f),new Vector3(.12f,2.96f,.12f),m[2]);
                foreach(float y in new[]{.5f,3.22f})Box(root,"Post Brass Collar",new Vector3(x,y,1.8f),new Vector3(.19f,.15f,.19f),m[6]);
                CraftGeometry.Rod(root,"Canopy Brace",new Vector3(x,2.75f,1.8f),new Vector3(x*.75f,3.29f,1.8f),.06f,m[6]);
            }
            Box(root,"Outfitter Canvas Roof",new Vector3(0,3.4f,1.2f),new Vector3(4.5f,.15f,2.2f),m[7]);
            for(float x=-2.1f;x<2.2f;x+=.4f)Box(root,"Outfitter Canvas Valance",new Vector3(x,3.24f,.08f),new Vector3(.3f,.21f,.035f),m[8]);
            for(float x=-1.55f;x<1.6f;x+=.22f)Box(root,"Outfitter Panel Joint",new Vector3(x,1.6f,1.132f),new Vector3(.01f,2.2f,.018f),m[5]);
            CraftGeometry.Bake(root);
        }
    }
}
