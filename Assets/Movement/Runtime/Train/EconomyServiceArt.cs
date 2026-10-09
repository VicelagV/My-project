using System.Linq;
using UnityEngine;
namespace AllAboard.Train
{
    public static class EconomyServiceArt
    {
        public static void Build(TrainDayEconomy economy,Transform parent)
        {
            var m=economy.Palette;
            foreach(var node in economy.World.Network.nodes.Where(PassengerSchedule.BoardsAt))
            {
                var root=new GameObject("Service Desk - "+node.title).transform;root.SetParent(parent,false);root.position=node.position;
                RouteProps.Box(root,"Ticket Desk",new Vector3(12.85f,1.65f,-5.2f),new Vector3(5.2f,.95f,1f),m[2],true);
                RouteProps.Box(root,"Service Console Backing",new Vector3(12.85f,2.45f,-5.65f),new Vector3(5.2f,1.5f,.12f),m[2],false);
                RouteProps.Board(root,"TICKETS & FREIGHT",new Vector3(12.85f,3f,-5.79f),new Vector3(5.2f,.4f,.08f),m,Quaternion.identity);
                var board=WorldButtonArt.Create(root,new Vector3(10.9f,2.25f,-5.75f),"BOARD\nPASSENGERS",m);board.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.BoardPassengers);
                int index=0;
                foreach(var offer in economy.Cargo.Offers.Where(o=>o.origin==node.id))
                {var cap=WorldButtonArt.Create(root,new Vector3(11.85f+index++*.95f,2.25f,-5.75f),offer.title.ToUpperInvariant()+"\n"+offer.reward+" GOLD",m);cap.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.AcceptCargo,offer);}
                var deliver=WorldButtonArt.Create(root,new Vector3(13.85f,2.25f,-5.75f),"DELIVER\nFREIGHT",m);deliver.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.DeliverCargo);
                var ledger=WorldButtonArt.Create(root,new Vector3(14.8f,2.25f,-5.75f),"SERVICE\nLEDGER",m);ledger.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.Manifest);
            }
            // Isolated stations are drop-off only, but still accept physically delivered freight.
            foreach(var node in economy.World.Network.nodes.Where(n=>n.place==RoutePlace.Station||n.place==RoutePlace.Terminum))
            {var root=new GameObject("Arrival Freight Desk").transform;root.SetParent(parent,false);root.position=node.position;RouteProps.Box(root,"Arrival Desk",new Vector3(4.65f,1.42f,-3.65f),new Vector3(2f,.54f,.8f),m[2],true);RouteProps.Box(root,"Arrival Console Backing",new Vector3(4.65f,1.95f,-3.94f),new Vector3(2f,1.05f,.12f),m[2],false);RouteProps.Board(root,"ARRIVALS",new Vector3(4.65f,2.31f,-4.06f),new Vector3(1.8f,.2f,.05f),m,Quaternion.identity);var cap=WorldButtonArt.Create(root,new Vector3(4.2f,1.8f,-4f),"DELIVER\nFREIGHT",m);cap.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.DeliverCargo);if(node.place==RoutePlace.Terminum){var end=WorldButtonArt.Create(root,new Vector3(5.1f,1.8f,-4f),"SETTLE\nTHE DAY",m);end.AddComponent<EconomyServiceControl>().Configure(economy,node.id,ServiceAction.FinishDay);}}
            var car=economy.World.Train.Cars[1];var locker=car.transform.Find("Rail Repair Locker");
            if(locker!=null&&car.transform.Find("Onboard Service Ledger") == null)
            {
                var cap=WorldButtonArt.Create(parent,Vector3.zero,"SERVICE\nLEDGER",m);var plate=cap.transform.parent;plate.name="Onboard Service Ledger";plate.SetParent(car.transform,false);plate.localPosition=new Vector3(-1.24f,2.3f,-1.5f);plate.localRotation=Quaternion.Euler(0f,-90f,0f);cap.AddComponent<EconomyServiceControl>().Configure(economy,"Redfort",ServiceAction.Manifest);
            }
        }
    }
}
