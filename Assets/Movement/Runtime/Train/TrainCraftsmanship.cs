using UnityEngine;

namespace AllAboard.Train
{
    [ExecuteAlways]
    public sealed class TrainCraftsmanship : MonoBehaviour
    {
        [SerializeField] private Material body, brass, steel, wood, leather, ivory, red;
        private const string Owned = "Detailed Train Craftsmanship";
        public void Configure(Material[] paints)
        { body=paints[0]; brass=paints[1]; steel=paints[2]; wood=paints[3]; leather=paints[4]; ivory=paints[5]; red=paints[6]; Rebuild(); }
        private void OnEnable() { if (body != null) Rebuild(); }
        public void Rebuild()
        {
            var old=transform.Find(Owned); if(old!=null){old.gameObject.SetActive(false);CraftGeometry.Dispose(old.gameObject);}
            var car=GetComponent<TrainCar>(); if(car==null||brass==null)return;
            var root=CraftGeometry.Group(transform,Owned);
            if(!Application.isPlaying)root.gameObject.hideFlags=HideFlags.DontSaveInEditor;
            var kind=car.Kind;
            foreach(float side in new[]{-1f,1f})
            {
                // Panel joints, cover plates and rivets sit outside the existing closed shell.
                for(float z=-4.1f;z<4.4f;z+=.55f)
                {
                    Bolt(root,new Vector3(side*1.992f,1.03f,z),.045f,side*Vector3.right);
                    Bolt(root,new Vector3(side*1.992f,kind==CarKind.Coal?1.56f:2.08f,z),.04f,side*Vector3.right);
                }
                foreach(float z in new[]{-2.8f,2.8f})
                {
                    Box(root,"Bearing Cover",new Vector3(side*1.978f,.48f,z),new Vector3(.06f,.22f,.29f),steel);
                    foreach(float dz in new[]{-.1f,.1f})Bolt(root,new Vector3(side*2.016f,.48f,z+dz),.035f,side*Vector3.right);
                    Box(root,"Brake Shoe",new Vector3(side*1.82f,.39f,z+.39f),new Vector3(.15f,.22f,.1f),steel);
                    Rod(root,"Brake Rigging",new Vector3(side*1.67f,.47f,z+.4f),new Vector3(side*1.67f,.68f,z-.4f),.045f,steel);
                }
                if(kind!=CarKind.Coal)
                {
                    foreach(float z in side<0?new[]{-3.45f,-1.72f,0f,1.72f,3.45f}:new[]{-3.45f,-1.72f,3.45f})
                    {
                        Box(root,"Window Drip Hood",new Vector3(side*2.025f,kind==CarKind.Locomotive?3.73f:3.37f,z),new Vector3(.18f,.035f,1.62f),brass);
                        Box(root,"Coach Recessed Panel",new Vector3(side*1.985f,1.65f,z),new Vector3(.015f,.55f,1.2f),wood);
                        foreach(float dz in new[]{-.62f,.62f})Box(root,"Panel Vertical Moulding",new Vector3(side*2.003f,1.65f,z+dz),new Vector3(.025f,.62f,.035f),brass);
                        foreach(float y in new[]{1.34f,1.96f})Box(root,"Panel Horizontal Moulding",new Vector3(side*2.003f,y,z),new Vector3(.025f,.035f,1.26f),brass);
                    }
                    for(float z=-4.15f;z<4.4f;z+=.75f)
                        Box(root,"Roof Sheet Lap",new Vector3(side*1.96f,3.90f,z),new Vector3(.018f,.035f,.055f),brass);
                }
                else
                {
                    foreach(float z in new[]{-3.75f,-1.9f,0f,1.9f,3.75f})
                    {Box(root,"Tender Reinforcing Shoe",new Vector3(side*1.99f,1.21f,z),new Vector3(.045f,.15f,.22f),steel);Bolt(root,new Vector3(side*2.02f,1.23f,z),.055f,side*Vector3.right);}
                }
            }
            foreach(float end in new[]{-4.45f,4.45f})
            {
                foreach(float x in new[]{-.3f,.3f})
                {
                    Box(root,"Draft Gear Retainer",new Vector3(x,.59f,end),new Vector3(.08f,.12f,.65f),steel);
                    Bolt(root,new Vector3(x,.67f,end),.065f,Vector3.up);
                }
                Box(root,"Buffer Beam Wear Strip",new Vector3(0,.71f,end),new Vector3(3.4f,.045f,.055f),steel);
            }
            if(kind==CarKind.Passenger)Coach(root);
            if(kind==CarKind.Locomotive)Locomotive(root);
            CraftGeometry.Bake(root);
            // Decorations on the door follow the existing hinge instead of being baked onto the car.
            var door=transform.Find("Sliding Door/Door Visual Details");
            if(door!=null)
            {
                var existing=door.Find("Detailed Door Joinery");if(existing!=null)CraftGeometry.Dispose(existing.gameObject);
                var joinery=CraftGeometry.Group(door,"Detailed Door Joinery");
                if(!Application.isPlaying)joinery.gameObject.hideFlags=HideFlags.DontSaveInEditor;
                foreach(float side in new[]{-1f,1f})
                {
                    foreach(float z in new[]{-.77f,0f,.77f})Box(joinery,"Door Raised Stile",new Vector3(side*.10f,-.47f,z),new Vector3(.025f,1.1f,.04f),brass);
                    foreach(float y in new[]{-.96f,-.03f})Box(joinery,"Door Raised Rail",new Vector3(side*.10f,y,0),new Vector3(.025f,.04f,1.65f),brass);
                }
                CraftGeometry.Bake(joinery);
            }
        }
        private void Coach(Transform root)
        {
            foreach(float z in new[]{-3.05f,-1.7f,-.35f,3.05f})foreach(float side in new[]{-1f,1f})
            {
                float x=side*1.16f;
                foreach(float seat in new[]{-.31f,.31f})
                {
                    Box(root,"Seat Cushion Piping",new Vector3(x+seat,1.68f,z+.37f),new Vector3(.55f,.025f,.025f),ivory);
                    foreach(float y in new[]{1.94f,2.18f,2.37f})
                        CraftGeometry.Bolt(root,new Vector3(x+seat,y,z-.245f),.04f,brass,Vector3.forward);
                }
                Box(root,"Seat Back Carved Top",new Vector3(x,2.51f,z-.33f),new Vector3(1.28f,.055f,.2f),wood);
                Box(root,"Seat Kick Plate",new Vector3(x,1.31f,z-.34f),new Vector3(1.15f,.18f,.025f),brass);
            }
            foreach(float side in new[]{-1f,1f})
            {
                for(float z=-4f;z<4.2f;z+=.32f)Box(root,"Luggage Rack Slat",new Vector3(side*1.47f,3.445f,z),new Vector3(.62f,.025f,.035f),wood);
                foreach(float z in new[]{-3.25f,-.9f,2.9f})
                {
                    Box(root,"Leather Travel Case",new Vector3(side*1.46f,3.59f,z),new Vector3(.4f,.27f,.55f),leather);
                    foreach(float dz in new[]{-.17f,.17f})Box(root,"Travel Case Strap",new Vector3(side*1.46f,3.736f,z+dz),new Vector3(.42f,.025f,.035f),brass);
                }
            }
            for(float z=-4f;z<4.1f;z+=.5f)foreach(float side in new[]{-1f,1f})
                Box(root,"Aisle Runner Woven Border",new Vector3(side*.405f,1.168f,z),new Vector3(.035f,.008f,.16f),ivory);
        }
        private void Locomotive(Transform root)
        {
            for(int i=0;i<20;i++)
            {
                float a=i*Mathf.PI/10f;
                Bolt(root,new Vector3(Mathf.Cos(a)*.9f,2.05f+Mathf.Sin(a)*.9f,8.558f),.055f,Vector3.forward);
            }
            foreach(float side in new[]{-1f,1f})
            {
                foreach(float z in new[]{4.5f,5.4f,6.55f,7.6f})
                {Box(root,"Boiler Band Clasp",new Vector3(side*1.07f,1.88f,z),new Vector3(.08f,.17f,.13f),brass);Bolt(root,new Vector3(side*1.12f,1.88f,z),.045f,side*Vector3.right);}
                Rod(root,"Feedwater Pipe",new Vector3(side*1.16f,1.54f,4.4f),new Vector3(side*1.16f,1.54f,7.2f),.085f,brass);
                Rod(root,"Feedwater Rise",new Vector3(side*1.16f,1.54f,7.2f),new Vector3(side*1.16f,2.0f,7.2f),.085f,brass);
                foreach(float z in new[]{4.5f,6.1f,7.2f})Box(root,"Pipe Bracket",new Vector3(side*1.11f,1.54f,z),new Vector3(.18f,.045f,.12f),steel);
                for(float z=4.5f;z<8.4f;z+=.3f)Box(root,"Running Board Tread",new Vector3(side*1.45f,1.16f,z),new Vector3(.48f,.018f,.045f),body);
                Rod(root,"Cylinder Drain Pipe",new Vector3(side*1.6f,.65f,7.8f),new Vector3(side*1.6f,.4f,8.23f),.06f,brass);
                foreach(float z in new[]{-.8f,.8f,3.2f})
                    Box(root,"Cab Wainscot Seam",new Vector3(side*1.81f,1.75f,z),new Vector3(.025f,.75f,.035f),brass);
            }
            foreach(float x in new[]{-.55f,.55f})
            {
                for(int i=0;i<9;i++)Box(root,"Cab Ventilation Louvre",new Vector3(x,1.63f+i*.055f,4.312f),new Vector3(.35f,.026f,.03f),steel);
            }
            // The headlamp retains its central clear lens.
            foreach(float x in new[]{-.19f,.19f})Rod(root,"Headlamp Cage",new Vector3(x,2.70f,8.77f),new Vector3(x,3.0f,8.77f),.025f,brass);
            var seat=GetComponentInChildren<ConductorSeat>();if(seat!=null)
            {
                // No decoration in the driver's front sight line or the furnace opening.
                Box(root,"Driver Footrest",new Vector3(-.85f,2.22f,2.2f),new Vector3(.72f,.07f,.22f),steel);
                for(int i=0;i<7;i++)Box(root,"Footrest Grip",new Vector3(-1.12f+i*.09f,2.26f,2.2f),new Vector3(.035f,.018f,.22f),brass);
            }
        }
        private void Box(Transform p,string n,Vector3 at,Vector3 size,Material m)=>CraftGeometry.Box(p,n,at,size,m);
        private void Bolt(Transform p,Vector3 at,float d,Vector3 normal)=>CraftGeometry.Bolt(p,at,d,brass,normal);
        private void Rod(Transform p,string n,Vector3 a,Vector3 b,float d,Material m)=>CraftGeometry.Rod(p,n,a,b,d,m);
    }
}
