using UnityEngine;
namespace AllAboard.Train
{
 public sealed class CombatEffects:MonoBehaviour
 {
  private float lifetime;private Material paint;private static readonly AudioClip[] sounds=new AudioClip[5];
  private static Material Bright(){var m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));m.SetColor("_BaseColor",new Color(1,.78f,.25f));return m;}
  public static void Trace(UnityEngine.SceneManagement.Scene scene,Vector3 start,Vector3 end){var obj=new GameObject("Shot Trace");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);var fx=obj.AddComponent<CombatEffects>();fx.paint=Bright();fx.lifetime=.065f;var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=fx.paint;line.positionCount=2;line.SetPositions(new[]{start,end});line.startWidth=.012f;line.endWidth=.003f;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
  public static void Impact(UnityEngine.SceneManagement.Scene scene,Vector3 point){var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);obj.name="Impact Puff";UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);obj.transform.position=point;obj.transform.localScale=Vector3.one*.1f;var collider=obj.GetComponent<Collider>();collider.enabled=false;Destroy(collider);var fx=obj.AddComponent<CombatEffects>();fx.paint=Bright();fx.lifetime=.12f;obj.GetComponent<Renderer>().sharedMaterial=fx.paint;}
  private void Update(){lifetime-=Time.deltaTime;if(lifetime<=0)Destroy(gameObject);}
  private void OnDestroy(){if(paint!=null)Destroy(paint);}
  public static AudioClip Sound(WeaponKind kind)
  {
   int index=(int)kind;if(sounds[index]!=null)return sounds[index];int samples=kind<=WeaponKind.Shotgun?12000:6500;var data=new float[samples];var rng=new System.Random(717+index);
   for(int i=0;i<samples;i++){float t=i/44100f;float envelope=Mathf.Exp(-t*(index<=2?28:18));float noise=(float)(rng.NextDouble()*2-1);data[i]=(noise*(index<=2?.55f:.12f)+Mathf.Sin(t*(index<=2?130:450)*Mathf.PI*2)*.28f)*envelope;}
   var clip=AudioClip.Create("Prototype "+kind,samples,1,44100,false);clip.SetData(data,0);sounds[index]=clip;return clip;
  }
 }
}
