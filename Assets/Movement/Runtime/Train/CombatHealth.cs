using UnityEngine;
namespace AllAboard.Train
{
 public sealed class CombatHealth:MonoBehaviour
 {
  public float Health {get;private set;}=100f;public float Maximum {get;private set;}=100f;public bool IsPlayer {get;private set;}public bool Down=>Health<=0;public event System.Action Defeated;
  public void Configure(float maximum,bool player=false){Maximum=Mathf.Max(1,maximum);Health=Maximum;IsPlayer=player;}
  public bool Damage(float amount,bool fromPlayer=false){if(Down||amount<=0||fromPlayer&&(IsPlayer||GetComponent<PassengerJourney>()!=null))return false;Health=Mathf.Max(0,Health-amount);if(Down)Defeated?.Invoke();return true;}
  public bool Heal(float amount){if(amount<=0||Health>=Maximum)return false;Health=Mathf.Min(Maximum,Health+amount);return true;}
 }
}
