using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class StoredRailTool:MonoBehaviour
    {
        private void LateUpdate(){if(GetComponent<PickupObject>().IsHeld){transform.SetParent(null,true);Destroy(this);}}
    }
}
