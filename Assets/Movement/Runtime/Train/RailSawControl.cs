using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailSawControl:PrototypeInteractable
    {
        private RailDamageSection section;private int side;private Vector3 rest;private float stroke;private int lastDirection;private int strokes;
        public bool Mounted=>section!=null;
        public bool Finished {get;private set;}
        public float Progress=>Mathf.Clamp01(strokes/8f);
        public override string Prompt=>Finished?"E: take saw":Mounted?"Hold M1: saw forward/back | E: take saw":"E: take saw";
        public void Mount(RailDamageSection owner,int railSide){section=owner;side=railSide;rest=transform.position;strokes=0;stroke=0;lastDirection=0;Finished=false;}
        public bool Drag(float mouseY)
        {
            if(!Mounted||Finished)return false;
            if(Mathf.Abs(mouseY)<.01f)return true;
            int direction=mouseY>0f?1:-1;
            if(lastDirection==direction)return true;
            if(Mathf.Sign(stroke)!=direction)stroke=0f;
            stroke+=mouseY;
            transform.position=rest+transform.forward*Mathf.Clamp(stroke/80f,-.14f,.14f);
            if(Mathf.Abs(stroke)>=24f){strokes++;lastDirection=direction;stroke=0f;if(strokes>=8){Finished=section.FinishCut(side);transform.position=rest;}}
            return true;
        }
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||actor.IsHolding||actor.IsUsingHandControl)return false;
            if(!actor.TryPickup(GetComponent<PickupObject>()))return false;
            section=null;Finished=false;return true;
        }
    }
}
