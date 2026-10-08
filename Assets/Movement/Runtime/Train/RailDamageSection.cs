using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailDamageSection:MonoBehaviour
    {
        private RailPath path;private Material[] materials;private Transform visuals;
        private readonly bool[] cut=new bool[2];private readonly float[] repaired=new float[2];
        public string EdgeId {get;private set;}
        public float Start {get;private set;}
        public RailDamageKind Kind {get;private set;}
        public Vector3 StartPosition {get{path.Sample(Start,out var p,out _);return p;}}
        public bool IsRepaired=>repaired[0]>=RailDamagePlan.Length-.001f&&repaired[1]>=RailDamagePlan.Length-.001f;
        public bool IsMissing=>!IsRepaired&&(Kind==RailDamageKind.Missing||cut[0]||cut[1]);
        public bool CanSaw(int side)=>Kind==RailDamageKind.Strained&&!cut[side];
        public float RepairedMetres(int side)=>repaired[side];
        public void Configure(RouteEdge edge,Material[] palette)
        {
            EdgeId=edge.id;materials=palette;Kind=RailDamagePlan.Kind(edge);
            path=gameObject.AddComponent<RailPath>();path.Configure(edge.points);Start=RailDamagePlan.Start(edge,path.Length);
            if(Kind==RailDamageKind.Missing){cut[0]=true;cut[1]=true;}
            Redraw();path.Sample(Start-20f,out var marker,out var facing);
            var sign=new GameObject("Rail Damage Warning").transform;sign.SetParent(transform,false);sign.position=marker;sign.rotation=facing;
            RouteProps.Box(sign,"Warning Post",new Vector3(3.7f,1f,0f),new Vector3(.12f,2f,.12f),palette[6]);
            RouteProps.Board(sign,Kind==RailDamageKind.Missing?"MISSING RAILS\nSTOP & REPAIR":Kind.ToString().ToUpperInvariant()+" RAILS\nMAX 3 m/s",new Vector3(3.7f,2.2f,0f),new Vector3(2.5f,.7f,.08f),palette,Quaternion.identity);
        }
        public Vector3 Point(int side,float distance)
        {path.Sample(Start+distance,out var p,out var q);return p+q*new Vector3(side==0?-1.98f:1.98f,.09f,0f);}
        public bool FinishCut(int side)
        {if(!CanSaw(side))return false;cut[side]=true;Redraw();return true;}
        public bool Replace(int side,RailwayInventory inventory)
        {
            if(!cut[side]||repaired[side]>=RailDamagePlan.Length-.001f||inventory==null)return false;
            float amount=Mathf.Min(5f,RailDamagePlan.Length-repaired[side]);
            if(!inventory.UseSteel(amount))return false;repaired[side]+=amount;Redraw();return true;
        }
        private void Redraw()
        {
            if(visuals!=null){visuals.gameObject.SetActive(false);if(Application.isPlaying)Destroy(visuals.gameObject);else DestroyImmediate(visuals.gameObject);}
            visuals=new GameObject("Damage Rail Geometry").transform;visuals.SetParent(transform,false);
            for(int side=0;side<2;side++)
            {
                float filled=cut[side]?repaired[side]:RailDamagePlan.Length;
                for(float d=0f;d<filled-.001f;d+=.5f)
                {
                    Vector3 a=Point(side,d),b=Point(side,Mathf.Min(d+.5f,filled));
                    if(Kind==RailDamageKind.Strained&&!cut[side]){a+=Vector3.right*Mathf.Sin(d*Mathf.PI/2f)*.34f;b+=Vector3.right*Mathf.Sin((d+.5f)*Mathf.PI/2f)*.34f;}
                    Quaternion facing=Quaternion.LookRotation(b-a);Vector3 p=(a+b)*.5f;
                    var head=RouteProps.Box(visuals,"Rail Head",p,new Vector3(.14f,.022f,Vector3.Distance(a,b)),materials[3],false);head.transform.rotation=facing;
                    var web=RouteProps.Box(visuals,"Rail Web",p-Vector3.up*.025f,new Vector3(.055f,.03f,Vector3.Distance(a,b)),materials[3],false);web.transform.rotation=facing;
                    var foot=RouteProps.Box(visuals,"Rail Foot",p-Vector3.up*.047f,new Vector3(.22f,.018f,Vector3.Distance(a,b)),materials[3],false);foot.transform.rotation=facing;
                    if(Kind==RailDamageKind.Brittle&&Mathf.RoundToInt(d*2)%3==0)
                    {var crack=RouteProps.Box(visuals,"Visible Rail Crack",p+Vector3.up*.013f,new Vector3(.16f,.007f,.028f),materials[5],false);crack.transform.rotation=facing*Quaternion.Euler(0f,35f,0f);}
                }
                if(Kind==RailDamageKind.Brittle||repaired[side]>=RailDamagePlan.Length-.001f)continue;
                for(int end=0;end<(CanSaw(side)?1:2);end++)
                {
                    float d=end==0?repaired[side]:RailDamagePlan.Length;
                    var target=RouteProps.Box(visuals,CanSaw(side)?"Saw Mount":"Missing Rail Endpoint",Point(side,d)+Vector3.up*.09f,new Vector3(.38f,.22f,.34f),materials[6]);
                    target.AddComponent<RailRepairEnd>().Configure(this,side,end==1);
                    target.AddComponent<ControlHighlight>().Configure(materials[9],new[]{target.GetComponent<Renderer>()});
                }
            }
        }
    }
}
