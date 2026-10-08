using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public static class WorldButtonArt
    {
        public static GameObject Create(Transform parent, Vector3 at, string label, Material[] m)
        {
            var root=new GameObject(label+" Purchase Plate").transform;root.SetParent(parent,false);root.localPosition=at;
            RouteProps.Box(root,"Brass Plate",Vector3.zero,new Vector3(.72f,.66f,.055f),m[6],false);
            RouteProps.Box(root,"Enamel Inset",new Vector3(0f,0f,-.035f),new Vector3(.64f,.58f,.028f),m[5],false);
            foreach(float x in new[]{-.29f,.29f})foreach(float y in new[]{-.25f,.25f})RouteProps.Box(root,"Plate Screw",new Vector3(x,y,-.058f),new Vector3(.025f,.025f,.01f),m[3],false);
            var bezel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bezel.name="Button Brass Collar";bezel.transform.SetParent(root,false);bezel.transform.localPosition=new Vector3(0f,-.07f,-.065f);bezel.transform.localRotation=Quaternion.Euler(90f,0f,0f);bezel.transform.localScale=new Vector3(.22f,.04f,.22f);bezel.GetComponent<Renderer>().sharedMaterial=m[6];var collider=bezel.GetComponent<Collider>();collider.enabled=false;if(Application.isPlaying)Object.Destroy(collider);else Object.DestroyImmediate(collider);
            var cap=GameObject.CreatePrimitive(PrimitiveType.Cylinder);cap.name="Purchase Button";cap.transform.SetParent(root,false);cap.transform.localPosition=new Vector3(0f,-.07f,-.115f);cap.transform.localRotation=Quaternion.Euler(90f,0f,0f);cap.transform.localScale=new Vector3(.17f,.033f,.17f);cap.GetComponent<Renderer>().sharedMaterial=m[7];cap.AddComponent<TrainButton>().Configure(new Vector3(0f,0f,.025f));
            var text=new GameObject("Button Engraving").AddComponent<TextMesh>();text.transform.SetParent(root,false);text.transform.localPosition=new Vector3(0f,.15f,-.07f);text.text=label;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=64;text.fontStyle=FontStyle.Bold;text.characterSize=.025f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.94f,.89f,.72f);text.GetComponent<Renderer>().sharedMaterial=text.font.material;
            text.gameObject.AddComponent<PlaqueLettering>().Configure(.64f,.31f);text.gameObject.AddComponent<WorldTextDepth>();
            cap.AddComponent<ControlHighlight>().Configure(m[9],new[]{cap.GetComponent<Renderer>()});
            return cap;
        }
    }
}
