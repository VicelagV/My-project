using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailDamageSection:MonoBehaviour
    {
        private RailPath path;private Material[] materials;private Transform visuals;
        private TextMesh warning;
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
            warning=RouteProps.Board(sign,"",new Vector3(3.7f,2.2f,0f),new Vector3(2.5f,.7f,.08f),palette,Quaternion.identity);UpdateWarning();
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
                if(Kind==RailDamageKind.Brittle)
                {
                    float previous=0f;foreach(float crackAt in new[]{1.25f,3.1f,5.4f,7f})
                    {DamageRailMesh.Build(visuals,this,side,previous,crackAt-.018f,false,materials[3]);previous=crackAt+.018f;
                        for(int chip=0;chip<3;chip++){var p=Point(side,crackAt)+(Vector3.right*(chip-1)*.047f);var crack=RouteProps.Box(visuals,"Visible Rail Crack",p+Vector3.up*.014f,new Vector3(.055f,.014f,.055f),materials[5],false);crack.transform.rotation=Quaternion.Euler(0f,chip%2==0?35f:-35f,0f);}}
                    DamageRailMesh.Build(visuals,this,side,previous,filled,false,materials[3]);
                }
                else DamageRailMesh.Build(visuals,this,side,0f,filled,Kind==RailDamageKind.Strained&&!cut[side],materials[3]);
                if(repaired[side]>0f)
                {float joint=Mathf.Min(5f,repaired[side]);var sleeve=RouteProps.Box(visuals,"New Repair Joint",Point(side,joint)-Vector3.up*.02f,new Vector3(.25f,.035f,.27f),materials[6],false);sleeve.transform.rotation=Quaternion.LookRotation(Point(side,Mathf.Min(8f,joint+.01f))-Point(side,Mathf.Max(0f,joint-.01f)));}
                if(Kind==RailDamageKind.Brittle||repaired[side]>=RailDamagePlan.Length-.001f)continue;
                for(int end=0;end<(CanSaw(side)?1:2);end++)
                {
                    float d=end==0?repaired[side]:RailDamagePlan.Length;
                    var target=RouteProps.Box(visuals,CanSaw(side)?"Saw Mount":"Missing Rail Endpoint",Point(side,d)+Vector3.up*.09f,new Vector3(.38f,.22f,.34f),materials[6]);
                    target.AddComponent<RailRepairEnd>().Configure(this,side,end==1);
                    target.AddComponent<ControlHighlight>().Configure(materials[9],new[]{target.GetComponent<Renderer>()});
                }
            }
            UpdateWarning();
        }
        private void UpdateWarning(){if(warning==null)return;warning.text=IsRepaired?"REPAIRED TRACK\nNORMAL SPEED":IsMissing?"MISSING RAILS\nSTOP & REPAIR":Kind.ToString().ToUpperInvariant()+" RAILS\nMAX 3 m/s";warning.color=IsRepaired?new Color(.65f,1f,.65f):new Color(1f,.88f,.55f);warning.GetComponent<PlaqueLettering>()?.Fit();}
    }
}
