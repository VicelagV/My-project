using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AllAboard.Train
{
    [DefaultExecutionOrder(-200)]
    public sealed class ProceduralRailway : MonoBehaviour
    {
        [SerializeField] private int seed=8008;
        [SerializeField,Min(1)] private int day=1;
        [SerializeField,Range(5f,30f)] private float driveMinutes=15f;
        [SerializeField] private TrainController train;
        [SerializeField] private RailPath railPath;
        [SerializeField] private GameObject coalPrefab;
        [SerializeField] private Material[] palette;
        private readonly Dictionary<string,int> choices=new Dictionary<string,int>();
        private Dictionary<string,int> nodeIndices;
        private Transform generated;
        private RailDamageSection[] damage;
        private readonly Dictionary<string,float> damageDistances=new Dictionary<string,float>();
        private readonly HashSet<string> passedDamage=new HashSet<string>();
        private System.Random derailRandom;
        private RunProgress loadedProgress;
        public string DayEndReason {get;private set;}
        public RouteNetwork Network {get;private set;}
        public RailPath Path=>railPath;
        public TrainController Train=>train;
        public RouteGeometry Geometry {get;private set;}
        public int DailySeed=>unchecked(seed+(day-1)*104729);
        public float DriveMinutes=>driveMinutes;
        public bool DayEnded {get;private set;}
        public void CheckDayEnd()
        {
            float end=NodeDistance("Return");
            if(end<0f||train.HeadDistance<end||DayEnded)return;
            var economy=train.GetComponent<TrainDayEconomy>();
            if(economy!=null)economy.Finish(false);
            else FinishEconomicDay("Day complete");
        }
        private void FixedUpdate(){CheckDayEnd();}
        public void FinishEconomicDay(string report){if(DayEnded)return;DayEnded=true;DayEndReason=report;train.StopJourney();}
        public void Configure(TrainController owner,RailPath path,GameObject coal,Material[] materials,int baseSeed=8008,float minutes=15f)
        {train=owner;railPath=path;coalPrefab=coal;palette=materials;seed=baseSeed;driveMinutes=minutes;}
        private void Awake()
        {
            if(Application.isPlaying&&!Application.isBatchMode){loadedProgress=RunProgressStore.SessionCheckpoint??RunProgressStore.Load(RunProgressStore.DefaultPath);if(loadedProgress!=null){day=loadedProgress.nextDay;seed=loadedProgress.seed;}}
            if(train!=null&&railPath!=null&&palette!=null&&palette.Length>=10)BuildInitialWorld();
            if(loadedProgress!=null&&loadedProgress.failed){train.GetComponent<RailwayInventory>().FailDay("Run ended — start a fresh run in the service ledger");FinishEconomicDay("Run ended — start a fresh run in the service ledger");}
        }
        public void SaveProgress(bool success,EconomyDifficulty difficulty)
        {
            if(Application.isBatchMode)return;
            var bag=train.GetComponent<RailwayInventory>();var data=new RunProgress{nextDay=success?day+1:day,blumt=bag.Blumt,repairDebt=bag.RepairDebt,seed=seed,difficulty=difficulty,failed=!success,upgrades=train.GetComponent<TrainMaintenance>()?.Snapshot()};
            RunProgressStore.SessionCheckpoint=data;
            try{RunProgressStore.Save(RunProgressStore.DefaultPath,data);}catch(System.IO.IOException error){Debug.LogWarning("Could not save run progress: "+error.Message);}
        }
        public void SavePreparation()
        {
            if(Application.isBatchMode)return;var bag=train.GetComponent<RailwayInventory>();var data=new RunProgress{nextDay=day,blumt=bag.Blumt,repairDebt=bag.RepairDebt,seed=seed,difficulty=train.GetComponent<TrainDayEconomy>().Difficulty,upgrades=train.GetComponent<TrainMaintenance>()?.Snapshot()};RunProgressStore.SessionCheckpoint=data;try{RunProgressStore.Save(RunProgressStore.DefaultPath,data);}catch(System.IO.IOException error){Debug.LogWarning("Could not save preparation: "+error.Message);}
        }
        public bool PrepareNextDay()
        {
            var bag=train.GetComponent<RailwayInventory>();if(!DayEnded||!bag.Settled||bag.DayReport==null||!bag.DayReport.StartsWith("Day complete"))return false;
            SaveProgress(true,train.GetComponent<TrainDayEconomy>().Difficulty);UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.path);return true;
        }
        public void StartFreshRun(){RunProgressStore.SessionCheckpoint=null;RunProgressStore.NewRun(RunProgressStore.DefaultPath);UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.path);}
        public void BuildInitialWorld()
        {
            if(Application.isPlaying&&train.HeadDistance>80.1f)throw new System.InvalidOperationException("Generate a new world before departure, not while travelling.");
            var old=transform.Find("Generated Route World");if(old!=null){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
            DayEnded=false;DayEndReason=null;damageDistances.Clear();passedDamage.Clear();derailRandom=new System.Random(DailySeed^193);
            choices.Clear();Network=RouteNetwork.Generate(8008,1f);
            var measurement=new GameObject("Route Length Measurement");measurement.transform.SetParent(transform,false);var probe=measurement.AddComponent<RailPath>();probe.Configure(PlanPoints(out _));
            float baseLength=probe.Length-240f;if(Application.isPlaying)Destroy(measurement);else DestroyImmediate(measurement);
            Network=RouteNetwork.Generate(DailySeed,driveMinutes*60f*12f/baseLength);
            railPath.Configure(PlanPoints(out nodeIndices));train.Configure(railPath,train.Cars.ToArray(),80f);
            var inventory=train.GetComponent<RailwayInventory>();if(inventory==null)inventory=train.gameObject.AddComponent<RailwayInventory>();if(loadedProgress!=null)inventory.RestoreProgress(loadedProgress.blumt,loadedProgress.repairDebt);inventory.BeginDay(day);
            generated=new GameObject("Generated Route World").transform;generated.SetParent(transform,false);
            if(!Application.isPlaying)generated.gameObject.hideFlags=HideFlags.DontSaveInEditor;
            Geometry=generated.gameObject.AddComponent<RouteGeometry>();Geometry.Build(Network,palette[0],palette[1],palette[2],palette[3],palette[4]);
            RailwayStartingGear.Install(train,palette);
            foreach(var edge in Network.edges.Where(RailDamagePlan.HasDamage))
            {var site=new GameObject("Rail Damage - "+edge.id);site.transform.SetParent(generated,false);site.AddComponent<RailDamageSection>().Configure(edge,palette);}
            damage=generated.GetComponentsInChildren<RailDamageSection>();train.RailSafetyCheck=CheckRailSafety;
            foreach(var landmark in Geometry.Landmarks)LandmarkArt.Build(generated,landmark,Geometry,palette);
            foreach(var node in Network.nodes)
            {
                if(node.place==RoutePlace.ForestRepair)RouteProps.RepairMarker(generated,node,palette);
                if(node.place==RoutePlace.Return)RouteProps.ReturnMarker(generated,node,palette);
                if(RouteGeometry.IsStop(node.place))RouteProps.Station(generated,node,palette,train,coalPrefab);
                if(Network.Outgoing(node.id).Count()>1)RouteProps.Switch(generated,node,this,palette);
            }
            var economy=train.GetComponent<TrainDayEconomy>();if(economy==null)economy=train.gameObject.AddComponent<TrainDayEconomy>();if(loadedProgress!=null)economy.SetDifficulty(loadedProgress.difficulty);economy.Configure(this,palette);if(loadedProgress!=null)train.GetComponent<TrainMaintenance>().Restore(loadedProgress.upgrades);
            Physics.SyncTransforms();
        }
        private Vector3[] PlanPoints(out Dictionary<string,int> indices)
        {
            indices=new Dictionary<string,int>();var points=new List<Vector3>();
            var start=Network.Node("Redfort").position;points.Add(start-Vector3.forward*80f);points.Add(start-Vector3.forward*40f);points.Add(start);
            string current="Redfort";indices[current]=points.Count-1;var visited=new HashSet<string>();
            while(visited.Add(current))
            {
                var exits=Network.Outgoing(current).ToArray();if(exits.Length==0)break;
                int choice=choices.TryGetValue(current,out int saved)?Mathf.Clamp(saved,0,exits.Length-1):0;var edge=exits[choice];
                for(int i=1;i<edge.points.Length;i++)points.Add(edge.points[i]);current=edge.to;indices[current]=points.Count-1;
            }
            Vector3 end=points[points.Count-1];points.Add(end+Vector3.forward*80f);points.Add(end+Vector3.forward*160f);return points.ToArray();
        }
        public int SelectedIndex(string node)=>choices.TryGetValue(node,out int value)?value:0;
        public string SelectedTitle(string node)=>Network.Outgoing(node).ElementAt(SelectedIndex(node)).title;
        public bool CanChange(string node)
        {
            if(Mathf.Abs(train.Speed)>0.1f)return false;
            if(nodeIndices.ContainsKey(node))
                foreach(var car in train.Cars)
                    if(car.Distance+(car.Kind==CarKind.Locomotive?8.8f:4.55f)>=IntersectionDistance(node))return false;
            return true;
        }
        public bool Choose(string node,int option)
        {
            var exits=Network.Outgoing(node).ToArray();if(DayEnded||option<0||option>=exits.Length||!CanChange(node))return false;
            choices[node]=option;railPath.Configure(PlanPoints(out nodeIndices));damageDistances.Clear();return true;
        }
        private void CheckRailSafety(float previous,float next)
        {
            if(damage==null||DayEnded)return;
            foreach(var section in damage)
            {
                var edge=Network.Edge(section.EdgeId);
                float start=NodeDistance(edge.from);if(start<0f||Network.Outgoing(edge.from).ElementAt(SelectedIndex(edge.from)).id!=edge.id)continue;
                if(!damageDistances.TryGetValue(edge.id,out float at))
                {
                    float guess=start+section.Start,best=float.PositiveInfinity;at=guess;
                    for(float d=guess-20f;d<=guess+20f;d+=.25f){railPath.Sample(d,out var p,out _);float error=(p-section.StartPosition).sqrMagnitude;if(error<best){best=error;at=d;}}
                    damageDistances[edge.id]=at;
                }
                int index=0;
                foreach(var car in train.Consist)
                {
                    float offset=9.5f*index++-(car.Kind==CarKind.Locomotive?7f:3.8f);
                    float a=previous-offset,b=next-offset;
                    if(Mathf.Max(a,b)<at||Mathf.Min(a,b)>at+RailDamagePlan.Length)continue;
                    string token=edge.id+"/"+car.GetEntityId()+"/"+(next>=previous?"F":"R");
                    // Slow entry is safe, but accelerating on damaged rails still checks the risk.
                    if(section.IsRepaired||(!section.IsMissing&&Mathf.Abs(train.Speed)<=3f))continue;
                    if(!passedDamage.Add(token))continue;
                    train.GetComponent<TrainMaintenance>()?.TrackImpact(section.IsMissing?60:Mathf.Max(0,Mathf.Abs(train.Speed)-3)*4);
                    if(section.IsMissing||derailRandom.NextDouble()<RailDamagePlan.DerailChance(train.Speed))
                    {DayEnded=true;DayEndReason=section.IsMissing?"Derailed: missing rails":"Derailed: exceeded 3 m/s on damaged rails";train.Derail();train.GetComponent<TrainDayEconomy>()?.Abort(DayEndReason);return;}
                }
            }
        }
        private void OnGUI(){if(DayEnded)GUI.Label(new Rect(25f,25f,600f,45f),DayEndReason+" — restart Play mode to begin again");}
        // Catmull-Rom's first differing point affects the segment two points earlier.
        private int SharedApproach(string node)
        {
            var exits=Network.Outgoing(node).ToArray();if(exits.Length<2)return 0;
            int common=0;
            while(common<exits.Min(x=>x.points.Length)&&exits.All(x=>Vector3.Distance(x.points[common],exits[0].points[common])<0.001f))common++;
            return Mathf.Max(0,common-2);
        }
        public Vector3 IntersectionPosition(string node)=>Network.Outgoing(node).First().points[SharedApproach(node)];
        public float IntersectionDistance(string node)=>nodeIndices.TryGetValue(node,out int index)?railPath.ControlPointDistance(index+SharedApproach(node)):-1f;
        public float NodeDistance(string id)=>nodeIndices.TryGetValue(id,out int index)?railPath.ControlPointDistance(index):-1f;
    }
}
