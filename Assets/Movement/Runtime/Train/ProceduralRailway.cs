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
            DayEnded=true;train.StopJourney();
        }
        private void FixedUpdate(){CheckDayEnd();}
        public void Configure(TrainController owner,RailPath path,GameObject coal,Material[] materials,int baseSeed=8008,float minutes=15f)
        {train=owner;railPath=path;coalPrefab=coal;palette=materials;seed=baseSeed;driveMinutes=minutes;}
        private void Awake(){if(train!=null&&railPath!=null&&palette!=null&&palette.Length>=10)BuildInitialWorld();}
        public void BuildInitialWorld()
        {
            if(Application.isPlaying&&train.HeadDistance>80.1f)throw new System.InvalidOperationException("Generate a new world before departure, not while travelling.");
            var old=transform.Find("Generated Route World");if(old!=null){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
            DayEnded=false;choices.Clear();Network=RouteNetwork.Generate(8008,1f);
            var measurement=new GameObject("Route Length Measurement");measurement.transform.SetParent(transform,false);var probe=measurement.AddComponent<RailPath>();probe.Configure(PlanPoints(out _));
            float baseLength=probe.Length-240f;if(Application.isPlaying)Destroy(measurement);else DestroyImmediate(measurement);
            Network=RouteNetwork.Generate(DailySeed,driveMinutes*60f*12f/baseLength);
            railPath.Configure(PlanPoints(out nodeIndices));train.Configure(railPath,train.Cars.ToArray(),80f);
            generated=new GameObject("Generated Route World").transform;generated.SetParent(transform,false);
            if(!Application.isPlaying)generated.gameObject.hideFlags=HideFlags.DontSaveInEditor;
            Geometry=generated.gameObject.AddComponent<RouteGeometry>();Geometry.Build(Network,palette[0],palette[1],palette[2],palette[3],palette[4]);
            foreach(var node in Network.nodes)
            {
                if(node.place==RoutePlace.ForestRepair)RouteProps.RepairMarker(generated,node,palette);
                if(node.place==RoutePlace.Return)RouteProps.ReturnMarker(generated,node,palette);
                if(RouteGeometry.IsStop(node.place))RouteProps.Station(generated,node,palette,train,coalPrefab);
                if(Network.Outgoing(node.id).Count()>1)RouteProps.Switch(generated,node,this,palette);
            }
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
            if(nodeIndices.TryGetValue(node,out int index))
                foreach(var car in train.Cars)if(car.Distance+9f>=railPath.ControlPointDistance(index)-5f)return false;
            return true;
        }
        public bool Choose(string node,int option)
        {
            var exits=Network.Outgoing(node).ToArray();if(DayEnded||option<0||option>=exits.Length||!CanChange(node))return false;
            choices[node]=option;railPath.Configure(PlanPoints(out nodeIndices));return true;
        }
        public float NodeDistance(string id)=>nodeIndices.TryGetValue(id,out int index)?railPath.ControlPointDistance(index):-1f;
    }
}
