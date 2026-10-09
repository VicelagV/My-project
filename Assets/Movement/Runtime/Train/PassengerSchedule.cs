using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AllAboard.Train
{
    public static class PassengerSchedule
    {
        public static bool Reachable(RouteNetwork network,string origin,string destination)
        {
            var visited=new HashSet<string>();var pending=new Stack<string>();pending.Push(origin);
            while(pending.Count>0){string id=pending.Pop();if(id==destination)return true;if(!visited.Add(id))continue;foreach(var edge in network.Outgoing(id))pending.Push(edge.to);}
            return false;
        }
        public static bool BoardsAt(RouteNode node)=>node.place==RoutePlace.Redfort||node.place==RoutePlace.Bluefort||((node.place==RoutePlace.Town||node.place==RoutePlace.Village)&&node.settlement!=SettlementKind.Abandoned);
        public static RouteNode[] Destinations(RouteNetwork network,string origin)=>network.nodes.Where(n=>n.id!=origin&&(n.place==RoutePlace.Town||n.place==RoutePlace.Village||n.place==RoutePlace.Station||n.place==RoutePlace.Bluefort||n.place==RoutePlace.Terminum)&&Reachable(network,origin,n.id)).ToArray();
        public static string InitialDestination(RouteNetwork network,int index)
        {
            if(index>=6)return index<10?"Bluefort":"Terminum";
            var family=index<2?RouteFamily.Safe:index<4?RouteFamily.Mixed:RouteFamily.Hard;
            var stops=network.nodes.Where(n=>n.family==family&&(n.place==RoutePlace.Town||n.place==RoutePlace.Village||n.place==RoutePlace.Station)).OrderBy(n=>n.position.z).ToArray();
            return stops.Length>0?stops[(index%2)%stops.Length].id:"Terminum";
        }
        public static int Demand(RouteNode node,EconomyConfig config)=>node.place==RoutePlace.Redfort?2:node.place==RoutePlace.Bluefort?config.bluefortDemand:node.family==RouteFamily.Hard?config.hardDemand:node.family==RouteFamily.Mixed?config.mixedDemand:config.safeDemand;
        public static float Deadline(RouteNetwork network,string origin,string destination,EconomyConfig config)
        {
            // A conservative straight-distance estimate plus generous repair/station time.
            float distance=Vector3.Distance(network.Node(origin).position,network.Node(destination).position);
            return distance/Mathf.Max(1f,config.expectedTravelSpeed)+config.repairGraceSeconds+config.stationGraceSeconds;
        }
    }
}
