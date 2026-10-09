using UnityEngine;
namespace AllAboard.Train
{
    public static class ModelDetails
    {
        public static void Lantern(Transform parent,Vector3 foot,Material[] m,string prefix,float height)
        {
            var root=new GameObject(prefix+" Framed Lantern").transform;root.SetParent(parent,false);root.localPosition=foot;
            RouteProps.Box(root,"Lamp Foot",new Vector3(0f,.1f,0f),new Vector3(.28f,.2f,.28f),m[5],false);
            RouteProps.Box(root,"Lamp Post",Vector3.up*(height*.5f),new Vector3(.12f,height,.12f),m[5],false);
            RouteProps.Box(root,"Lantern Base",Vector3.up*height,new Vector3(.56f,.12f,.56f),m[5],false);
            RouteProps.Box(root,"Visible Lantern Light",Vector3.up*(height+.3f),new Vector3(.37f,.48f,.37f),m[8],false);
            foreach(float x in new[]{-.23f,.23f})foreach(float z in new[]{-.23f,.23f})RouteProps.Box(root,"Lantern Corner Frame",new Vector3(x,height+.3f,z),new Vector3(.055f,.6f,.055f),m[5],false);
            RouteProps.Box(root,"Lantern Cap",Vector3.up*(height+.62f),new Vector3(.63f,.14f,.63f),m[5],false);
            RouteProps.Box(root,"Lantern Finial",Vector3.up*(height+.76f),new Vector3(.10f,.16f,.10f),m[6],false);
        }
    }
}
