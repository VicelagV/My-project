using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class WorldArtAssets:MonoBehaviour
    {
        public static void BatchBuilding(Transform building)
        {
            if(!Application.isPlaying)return;
            var parts=building.GetComponentsInChildren<MeshFilter>().Where(x=>x.GetComponent<Renderer>()!=null&&x.GetComponent<Renderer>().enabled).Select(x=>x.gameObject).ToArray();
            if(parts.Length>1)StaticBatchingUtility.Combine(parts,building.gameObject);
        }
        private readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        public Material Paint(Material source,string name,Color colour)
        {if(materials.TryGetValue(name,out var existing))return existing;var paint=new Material(source){name=name};paint.SetColor("_BaseColor",colour);materials.Add(name,paint);return paint;}
        private void OnDestroy(){foreach(var material in materials.Values)if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}}
        public static int Hash(string value){unchecked{int hash=17;foreach(char c in value)hash=hash*31+c;return hash;}}
    }
}
