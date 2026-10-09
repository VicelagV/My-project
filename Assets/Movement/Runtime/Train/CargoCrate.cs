using UnityEngine;
namespace AllAboard.Train
{
    public sealed class CargoCrate:MonoBehaviour
    {
        public CargoOffer Contract {get;private set;}
        public float Health {get;private set;}=100f;
        private CargoService service;
        private Renderer seal;
        public void Configure(CargoService owner,CargoOffer offer,Renderer sealRenderer){service=owner;Contract=offer;seal=sealRenderer;}
        public void Damage(float amount)
        {if(Contract==null||Contract.Resolved)return;Health=Mathf.Clamp(Health-Mathf.Max(0f,amount),0f,100f);if(seal!=null){var properties=new MaterialPropertyBlock();properties.SetColor("_BaseColor",Color.Lerp(Color.red,Color.green,Health/100f));properties.SetColor("_Color",Color.Lerp(Color.red,Color.green,Health/100f));seal.SetPropertyBlock(properties);}if(Health<=0f)service.Fail(Contract);}
        private void OnCollisionEnter(Collision collision)
        {if(service==null)return;float excess=collision.relativeVelocity.magnitude-service.Economy.Wallet.Config.cargoDamageSpeed;if(excess>0f)Damage(excess*service.Economy.Wallet.Config.cargoDamageScale);}
        private void OnDestroy(){if(service!=null&&Contract!=null&&!Contract.Resolved)service.Fail(Contract);}
    }
}
