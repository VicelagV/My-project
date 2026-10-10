using UnityEngine;
namespace AllAboard.Train
{
    public static class FrontierHouseModels
    {
        public static void Build(Transform house,float width,float height,float pitch,int style,bool abandoned,Material[] m)
        {
            var body=house.Find("House Body");if(body==null)return;var wall=body.GetComponent<Renderer>().sharedMaterial;
            foreach(var draw in house.GetComponentsInChildren<MeshRenderer>())draw.enabled=false;
            var root=CraftGeometry.Group(house,"Rebuilt Frontier House");
            Box(root,"Closed Bevelled House Shell",new Vector3(0,height*.5f,0),new Vector3(width,height,7),wall);
            Box(root,"Deep Timber Foundation",new Vector3(0,.25f,0),new Vector3(width+.2f,.5f,7.2f),m[2]);
            float roofPitch=style==0?14:style==1?32:style==3?22:style==4?18:26;
            BuildingRoof.Build(root,"New Heavy Gabled Roof",Vector3.zero,width,7,height,roofPitch,m[5],wall,.5f);
            float rise=BuildingRoof.Rise(width,roofPitch);
            Box(root,"Broad Roof Ridge",new Vector3(0,height+rise+.01f,0),new Vector3(.28f,.16f,8.05f),m[5]);
            foreach(float side in new[]{-1f,1f})
            {
                float edge=width*.5f;
                Box(root,"Heavy Roof Fascia",new Vector3(side*(edge+.43f),height-.11f,0),new Vector3(.22f,.27f,8),m[2]);
                Box(root,"Broad Corner Post",new Vector3(side*(edge-.07f),height*.5f,-3.55f),new Vector3(.22f,height,.22f),m[8]);
                Window(root,new Vector3(side*width*.30f,1.95f,-3.56f),Quaternion.identity,m,style==5?1.2f:1.4f);
                Window(root,new Vector3(side*(edge+.02f),1.95f,.8f),Quaternion.Euler(0,side>0?-90:90,0),m,1.4f);
            }
            if(style==3)
            {
                for(float y=.55f;y<height;y+=.43f)foreach(float side in new[]{-1f,1f})
                {
                    FrontierModelMesh.Part(root,"Rounded Cabin Log",new Vector3(side*(width*.5f),y,0),new Vector3(.38f,7.25f,.38f),m[2],FrontierModelMesh.Cylinder(8),Quaternion.Euler(90,0,0));
                    FrontierModelMesh.Part(root,"Visible Log End",new Vector3(side*(width*.5f),y,-3.66f),new Vector3(.31f,.035f,.31f),m[8],FrontierModelMesh.Cylinder(8),Quaternion.Euler(90,0,0));
                }
            }
            else
            {
                for(float y=.9f;y<height-.1f;y+=.85f)Box(root,"Broad Weatherboard",new Vector3(0,y,-3.535f),new Vector3(width,.05f,.04f),m[2]);
            }
            Box(root,"Inset Front Door",new Vector3(0,1.6f,-3.64f),new Vector3(1.18f,2.45f,.14f),m[5]);
            foreach(float side in new[]{-1f,1f})Box(root,"Chunky Door Jamb",new Vector3(side*.65f,1.6f,-3.74f),new Vector3(.17f,2.6f,.16f),m[8]);
            Box(root,"Chunky Door Lintel",new Vector3(0,2.96f,-3.74f),new Vector3(1.47f,.19f,.19f),m[8]);
            Box(root,"Door Handle",new Vector3(.36f,1.55f,-3.78f),new Vector3(.08f,.22f,.09f),m[6]);
            Box(root,"Broad Porch Deck",new Vector3(0,.43f,-4.67f),new Vector3(width+.3f,.16f,2.2f),m[2]);
            Box(root,"Two Step Entry",new Vector3(.3f,.16f,-6),new Vector3(2.05f,.32f,.7f),m[2]);
            float porchTop=style==2?3.55f:3.12f;
            foreach(float side in new[]{-1f,1f})
            {
                Box(root,"Thick Porch Post",new Vector3(side*(width*.5f-.25f),1.8f,-5.58f),new Vector3(.25f,2.7f,.25f),m[8]);
                Box(root,"Porch Post Foot",new Vector3(side*(width*.5f-.25f),.67f,-5.58f),new Vector3(.37f,.43f,.37f),m[2]);
            }
            Box(root,"Heavy Porch Canopy",new Vector3(0,porchTop,-4.8f),new Vector3(width+.6f,.25f,2.65f),m[5],Quaternion.Euler(-5,0,0));
            if(style==0)
            {
                Box(root,"Stepped False Front",new Vector3(0,height+.48f,-3.61f),new Vector3(width+.22f,.98f,.24f),wall);
                Box(root,"Raised Centre Pediment",new Vector3(0,height+1.13f,-3.62f),new Vector3(width*.58f,.44f,.25f),wall);
                Box(root,"Wide Store Cornice",new Vector3(0,height+.99f,-3.8f),new Vector3(width+.7f,.2f,.42f),m[8]);
            }
            if(style==1)
            {
                var dormer=CraftGeometry.Group(root,"Cottage Dormer");dormer.localPosition=new Vector3(.45f,height+.55f,-1.8f);
                Box(dormer,"Dormer Body",new Vector3(0,.35f,0),new Vector3(1.7f,.7f,1.7f),wall);BuildingRoof.Build(dormer,"Dormer Cap",Vector3.zero,1.7f,1.7f,.7f,32,m[5],wall,.15f);Window(dormer,new Vector3(0,.34f,-.91f),Quaternion.identity,m,.64f);
            }
            if(style==2)
            {
                Box(root,"Balcony Floor",new Vector3(0,3.61f,-4.73f),new Vector3(width+.35f,.25f,2.2f),m[2]);
                Box(root,"Balcony Handrail",new Vector3(0,4.52f,-5.75f),new Vector3(width+.3f,.17f,.17f),m[8]);
                for(float x=-width*.45f;x<width*.5f;x+=.75f)Box(root,"Broad Balcony Baluster",new Vector3(x,4.09f,-5.75f),new Vector3(.16f,.85f,.16f),m[8]);
                Window(root,new Vector3(-width*.25f,5.3f,-3.57f),Quaternion.identity,m,1.5f);Window(root,new Vector3(width*.25f,5.3f,-3.57f),Quaternion.identity,m,1.5f);
            }
            if(style==4)
            {
                Box(root,"Workshop Sliding Door",new Vector3(width*.19f,1.8f,-3.77f),new Vector3(width*.43f,2.75f,.17f),m[2]);
                Box(root,"Workshop Track",new Vector3(width*.19f,3.22f,-3.94f),new Vector3(width*.48f,.16f,.13f),m[3]);
                Box(root,"Offset Workshop Vent",new Vector3(-width*.25f,height-.8f,-3.6f),new Vector3(.75f,.55f,.14f),m[5]);
            }
            if(style==5)
            {
                Box(root,"Projecting Stone Bay",new Vector3(-width*.22f,1.3f,-3.92f),new Vector3(2.1f,1.5f,.75f),wall);
                Window(root,new Vector3(-width*.22f,2.23f,-4.34f),Quaternion.identity,m,1.65f);
                for(float y=.65f;y<height;y+=.75f)foreach(float side in new[]{-1f,1f})Box(root,"Large Corner Quoin",new Vector3(side*(width*.5f-.1f),y,-3.62f),new Vector3(.38f,.32f,.22f),m[8]);
            }
            Box(root,"Broad Brick Chimney",new Vector3(width*.28f,height+rise*.5f+.45f,1.2f),new Vector3(.72f,1.6f,.72f),m[7]);
            Box(root,"Chimney Cap",new Vector3(width*.28f,height+rise*.5f+1.28f,1.2f),new Vector3(.9f,.18f,.9f),m[3]);
            if(abandoned)foreach(var frame in root.GetComponentsInChildren<Transform>())if(frame.name=="Broad Sash Window")
            {
                Box(frame,"Boarded Abandoned Window",new Vector3(0,0,-.29f),new Vector3(1.5f,.18f,.1f),m[2],Quaternion.Euler(0,0,22));
                Box(frame,"Crossed Abandoned Window",new Vector3(0,.2f,-.4f),new Vector3(1.5f,.18f,.1f),m[2],Quaternion.Euler(0,0,-18));
            }
            CraftGeometry.Bake(root);
        }
        private static void Window(Transform root,Vector3 at,Quaternion facing,Material[] m,float width)
        {
            var frame=CraftGeometry.Group(root,"Broad Sash Window");frame.localPosition=at;frame.localRotation=facing;float height=width*1.15f;
            Box(frame,"Deep Blue Window",new Vector3(0,0,-.07f),new Vector3(width,height,.1f),m[5]);
            foreach(float side in new[]{-1f,1f})
            {
                Box(frame,"Heavy Sash Stile",new Vector3(side*(width*.5f+.03f),0,-.15f),new Vector3(.14f,height+.23f,.18f),m[8]);
                Box(frame,"Heavy Sash Rail",new Vector3(0,side*(height*.5f+.035f),-.15f),new Vector3(width+.28f,.14f,.18f),m[8]);
            }
            Box(frame,"Single Sash Divider",new Vector3(0,0,-.16f),new Vector3(width,.075f,.075f),m[8]);
            Box(frame,"Deep Window Ledge",new Vector3(0,-height*.5f-.14f,-.22f),new Vector3(width+.43f,.16f,.4f),m[2]);
        }
        private static void Box(Transform p,string name,Vector3 at,Vector3 size,Material paint,Quaternion? rotation=null)=>FrontierModelMesh.Box(p,name,at,size,paint,rotation);
    }
}
