using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AllAboard.Train
{
    public enum RouteFamily { Shared, Safe, Mixed, Hard }
    public enum RoutePlace { Junction, Redfort, ForestRepair, SupplyStop, Station, Town, Bluefort, Gate, Terminum, Return, Village }
    public enum SettlementKind { Frontier, Mining, Lumber, IsolatedStation, Abandoned }

    [Serializable]
    public sealed class RouteNode
    {
        public string id;
        public string title;
        public Vector3 position;
        public RoutePlace place;
        public RouteFamily family;
        public SettlementKind settlement;
        public int shops;
        public int fareMultiplier=1;
    }

    [Serializable]
    public sealed class RouteEdge
    {
        public string id, from, to, title;
        public RouteFamily family;
        public bool crossover;
        public Vector3[] points;
    }

    [Serializable]
    public sealed class RouteNetwork
    {
        public int seed;
        public List<RouteNode> nodes = new List<RouteNode>();
        public List<RouteEdge> edges = new List<RouteEdge>();
        public const int TerminumEntryFee = 1000;
        public RouteNode Node(string id) => nodes.First(x => x.id == id);
        public IEnumerable<RouteEdge> Outgoing(string node) => edges.Where(x => x.from == node);
        public RouteEdge Edge(string id) => edges.First(x => x.id == id);

        // Local RNG keeps generation independent of gameplay's UnityEngine.Random state.
        public static RouteNetwork Generate(int seed, float scale)
        {
            var random = new System.Random(seed);
            var graph = new RouteNetwork { seed = seed };
            graph.AddNode("Redfort", "REDFORT", 0f, 0f, RoutePlace.Redfort, RouteFamily.Shared, scale);
            graph.AddNode("Forest", "FOREST REPAIR SITE", 0f, 190f, RoutePlace.ForestRepair, RouteFamily.Shared, scale);
            graph.AddNode("Starter", "STARTER JUNCTION", 0f, 320f, RoutePlace.Junction, RouteFamily.Shared, scale);
            graph.AddNode("Supply", "FOREST SUPPLY STOP", 0f, 700f, RoutePlace.SupplyStop, RouteFamily.Shared, scale);
            graph.AddEdge("Opening", "Redfort", "Forest", "FOREST", RouteFamily.Shared, 0f, scale, random);
            graph.AddEdge("ForestExit", "Forest", "Starter", "FOREST", RouteFamily.Shared, 0f, scale, random);
            graph.AddEdge("StarterSafe", "Starter", "Supply", "SAFE", RouteFamily.Safe, -95f, scale, random);
            graph.AddEdge("StarterRisky", "Starter", "Supply", "RISKY", RouteFamily.Hard, 95f, scale, random);
            var families = new[] { RouteFamily.Safe, RouteFamily.Mixed, RouteFamily.Hard };
            foreach (RouteFamily family in families)
            {
                float lane = ((int)family - 2) * 165f;
                int count=family==RouteFamily.Safe?1:family==RouteFamily.Hard?3:random.Next(1,4);
                var stopStages=Enumerable.Range(0,3).OrderBy(_=>random.Next()).Take(count).ToArray();
                for (int stage = 0; stage < 3; stage++)
                {
                    RoutePlace place = !stopStages.Contains(stage)?RoutePlace.Junction:family==RouteFamily.Safe?RoutePlace.Town:
                        (RoutePlace)new[]{(int)RoutePlace.Town,(int)RoutePlace.Village,(int)RoutePlace.Station}[random.Next(3)];
                    string id = family + stage.ToString();
                    string title=place==RoutePlace.Junction?family.ToString().ToUpperInvariant()+" JUNCTION "+(stage+1):
                        new[]{"CEDAR","DUST","COPPER","SILVER","WILLOW","CANYON","PINE","DRY"}[random.Next(8)]+new[]{" CREEK"," RIDGE"," CROSSING"," FORD"," VALLEY"," SPRINGS"}[random.Next(6)]+" "+(stage+1);
                    if(graph.nodes.Any(x=>x.title==title))title += " " + family.ToString().ToUpperInvariant();
                    graph.AddNode(id,title,
                        lane + (stage == 0 ? 0f : (float)(random.NextDouble()-0.5)*55f), 1100f + stage*510f, place, family, scale);
                    var node=graph.Node(id);node.settlement=place==RoutePlace.Station?SettlementKind.IsolatedStation:family==RouteFamily.Safe?SettlementKind.Frontier:(SettlementKind)new[]{0,1,2,4}[random.Next(4)];
                    node.shops=place==RoutePlace.Town?2+(random.NextDouble()<1d/3?1:0)+(random.NextDouble()<1d/3?1:0):place==RoutePlace.Village?1+(random.NextDouble()<0.5?1:0):0;
                    node.fareMultiplier=place==RoutePlace.Station?5:1;
                    if(stage == 0) graph.AddEdge("To"+id,"Supply",id,family.ToString().ToUpperInvariant(),family,0f,scale,random);
                    else graph.AddEdge("Along"+id,family+(stage-1).ToString(),id,"CONTINUE "+family.ToString().ToUpperInvariant(),family,0f,scale,random);
                }
            }
            for(int stage=0;stage<2;stage++)
                foreach(RouteFamily from in families)
                    foreach(RouteFamily to in families)
                        if(Math.Abs((int)from-(int)to)==1)
                            graph.AddEdge("Cross"+from+stage+to,from+stage.ToString(),to+(stage+1).ToString(),"CROSS TO "+to.ToString().ToUpperInvariant(),to,0f,scale,random,true);
            graph.AddNode("Bluefort","BLUEFORT",0f,2640f,RoutePlace.Bluefort,RouteFamily.Shared,scale);
            graph.AddNode("Gate","TERMINUM APPROACH",0f,2850f,RoutePlace.Gate,RouteFamily.Shared,scale);
            graph.AddNode("Terminum","TERMINUM",0f,3100f,RoutePlace.Terminum,RouteFamily.Shared,scale);
            foreach(RouteFamily family in families) graph.AddEdge("Merge"+family,family+"2","Bluefort","BLUEFORT",RouteFamily.Shared,0f,scale,random);
            graph.AddEdge("CapitalApproach","Bluefort","Gate","TERMINUM",RouteFamily.Shared,0f,scale,random);
            graph.AddEdge("CapitalEntry","Gate","Terminum","TERMINUM ENTRY",RouteFamily.Shared,0f,scale,random);
            graph.AddNode("Return", "RETURN TO REDFORT", 180f, 3180f, RoutePlace.Return, RouteFamily.Shared, scale);
            graph.AddEdge("CapitalBypass", "Gate", "Return", "BYPASS / RETURN", RouteFamily.Shared, 80f, scale, random);
            return graph;
        }
        private void AddNode(string id,string title,float x,float z,RoutePlace place,RouteFamily family,float scale)
        { nodes.Add(new RouteNode{id=id,title=title,position=new Vector3(x,0f,z)*scale,place=place,family=family}); }
        private void AddEdge(string id,string from,string to,string title,RouteFamily family,float bow,float scale,System.Random random,bool crossover=false)
        {
            Vector3 a=Node(from).position,b=Node(to).position;
            // Shared straight lead-in/out keeps station platforms and switch tangents aligned.
            float lead=Mathf.Min(55f*scale,(b.z-a.z)*0.23f);
            float wobble=family==RouteFamily.Shared ? 0f : (float)(random.NextDouble()-0.5)*35f*scale;
            edges.Add(new RouteEdge{id=id,from=from,to=to,title=title,family=family,crossover=crossover,
                points=new[]{a,a+Vector3.forward*lead*0.5f,a+Vector3.forward*lead,
                    Vector3.Lerp(a,b,0.35f)+Vector3.right*(bow*scale+wobble),Vector3.Lerp(a,b,0.65f)+Vector3.right*(bow*scale-wobble),
                    b-Vector3.forward*lead,b-Vector3.forward*lead*0.5f,b}});
        }
    }
}
