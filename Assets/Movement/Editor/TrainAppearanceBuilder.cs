using System.Collections.Generic;
using System.IO;
using System.Linq;
using AllAboard.Movement;
using AllAboard.Train;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace AllAboard.Editor
{
    public static class TrainAppearanceBuilder
    {
        private const string Owned = "Train Visual Details", CabOwned = "Cab Visual Details";
        private static Material ink, navy, brass, red, steel, wood, leather, ivory, glass, fire, smoke;
        private static Mesh ring, thinRing, cylinder, button, rounded, roof, coal, panelBox;
        [MenuItem("All Aboard/Apply Train Visual Pass")]
        public static void Apply()
        {
            Initialize();
            foreach (string name in new[] { "Locomotive", "Passenger", "CoalWagon" })
            {
                string file = "Assets/Movement/Prefabs/Train" + name + ".prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(file);
                try { DecorateCar(root, root.GetComponent<TrainCar>().Kind); PrefabUtility.SaveAsPrefabAsset(root, file); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string coalFile = "Assets/Movement/Prefabs/Coal.prefab";
            var lump = PrefabUtility.LoadPrefabContents(coalFile);
            try { foreach (MeshFilter mesh in lump.GetComponentsInChildren<MeshFilter>()) mesh.sharedMesh = coal; PrefabUtility.SaveAsPrefabAsset(lump, coalFile); }
            finally { PrefabUtility.UnloadPrefabContents(lump); }
            var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach(string scenePath in new[]{TrainPrototypeBuilder.ScenePath,ProceduralRailwayBuilder.ScenePath})
            {
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)==null)continue;
                var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
                bool alreadyOpen=scene.IsValid()&&scene.isLoaded;
                if(!alreadyOpen)scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);
                var train=scene.GetRootGameObjects().Select(x=>x.GetComponent<TrainController>()).First(x=>x!=null);
                foreach(TrainCar car in train.Cars)DecorateCar(car.gameObject,car.Kind);
                DressCab(train.Cars[0]);
                var ambience=train.GetComponent<TrainAmbience>();if(ambience==null)ambience=train.gameObject.AddComponent<TrainAmbience>();
                ambience.Configure(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/CountrysideLoop.wav"));EditorUtility.SetDirty(ambience);
                var drive=train.GetComponent<TrainDriveAudio>();
                drive.Configure(train,AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainSteamLoop.wav"),AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainRollingLoop.wav"),AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainChuffLoop.wav"));EditorUtility.SetDirty(drive);
                foreach(TrainCar car in train.Cars)
                    foreach(PrototypeInteractable target in car.GetComponentsInChildren<PrototypeInteractable>())
                    {
                        var highlight=target.GetComponent<ControlHighlight>();if(highlight==null)highlight=target.gameObject.AddComponent<ControlHighlight>();
                        Renderer[] geometry=target is TrainLadder?car.GetComponentsInChildren<Renderer>().Where(x=>x.enabled&&x.name.Contains("Ladder")).ToArray():target.GetComponentsInChildren<Renderer>().Where(x=>x.enabled&&x.name!="Black Train Outline"&&x.GetComponent<MeshFilter>()!=null).ToArray();
                        highlight.Configure(AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/HoverOutline.mat"),geometry);
                    }
                var oldTrack=scene.GetRootGameObjects().FirstOrDefault(x=>x.name=="Primitive Track");
                if(oldTrack!=null)
                {
                    foreach(Transform child in oldTrack.transform.Cast<Transform>().Where(x=>x.name!="Track End Buffer"&&x.name!="Generated Test Track").ToArray())Object.DestroyImmediate(child.gameObject);
                    var visual=oldTrack.GetComponent<RailTrackVisual>();if(visual==null)visual=oldTrack.AddComponent<RailTrackVisual>();
                    visual.Configure(train.Path,AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/RouteBallast.mat"),wood,steel);
                }
                EditorSceneManager.SaveScene(scene);
                if(!alreadyOpen)EditorSceneManager.CloseScene(scene,true);
            }
            if(previous.IsValid()&&previous.isLoaded)UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            AssetDatabase.SaveAssets(); Debug.Log("Applied train meshes, rolling gear, coach/tender details and cab craftsmanship.");
        }
        private static void Initialize()
        {
            Directory.CreateDirectory("Assets/Movement/Meshes"); Directory.CreateDirectory("Assets/Movement/Textures"); AssetDatabase.Refresh();
            string inkPath="Assets/Movement/Materials/TrainInk.mat";
            ink=AssetDatabase.LoadAssetAtPath<Material>(inkPath);
            if(ink==null){ink=new Material(Shader.Find("AllAboard/Hand Outline"));ink.SetColor("_OutlineColor",Color.black);ink.SetFloat("_OutlineWidth",0.007f);AssetDatabase.CreateAsset(ink,inkPath);}
            navy = Mat("TrainNavy", new Color(0.045f, 0.085f, 0.13f), 0.3f); brass = Mat("TrainBrass", new Color(0.78f, 0.57f, 0.22f), 0.72f);
            red = Mat("TrainOxideRed", new Color(0.55f, 0.10f, 0.12f), 0.25f); steel = Mat("TrainMachinedSteel", new Color(0.23f, 0.27f, 0.31f), 0.75f);
            wood = Mat("TrainWalnut", new Color(0.32f, 0.19f, 0.105f)); leather = Mat("TrainUpholstery", new Color(0.38f, 0.11f, 0.15f)); ivory = Mat("TrainIvory", new Color(0.88f, 0.83f, 0.65f));
            glass = Mat("TrainWindowGlass", new Color(0.55f, 0.75f, 0.82f, 0.12f), 0f, true);
            glass.SetFloat("_Smoothness",0.02f);glass.SetFloat("_SpecularHighlights",0f);glass.SetFloat("_EnvironmentReflections",0f);glass.SetFloat("_BlendModePreserveSpecular",0f);
            glass.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");glass.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");EditorUtility.SetDirty(glass);
            var hands=AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/Hands.mat");
            if(hands!=null){Color paint=hands.GetColor("_BaseColor");hands.shader=Shader.Find("AllAboard/Stylized Train");hands.SetColor("_BaseColor",paint);hands.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(hands);}
            fire = Mat("TrainFireGlow", new Color(0.9f, 0.23f, 0.04f)); fire.EnableKeyword("_EMISSION"); fire.SetColor("_EmissionColor", new Color(2.8f, 0.35f, 0.015f)); EditorUtility.SetDirty(fire);
            ring = MeshAsset("TrainRoundRing", RingMesh(0.44f, 0.06f)); thinRing = MeshAsset("TrainThinRing", RingMesh(0.488f, 0.012f));
            cylinder = MeshAsset("TrainCylinderY", CylinderMesh(false)); button = MeshAsset("TrainButtonZ", CylinderMesh(true)); rounded = MeshAsset("TrainBevelledBox", RoundedBox());
            panelBox=MeshAsset("TrainPanelBox", PanelBox());
            roof = MeshAsset("TrainCoachRoof", RoofMesh(false)); coal = MeshAsset("TrainCoalRock", CoalMesh()); smoke = SmokeMaterial();
        }
        private static Transform Root(Transform parent, string name, bool undoScale = false)
        {
            foreach(var old in parent.Cast<Transform>().Where(x=>x.name==name).ToArray())Object.DestroyImmediate(old.gameObject);
            var root = new GameObject(name).transform; root.SetParent(parent, false);
            if (undoScale) root.localScale = new Vector3(1f / parent.localScale.x, 1f / parent.localScale.y, 1f / parent.localScale.z);
            return root;
        }
        private static GameObject Part(Transform parent, string name, Vector3 position, Vector3 size, Material material, Mesh mesh = null, Quaternion? rotation = null)
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = size; obj.transform.localRotation = rotation ?? Quaternion.identity;
            obj.GetComponent<MeshFilter>().sharedMesh = mesh != null ? mesh : rounded; obj.GetComponent<Renderer>().sharedMaterial = material;
            if(name=="Steam Boiler"||name=="Rounded Roof"||name=="Coach Lower Body"||name=="Tender Low Side"||name=="Box Frame"||name=="Door Lower Panel")
            {
                var border=new GameObject("Black Train Outline",typeof(MeshFilter),typeof(MeshRenderer));border.transform.SetParent(obj.transform,false);
                border.GetComponent<MeshFilter>().sharedMesh=obj.GetComponent<MeshFilter>().sharedMesh;
                border.GetComponent<MeshRenderer>().sharedMaterial=ink;border.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
            return obj;
        }
        private static void Colour(Transform obj, Material material, Mesh shape = null)
        { if (obj == null) return; var renderer = obj.GetComponent<Renderer>(); if (renderer != null) renderer.sharedMaterial = material; if (shape != null && obj.GetComponent<MeshFilter>() != null) obj.GetComponent<MeshFilter>().sharedMesh = shape; }
        public static void DecorateCar(GameObject car, CarKind kind)
        {
            if (navy == null) Initialize();
            Transform body = car.transform, details = Root(body, Owned);
            foreach (Transform child in body)
                if (child.name.Contains("Wall") || child.name.StartsWith("End Portal") || child.name.Contains("Roof") || child.name == "Door Header" || child.name == "Visual Wheel" || child.name.StartsWith("Low "))
                    if (child.GetComponent<Renderer>() != null) child.GetComponent<Renderer>().enabled = false;
            Colour(body.Find("Walkable Floor"), kind == CarKind.Coal ? AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/Coal.mat") : wood);
            Colour(body.Find("Rear Walkway"), steel); Colour(body.Find("Rear Roof Walkway"), steel);
            if(body.Find("Rear Roof Walkway") != null) body.Find("Rear Roof Walkway").GetComponent<Renderer>().enabled=true;
            Part(details, "Box Frame", new Vector3(0f, 0.88f, 0f), new Vector3(3.75f, 0.30f, 8.7f), navy);
            foreach (float x in new[] { -1.75f, 1.75f }) Part(details, "Chassis Rail", new Vector3(x, 0.63f, 0f), new Vector3(0.12f, 0.30f, 8.6f), steel);
            if (kind == CarKind.Coal)
            {
                foreach (float x in new[] { -1.9f, 1.9f })
                {
                    Part(details, "Tender Low Side", new Vector3(x, 1.375f, 0f), new Vector3(0.15f, 0.45f, 8.8f), navy);
                    Part(details, "Tender Brass Lip", new Vector3(x, 1.61f, 0f), new Vector3(0.17f, 0.035f, 8.8f), brass);
                    foreach (float z in new[] { -3.8f, -2f, 0f, 2f, 3.8f }) Part(details, "Tender Rib", new Vector3(x * 1.02f, 1.35f, z), new Vector3(0.07f, 0.45f, 0.07f), steel);
                }
                foreach (float end in new[] { -4.4f, 4.4f }) foreach (float x in new[] { -1.3f, 1.3f }) Part(details, "Tender End Guard", new Vector3(x, 1.375f, end), new Vector3(1.2f, 0.45f, 0.15f), navy);
                Part(details, "Water Filler Cap", new Vector3(-1.48f, 1.63f, -3.8f), new Vector3(0.32f, 0.07f, 0.32f), brass, cylinder);
            }
            else
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    foreach (var span in side < 0f ? new[] { new Vector2(0f, 8.8f) } : new[] { new Vector2(-1.95f, 4.9f), new Vector2(3.45f, 1.9f) })
                    {
                        Part(details, "Coach Lower Body", new Vector3(side * 1.9f, 1.69f, span.x), new Vector3(0.15f, 1.08f, span.y), navy, panelBox);
                        Part(details, "Coach Upper Body", new Vector3(side * 1.9f, kind == CarKind.Locomotive ? 3.795f : 3.60f, span.x), new Vector3(0.15f, kind == CarKind.Locomotive ? 0.15f : 0.54f, span.y), kind == CarKind.Locomotive ? ivory : navy, panelBox);
                        Part(details, "Gold Waist Stripe", new Vector3(side * 1.984f, 2.17f, span.x), new Vector3(0.012f, 0.065f, span.y), brass);
                    }
                    var centers = side < 0f ? new[] { -3.45f, -1.72f, 0f, 1.72f, 3.45f } : new[] { -3.45f, -1.72f, 0f, 3.45f };
                    foreach (float z in centers) { float width = side > 0f && z == 0f ? 0.70f : 1.45f; Window(details, side, z - (side > 0f && z == 0f ? 0.20f : 0f), width, kind == CarKind.Locomotive); }
                    FillWindowGaps(details,side,kind==CarKind.Locomotive,centers);
                    if(side > 0f) Part(details, "Door Top Trim", new Vector3(side * 1.92f, 3.44f, 1.5f), new Vector3(0.20f, 0.1f, 2.05f), brass);
                }
                foreach (float end in new[] { -4.4f, 4.4f })
                {
                    if(kind == CarKind.Locomotive && end > 0f) continue;
                    foreach (float x in new[] { -1.3f, 1.3f }) Part(details, "End Vestibule", new Vector3(x, 2.5f, end), new Vector3(1.2f, 2.8f, 0.15f), navy);
                    Part(details, "End Lintel", new Vector3(0f, 3.65f, end), new Vector3(1.4f, 0.5f, 0.15f), navy);
                    Part(details, "Vestibule Brass Arch", new Vector3(0f, 3.42f, end * 1.02f), new Vector3(1.45f, 0.06f, 0.05f), brass);
                }
                Mesh roofShape = kind == CarKind.Locomotive ? MeshAsset("TrainCabRoofHatch", RoofMesh(true)) : roof;
                Part(details, "Rounded Roof", new Vector3(0f, 3.8f, 0f), new Vector3(4.1f, 0.6f, 9f), kind == CarKind.Locomotive ? red : navy, roofShape);
                Part(details, "Roof Walking Strip", new Vector3(-0.15f, 4.085f, 0f), new Vector3(1.1f, 0.03f, 8.8f), steel);
                foreach (float x in new[] { -1.9f, 1.9f }) Part(details, "Roof Brass Eave", new Vector3(x, 3.84f, 0f), new Vector3(0.06f, 0.06f, 9f), brass);
                for (float x = -1.7f; x < 1.8f; x += 0.24f) Part(details, "Floor Plank Seam", new Vector3(x, 1.153f, 0f), new Vector3(0.012f, 0.004f, 8.8f), navy);
                var door = car.GetComponentInChildren<TrainDoor>(); if (door != null) door.ConfigureTravel(new Vector3(0.18f, 0f, -2.1f));
                var panel = body.Find("Sliding Door"); if (panel != null)
                {
                    panel.GetComponent<Renderer>().enabled = false; var doorDetails = Root(panel, "Door Visual Details", true);
                    Part(doorDetails, "Door Lower Panel", new Vector3(0f, -0.47f, 0f), new Vector3(0.17f, 1.27f, 2f), wood);
                    foreach (float z in new[] { -0.92f, 0.92f }) Part(doorDetails, "Door Frame", new Vector3(0f, 0.47f, z), new Vector3(0.18f, 1.1f, 0.16f), brass);
                    Part(doorDetails, "Door Frame Top", new Vector3(0f, 1.03f, 0f), new Vector3(0.18f, 0.14f, 2f), brass);
                    Part(doorDetails, "Door Window", new Vector3(0f, 0.49f, 0f), new Vector3(0.018f, 1.02f, 1.64f), glass);
                    foreach (float side in new[] { -1f, 1f }) Part(doorDetails, "Door Handle", new Vector3(side * 0.13f, 0f, 0.65f), new Vector3(0.05f, 0.30f, 0.05f), brass, cylinder);
                }
                if (kind == CarKind.Passenger)
                {
                    foreach(Transform child in body.Cast<Transform>().Where(x=>x.name.StartsWith("Placeholder Seat")).ToArray())Object.DestroyImmediate(child.gameObject);
                    foreach(float z in new[]{-3.05f,-1.70f,-0.35f,3.05f})
                        foreach(float side in new[]{-1f,1f})PassengerSeats(details,side,z);
                    Part(details,"Centre Aisle Runner",new Vector3(0f,1.158f,0f),new Vector3(0.88f,0.012f,8.5f),navy,panelBox);
                    foreach(float side in new[]{-1f,1f})
                    {
                        foreach(var span in side<0f?new[]{new Vector2(0f,8.6f)}:new[]{new Vector2(-1.95f,4.9f),new Vector2(3.45f,1.9f)})
                            Part(details,"Interior Window Rail",new Vector3(side*1.77f,2.18f,span.x),new Vector3(0.045f,0.08f,span.y),wood);
                        Part(details,"Overhead Luggage Shelf",new Vector3(side*1.48f,3.42f,0f),new Vector3(0.68f,0.04f,8.5f),brass);
                        foreach(float z in new[]{-3f,0f,3f})Part(details,"Luggage Shelf Bracket",new Vector3(side*1.74f,3.29f,z),new Vector3(0.05f,0.25f,0.05f),brass);
                    }
                    foreach (float z in new[] { -2.5f, 0f, 2.5f })
                    {
                        Part(details, "Cabin Lamp", new Vector3(0f, 3.70f, z), new Vector3(0.22f, 0.10f, 0.22f), ivory, cylinder);
                        var light = new GameObject("Warm Cabin Light").AddComponent<Light>(); light.transform.SetParent(details, false); light.transform.localPosition = new Vector3(0f, 3.6f, z);
                        light.type = LightType.Point; light.color = new Color(1f, 0.78f, 0.5f); light.intensity = 0.65f; light.range = 3f;
                    }

                }
            }
            foreach (float z in new[] { -2.8f, 2.8f }) foreach (float side in new[] { -1f, 1f })
            {
                Wheel(details, side * 1.98f, z, 0.4f, navy);
                Part(details, "Axle Box", new Vector3(side * 1.81f, 0.45f, z), new Vector3(0.28f, 0.27f, 0.38f), steel);
                for (int spring = 0; spring < 3; spring++) Part(details, "Leaf Spring", new Vector3(side * 1.86f, 0.65f + spring * 0.04f, z), new Vector3(0.12f, 0.025f, 0.7f - spring * 0.14f), steel);
            }
            foreach (TrainCoupler control in car.GetComponentsInChildren<TrainCoupler>())
            {
                if (kind == CarKind.Locomotive && control.IsFront) { control.Point.localPosition = new Vector3(0f, 0.75f, 8.65f); control.transform.localPosition = new Vector3(1.25f, 1.65f, 8.05f); }
                Coupler(details, control, kind == CarKind.Locomotive && control.IsFront ? 8.4f : 4.4f); Colour(control.transform, red, button);
            }
            if (kind == CarKind.Locomotive) Engine(car, details);
        }
        private static void FillWindowGaps(Transform parent,float side,bool cab,float[] centers)
        {
            var openings=centers.Select(z=>new Vector2(z-(side>0f&&z==0f?0.20f:0f)-(side>0f&&z==0f?0.70f:1.45f)*0.5f,
                z-(side>0f&&z==0f?0.20f:0f)+(side>0f&&z==0f?0.70f:1.45f)*0.5f)).ToList();
            if(side>0f)openings.Add(new Vector2(0.5f,2.5f));
            openings=openings.OrderBy(x=>x.x).ToList();float start=-4.4f;
            foreach(var hole in openings.Concat(new[]{new Vector2(4.4f,4.4f)}))
            {
                if(hole.x>start)Part(parent,"Solid Window Infill",new Vector3(side*1.9f,cab?2.96f:2.76f,(start+hole.x)*0.5f),
                    new Vector3(0.15f,cab?1.55f:1.17f,hole.x-start+0.025f),navy,panelBox);
                start=Mathf.Max(start,hole.y);
            }
            // Close the top corners between the curved roof and the flat wall panels.
            Part(parent,"Roof Wall Seam",new Vector3(side*1.9f,3.845f,0f),new Vector3(0.15f,0.05f,8.85f),navy,panelBox);
        }
        private static void PassengerSeats(Transform parent,float side,float z)
        {
            var row=new GameObject("Passenger Seat Pair").transform;row.SetParent(parent,false);row.localPosition=new Vector3(side*1.16f,0f,z);
            var cushion=Part(row,"Paired Seat Cushion",new Vector3(0f,1.65f,0f),new Vector3(1.24f,0.22f,0.76f),leather);
            cushion.AddComponent<BoxCollider>();
            var back=Part(row,"Paired Seat Back",new Vector3(0f,2.04f,-0.33f),new Vector3(1.24f,0.86f,0.15f),leather);
            back.AddComponent<BoxCollider>();
            foreach(float x in new[]{-0.56f,0.56f})
            {
                Part(row,"Seat Armrest",new Vector3(x,1.94f,0f),new Vector3(0.07f,0.09f,0.78f),wood);
                foreach(float dz in new[]{-0.24f,0.24f})Part(row,"Seat Leg",new Vector3(x,1.34f,dz),new Vector3(0.065f,0.38f,0.065f),steel);
            }
            Part(row,"Seat Divider",new Vector3(0f,1.78f,0f),new Vector3(0.025f,0.03f,0.7f),brass);
            Part(row,"Seat Back Piping",new Vector3(0f,2.48f,-0.33f),new Vector3(1.2f,0.025f,0.17f),brass);
        }
        private static Mesh PanelBox()
        {
            var v=new List<Vector3>();var t=new List<int>();var p=new Vector3[8];
            for(int i=0;i<8;i++)p[i]=new Vector3((i&1)==0?-0.5f:0.5f,(i&2)==0?-0.5f:0.5f,(i&4)==0?-0.5f:0.5f);
            int[] faces={0,4,6,2,1,3,7,5,0,1,5,4,2,6,7,3,0,2,3,1,4,5,7,6};
            for(int i=0;i<faces.Length;i+=4)
            {var normal=Vector3.Cross(p[faces[i+1]]-p[faces[i]],p[faces[i+2]]-p[faces[i]]);Tri(v,t,p[faces[i]],p[faces[i+1]],p[faces[i+2]],normal);Tri(v,t,p[faces[i]],p[faces[i+2]],p[faces[i+3]],normal);}
            return Make(v,t);
        }
        private static void Window(Transform parent, float side, float z, float width, bool cab)
        {
            float x = side * 1.97f;
            Part(parent, "Clear Coach Window", new Vector3(x, cab ? 2.96f : 2.77f, z), new Vector3(0.012f, cab ? 1.45f : 1.12f, width), glass);
            foreach (float y in new[] { 2.2f, cab ? 3.70f : 3.34f }) Part(parent, "Window Brass Sill", new Vector3(x, y, z), new Vector3(0.10f, 0.055f, width + 0.1f), brass);
            foreach (float edge in new[] { -1f, 1f }) Part(parent, "Window Brass Upright", new Vector3(x, cab ? 2.96f : 2.77f, z + edge * width * 0.5f), new Vector3(0.10f, cab ? 1.55f : 1.16f, 0.055f), brass);
        }
        private static TrainWheel Wheel(Transform parent, float x, float z, float radius, Material rimColour)
        {
            var pivot = new GameObject("Spoked Wheel").transform; pivot.SetParent(parent, false); pivot.localPosition = new Vector3(x, radius == 0.4f ? 0.45f : radius + 0.15f, z);
            var wheel = pivot.gameObject.AddComponent<TrainWheel>(); wheel.Configure(radius);
            Quaternion face = Quaternion.Euler(0f, 90f, 0f);
            Part(pivot, "Wheel Tyre", Vector3.zero, new Vector3(radius * 2f, radius * 2f, 0.28f), steel, ring, face);
            Part(pivot, "Wheel Painted Rim", new Vector3(Mathf.Sign(x) * 0.06f, 0f, 0f), new Vector3(radius * 1.72f, radius * 1.72f, 0.12f), rimColour, thinRing, face);
            Part(pivot, "Wheel Hub", Vector3.zero, new Vector3(0.18f, 0.22f, 0.18f), brass, cylinder, Quaternion.Euler(0f, 0f, 90f));
            for (int spoke = 0; spoke < 10; spoke++)
            {
                var angle = Quaternion.AngleAxis(spoke * 36f, Vector3.right);
                Part(pivot, "Wheel Spoke", angle * Vector3.up * radius * 0.45f, new Vector3(0.10f, radius * 0.9f, 0.06f), rimColour, rounded, angle);
            }
            return wheel;
        }
        private static void Coupler(Transform parent, TrainCoupler control, float endDistance)
        {
            float sign = control.IsFront ? 1f : -1f;
            Part(parent, "Buffer Beam", new Vector3(0f, 0.84f, sign * endDistance), new Vector3(3.7f, 0.40f, 0.16f), red);
            foreach (float x in new[] { -1.3f, 1.3f })
            {
                Part(parent, "Buffer Shank", new Vector3(x, 0.88f, sign * (endDistance + 0.13f)), new Vector3(0.17f, 0.36f, 0.17f), steel, cylinder, Quaternion.Euler(90f, 0f, 0f));
                Part(parent, "Buffer Pad", new Vector3(x, 0.88f, sign * (endDistance + 0.30f)), new Vector3(0.45f, 0.07f, 0.45f), navy, cylinder, Quaternion.Euler(90f, 0f, 0f));
                for (int coil = 0; coil < 4; coil++) Part(parent, "Buffer Spring Ring", new Vector3(x, 0.88f, sign * (endDistance + coil * 0.06f)), new Vector3(0.22f, 0.22f, 0.24f), steel, ring);
            }
            var head=new GameObject("Janney Coupler").transform;head.SetParent(parent,false);
            head.localPosition=control.Point.localPosition;head.localRotation=Quaternion.Euler(0f,control.IsFront?0f:180f,0f);
            Part(head,"Coupler Draft Shank",new Vector3(0f,0f,-0.27f),new Vector3(0.28f,0.25f,0.72f),steel);
            Part(head,"Draft Gear Collar",new Vector3(0f,0f,-0.40f),new Vector3(0.48f,0.36f,0.13f),navy);
            Part(head,"Coupler Cast Housing",new Vector3(0f,0f,-0.15f),new Vector3(0.72f,0.36f,0.33f),steel);
            Part(head,"Coupler Fixed Cheek",new Vector3(-0.27f,0f,0.015f),new Vector3(0.22f,0.36f,0.32f),steel,rounded,Quaternion.Euler(0f,-15f,0f));
            foreach(float y in new[]{-0.15f,0.15f})Part(head,"Coupler Housing Lip",new Vector3(-0.03f,y,-0.015f),new Vector3(0.52f,0.075f,0.30f),steel);
            var pivot=new GameObject("Coupler Knuckle Pivot").transform;pivot.SetParent(head,false);pivot.localPosition=new Vector3(0.22f,0f,-0.035f);
            Part(pivot,"Knuckle Heel",new Vector3(0.015f,0f,0.025f),new Vector3(0.24f,0.27f,0.27f),steel,rounded,Quaternion.Euler(0f,-20f,0f));
            Part(pivot,"Knuckle Hook",new Vector3(-0.19f,0f,0.16f),new Vector3(0.40f,0.27f,0.17f),steel,rounded,Quaternion.Euler(0f,10f,0f));
            Part(pivot,"Knuckle Nose",new Vector3(-0.36f,0f,0.115f),new Vector3(0.17f,0.27f,0.20f),steel,rounded,Quaternion.Euler(0f,-25f,0f));
            Part(head,"Knuckle Hinge Pin",new Vector3(0.22f,0f,-0.035f),new Vector3(0.14f,0.24f,0.14f),brass,cylinder);
            Part(head,"Knuckle Pin Cap",new Vector3(0.22f,0.245f,-0.035f),new Vector3(0.20f,0.04f,0.20f),brass,cylinder);
            Part(head,"Coupler Lock Pin",new Vector3(-0.10f,0.22f,-0.21f),new Vector3(0.065f,0.10f,0.065f),steel,cylinder);
            Part(head,"Uncoupling Rod",new Vector3(-0.42f,0.30f,-0.21f),new Vector3(0.65f,0.045f,0.045f),brass);
            var feedback = control.GetComponent<TrainCouplerVisual>(); if (feedback == null) feedback = control.gameObject.AddComponent<TrainCouplerVisual>();
            feedback.Configure(control, pivot, control.GetComponent<Renderer>());
            EditorUtility.SetDirty(feedback);
            if(PrefabUtility.IsPartOfPrefabInstance(feedback))PrefabUtility.RecordPrefabInstancePropertyModifications(feedback);
            for (int segment = 0; segment < 7; segment++)
            {
                float angle = segment * Mathf.PI / 6f;
                Part(parent, "Brake Hose", new Vector3(0.55f + Mathf.Cos(angle) * 0.14f, 0.6f - Mathf.Sin(angle) * 0.23f, sign * (endDistance + 0.12f)), new Vector3(0.055f, 0.12f, 0.055f), navy, cylinder, Quaternion.Euler(0f, 0f, segment * 30f));
            }
        }
        private static void Engine(GameObject car, Transform details)
        {
            var dark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainSteel.mat");
            Part(details, "Steam Boiler", new Vector3(0f, 2.05f, 6.30f), new Vector3(2.1f, 4.2f, 2.1f), navy, cylinder, Quaternion.Euler(90f, 0f, 0f));
            foreach (float z in new[] { 4.45f, 5.4f, 6.55f, 7.6f }) Part(details, "Boiler Brass Band", new Vector3(0f, 2.05f, z), new Vector3(2.18f, 2.18f, 1.5f), brass, thinRing);
            Part(details, "Smokebox Door", new Vector3(0f, 2.05f, 8.45f), new Vector3(2f, 0.16f, 2f), dark, cylinder, Quaternion.Euler(90f, 0f, 0f));
            Part(details, "Smokebox Rim", new Vector3(0f, 2.05f, 8.54f), new Vector3(2.02f, 2.02f, 0.4f), brass, thinRing);
            Part(details, "Smokebox Latch", new Vector3(0f, 2.05f, 8.60f), new Vector3(0.6f, 0.07f, 0.07f), brass);
            Part(details, "Chimney", new Vector3(0f, 3.25f, 7.65f), new Vector3(0.63f, 1.15f, 0.63f), dark, cylinder);
            Part(details, "Chimney Flared Lip", new Vector3(0f, 3.85f, 7.65f), new Vector3(0.90f, 0.9f, 0.5f), brass, ring, Quaternion.Euler(90f, 0f, 0f));
            Part(details, "Steam Dome", new Vector3(0f, 3.22f, 5.45f), new Vector3(0.72f, 0.52f, 0.72f), brass, rounded);
            var lamp = Part(details, "Front Headlamp", new Vector3(0f, 2.85f, 8.62f), new Vector3(0.48f, 0.25f, 0.48f), brass, cylinder, Quaternion.Euler(90f, 0f, 0f));
            Part(details, "Headlamp Lens", new Vector3(0f, 2.85f, 8.76f), new Vector3(0.38f, 0.01f, 0.38f), ivory, cylinder, Quaternion.Euler(90f, 0f, 0f));
            var headlight = lamp.AddComponent<Light>(); headlight.type = LightType.Point; headlight.color = new Color(1f, 0.85f, 0.57f); headlight.range = 9f; headlight.intensity = 1.2f;
            var extendedFloor = Part(details, "Locomotive Front Running Deck", new Vector3(0f, 1.05f, 6.45f), new Vector3(3.8f, 0.20f, 4.1f), steel); extendedFloor.AddComponent<BoxCollider>().sharedMaterial = car.transform.Find("Walkable Floor").GetComponent<Collider>().sharedMaterial;
            var boilerHull = new GameObject("Boiler Collision Hull").transform; boilerHull.SetParent(details, false); boilerHull.localPosition = new Vector3(0f, 2.05f, 6.30f);
            var collider = boilerHull.gameObject.AddComponent<BoxCollider>(); collider.size = new Vector3(1.9f, 1.9f, 4.2f); collider.sharedMaterial = car.transform.Find("Walkable Floor").GetComponent<Collider>().sharedMaterial;
            Part(details, "Boiler Top Walking Strip", new Vector3(0f, 3.14f, 6.4f), new Vector3(0.9f, 0.08f, 4.15f), steel).AddComponent<BoxCollider>().sharedMaterial = car.transform.Find("Walkable Floor").GetComponent<Collider>().sharedMaterial;
            foreach (float side in new[] { -1f, 1f })
            {
                TrainWheel first = null, last = null;
                foreach (float z in new[] { 4.8f, 6.3f, 7.8f }) { var wheel = Wheel(details, side * 1.98f, z, 0.75f, red); if (first == null) first = wheel; last = wheel; }
                var rod = Part(details, "Moving Connecting Rod", Vector3.zero, new Vector3(0.07f, 0.09f, 3f), brass); rod.AddComponent<TrainSideRod>().Configure(first, last, side * 0.18f);
                Part(details, "Steam Cylinder Housing", new Vector3(side * 1.6f, 0.88f, 8.1f), new Vector3(0.50f, 0.75f, 0.50f), navy, cylinder, Quaternion.Euler(90f, 0f, 0f));
                Part(details, "Boiler Handrail", new Vector3(side * 1.01f, 2.64f, 6.3f), new Vector3(0.035f, 4f, 0.035f), brass, cylinder, Quaternion.Euler(90f, 0f, 0f));
                for (float z = 4.6f; z <= 8f; z += 0.85f) Part(details, "Handrail Standoff", new Vector3(side * 0.99f, 2.64f, z), new Vector3(0.20f, 0.03f, 0.03f), brass);
            }
            foreach (float x in new[] { 0.75f, 1.45f }) Part(details, "Ladder Side Rail", new Vector3(x, 2.67f, -3.45f), new Vector3(0.055f, 2.8f, 0.055f), brass, cylinder);
            foreach (Transform child in car.transform) if (child.name == "Ladder Rung") Colour(child, brass);
            var chimney = Particles(details, "Chimney Smoke", new Vector3(0f, 4f, 7.65f), new Color(0.6f, 0.63f, 0.65f, 0.65f), 0.6f, 2.8f);
            var steam = Particles(details, "Cylinder Steam", new Vector3(1.65f, 0.8f, 8.2f), new Color(0.9f, 0.95f, 1f, 0.4f), 0.35f, 1.5f);
            details.gameObject.AddComponent<TrainSteamVisual>().Configure(chimney, steam);
        }
        public static void DressCab(TrainCar car)
        {
            if (navy == null) Initialize();
            Transform details = Root(car.transform, CabOwned);
            Colour(car.transform.Find("Cab Bulkhead"), navy, panelBox); Colour(car.transform.Find("Driver Instrument Panel"), navy, rounded);
            var bulkhead=car.transform.Find("Cab Bulkhead");
            bulkhead.localPosition=new Vector3(0f,1.9f,4.2f); bulkhead.localScale=new Vector3(3.8f,1.55f,0.2f);
            foreach(float x in new[]{-0.85f,0.85f})
            {
                Part(details,"Front Cab Window",new Vector3(x,3.24f,4.2f),new Vector3(1.5f,1.12f,0.012f),glass);
                foreach(float y in new[]{2.66f,3.82f}) Part(details,"Front Window Brass Sill",new Vector3(x,y,4.17f),new Vector3(1.56f,0.055f,0.09f),brass);
                foreach(float edge in new[]{-1f,1f}) Part(details,"Front Window Frame",new Vector3(x+edge*0.77f,3.24f,4.2f),new Vector3(0.065f,1.2f,0.15f),navy);
            }
            Part(details,"Front Cab Window Header",new Vector3(0f,3.74f,4.2f),Vector3.one,navy,MeshAsset("TrainCabCurvedHeader",CabHeaderMesh()));
            foreach(float x in new[]{-1.75f,1.75f})Part(details,"Front Cab Corner Jamb",new Vector3(x,3.225f,4.2f),new Vector3(0.34f,1.30f,0.15f),navy,panelBox);
            Part(details,"Front Centre Mullion",new Vector3(0f,3.25f,4.2f),new Vector3(0.2f,1.2f,0.15f),navy,panelBox);
            var frontGlassCollision=Part(details,"Front Window Safety Glass",new Vector3(0f,3.25f,4.24f),new Vector3(3.8f,1.2f,0.02f),glass);
            frontGlassCollision.GetComponent<Renderer>().enabled=false; frontGlassCollision.AddComponent<BoxCollider>();
            var driverSeat=car.GetComponentInChildren<ConductorSeat>();
            driverSeat.transform.localPosition=new Vector3(0f,0.9f,0f); driverSeat.ConfigureFeet(new Vector3(-0.85f,2.35f,1.6f));
            foreach(var lever in car.GetComponentsInChildren<TrainLever>()) {var p=lever.transform.localPosition;p.y=2.93f;lever.transform.localPosition=p;}
            var reverseControl=car.GetComponentsInChildren<TrainControl>().First(x=>x.Kind==ControlKind.Reverse);
            reverseControl.transform.localPosition=new Vector3(-0.3f,2.98f,1.4f);
            foreach(Transform child in car.transform)
            {
                if(child.name=="THROTTLE Label" || child.name=="BRAKE Label") {var p=child.localPosition;p.y=2.95f;child.localPosition=p;}
                if(child.name=="PRESSURE VALVE Label") {child.localPosition=new Vector3(-1.81f,3.85f,2.08f);child.localRotation=Quaternion.Euler(0f,-90f,0f);}
                if(child.name=="FURNACE Label") child.localPosition=new Vector3(0.72f,2.55f,3.025f);
                if(child.name=="DRIVER Label") child.localPosition=new Vector3(-0.85f,3.35f,1.29f);
            }
            foreach (TrainInstrument gauge in car.GetComponentsInChildren<TrainInstrument>())
            {
                gauge.GetComponent<Renderer>().sharedMaterial = ivory; Vector3 at = gauge.transform.localPosition;
                Part(details, "Instrument Brass Bezel", at + Vector3.back * 0.026f, new Vector3(0.55f, 0.55f, 0.15f), brass, ring);
                Part(details, "Instrument Glass", at + Vector3.back * 0.048f, new Vector3(0.44f, 0.01f, 0.44f), glass, cylinder, Quaternion.Euler(90f, 0f, 0f));
                for (int i = 0; i < 4; i++) Part(details, "Instrument Bezel Screw", at + Quaternion.Euler(0f, 0f, 45f + i * 90f) * Vector3.up * 0.257f + Vector3.back * 0.04f, new Vector3(0.028f, 0.018f, 0.028f), steel, cylinder, Quaternion.Euler(90f, 0f, 0f));
                foreach (Renderer renderer in gauge.Needle.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = red;
            }
            foreach (TrainControl control in car.GetComponentsInChildren<TrainControl>())
            {
                if (control.Kind == ControlKind.Firebox) { control.GetComponent<Renderer>().sharedMaterial = fire; continue; }
                if (control.Kind == ControlKind.Vent) continue;
                Colour(control.transform, control.Kind == ControlKind.Reverse ? red : control.Kind == ControlKind.Door ? navy : brass, control.Kind == ControlKind.Reverse ? cylinder : button);
                Vector3 p = control.transform.localPosition;
                if (control.Kind == ControlKind.Reverse) Part(details, "Reverse Button Bezel", p + Vector3.down * 0.035f, new Vector3(0.24f, 0.24f, 0.16f), brass, ring, Quaternion.Euler(90f, 0f, 0f));
                else Part(details, "Button Fixed Bezel", p + Vector3.forward * 0.015f, new Vector3(0.255f, 0.235f, 0.16f), steel, ring);
            }
            var valve = car.GetComponentsInChildren<TrainControl>().First(x => x.Kind == ControlKind.Vent).transform; var wheelDetails = Root(valve, "Valve Visual Details"); valve.localPosition=new Vector3(-1.68f,3.5f,2.08f);
            foreach (Transform child in valve) if (child.name == "Valve Rim") child.GetComponent<Renderer>().enabled = false;
            Part(wheelDetails, "Pressure Valve Wheel", Vector3.zero, new Vector3(0.42f, 0.42f, 0.5f), red, ring);
            foreach (Transform child in valve) if (child.name == "Valve Spoke") Colour(child, red, rounded);
            Part(details, "Pressure Valve Pipe", new Vector3(-1.78f, 2.88f, 2.08f), new Vector3(0.08f, 1.25f, 0.08f), brass, cylinder);
            Part(details, "Valve Pipe Elbow", new Vector3(-1.70f, 3.44f, 2.08f), new Vector3(0.25f, 0.08f, 0.08f), brass, cylinder, Quaternion.Euler(0f, 0f, 90f));
            foreach (TrainLever lever in car.GetComponentsInChildren<TrainLever>())
            {
                Colour(lever.Handle, lever is TrainBrakeLever ? red : wood); Colour(lever.transform.Find("Lever Shaft"), steel, cylinder);
                Part(details, "Lever Gate", lever.transform.localPosition, new Vector3(0.21f, 0.015f, 0.49f), brass);
                Part(details, "Lever Slot", lever.transform.localPosition + Vector3.up * 0.010f, new Vector3(0.055f, 0.012f, 0.44f), navy);
                for (int mark = 0; mark < 5; mark++) Part(details, "Lever Travel Mark", lever.transform.localPosition + new Vector3(0.077f, 0.011f, -0.18f + mark * 0.09f), new Vector3(0.025f, 0.012f, 0.012f), navy);
            }
            var seat = car.GetComponentInChildren<ConductorSeat>(); Colour(seat.transform.Find("Seat Cushion"), leather, rounded); Colour(seat.transform.Find("Seat Back"), leather, rounded);
            foreach (Transform child in seat.transform) if (child.name == "Armrest") { Colour(child, wood, rounded); child.localScale=new Vector3(0.25f,0.15f,0.8f); var p=child.localPosition;p.z=1.55f;child.localPosition=p; }
            Part(details, "Seat Pedestal", new Vector3(-0.85f, 1.90f, 1.6f), new Vector3(0.13f, 1.5f, 0.13f), steel, cylinder);
            var furnace = car.GetComponentInChildren<TrainFurnace>().transform;
            furnace.localPosition=new Vector3(0.72f,1.98f,3.25f);
            furnace.GetComponent<BoxCollider>().size=new Vector3(1.1f,0.9f,0.45f);
            furnace.Find("Firebox Coal Target").localPosition=new Vector3(0f,0f,0.23f);
            foreach(Transform child in furnace)
                if(child.name=="Firebox Side" || child.name=="Firebox Edge") {var size=child.localScale;size.z=0.4f;child.localScale=size;}
            foreach (Transform child in furnace) if (child.GetComponent<Renderer>() != null && child.GetComponent<TrainControl>() == null) Colour(child, steel, rounded);
            for (int i = 0; i < 9; i++) Part(details, "Firebox Grate", furnace.localPosition + new Vector3(-0.48f + i * 0.12f, 0f, 0.183f), new Vector3(0.023f, 0.83f, 0.03f), navy);
            foreach (float x in new[] { -0.63f, 0.63f }) foreach (float y in new[] { -0.43f, 0f, 0.43f }) Part(details, "Firebox Frame Rivet", furnace.localPosition + new Vector3(x, y, -0.21f), new Vector3(0.055f, 0.022f, 0.055f), brass, cylinder, Quaternion.Euler(90f, 0f, 0f));
            var glow = new GameObject("Furnace Warm Light").AddComponent<Light>(); glow.transform.SetParent(details, false); glow.transform.localPosition = furnace.localPosition + Vector3.back * 0.25f;
            glow.type = LightType.Point; glow.color = new Color(1f, 0.35f, 0.06f); glow.intensity = 1.8f; glow.range = 3f;
            foreach (TextMesh label in car.GetComponentsInChildren<TextMesh>()) MountLabel(label);
        }
        private static void MountLabel(TextMesh label)
        {
            label.color=new Color(0.94f,0.89f,0.72f); label.fontStyle=FontStyle.Bold; label.fontSize=64;
            if(label.text=="0" || label.text=="100" || label.text=="15") return;
            if(label.name=="REVERSE Label") label.transform.localPosition=new Vector3(0f,0.65f,-0.85f);
            label.transform.localScale=Vector3.one;
            Vector3 scale=new Vector3(label.transform.TransformVector(Vector3.right).magnitude,label.transform.TransformVector(Vector3.up).magnitude,label.transform.TransformVector(Vector3.forward).magnitude);
            label.transform.localScale=new Vector3(1f/scale.x,1f/scale.y,1f/scale.z);
            label.characterSize=0.0085f;
            bool gauge=label.name=="Steam Label" || label.name=="Pressure Label" || label.name=="Speed Label";
            float width=gauge?0.49f:Mathf.Max(0.34f,label.text.Length*0.045f+0.07f);
            float height=gauge?0.25f:0.13f;
            var plaque=Root(label.transform,"Mounted Label Plaque");
            Part(plaque,"Plaque Brass Edge",new Vector3(0f,0f,0.027f),new Vector3(width+0.025f,height+0.025f,0.025f),brass,rounded);
            Part(plaque,"Plaque Enamel Face",new Vector3(0f,0f,0.013f),new Vector3(width,height,0.02f),navy,rounded);
            Part(plaque,"Plaque Mount",new Vector3(0f,0f,0.065f),new Vector3(width*0.75f,height*0.75f,0.08f),steel);
            foreach(float x in new[]{-1f,1f}) Part(plaque,"Plaque Fixing",new Vector3(x*(width*0.5f-0.018f),0f,0.001f),new Vector3(0.018f,0.004f,0.018f),brass,cylinder,Quaternion.Euler(90f,0f,0f));
        }
        private static Material Mat(string name, Color colour, float metallic = 0f, bool transparent = false)
        {
            string file = "Assets/Movement/Materials/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(file);
            if (material != null) return material;
            material = new Material(Shader.Find(transparent ? "Universal Render Pipeline/Lit" : "AllAboard/Stylized Train")); material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (transparent)
            {
                material.SetFloat("_Smoothness", 0.65f); material.SetFloat("_Surface", 1f); material.SetFloat("_Blend", 0f); material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f); material.SetFloat("_Cull", 0f); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(material, file); return material;
        }
        private static Mesh MeshAsset(string name, Mesh generated)
        {
            string path = "Assets/Movement/Meshes/" + name + ".asset"; var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) { generated.name=name; EditorUtility.CopySerialized(generated,existing); EditorUtility.SetDirty(existing); Object.DestroyImmediate(generated); return existing; }
            generated.name = name; AssetDatabase.CreateAsset(generated, path); return generated;
        }
        private static Mesh Make(List<Vector3> vertices, List<int> triangles)
        { var mesh = new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh; }
        private static void Tri(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c, Vector3 outward)
        {
            int start = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(start);
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) >= 0f) { t.Add(start + 1); t.Add(start + 2); } else { t.Add(start + 2); t.Add(start + 1); }
        }
        private static Mesh RingMesh(float radius, float thickness)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int ringSegment = 0; ringSegment < 24; ringSegment++) for (int section = 0; section < 8; section++)
            {
                var corners = new Vector3[4];
                for (int k = 0; k < 4; k++)
                {
                    float a = (ringSegment + (k == 1 || k == 2 ? 1 : 0)) * Mathf.PI * 2f / 24f, b = (section + (k >= 2 ? 1 : 0)) * Mathf.PI * 2f / 8f;
                    corners[k] = new Vector3(Mathf.Cos(a) * (radius + Mathf.Cos(b) * thickness), Mathf.Sin(a) * (radius + Mathf.Cos(b) * thickness), Mathf.Sin(b) * thickness);
                }
                Vector3 center = (corners[0] + corners[2]) * 0.5f, normal = center - new Vector3(center.x, center.y, 0f).normalized * radius;
                Tri(v,t,corners[0],corners[1],corners[2],normal); Tri(v,t,corners[0],corners[2],corners[3],normal);
            }
            return Make(v,t);
        }
        private static Mesh CylinderMesh(bool axisZ)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12f, b = (i + 1) * Mathf.PI / 12f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 0.5f, -0.5f, Mathf.Sin(a) * 0.5f), q = new Vector3(Mathf.Cos(b) * 0.5f,-0.5f,Mathf.Sin(b)*0.5f);
                Tri(v,t,p,q,q+Vector3.up,(p+q)*0.5f); Tri(v,t,p,q+Vector3.up,p+Vector3.up,(p+q)*0.5f);
                Tri(v,t,Vector3.down*0.5f,p,q,Vector3.down); Tri(v,t,Vector3.up*0.5f,q+Vector3.up,p+Vector3.up,Vector3.up);
            }
            if (axisZ) for (int i = 0; i < v.Count; i++) v[i] = Quaternion.Euler(90f,0f,0f)*v[i]; return Make(v,t);
        }
        private static Mesh RoundedBox()
        {
            var v = new List<Vector3>(); var t = new List<int>(); float[] steps = { -0.5f, -0.40f, 0.40f, 0.5f };
            foreach (Vector3 normal in new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back })
            {
                Vector3 u = normal.x != 0f ? Vector3.up : Vector3.right, w = Vector3.Cross(normal,u);
                for (int x = 0; x < 3; x++) for (int y = 0; y < 3; y++)
                {
                    var corners = new Vector3[4];
                    for (int k = 0; k < 4; k++)
                    {
                        Vector3 point = normal * 0.5f + u * steps[x+(k==1||k==2?1:0)] + w * steps[y+(k>=2?1:0)];
                        Vector3 clamp = new Vector3(Mathf.Clamp(point.x,-0.4f,0.4f),Mathf.Clamp(point.y,-0.4f,0.4f),Mathf.Clamp(point.z,-0.4f,0.4f));
                        corners[k] = clamp + (point-clamp).normalized*0.1f;
                    }
                    Tri(v,t,corners[0],corners[1],corners[2],normal); Tri(v,t,corners[0],corners[2],corners[3],normal);
                }
            }
            return Make(v,t);
        }
        private static Mesh CabHeaderMesh()
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<24;i++)
            {
                float x=-1.9f+3.8f*i/24f,n=-1.9f+3.8f*(i+1)/24f;
                float y=0.055f+0.3f*Mathf.Sqrt(1f-x*x/(2.05f*2.05f)),h=0.055f+0.3f*Mathf.Sqrt(1f-n*n/(2.05f*2.05f));
                foreach(float z in new[]{-0.075f,0.075f})
                {Tri(v,t,new Vector3(x,0f,z),new Vector3(n,0f,z),new Vector3(n,h,z),Vector3.forward*Mathf.Sign(z));Tri(v,t,new Vector3(x,0f,z),new Vector3(n,h,z),new Vector3(x,y,z),Vector3.forward*Mathf.Sign(z));}
                Tri(v,t,new Vector3(x,y,-0.075f),new Vector3(n,h,-0.075f),new Vector3(n,h,0.075f),Vector3.up);Tri(v,t,new Vector3(x,y,-0.075f),new Vector3(n,h,0.075f),new Vector3(x,y,0.075f),Vector3.up);
                Tri(v,t,new Vector3(x,0f,-0.075f),new Vector3(n,0f,-0.075f),new Vector3(n,0f,0.075f),Vector3.down);Tri(v,t,new Vector3(x,0f,-0.075f),new Vector3(n,0f,0.075f),new Vector3(x,0f,0.075f),Vector3.down);
                if(i==0||i==23){float e=i==0?x:n,top=i==0?y:h;Vector3 normal=i==0?Vector3.left:Vector3.right;Tri(v,t,new Vector3(e,0f,-0.075f),new Vector3(e,top,-0.075f),new Vector3(e,top,0.075f),normal);Tri(v,t,new Vector3(e,0f,-0.075f),new Vector3(e,top,0.075f),new Vector3(e,0f,0.075f),normal);}
            }
            return Make(v,t);
        }
        private static Mesh RoofMesh(bool hatch)
        {
            var v = new List<Vector3>(); var t = new List<int>(); var xs = new List<float>(); for(int i=0;i<=24;i++) xs.Add(-0.5f+i/24f);
            if(hatch) { xs.Add(0.5f/4.1f); xs.Add(1.7f/4.1f); xs.Sort(); }
            float[] zs = hatch ? new[] { -0.5f,-3.7f/9f,-2.3f/9f,0.5f } : new[] { -0.5f,0.5f };
            for(int i=0;i<xs.Count-1;i++) for(int j=0;j<zs.Length-1;j++)
            {
                float x=(xs[i]+xs[i+1])*0.5f,z=(zs[j]+zs[j+1])*0.5f;
                if(hatch&&x>0.5f/4.1f&&x<1.7f/4.1f&&z>-3.7f/9f&&z<-2.3f/9f) continue;
                var a=new Vector3(xs[i],Mathf.Sqrt(Mathf.Max(0f,1f-xs[i]*xs[i]*4f))*0.5f,zs[j]);
                var b=new Vector3(xs[i+1],Mathf.Sqrt(Mathf.Max(0f,1f-xs[i+1]*xs[i+1]*4f))*0.5f,zs[j]);
                Tri(v,t,a,b,new Vector3(b.x,b.y,zs[j+1]),Vector3.up); Tri(v,t,a,new Vector3(b.x,b.y,zs[j+1]),new Vector3(a.x,a.y,zs[j+1]),Vector3.up);
                Tri(v,t,a,b,new Vector3(b.x,b.y,zs[j+1]),Vector3.down); Tri(v,t,a,new Vector3(b.x,b.y,zs[j+1]),new Vector3(a.x,a.y,zs[j+1]),Vector3.down);
                if(j==0) foreach(float end in new[]{-0.5f,0.5f}) Tri(v,t,new Vector3(a.x,a.y,end),new Vector3(b.x,b.y,end),new Vector3(0f,0f,end),Vector3.forward*Mathf.Sign(end));
            }
            return Make(v,t);
        }
        private static Mesh CoalMesh()
        {
            float q=(1f+Mathf.Sqrt(5f))*0.5f;
            Vector3[] p={new Vector3(-1,q,0),new Vector3(1,q,0),new Vector3(-1,-q,0),new Vector3(1,-q,0),new Vector3(0,-1,q),new Vector3(0,1,q),new Vector3(0,-1,-q),new Vector3(0,1,-q),new Vector3(q,0,-1),new Vector3(q,0,1),new Vector3(-q,0,-1),new Vector3(-q,0,1)};
            for(int i=0;i<p.Length;i++) p[i]=p[i].normalized*(0.47f-(i%3)*0.035f);
            int[] faces={0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
            var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<faces.Length;i+=3) Tri(v,t,p[faces[i]],p[faces[i+1]],p[faces[i+2]],p[faces[i]]+p[faces[i+1]]+p[faces[i+2]]);return Make(v,t);
        }
        private static Material SmokeMaterial()
        {
            const string texturePath="Assets/Movement/Textures/TrainSmoke.png", materialPath="Assets/Movement/Materials/TrainSmoke.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(material!=null)return material;
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++) for(int x=0;x<64;x++){float radius=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;texture.SetPixel(x,y,new Color(1f,1f,1f,Mathf.Pow(Mathf.Clamp01(1f-radius),2f)));}
            texture.Apply();File.WriteAllBytes(texturePath,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texturePath);
            material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.SetColor("_BaseColor",Color.white);
            material.SetFloat("_Surface",1f);material.SetFloat("_Blend",0f);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0f);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;AssetDatabase.CreateAsset(material,materialPath);return material;
        }
        private static ParticleSystem Particles(Transform parent,string name,Vector3 position,Color colour,float size,float lifetime)
        {
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;var particles=obj.AddComponent<ParticleSystem>();
            var main=particles.main;main.startLifetime=lifetime;main.startSpeed=1.3f;main.startSize=size;main.startColor=colour;main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=0.12f;
            var velocity=particles.velocityOverLifetime;velocity.enabled=true;velocity.y=1.2f;velocity.space=ParticleSystemSimulationSpace.World;
            var sizeOverLife=particles.sizeOverLifetime;sizeOverLife.enabled=true;sizeOverLife.size=new ParticleSystem.MinMaxCurve(1f,AnimationCurve.Linear(0f,0.6f,1f,2.5f));
            var colourOverLife=particles.colorOverLifetime;colourOverLife.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0f),new GradientColorKey(Color.white,1f)},new[]{new GradientAlphaKey(0.7f,0f),new GradientAlphaKey(0f,1f)});colourOverLife.color=gradient;
            obj.GetComponent<ParticleSystemRenderer>().sharedMaterial=smoke;return particles;
        }
    }
}
