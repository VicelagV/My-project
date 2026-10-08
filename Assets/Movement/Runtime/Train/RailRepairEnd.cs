using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailRepairEnd:PrototypeInteractable
    {
        private RailDamageSection section;private int side;private bool farEnd;
        public int Side=>side;
        public bool IsFarEnd=>farEnd;
        public override string Prompt=>section.CanSaw(side)?"E: mount rail saw | two cuts, one per rail":farEnd?"E: connect previewed steel (up to 5 m)":"E: align steel connector";
        public void Configure(RailDamageSection owner,int railSide,bool end){section=owner;side=railSide;farEnd=end;}
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||actor.HeldItem==null)return false;
            var tool=actor.HeldItem.GetComponent<RailTool>();if(tool==null)return false;
            if(section.CanSaw(side))
            {
                if(tool.Kind!=RailToolKind.Saw)return false;
                var saw=tool.GetComponent<RailSawControl>();if(saw==null)saw=tool.gameObject.AddComponent<RailSawControl>();
                actor.ReleaseCharged(0f);tool.transform.SetParent(null,true);tool.GetComponent<Rigidbody>().isKinematic=true;
                tool.transform.position=section.Point(side,0f)+Vector3.up*.29f;tool.transform.rotation=Quaternion.LookRotation(section.Point(side,1f)-section.Point(side,0f));
                saw.Mount(section,side);return true;
            }
            if(tool.Kind!=RailToolKind.Connector)return false;
            if(!farEnd){tool.Align(section,side);return true;}
            if(!tool.IsAligned(section,side))return false;
            var railway=section.GetComponentInParent<ProceduralRailway>();
            if(railway==null||!section.Replace(side,railway.Train.GetComponent<RailwayInventory>()))return false;
            tool.ClearAlignment();return true;
        }
    }
}
