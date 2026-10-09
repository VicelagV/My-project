using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public enum RailToolKind { Saw, Connector }
    public sealed class RailTool : MonoBehaviour
    {
        public RailToolKind Kind { get; private set; }
        private RailDamageSection alignedSection;private int alignedSide;private float alignedMetres;
        private LineRenderer preview;private Material previewPaint;
        public bool IsAligned(RailDamageSection section,int side)=>alignedSection==section&&alignedSide==side&&Mathf.Abs(alignedMetres-section.RepairedMetres(side))<.001f;
        public void Align(RailDamageSection section,int side)
        {
            ClearAlignment();alignedSection=section;alignedSide=side;alignedMetres=section.RepairedMetres(side);
            var ghost=new GameObject("Steel Placement Preview");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(ghost,gameObject.scene);
            preview=ghost.AddComponent<LineRenderer>();preview.useWorldSpace=true;preview.widthMultiplier=.16f;preview.positionCount=11;
            previewPaint=new Material(Shader.Find("Universal Render Pipeline/Unlit"));previewPaint.SetColor("_BaseColor",new Color(.2f,1f,.7f));preview.sharedMaterial=previewPaint;
            float length=Mathf.Min(5f,RailDamagePlan.Length-alignedMetres);
            for(int i=0;i<11;i++)preview.SetPosition(i,section.Point(side,alignedMetres+length*i/10f)+Vector3.up*.06f);
        }
        public void ClearAlignment()
        {alignedSection=null;if(preview!=null){preview.gameObject.SetActive(false);Dispose(preview.gameObject);}if(previewPaint!=null)Dispose(previewPaint);preview=null;previewPaint=null;}
        private static void Dispose(Object value){if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
        private void Update(){if(alignedSection!=null&&(!GetComponent<PickupObject>().IsHeld||!IsAligned(alignedSection,alignedSide)))ClearAlignment();}
        private void OnDestroy()=>ClearAlignment();
        public static GameObject Create(RailToolKind kind, Material[] m)
        {
            var item = new GameObject(kind == RailToolKind.Saw ? "Rail Saw" : "Steel Connector");
            item.AddComponent<Rigidbody>().mass = 1.2f;
            var box = item.AddComponent<BoxCollider>(); box.size = new Vector3(.65f,.32f,.16f);
            item.AddComponent<PickupObject>(); item.AddComponent<RailTool>().Kind = kind;
            if (kind == RailToolKind.Saw)
            {
                item.AddComponent<RailSawControl>();
                RouteProps.Box(item.transform,"Saw Blade",new Vector3(-.1f,0f,0f),new Vector3(.48f,.18f,.03f),m[3],false);
                for(int i=0;i<12;i++)RouteProps.Box(item.transform,"Saw Tooth",new Vector3(-.32f+i*.04f,-.105f,0f),new Vector3(.022f,.035f,.03f),m[3],false);
                foreach(float y in new[]{-.12f,.12f})RouteProps.Box(item.transform,"Saw Handle Bridge",new Vector3(.23f,y,0f),new Vector3(.19f,.055f,.10f),m[2],false);
                RouteProps.Box(item.transform,"Saw Handle Grip",new Vector3(.31f,0f,0f),new Vector3(.055f,.25f,.10f),m[2],false);
                RouteProps.Box(item.transform,"Saw Handle Heel",new Vector3(.145f,0f,0f),new Vector3(.04f,.28f,.10f),m[2],false);
                foreach(float y in new[]{-.095f,.095f})RouteProps.Box(item.transform,"Saw Handle Rivet",new Vector3(.17f,y,-.057f),new Vector3(.027f,.027f,.018f),m[6],false);
            }
            else
            {
                RouteProps.Box(item.transform,"Connector Handle",Vector3.zero,new Vector3(.5f,.12f,.11f),m[2],false);
                foreach(float x in new[]{-.24f,.24f})RouteProps.Box(item.transform,"Rail Alignment Jaw",new Vector3(x,-.08f,0f),new Vector3(.09f,.22f,.15f),m[6],false);
                RouteProps.Box(item.transform,"Connector Steel Spine",new Vector3(0f,.07f,0f),new Vector3(.5f,.025f,.13f),m[3],false);
                for(int i=0;i<5;i++)RouteProps.Box(item.transform,"Connector Grip Band",new Vector3(-.12f+i*.06f,0f,0f),new Vector3(.018f,.135f,.125f),m[3],false);
            }
            var outline=RouteProps.Box(item.transform,"Tool Hover Outline",Vector3.zero,box.size,m[9],false).GetComponent<Renderer>();
            item.GetComponent<PickupObject>().Configure(outline); outline.enabled=false;
            return item;
        }
    }
}
