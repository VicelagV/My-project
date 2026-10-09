using UnityEngine;
namespace AllAboard.Train
{
    public static class EquipmentCraftsmanship
    {
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m)=>CraftGeometry.Box(p,n,at,size,m);
        public static void Weapon(Transform weapon,WeaponKind kind,float length,Material[] m)
        {
            var root=CraftGeometry.Group(weapon,"Detailed Weapon Craftsmanship");
            if(kind<=WeaponKind.Shotgun)
            {
                foreach(float side in new[]{-1f,1f})
                {
                    foreach(float z in new[]{-.16f,-.02f})CraftGeometry.Bolt(root,new Vector3(side*.079f,.035f,z),.024f,m[6],Vector3.right*side);
                    for(int i=0;i<4;i++)Box(root,"Grip Checkering",new Vector3(side*.059f,-.18f+i*.035f,-length*.3f),new Vector3(.008f,.006f,.1f),m[5]);
                }
                Box(root,"Raised Rear Sight",new Vector3(0,.116f,-.145f),new Vector3(.075f,.026f,.035f),m[3]);
                foreach(float side in new[]{-1f,1f})Box(root,"Sight Notch Ear",new Vector3(side*.03f,.145f,-.145f),new Vector3(.017f,.034f,.025f),m[3]);
                Box(root,"Sight Bead",new Vector3(0,.157f,length*.5f-.04f),new Vector3(.022f,.015f,.026f),m[8]);
                Box(root,"Receiver Top Rail",new Vector3(0,.104f,-.06f),new Vector3(.066f,.018f,.18f),m[3]);
                if(kind==WeaponKind.Revolver)
                {
                    for(int i=0;i<6;i++)
                    {
                        float a=i*Mathf.PI/3f;
                        var position=new Vector3(Mathf.Cos(a)*.061f,.04f+Mathf.Sin(a)*.061f,-.171f);
                        CraftGeometry.Round(root,"Cylinder Chamber",position,new Vector3(.027f,.003f,.027f),m[5],Quaternion.Euler(90,0,0));
                        CraftGeometry.Rod(root,"Cylinder Fluting",new Vector3(Mathf.Cos(a)*.079f,.04f+Mathf.Sin(a)*.079f,-.13f),new Vector3(Mathf.Cos(a)*.079f,.04f+Mathf.Sin(a)*.079f,-.04f),.012f,m[5]);
                    }
                    Box(root,"Hammer Spur",new Vector3(0,.126f,-.22f),new Vector3(.04f,.04f,.065f),m[3]);
                    CraftGeometry.Rod(root,"Ejector Rod",new Vector3(.048f,.006f,.05f),new Vector3(.048f,.006f,.19f),.022f,m[6]);
                    Box(root,"Grip Heel Cap",new Vector3(0,-.217f,-length*.3f),new Vector3(.114f,.026f,.13f),m[6]);
                }
                else
                {
                    foreach(float z in new[]{.1f,length*.34f})
                    {Box(root,"Barrel Retaining Band",new Vector3(0,.025f,z),new Vector3(kind==WeaponKind.Shotgun?.175f:.09f,.15f,.035f),m[6]);}
                    foreach(float side in new[]{-1f,1f})
                    {
                        for(int i=0;i<7;i++)Box(root,"Stock Carved Line",new Vector3(side*.059f,-.03f,-length*.44f+i*.027f),new Vector3(.005f,.10f,.007f),m[5]);
                        CraftGeometry.Bolt(root,new Vector3(side*.06f,-.028f,-length*.32f),.024f,m[6],Vector3.right*side);
                    }
                    if(kind==WeaponKind.Rifle)
                    {
                        CraftGeometry.Rod(root,"Bolt Handle",new Vector3(.065f,.09f,-.06f),new Vector3(.085f,.034f,-.06f),.026f,m[3]);
                        CraftGeometry.Round(root,"Bolt Handle Knob",new Vector3(.087f,.025f,-.06f),new Vector3(.026f,.016f,.026f),m[6],Quaternion.identity);
                    }
                    else
                    {
                        Box(root,"Break Action Seam",new Vector3(0,.073f,.015f),new Vector3(.16f,.055f,.006f),m[5]);
                        Box(root,"Break Action Latch",new Vector3(0,.12f,-.05f),new Vector3(.025f,.025f,.085f),m[6]);
                    }
                }
            }
            else
            {
                for(int i=0;i<7;i++)
                {
                    var wrap=Box(root,"Leather Grip Winding",new Vector3(0,0,-.33f+i*.027f),new Vector3(.118f,.116f,.018f),m[2]);wrap.transform.localRotation=Quaternion.Euler(0,0,i%2==0?12:-12);
                }
                Box(root,"Grip Pommel",new Vector3(0,0,-.357f),new Vector3(.12f,.12f,.027f),m[6]);
                if(kind==WeaponKind.Club)
                {
                    for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;CraftGeometry.Rod(root,"Club Head Grain",new Vector3(Mathf.Cos(a)*.082f,Mathf.Sin(a)*.082f,.05f),new Vector3(Mathf.Cos(a)*.082f,Mathf.Sin(a)*.082f,.27f),.008f,m[5]);}
                    foreach(float side in new[]{-1f,1f})CraftGeometry.Bolt(root,new Vector3(side*.098f,0,.26f),.025f,m[6],side*Vector3.right);
                }
                else
                {
                    Box(root,"Hatchet Head Socket",new Vector3(0,0,.27f),new Vector3(.12f,.1f,.17f),m[5]);
                    Box(root,"Hatchet Handle Wedge",new Vector3(0,.056f,.27f),new Vector3(.025f,.012f,.12f),m[6]);
                    foreach(float z in new[]{.22f,.31f})CraftGeometry.Bolt(root,new Vector3(.07f,-.044f,z),.026f,m[6],Vector3.down);
                }
            }
            CraftGeometry.Bake(root);
        }
        public static void Tool(Transform item,Material[] m,bool saw)
        {
            var root=CraftGeometry.Group(item,"Detailed Rail Tool Craftsmanship");
            if(saw)
            {
                foreach(float side in new[]{-1f,1f})
                {
                    Box(root,"Saw Blade Etched Spine",new Vector3(-.1f,.065f,side*.017f),new Vector3(.4f,.013f,.005f),m[6]);
                    foreach(float y in new[]{-.095f,.095f})CraftGeometry.Bolt(root,new Vector3(.17f,y,side*.059f),.029f,m[6],side*Vector3.forward);
                    for(int i=0;i<5;i++)Box(root,"Saw Grip Carving",new Vector3(.31f,-.075f+i*.035f,side*.052f),new Vector3(.045f,.008f,.012f),m[5]);
                }
            }
            else
            {
                foreach(float x in new[]{-.24f,.24f})
                {
                    Box(root,"Connector Jaw Pad",new Vector3(x,-.19f,0),new Vector3(.095f,.024f,.154f),m[3]);
                    for(int i=0;i<3;i++)Box(root,"Jaw Grip Tooth",new Vector3(x,-.176f,-.05f+i*.05f),new Vector3(.09f,.012f,.016f),m[5]);
                    CraftGeometry.Bolt(root,new Vector3(x,.085f,0),.052f,m[6],Vector3.up);
                }
            }
            CraftGeometry.Bake(root);
        }
        public static void Wrench(Transform item,Material[] m)
        {
            var root=CraftGeometry.Group(item,"Detailed Wrench Craftsmanship");
            foreach(float side in new[]{-1f,1f})
            {
                Box(root,"Wrench Forged Ridge",new Vector3(0,side*.03f,.04f),new Vector3(.035f,.008f,.22f),m[6]);
                for(int i=0;i<6;i++)Box(root,"Wrench Grip Rib",new Vector3(0,side*.044f,-.2f+i*.033f),new Vector3(.112f,.012f,.012f),m[2]);
            }
            foreach(float x in new[]{-.079f,.079f})Box(root,"Jaw Machined Face",new Vector3(x,0,.262f),new Vector3(.012f,.067f,.105f),m[8]);
            CraftGeometry.Bake(root);
        }
    }
}
