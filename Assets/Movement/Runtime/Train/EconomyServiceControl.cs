using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public enum ServiceAction { Manifest,BoardPassengers,AcceptCargo,DeliverCargo,FinishDay }
    public sealed class EconomyServiceControl:PrototypeInteractable
    {
        private TrainDayEconomy economy;private string station;private CargoOffer offer;private ServiceAction action;
        public override bool AllowedWhileSeated=>action==ServiceAction.Manifest;
        public override string Prompt=>action==ServiceAction.Manifest?"E / M1: service ledger (Tab)":action==ServiceAction.BoardPassengers?"E / M1: board waiting passengers":action==ServiceAction.AcceptCargo?"E / M1: "+offer.title+" to "+economy.DestinationTitle(offer.destination)+" | "+offer.reward+" Gold":action==ServiceAction.FinishDay?"E / M1: settle the day (unfinished freight incurs penalties)":"E / M1: deliver contract crate here";
        public void Configure(TrainDayEconomy owner,string node,ServiceAction kind,CargoOffer contract=null){economy=owner;station=node;action=kind;offer=contract;}
        public override bool Interact(PlayerInteraction actor)
        {
            var button=GetComponent<TrainButton>();if(button!=null&&button.IsPressed)return false;
            bool success=false;
            if(action==ServiceAction.Manifest){economy.GetComponent<EconomyManifest>().Open();success=true;}
            else if(action==ServiceAction.BoardPassengers)success=economy.BoardWaiting(station)>0;
            else if(action==ServiceAction.AcceptCargo)success=economy.Cargo.Accept(offer);
            else if(action==ServiceAction.FinishDay){if(economy.ReadyToSettle){economy.Finish(true);success=true;}}
            else
            {
                var cargo=actor!=null&&actor.HeldItem!=null?actor.HeldItem.GetComponent<CargoCrate>():null;
                if(cargo!=null&&cargo.Contract.destination==station)success=economy.Cargo.Deliver(cargo.Contract,actor,transform.position);
                else foreach(var active in economy.Cargo.Offers)if(active.destination==station&&economy.Cargo.Deliver(active,actor,transform.position)){success=true;break;}
            }
            if(success)button?.Press(actor);return success;
        }
    }
}
