using UnityEngine;
namespace AllAboard.Movement
{
    [ExecuteAlways,RequireComponent(typeof(TextMesh))]
    public sealed class WorldTextDepth:MonoBehaviour
    {
        private static Material generatedMaterial;
        private MaterialPropertyBlock properties;
        private void OnEnable(){Font.textureRebuilt+=FontChanged;Refresh();}
        private void OnDisable(){Font.textureRebuilt-=FontChanged;}
        private void FontChanged(Font font){if(GetComponent<TextMesh>().font==font)Refresh();}
        public void Refresh()
        {
            var text=GetComponent<TextMesh>();var renderer=GetComponent<Renderer>();
            var shader=Shader.Find("AllAboard/World Lettering");if(text.font==null||shader==null)return;
            if(renderer.sharedMaterial==null||renderer.sharedMaterial.shader!=shader)
            {if(generatedMaterial==null)generatedMaterial=new Material(shader){name="World Lettering Runtime"};renderer.sharedMaterial=generatedMaterial;}
            if(properties==null)properties=new MaterialPropertyBlock();properties.SetTexture("_MainTex",text.font.material.mainTexture);renderer.SetPropertyBlock(properties);
        }
    }
}
