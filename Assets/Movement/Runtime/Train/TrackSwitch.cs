using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrackSwitch : PrototypeInteractable
    {
        private ProceduralRailway railway;
        private string node;
        private Transform lever,grip;
        private TextMesh label;
        public string NodeId=>node;
        public void Configure(ProceduralRailway world,string id,Transform arm,Transform hand,TextMesh title)
        {railway=world;node=id;lever=arm;grip=hand;label=title;lever.localRotation=Quaternion.Euler(0f,0f,-35f);}
        public override string Prompt
        {
            get
            {
                if(railway==null)return "Track switch";
                if(Mathf.Abs(railway.Train.Speed)>0.1f)return "Stop the train before changing the switch";
                if(!railway.CanChange(node))return "Train has passed this switch";
                var exits=railway.Network.Outgoing(node).ToArray();return "E: select "+exits[(railway.SelectedIndex(node)+1)%exits.Length].title;
            }
        }
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||actor.IsHolding||actor.GetComponent<FirstPersonMotor>().IsSeated)return false;
            int count=railway.Network.Outgoing(node).Count();
            if(!railway.Choose(node,(railway.SelectedIndex(node)+1)%count))return false;
            actor.GetComponent<FirstPersonView>().ReachForButton(grip);return true;
        }
        private void Update()
        {
            if(railway==null)return;
            int count=railway.Network.Outgoing(node).Count();float angle=Mathf.Lerp(-35f,35f,railway.SelectedIndex(node)/(float)(count-1));
            lever.localRotation=Quaternion.Slerp(lever.localRotation,Quaternion.Euler(0f,0f,angle),1f-Mathf.Exp(-12f*Time.deltaTime));
            label.text=railway.SelectedTitle(node);
        }
    }
}
