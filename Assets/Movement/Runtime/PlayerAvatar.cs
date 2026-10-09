using UnityEngine;
using UnityEngine.Rendering;

namespace AllAboard.Movement
{
    // The local character casts a body shadow without putting eyes inside its own camera.
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private GameObject modelPrefab;
        private Transform model;
        private CharacterController capsule;
        private Transform left, right, externalLeft, externalRight;
        public Transform Model => model;
        private void Awake()
        {
            capsule=GetComponent<CharacterController>();
            var prefab=modelPrefab!=null?modelPrefab:Resources.Load<GameObject>("BrightCharacter");
            if(prefab==null)return;
            model=Instantiate(prefab,transform,false).transform;model.name="Character Body";
            externalLeft=model.Find("Left Hand");externalRight=model.Find("Right Hand");
            foreach(var t in GetComponentsInChildren<Transform>())
            {if(t.name=="Floating Hands"){left=t.Find("Left Hand");right=t.Find("Right Hand");break;}}
            SetLocalVisibility(true);
            var paint=Resources.Load<Material>("CharacterBlue");
            if(paint!=null)
                foreach(var hand in new[]{left,right})
                    if(hand!=null&&hand.TryGetComponent<Renderer>(out var renderer))renderer.sharedMaterial=paint;
        }
        public void SetLocalVisibility(bool local)
        {
            if(model==null)return;
            foreach(var r in model.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode=local?ShadowCastingMode.ShadowsOnly:ShadowCastingMode.On;
        }
        private void LateUpdate()
        {
            if(model==null)return;
            model.localScale=new Vector3(1f,capsule.height/1.8f,1f);
            // Follow the existing interaction hands: welding, controls, ladders and holding still work.
            if(left!=null&&externalLeft!=null){externalLeft.position=left.position;externalLeft.rotation=left.rotation;}
            if(right!=null&&externalRight!=null){externalRight.position=right.position;externalRight.rotation=right.rotation;}
        }
    }
}
