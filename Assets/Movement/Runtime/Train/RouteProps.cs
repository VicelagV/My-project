using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public static class RouteProps
    {
        private static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Material material,bool solid=true)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid){obj.GetComponent<Collider>().enabled=false;if(Application.isPlaying)Object.Destroy(obj.GetComponent<Collider>());else Object.DestroyImmediate(obj.GetComponent<Collider>());}return obj;
        }
        private static TextMesh Board(Transform parent,string text,Vector3 p,Vector3 size,Material[] palette,Quaternion rotation)
        {
            var root=new GameObject(text+" Sign").transform;root.SetParent(parent,false);root.localPosition=p;root.localRotation=rotation;
            Box(root,"Brass Sign Edge",Vector3.zero,size,palette[6],false);Box(root,"Enamel Sign Face",Vector3.back*0.018f,size-new Vector3(0.07f,0.07f,0f),palette[5],false);
            var letters=new GameObject("Mounted Lettering").AddComponent<TextMesh>();letters.transform.SetParent(root,false);letters.transform.localPosition=Vector3.back*(size.z*0.5f+0.03f);
            letters.text=text;letters.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");letters.fontStyle=FontStyle.Bold;letters.fontSize=64;letters.characterSize=0.012f;letters.anchor=TextAnchor.MiddleCenter;letters.alignment=TextAlignment.Center;letters.color=new Color(0.94f,0.89f,0.72f);letters.GetComponent<Renderer>().sharedMaterial=letters.font.material;return letters;
        }
        public static void Station(Transform parent,RouteNode node,Material[] palette,TrainController train,GameObject coal)
        {
            var station=new GameObject("Station - "+node.title).transform;station.SetParent(parent,false);station.localPosition=node.position;
            Box(station,"Boarding Platform",new Vector3(4.8f,0.575f,-8f),new Vector3(5.3f,1.15f,74f),palette[5]);
            Box(station,"Platform Brass Safety Edge",new Vector3(2.25f,1.158f,-8f),new Vector3(0.16f,0.02f,73f),palette[6],false);
            Box(station,"Locomotive Stop Mark",new Vector3(4.8f,1.165f,0f),new Vector3(5.1f,0.025f,0.28f),palette[8],false);
            for(int step=0;step<4;step++)Box(station,"Boarding Step",new Vector3(7.9f+step*0.6f,0.46f-step*0.11f,-29f),new Vector3(0.65f,0.92f-step*0.22f,3f),palette[5]);
            // Raised room and walkway continue the platform surface without an invisible gap.
            Box(station,"Station Landing",new Vector3(8.5f,1.10f,-2f),new Vector3(3.3f,0.1f,3f),palette[2]);
            Box(station,"Station Room Floor",new Vector3(14f,1.05f,-2f),new Vector3(10f,0.2f,14f),palette[2]);
            Box(station,"Station Back Wall",new Vector3(19f,2.75f,-2f),new Vector3(0.2f,3.2f,14f),palette[5]);
            foreach(float z in new[]{-9f,5f})Box(station,"Station Side Wall",new Vector3(14f,2.75f,z),new Vector3(10f,3.2f,0.2f),palette[5]);
            foreach(float z in new[]{-6.8f,2.8f})Box(station,"Station Entrance Side",new Vector3(9f,2.75f,z),new Vector3(0.2f,3.2f,4.4f),palette[5]);
            Box(station,"Station Door Lintel",new Vector3(9f,3.95f,-2f),new Vector3(0.2f,0.8f,5.2f),palette[6]);
            Box(station,"Station Roof",new Vector3(14f,4.4f,-2f),new Vector3(10.6f,0.22f,14.6f),palette[7]);
            foreach(float z in new[]{-17f,13f})
            {
                Box(station,"Station Sign Post",new Vector3(4.8f,2.1f,z),new Vector3(0.14f,1.9f,0.14f),palette[6]);
                Board(station,node.title,new Vector3(4.8f,3.1f,z),new Vector3(3.3f,0.5f,0.07f),palette,Quaternion.identity);
            }
            foreach(float z in new[]{-21f,18f})
            {
                Box(station,"Platform Bench",new Vector3(6f,1.63f,z),new Vector3(1.1f,0.15f,3f),palette[2]);
                foreach(float dz in new[]{-1f,1f})Box(station,"Bench Leg",new Vector3(6f,1.37f,z+dz),new Vector3(0.12f,0.44f,0.12f),palette[3]);
            }
            if(node.place==RoutePlace.SupplyStop||node.place==RoutePlace.Redfort||node.place==RoutePlace.Station||node.place==RoutePlace.Town)
            {
                var crate=Box(station,"Coal Supply",new Vector3(5.9f,1.65f,-8f),new Vector3(0.9f,1f,1.1f),palette[2]);
                var supply=crate.AddComponent<StationCoalSupply>();supply.Configure(train.Resources,coal);
                var lid=Box(crate.transform,"Coal Bin Top",new Vector3(0f,0.48f,0f),new Vector3(0.9f,0.1f,0.95f),palette[5],false);
                Board(crate.transform,"COAL",new Vector3(0f,0f,-0.51f),new Vector3(0.66f,0.22f,0.04f),palette,Quaternion.identity);
                crate.AddComponent<ControlHighlight>().Configure(palette[9],crate.GetComponentsInChildren<Renderer>().Where(x=>x.GetComponent<MeshFilter>()!=null).ToArray());
            }
            if(node.place==RoutePlace.Town)
                foreach(float z in new[]{-23f,22f})
                {
                    Box(station,"Town House",new Vector3(23f,2f,z),new Vector3(7f,4f,8f),palette[5]);
                    Box(station,"Town House Roof",new Vector3(23f,4.1f,z),new Vector3(7.5f,0.22f,8.5f),palette[7]);
                }
        }
        public static void RepairMarker(Transform parent,RouteNode node,Material[] palette)
        {
            var root=new GameObject("Marked Repair Site").transform;root.SetParent(parent,false);root.localPosition=node.position;
            Box(root,"Repair Sign Post",new Vector3(4f,1f,0f),new Vector3(0.12f,2f,0.12f),palette[6]);
            Board(root,"DAMAGED TRACK",new Vector3(4f,2f,0f),new Vector3(2.4f,0.45f,0.07f),palette,Quaternion.identity);
            for(int i=0;i<3;i++)Box(root,"Spare Sleeper",new Vector3(-4f,0.15f+i*0.2f,0f),new Vector3(0.4f,0.2f,4f),palette[2]);
            Box(root,"Spare Rail",new Vector3(-5f,0.1f,0f),new Vector3(0.14f,0.16f,6f),palette[3]);
        }
        public static void ReturnMarker(Transform parent,RouteNode node,Material[] palette)
        {
            var root=new GameObject("Day End Marker").transform;root.SetParent(parent,false);root.localPosition=node.position;
            foreach(float x in new[]{-4f,4f})
            {
                Box(root,"Return Sign Post",new Vector3(x,1.5f,0f),new Vector3(0.15f,3f,0.15f),palette[6]);
                Board(root,"DAY COMPLETE\nRETURN TO REDFORT",new Vector3(x,3f,0f),new Vector3(3.5f,0.8f,0.07f),palette,Quaternion.identity);
            }
        }
        public static void Switch(Transform parent,RouteNode node,ProceduralRailway railway,Material[] palette)
        {
            var root=new GameObject("Track Switch - "+node.id).transform;root.SetParent(parent,false);root.localPosition=node.position+new Vector3(3.8f,0f,-12f);
            Box(root,"Switch Standing Pad",new Vector3(0f,0.04f,0f),new Vector3(2.2f,0.08f,3f),palette[1]);
            var casing=Box(root,"Trackside Switch",new Vector3(0f,1f,0f),new Vector3(0.55f,0.32f,0.65f),palette[5]);
            var unit=new GameObject("Switch Unit Scale").transform;unit.SetParent(casing.transform,false);unit.localScale=new Vector3(1f/0.55f,1f/0.32f,1f/0.65f);
            var arm=new GameObject("Switch Lever Pivot").transform;arm.SetParent(unit,false);
            Box(arm,"Switch Lever",new Vector3(0f,0.25f,0f),new Vector3(0.06f,0.5f,0.06f),palette[6],false);
            var grip=Box(arm,"Switch Grip",new Vector3(0f,0.53f,0f),new Vector3(0.23f,0.14f,0.14f),palette[7]);
            var label=Board(root,railway.SelectedTitle(node.id),new Vector3(0f,1.9f,0.28f),new Vector3(1.8f,0.38f,0.07f),palette,Quaternion.identity);
            Box(root,"Switch Sign Mount",new Vector3(0f,1.35f,0.34f),new Vector3(0.08f,1.1f,0.08f),palette[6]);
            var control=casing.AddComponent<TrackSwitch>();control.Configure(railway,node.id,arm,grip.transform,label);
            control.gameObject.AddComponent<ControlHighlight>().Configure(palette[9],casing.GetComponentsInChildren<Renderer>());
        }
    }
}
