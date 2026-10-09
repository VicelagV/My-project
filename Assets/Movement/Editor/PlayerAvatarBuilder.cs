using System.IO;
using AllAboard.Movement;
using AllAboard.Train;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllAboard.Editor
{
    public static class PlayerAvatarBuilder
    {
        private const string Root="Assets/Movement/Resources/";
        [MenuItem("All Aboard/Build Bright Player Character")]
        public static void Apply()=>Build(true);
        public static void RefineModels()=>Build(false);
        private static void Build(bool updateScenes)
        {
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            var blue=Paint("CharacterBlue",new Color(.04f,.55f,1f));
            var yellow=Paint("CharacterYellow",new Color(1f,.8f,.025f));
            var green=Paint("CharacterGreen",new Color(.25f,.9f,.12f));
            var white=Paint("CharacterWhite",new Color(.98f,.98f,.92f));
            var black=Paint("CharacterPupil",new Color(.035f,.025f,.05f));
            var nose=Paint("CharacterNose",new Color(1f,.38f,.08f));
            var mesh=Pear();string meshPath=Root+"CharacterBody.asset";
            var oldMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(oldMesh==null)AssetDatabase.CreateAsset(mesh,meshPath);
            else{EditorUtility.CopySerialized(mesh,oldMesh);Object.DestroyImmediate(mesh);mesh=oldMesh;}
            foreach(var paint in new[]{blue,yellow,green})
            {
                var root=new GameObject("Bright Character");
                var body=new GameObject("Rounded Body");body.transform.SetParent(root.transform,false);
                body.AddComponent<MeshFilter>().sharedMesh=mesh;body.AddComponent<MeshRenderer>().sharedMaterial=paint;
                Ball(root.transform,"Round Head",new Vector3(0f,1.4f,0f),new Vector3(.7f,.68f,.64f),paint);
                for(int side=-1;side<=1;side+=2)
                {
                    Ball(root.transform,side<0?"Left Eye":"Right Eye",new Vector3(side*.145f,1.43f,.255f),new Vector3(.285f,.3f,.14f),white);
                    Ball(root.transform,side<0?"Left Pupil":"Right Pupil",new Vector3(side*.145f,1.43f,.323f),new Vector3(.16f,.18f,.045f),black);
                    Ball(root.transform,"Eye Catchlight",new Vector3(side*.145f-.028f,1.477f,.349f),new Vector3(.045f,.05f,.018f),white);
                    Ball(root.transform,"Eye Small Catchlight",new Vector3(side*.145f+.024f,1.402f,.349f),new Vector3(.018f,.021f,.012f),white);
                    var hand=new GameObject(side<0?"Left Hand":"Right Hand").transform;hand.SetParent(root.transform,false);hand.localPosition=new Vector3(side*.49f,.98f,.035f);
                    Ball(hand,"Palm",Vector3.zero,new Vector3(.23f,.19f,.15f),paint);
                    for(int i=0;i<3;i++)
                    {var finger=Ball(hand,"Finger "+(i+1),new Vector3(side*(.09f+i*.035f),.075f-i*.055f,0f),new Vector3(.11f,.17f,.12f),paint);finger.localRotation=Quaternion.Euler(0f,0f,-side*(28f+i*23f));}
                    Ball(hand,"Thumb",new Vector3(-side*.075f,.045f,.025f),new Vector3(.1f,.14f,.12f),paint);
                }
                Ball(root.transform,"Little Nose",new Vector3(0f,1.3f,.313f),new Vector3(.115f,.085f,.115f),nose);
                string name=paint==blue?"BrightCharacter":paint==yellow?"BrightCharacterYellow":"BrightCharacterGreen";
                PrefabUtility.SaveAsPrefabAsset(root,Root+name+".prefab");Object.DestroyImmediate(root);
            }
            if(AssetDatabase.LoadAssetAtPath<EconomyConfig>(Root+"EconomyDefaults.asset")==null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EconomyConfig>(),Root+"EconomyDefaults.asset");
            if(!updateScenes){AssetDatabase.SaveAssets();return;}
            const string playerPath="Assets/Movement/Prefabs/PrototypePlayer.prefab";
            var player=PrefabUtility.LoadPrefabContents(playerPath);
            if(player.GetComponent<PlayerAvatar>()==null)player.AddComponent<PlayerAvatar>();
            PrefabUtility.SaveAsPrefabAsset(player,playerPath);PrefabUtility.UnloadPrefabContents(player);
            foreach(string path in Directory.GetFiles("Assets/Movement/Scenes","*.unity"))
            {
                if(path.EndsWith("CharacterPreview.unity"))continue;
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                foreach(var motor in Object.FindObjectsByType<FirstPersonMotor>(FindObjectsInactive.Include))
                    if(motor.GetComponent<PlayerAvatar>()==null)motor.gameObject.AddComponent<PlayerAvatar>();
                EditorSceneManager.SaveScene(scene);
            }
            BuildPreview();AssetDatabase.SaveAssets();Debug.Log("BRIGHT CHARACTER build complete");
        }
        private static Material Paint(string name,Color color)
        {
            string path=Root+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.color=color;mat.SetFloat("_Smoothness",.22f);EditorUtility.SetDirty(mat);return mat;
        }
        private static Transform Ball(Transform parent,string name,Vector3 at,Vector3 size,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);obj.name=name;obj.transform.SetParent(parent,false);
            obj.transform.localPosition=at;obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(obj.GetComponent<Collider>());return obj.transform;
        }
        private static Mesh Pear()
        {
            const int rings=24,sides=40;var vertices=new Vector3[(rings-1)*sides+2];int top=vertices.Length-1;
            vertices[0]=new Vector3(0f,.035f,0f);vertices[top]=new Vector3(0f,1.24f,0f);
            for(int r=1;r<rings;r++)
            {
                float t=r/(float)rings,angle=t*Mathf.PI;
                float radius=Mathf.Sin(angle)*Mathf.Lerp(.39f,.235f,t);
                for(int s=0;s<sides;s++){float a=s*Mathf.PI*2f/sides;vertices[1+(r-1)*sides+s]=new Vector3(Mathf.Cos(a)*radius,.035f+(1f-Mathf.Cos(angle))*.6025f,Mathf.Sin(a)*radius*.82f);}
            }
            var triangles=new System.Collections.Generic.List<int>();
            for(int s=0;s<sides;s++){int n=(s+1)%sides;triangles.AddRange(new[]{0,1+s,1+n});}
            for(int r=0;r<rings-2;r++)for(int s=0;s<sides;s++)
            {int a=1+r*sides+s,b=1+r*sides+(s+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,c,b,b,c,d});}
            for(int s=0;s<sides;s++){int a=1+(rings-2)*sides+s,b=1+(rings-2)*sides+(s+1)%sides;triangles.AddRange(new[]{a,top,b});}
            var mesh=new Mesh{name="Closed Rounded Character Body",vertices=vertices,triangles=triangles.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private static void BuildPreview()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Preview Camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            camera.transform.position=new Vector3(3.1f,2.4f,5.7f);camera.transform.LookAt(new Vector3(0f,.9f,0f));camera.fieldOfView=38f;
            camera.backgroundColor=new Color(.075f,.1f,.16f);camera.clearFlags=CameraClearFlags.SolidColor;
            var sun=new GameObject("Soft Key Light").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=2f;sun.transform.rotation=Quaternion.Euler(38f,-35f,0f);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientLight=new Color(.55f,.6f,.7f);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Preview Floor";floor.transform.position=new Vector3(0f,-.16f,0f);floor.transform.localScale=new Vector3(30f,.3f,30f);floor.GetComponent<Renderer>().sharedMaterial=Paint("CharacterStage",new Color(.19f,.23f,.3f));
            var names=new[]{"BrightCharacterYellow","BrightCharacter","BrightCharacterGreen"};
            for(int i=0;i<3;i++)
            {var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+names[i]+".prefab"));obj.transform.position=new Vector3((i-1)*1.35f,0f,0f);obj.transform.rotation=Quaternion.Euler(0f,i==1?18f:0f,0f);}
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Movement/Scenes/CharacterPreview.unity");
        }
    }
}
