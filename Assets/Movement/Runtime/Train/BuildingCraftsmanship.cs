using UnityEngine;

namespace AllAboard.Train
{
    public static class BuildingCraftsmanship
    {
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m)=>CraftGeometry.Box(p,n,at,size,m);
        public static void House(Transform house,float width,float height,float pitch,int style,bool abandoned,Material[] m)
        {
            FrontierHouseModels.Build(house,width,height,pitch,style,abandoned,m);
        }
        public static void Roof(Transform root,Vector3 at,float width,float length,float baseY,float pitch,Material material)
        {
            float rise=BuildingRoof.Rise(width,pitch);
            for(float z=-length*.5f+.4f;z<length*.5f;z+=1.2f)foreach(float side in new[]{-1f,1f})
                CraftGeometry.Rod(root,"Roof Standing Seam",at+new Vector3(side*(width*.5f+.14f),baseY+.01f,z),at+new Vector3(0,baseY+rise+.035f,z),.025f,material);
            foreach(float side in new[]{-1f,1f})
            {
                CraftGeometry.Rod(root,"Eave Gutter",at+new Vector3(side*(width*.5f+.25f),baseY-.05f,-length*.5f),at+new Vector3(side*(width*.5f+.25f),baseY-.05f,length*.5f),.075f,material);
                CraftGeometry.Rod(root,"Downpipe",at+new Vector3(side*(width*.5f+.09f),baseY-.05f,length*.5f-.18f),at+new Vector3(side*(width*.5f+.09f),.22f,length*.5f-.18f),.065f,material);
            }
        }
        public static void Station(Transform station,Material[] m,bool capital=false)
        {
            var root=CraftGeometry.Group(station,"Detailed Station Craftsmanship");
            for(float z=-27;z<27;z+=1.2f)Box(root,"Platform Coping Joint",new Vector3(3.1f,1.151f,z),new Vector3(1.5f,.008f,.015f),m[1]);
            foreach(float z in capital?new[]{-40f,-28f,-16f,-4f,8f,20f,32f}:new[]{-25f,-14f,-3f,8f,19f})
            {
                float x=capital?6.7f:6.9f;
                foreach(float y in capital?new[]{1.4f,6.1f}:new[]{1.52f,3.7f})Box(root,"Canopy Column Capital",new Vector3(x,y,z),new Vector3(.32f,.16f,.32f),m[6]);
                CraftGeometry.Rod(root,"Canopy Iron Diagonal",new Vector3(3.1f,capital?6.24f:4.04f,z),new Vector3(x,capital?5.7f:3.55f,z),.055f,m[5]);
            }
            for(float z=-8.4f;z<5;z+=.38f)Box(root,"Station Side Cladding",new Vector3(19.11f,2.6f,z),new Vector3(.028f,2.95f,.03f),m[2]);
            Roof(root,new Vector3(14,0,-2),10.2f,14.2f,4.35f,17,m[3]);
            foreach(float z in new[]{-21f,18f})
            {
                for(int i=0;i<6;i++)Box(root,"Platform Bench Slat",new Vector3(5.55f+i*.18f,1.713f,z),new Vector3(.14f,.025f,2.9f),m[2]);
                foreach(float end in new[]{-1f,1f})CraftGeometry.Rod(root,"Bench Armrest",new Vector3(5.6f,1.9f,z+end*1.3f),new Vector3(6.35f,1.9f,z+end*1.3f),.065f,m[3]);
            }
            // A compact supported luggage trolley beside the entrance, outside the aisle.
            Box(root,"Luggage Trolley Deck",new Vector3(7.8f,1.52f,8),new Vector3(1.25f,.1f,1.8f),m[2]);
            foreach(float x in new[]{7.25f,8.35f})foreach(float z in new[]{7.35f,8.65f})
                CraftGeometry.Round(root,"Trolley Wheel",new Vector3(x,1.35f,z),new Vector3(.3f,.055f,.3f),m[5],Quaternion.Euler(0,0,90));
            foreach(float x in new[]{7.3f,8.3f})CraftGeometry.Rod(root,"Trolley Handle",new Vector3(x,1.52f,8.85f),new Vector3(x,2.4f,8.85f),.05f,m[3]);
            CraftGeometry.Rod(root,"Trolley Handle Crossbar",new Vector3(7.3f,2.4f,8.85f),new Vector3(8.3f,2.4f,8.85f),.05f,m[3]);
            Box(root,"Trolley Suitcase",new Vector3(7.8f,1.83f,8),new Vector3(.8f,.55f,1.05f),m[7]);
            foreach(float z in new[]{7.68f,8.32f})Box(root,"Luggage Leather Strap",new Vector3(7.8f,2.12f,z),new Vector3(.83f,.035f,.07f),m[6]);
            CraftGeometry.Bake(root);
        }
        public static void Terrace(Transform building,float height,Material stone,Material brick,Material[] m)
        {
            var root=CraftGeometry.Group(building,"Detailed Victorian Masonry");
            for(float y=1f;y<height;y+=.55f)Box(root,"Brick Course Shadow",new Vector3(0,y,-8.025f),new Vector3(31.6f,.018f,.02f),m[5]);
            foreach(float x in new[]{-12f,-4f,4f,12f})
            {
                CraftGeometry.Frame(root,new Vector3(x,1.45f,-8.22f),new Vector2(1.45f,2.85f),.09f,stone);
                Box(root,"Door Transom Glass",new Vector3(x,3.2f,-8.17f),new Vector3(1.3f,.36f,.04f),m[3]);
                Box(root,"Door Brass Kick Plate",new Vector3(x,.3f,-8.23f),new Vector3(1.1f,.25f,.025f),m[6]);
                for(float y=4.5f;y<height-1f;y+=3.1f)
                {
                    foreach(float side in new[]{-1f,1f})Box(root,"Sash Frame Stile",new Vector3(x+side*.9f,y,-8.26f),new Vector3(.08f,2.1f,.075f),stone);
                    Box(root,"Victorian Keystone",new Vector3(x,y+1.16f,-8.28f),new Vector3(.26f,.25f,.17f),stone);
                    foreach(float side in new[]{-1f,1f})Box(root,"Window Corbel",new Vector3(x+side*.7f,y-1.15f,-8.27f),new Vector3(.14f,.26f,.22f),stone);
                }
                foreach(float z in new[]{2.76f,3.24f})CraftGeometry.Round(root,"Terracotta Chimney Pot",new Vector3(x,height+3.5f,z),new Vector3(.3f,.23f,.3f),brick,Quaternion.identity);
            }
            for(float x=-15.5f;x<16;x+=1f)Box(root,"Cornice Dentil",new Vector3(x,height-.08f,-8.31f),new Vector3(.25f,.25f,.3f),stone);
            CraftGeometry.Bake(root);
        }
        public static void Stall(Transform shop,Material[] m)
        {
            var root=CraftGeometry.Group(shop,"Detailed Merchant Stall");
            for(float x=-1.55f;x<1.6f;x+=.25f)Box(root,"Stall Counter Panel Joint",new Vector3(x,.6f,-.511f),new Vector3(.012f,1.1f,.025f),m[5]);
            foreach(float x in new[]{-1.55f,1.55f})foreach(float y in new[]{.15f,1.1f})CraftGeometry.Bolt(root,new Vector3(x,y,-.53f),.045f,m[6],Vector3.back);
            for(float x=-1.8f;x<1.9f;x+=.45f)Box(root,"Canvas Valance Tab",new Vector3(x,3.14f,-.7f),new Vector3(.36f,.22f,.04f),m[8]);
            foreach(float x in new[]{-1.7f,1.7f})CraftGeometry.Rod(root,"Awning Diagonal Stay",new Vector3(x,2.6f,0),new Vector3(x,3.24f,-.65f),.045f,m[3]);
            CraftGeometry.Bake(root);
        }
    }
}
