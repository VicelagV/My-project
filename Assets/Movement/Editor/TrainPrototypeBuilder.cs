using System.Collections.Generic;
using System.IO;
using System.Linq;
using AllAboard.Movement;
using AllAboard.Train;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllAboard.Editor
{
    public static class TrainPrototypeBuilder
    {
        public const string ScenePath = "Assets/Movement/Scenes/TrainPlayground.unity";
        private static PhysicsMaterial contact;
        private static Material shell, deck, metal, doorMaterial;

        [MenuItem("All Aboard/Create Train Playground")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Train playground already exists; preserving edits: " + ScenePath); return; }
            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path) && previous.isDirty)
            { Debug.LogWarning("Save the untitled scene before creating the train playground."); return; }
            shell = Material("TrainShell", new Color(0.26f, 0.48f, 0.49f));
            deck = Material("TrainDeck", new Color(0.28f, 0.31f, 0.36f));
            metal = Material("TrainSteel", new Color(0.12f, 0.15f, 0.19f));
            doorMaterial = Material("TrainDoor", new Color(0.95f, 0.62f, 0.22f));
            const string physicsPath = "Assets/Movement/Materials/TrainContact.physicMaterial";
            contact = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(physicsPath);
            if (contact == null)
            {
                contact = new PhysicsMaterial("TrainContact") { staticFriction = 1f, dynamicFriction = 0.8f,
                    bounciness = 0f, frictionCombine = PhysicsMaterialCombine.Maximum, bounceCombine = PhysicsMaterialCombine.Minimum };
                AssetDatabase.CreateAsset(contact, physicsPath);
            }
            GameObject coalPrefab = ItemPrefab("Coal", Material("Coal", new Color(0.06f, 0.065f, 0.08f)), true, 1f, 0.35f);
            GameObject scrapPrefab = ItemPrefab("ScrapTest", Material("ScrapTest", new Color(0.64f, 0.42f, 0.61f)), false, 4f, 0.5f);
            var prefabs = new[] { CarPrefab("Locomotive", CarKind.Locomotive, 10f), CarPrefab("Passenger", CarKind.Passenger, 6f), CarPrefab("CoalWagon", CarKind.Coal, 4f) };
            bool additive = !string.IsNullOrEmpty(previous.path);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, additive ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Box(null, "Large Test Ground", new Vector3(85f, -0.3f, 80f), new Vector3(420f, 0.6f, 380f), Material("TrainGround", new Color(0.2f, 0.26f, 0.24f)));
            RailPath path = new GameObject("Test Railway - Straight and Gentle Curve").AddComponent<RailPath>();
            path.Configure(new[] { new Vector3(0f, 0f, -60f), Vector3.zero, new Vector3(0f, 0f, 70f),
                new Vector3(0f, 0f, 140f), new Vector3(20f, 0f, 190f), new Vector3(70f, 0f, 220f),
                new Vector3(140f, 0f, 220f), new Vector3(230f, 0f, 220f) });
            BuildRails(path);
            var station = new GameObject("Station and Stop Zone");
            Box(station.transform, "Station Platform", new Vector3(4.8f, 0.575f, 5f), new Vector3(5.3f, 1.15f, 62f), shell);
            Box(station.transform, "Boarding Step 1", new Vector3(8f, 0.15f, -18f), new Vector3(1.2f, 0.3f, 4f), deck);
            Box(station.transform, "Boarding Step 2", new Vector3(7.5f, 0.3f, -18f), new Vector3(1.2f, 0.6f, 4f), deck);
            Box(station.transform, "Boarding Step 3", new Vector3(7f, 0.45f, -18f), new Vector3(1.2f, 0.9f, 4f), deck);
            Box(station.transform, "Stop Area Marker", new Vector3(4.8f, 1.165f, 20f), new Vector3(5f, 0.025f, 1.5f), Material("TrainStop", new Color(0.4f, 0.78f, 0.35f)));
            Room("Station Test Room", new Vector3(16f, 0f, 12f), new Vector2(10f, 12f));
            Room("Workshop Test Room", new Vector3(-13f, 0f, -8f), new Vector2(12f, 14f));
            var controller = new GameObject("Train Systems").AddComponent<TrainController>();
            var cars = new TrainCar[4];
            for (int i = 0; i < 4; i++)
            {
                int type = i == 0 ? 0 : i == 1 ? 2 : 1;
                cars[i] = ((GameObject)PrefabUtility.InstantiatePrefab(prefabs[type], scene)).GetComponent<TrainCar>();
                cars[i].name = i == 0 ? "Locomotive" : i == 1 ? "Coal Storage Car" : "Passenger Car " + i;
            }
            controller.Configure(path, cars);
            controller.gameObject.AddComponent<TrainDriveAudio>().Configure(controller,
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainSteamLoop.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainRollingLoop.wav"));
            DamageableComponent speedometer = AddCabControls(controller, cars[0]);
            TrainAppearanceBuilder.DressCab(cars[0]);
            foreach (TrainCar car in cars)
                foreach (PrototypeInteractable control in car.GetComponentsInChildren<PrototypeInteractable>())
                {
                    Renderer[] geometry = control is TrainLadder ? car.GetComponentsInChildren<Renderer>().Where(x => x.name == "Ladder Rung" || x.gameObject == control.gameObject).ToArray() : control.GetComponentsInChildren<Renderer>();
                    control.gameObject.AddComponent<ControlHighlight>().Configure(
                        AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/HoverOutline.mat"), geometry);
                    if (control is TrainDoor)
                        foreach (Transform child in control.transform)
                            if (child.name.Contains("Door Button")) child.gameObject.AddComponent<TrainButton>().Configure(new Vector3(child.localPosition.x > 1.9f ? -0.05f : 0.05f, 0f, 0f));
                    if (control is TrainCoupler coupler)
                        control.gameObject.AddComponent<TrainButton>().Configure(new Vector3(0f, 0f, coupler.IsFront ? -0.05f : 0.05f));
                }
            controller.gameObject.AddComponent<TrainDebugHud>().Configure(controller, speedometer);
            for (int i = 0; i < 10; i++)
            {
                var coal = (GameObject)PrefabUtility.InstantiatePrefab(coalPrefab, scene);
                coal.name = "Physical Coal " + (i + 1);
                coal.transform.position = cars[1].transform.TransformPoint(new Vector3(i % 2 == 0 ? -1.15f : 1.15f, 1.325f, 2f - (i / 2) * 0.9f));
                coal.GetComponent<CoalItem>().StoreIn(cars[1]);
            }
            for (int i = 0; i < 2; i++)
            {
                var scrap = (GameObject)PrefabUtility.InstantiatePrefab(scrapPrefab, scene);
                scrap.transform.position = cars[2].transform.TransformPoint(new Vector3(-0.8f, 1.6f, i * 1.2f));
            }
            var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/PrototypePlayer.prefab"), scene);
            player.transform.position = new Vector3(3f, 1.2f, 12f);
            player.GetComponent<PlayerInteraction>().ShowInteractionPrompts(true);
            player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.55f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (additive) { EditorSceneManager.CloseScene(scene, true); SceneManager.SetActiveScene(previous); }
            AssetDatabase.SaveAssets();
            Debug.Log("Created four-car train playground with track, station, rooms, controls and physical coal.");
        }

        private static GameObject CarPrefab(string name, CarKind kind, float weight)
        {
            string file = "Assets/Movement/Prefabs/Train" + name + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file);
            if (prefab != null) return prefab;
            var root = new GameObject(name);
            TrainCar car = root.AddComponent<TrainCar>();
            car.Body.isKinematic = true; car.Body.useGravity = false; car.Body.interpolation = RigidbodyInterpolation.Interpolate;
            Box(root.transform, "Walkable Floor", new Vector3(0f, 1.05f, 0f), new Vector3(3.8f, 0.2f, 8.8f), kind == CarKind.Coal ? Material("Coal", new Color(0.06f, 0.065f, 0.08f)) : deck);
            if (kind != CarKind.Coal)
            {
            Box(root.transform, "Left Wall", new Vector3(-1.9f, 2.5f, 0f), new Vector3(0.15f, 2.8f, 8.8f), shell);
            Box(root.transform, "Right Rear Wall", new Vector3(1.9f, 2.5f, -1.95f), new Vector3(0.15f, 2.8f, 4.9f), shell);
            Box(root.transform, "Right Front Wall", new Vector3(1.9f, 2.5f, 3.45f), new Vector3(0.15f, 2.8f, 1.9f), shell);
            Box(root.transform, "Door Header", new Vector3(1.9f, 3.65f, 1.5f), new Vector3(0.15f, 0.5f, 2f), shell);
            for (int end = -1; end <= 1; end += 2)
            {
                Box(root.transform, "End Portal Left", new Vector3(-1.3f, 2.5f, end * 4.4f), new Vector3(1.2f, 2.8f, 0.15f), shell);
                Box(root.transform, "End Portal Right", new Vector3(1.3f, 2.5f, end * 4.4f), new Vector3(1.2f, 2.8f, 0.15f), shell);
                Box(root.transform, "End Portal Header", new Vector3(0f, 3.65f, end * 4.4f), new Vector3(1.4f, 0.5f, 0.15f), shell);
            }
            if (kind == CarKind.Locomotive)
            {
                Box(root.transform, "Roof Left", new Vector3(-0.7f, 4f, 0f), new Vector3(2.4f, 0.2f, 8.8f), deck);
                Box(root.transform, "Roof Right", new Vector3(1.8f, 4f, 0f), new Vector3(0.2f, 0.2f, 8.8f), deck);
                Box(root.transform, "Roof Front", new Vector3(1.1f, 4f, 1.05f), new Vector3(1.2f, 0.2f, 6.7f), deck);
                Box(root.transform, "Roof Rear", new Vector3(1.1f, 4f, -4.05f), new Vector3(1.2f, 0.2f, 0.7f), deck);
                for (int i = 0; i < 9; i++) VisualBox(root.transform, "Ladder Rung", new Vector3(1.1f, 1.4f + i * 0.3f, -3.45f), new Vector3(0.7f, 0.05f, 0.08f), metal);
                foreach (float y in new[] { 2f, 4.8f })
                    Box(root.transform, "Ladder Interaction", new Vector3(1.6f, y, -3f), new Vector3(0.15f, 0.3f, 0.3f), doorMaterial).AddComponent<TrainLadder>().Configure(car);
            }
            else Box(root.transform, "Walkable Roof", new Vector3(0f, 4f, 0f), new Vector3(3.8f, 0.2f, 8.8f), deck);
            if (kind == CarKind.Passenger)
                foreach (float z in new[] { -2.5f, -0.6f, 2.5f })
                {
                    Box(root.transform, "Placeholder Seat", new Vector3(-1.25f, 1.6f, z), new Vector3(0.8f, 0.85f, 0.9f), doorMaterial);
                    Box(root.transform, "Placeholder Seat Back", new Vector3(-1.6f, 2.1f, z), new Vector3(0.15f, 1.1f, 0.9f), doorMaterial);
                }
            GameObject panel = Box(root.transform, "Sliding Door", new Vector3(1.9f, 2.25f, 1.5f), new Vector3(0.15f, 2.2f, 2f), doorMaterial);
            var switchRoot = new GameObject("Door Switch"); switchRoot.transform.SetParent(root.transform, false);
            var door = switchRoot.AddComponent<TrainDoor>(); door.Configure(panel.transform, panel.transform.localPosition);
            Box(switchRoot.transform, "Outside Door Button", new Vector3(2.08f, 2f, 0.2f), new Vector3(0.2f, 0.3f, 0.3f), doorMaterial);
            Box(switchRoot.transform, "Inside Door Button", new Vector3(1.72f, 2f, 0.2f), new Vector3(0.2f, 0.3f, 0.3f), doorMaterial);
            }
            else
            {
                foreach (float side in new[] { -1.9f, 1.9f })
                    Box(root.transform, "Low Side Guard", new Vector3(side, 1.375f, 0f), new Vector3(0.15f, 0.45f, 8.8f), metal);
                foreach (float end in new[] { -4.4f, 4.4f }) foreach (float x in new[] { -1.3f, 1.3f })
                    Box(root.transform, "Low End Guard", new Vector3(x, 1.375f, end), new Vector3(1.2f, 0.45f, 0.15f), metal);
            }
            foreach (bool front in new[] { true, false })
            {
                var point = new GameObject(front ? "Front Coupler Point" : "Rear Coupler Point").transform;
                point.SetParent(root.transform, false); point.localPosition = new Vector3(0f, 0.75f, front ? 4.65f : -4.65f);
                var button = Box(root.transform, front ? "Front Coupler Switch" : "Rear Coupler Switch", new Vector3(1.25f, 1.65f, front ? 4.05f : -4.05f), new Vector3(0.2f, 0.3f, 0.2f), doorMaterial);
                button.AddComponent<TrainCoupler>().Configure(car, front, point);
            }
            var wheelList = new List<Transform>();
            foreach (float x in new[] { -2f, 2f }) foreach (float z in new[] { -2.8f, 2.8f })
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name = "Visual Wheel";
                Object.DestroyImmediate(wheel.GetComponent<Collider>()); wheel.transform.SetParent(root.transform, false);
                wheel.transform.localPosition = new Vector3(x, 0.45f, z); wheel.transform.localScale = new Vector3(0.8f, 0.12f, 0.8f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); wheel.GetComponent<Renderer>().sharedMaterial = metal; wheelList.Add(wheel.transform);
            }
            GameObject bridge = Box(root.transform, "Rear Walkway", new Vector3(0f, 1.05f, -4.85f), new Vector3(1.5f, 0.2f, 0.9f), deck);
            GameObject roofBridge = kind == CarKind.Coal ? null : Box(root.transform, "Rear Roof Walkway", new Vector3(0f, 4f, -4.85f), new Vector3(1.5f, 0.2f, 0.9f), deck);
            car.Configure(kind, weight, bridge, roofBridge, wheelList.ToArray());
            TrainAppearanceBuilder.DecorateCar(root, kind);
            prefab = PrefabUtility.SaveAsPrefabAsset(root, file); Object.DestroyImmediate(root); return prefab;
        }

        private static DamageableComponent AddCabControls(TrainController controller, TrainCar locomotive)
        {
            Transform cab = locomotive.transform;
            Box(cab, "Cab Bulkhead", new Vector3(0f, 2.55f, 4.2f), new Vector3(3.3f, 2.7f, 0.2f), metal);
            Box(cab, "Driver Instrument Panel", new Vector3(-0.8f, 2.75f, 3.0f), new Vector3(1.85f, 0.85f, 0.12f), metal);
            Material face = Material("GaugeFace", new Color(0.86f, 0.83f, 0.72f));
            DamageableComponent damage = null;
            foreach (InstrumentKind kind in new[] { InstrumentKind.Steam, InstrumentKind.Pressure, InstrumentKind.Speed })
            {
                float x = -1.38f + (int)kind * 0.58f;
                var dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder); dial.name = kind + " Dial";
                dial.transform.SetParent(cab, false); dial.transform.localPosition = new Vector3(x, 2.86f, 2.9f);
                dial.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); dial.transform.localScale = new Vector3(0.46f, 0.025f, 0.46f);
                dial.GetComponent<Renderer>().sharedMaterial = face;
                // Tick marks and needles live in the cab's XY plane, independent of cylinder scaling.
                var pivot = new GameObject(kind + " Needle Pivot").transform; pivot.SetParent(cab, false);
                pivot.localPosition = new Vector3(x, 2.86f, 2.865f);
                VisualBox(pivot, "Needle", new Vector3(0f, 0.075f, 0f), new Vector3(0.015f, 0.15f, 0.008f), doorMaterial);
                for (int tick = 0; tick <= 10; tick++)
                {
                    float angle = Mathf.Lerp(135f, -135f, tick / 10f);
                    Vector3 radial = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
                    var mark = Box(cab, "Gauge Tick", new Vector3(x, 2.86f, 2.865f) + radial * 0.19f,
                        new Vector3(0.012f, tick % 5 == 0 ? 0.045f : 0.023f, 0.007f), metal);
                    Object.DestroyImmediate(mark.GetComponent<Collider>()); mark.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
                TextMesh text = Label(cab, kind.ToString(), new Vector3(x, 2.48f, 2.88f), 0.016f);
                if (kind == InstrumentKind.Speed) damage = dial.AddComponent<DamageableComponent>();
                var instrument = dial.AddComponent<TrainInstrument>(); instrument.Configure(controller, kind, text, kind == InstrumentKind.Speed ? damage : null);
                instrument.ConfigureDial(pivot);
                Label(cab, "0", new Vector3(x - 0.15f, 2.68f, 2.85f), 0.012f);
                Label(cab, kind == InstrumentKind.Speed ? "15" : "100", new Vector3(x + 0.15f, 2.68f, 2.85f), 0.012f);
            }
            var seatRoot = new GameObject("Conductor Seat"); seatRoot.transform.SetParent(cab, false);
            ConductorSeat seat = seatRoot.AddComponent<ConductorSeat>(); seat.Configure(locomotive);
            Box(seatRoot.transform, "Seat Cushion", new Vector3(-0.85f, 1.75f, 1.6f), new Vector3(0.8f, 0.2f, 0.8f), doorMaterial);
            Box(seatRoot.transform, "Seat Back", new Vector3(-0.85f, 2.2f, 1.2f), new Vector3(0.8f, 0.9f, 0.15f), shell);
            Label(cab, "DRIVER", new Vector3(-0.85f, 2.8f, 1.12f), 0.022f).transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            foreach (float x in new[] { -1.4f, -0.3f })
                Box(seatRoot.transform, "Armrest", new Vector3(x, 1.95f, 1.6f), new Vector3(0.25f, 0.15f, 1.1f), metal);
            AddLever<TrainThrottleLever>(cab, "THROTTLE", -1.4f, controller, seat);
            AddLever<TrainBrakeLever>(cab, "BRAKE", -0.3f, controller, seat);
            var reverse = AddButton(cab, "REVERSE", new Vector3(-0.3f, 2.08f, 1.4f), new Vector3(0.18f, 0.09f, 0.18f), controller, ControlKind.Reverse);
            reverse.GetComponent<TrainButton>().Configure(new Vector3(0f, -0.045f, 0f));
            reverse.transform.Find("REVERSE Label").localRotation = Quaternion.Euler(90f, 0f, 0f);
            reverse.transform.Find("REVERSE Label").localPosition = new Vector3(0f, 0.65f, -1.6f);
            foreach (ControlKind action in new[] { ControlKind.Whistle, ControlKind.Bell, ControlKind.Door, ControlKind.DamageSpeedometer })
            {
                int index = action == ControlKind.Whistle ? 0 : action == ControlKind.Bell ? 1 : action == ControlKind.Door ? 2 : 3;
                string title = action == ControlKind.DamageSpeedometer ? "TEST DAMAGE" : action.ToString().ToUpperInvariant();
                AudioClip clip = action == ControlKind.Whistle ? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainWhistle.wav") :
                    action == ControlKind.Bell ? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/TrainBell.wav") : null;
                var button = AddButton(cab, title, new Vector3(-1.48f + index * 0.42f, 2.14f, 2.9f), new Vector3(0.18f, 0.16f, 0.1f), controller, action);
                button.GetComponent<TrainControl>().Configure(controller, action, locomotive.GetComponentInChildren<TrainDoor>(), damage, clip);
            }
            // A real side-mounted wheel, rather than a button on the main dashboard.
            var valve = new GameObject("Pressure Valve"); valve.transform.SetParent(cab, false);
            valve.transform.localPosition = new Vector3(-1.68f, 2.62f, 2.08f);
            valve.transform.localRotation = Quaternion.Euler(0f, -65f, 0f);
            valve.AddComponent<SphereCollider>().radius = 0.22f;
            for (int spoke = 0; spoke < 8; spoke++)
            {
                float angle = spoke * 45f; Vector3 radial = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
                VisualBox(valve.transform, "Valve Rim", radial * 0.18f, new Vector3(0.15f, 0.035f, 0.035f), doorMaterial);
                valve.transform.GetChild(spoke).localRotation = Quaternion.Euler(0f, 0f, angle);
            }
            VisualBox(valve.transform, "Valve Spoke", Vector3.zero, new Vector3(0.34f, 0.035f, 0.035f), metal);
            VisualBox(valve.transform, "Valve Spoke", Vector3.zero, new Vector3(0.035f, 0.34f, 0.035f), metal);
            var grip = new GameObject("Valve Hand Contact").transform; grip.SetParent(valve.transform, false); grip.localPosition = new Vector3(0.18f, 0f, -0.06f);
            valve.AddComponent<TrainControl>().Configure(controller, ControlKind.Vent);
            valve.AddComponent<TrainButton>().Configure(Vector3.zero, true, grip);
            Label(cab, "PRESSURE VALVE", new Vector3(-1.65f, 2.97f, 2.1f), 0.012f).transform.localRotation = valve.transform.localRotation;
            // Open firebox: the large central opening contains only an intake trigger.
            Vector3 mouth = new Vector3(0.72f, 1.98f, 3.55f);
            var furnace = new GameObject("Furnace Intake"); furnace.transform.SetParent(cab, false); furnace.transform.localPosition = mouth;
            var intake = furnace.AddComponent<BoxCollider>(); intake.size = new Vector3(1.1f, 0.9f, 0.7f); intake.isTrigger = true;
            furnace.AddComponent<TrainFurnace>().Configure(controller.Resources);
            foreach (float side in new[] { -1f, 1f })
                Box(furnace.transform, "Firebox Side", new Vector3(side * 0.63f, 0f, 0f), new Vector3(0.16f, 1.22f, 0.7f), metal);
            foreach (float side in new[] { -1f, 1f })
                Box(furnace.transform, "Firebox Edge", new Vector3(0f, side * 0.53f, 0f), new Vector3(1.1f, 0.16f, 0.7f), metal);
            var back = Box(furnace.transform, "Firebox Coal Target", new Vector3(0f, 0f, 0.38f), new Vector3(1.1f, 0.9f, 0.08f), doorMaterial);
            // The recessed back is also the E interaction target; raycasts do not stop at triggers.
            back.AddComponent<TrainControl>().Configure(controller, ControlKind.Firebox);
            Label(cab, "FURNACE", mouth + new Vector3(0f, 0.72f, -0.36f), 0.024f);
            return damage;
        }

        private static void AddLever<T>(Transform cab, string title, float x, TrainController controller, ConductorSeat seat) where T : TrainLever
        {
            var root = new GameObject(title + " Lever"); root.transform.SetParent(cab, false);
            root.transform.localPosition = new Vector3(x, 2.03f, 2f);
            VisualBox(root.transform, "Lever Shaft", new Vector3(0f, 0.19f, 0f), new Vector3(0.045f, 0.38f, 0.045f), metal);
            var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere); knob.name = title + " Grip";
            knob.transform.SetParent(root.transform, false); knob.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            knob.transform.localScale = Vector3.one * 0.18f; knob.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            root.AddComponent<T>().Configure(controller, seat, knob.transform);
            var label = Label(cab, title, new Vector3(x, 2.05f, 1.8f), 0.010f); label.transform.localRotation = Quaternion.Euler(90f, 0f, 90f);
        }
        private static GameObject AddButton(Transform cab, string title, Vector3 position, Vector3 size, TrainController train, ControlKind kind)
        {
            var button = Box(cab, title + " Button", position, size, doorMaterial);
            button.AddComponent<TrainControl>().Configure(train, kind);
            button.AddComponent<TrainButton>();
            // Labels use inverse scale so titles stay small and have consistent character size.
            var label = Label(button.transform, title, new Vector3(0f, 1.05f, -0.6f), 0.010f);
            label.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);
            return button;
        }

        private static void BuildRails(RailPath path)
        {
            var root = new GameObject("Primitive Track");
            for (float distance = 0f; distance < path.Length; distance += 2f)
            {
                path.Sample(distance, out Vector3 position, out Quaternion rotation);
                var tie = Box(root.transform, "Sleeper", position + Vector3.up * 0.04f, new Vector3(2.6f, 0.08f, 0.24f), deck);
                tie.transform.rotation = rotation;
                foreach (float side in new[] { -0.75f, 0.75f })
                {
                    var rail = Box(root.transform, "Rail", position + rotation * new Vector3(side, 0.15f, 1f), new Vector3(0.1f, 0.15f, 2.08f), metal);
                    rail.transform.rotation = rotation;
                }
            }
            foreach (float end in new[] { 1f, path.Length - 1f })
            {
                path.Sample(end, out Vector3 p, out Quaternion q);
                var stop = Box(root.transform, "Track End Buffer", p + Vector3.up, new Vector3(3f, 1.5f, 0.3f), doorMaterial); stop.transform.rotation = q;
            }
        }
        private static void Room(string name, Vector3 position, Vector2 size)
        {
            var room = new GameObject(name); room.transform.position = position;
            Box(room.transform, "Room Floor", Vector3.zero, new Vector3(size.x, 0.1f, size.y), deck);
            Box(room.transform, "Back Wall", new Vector3(0f, 2f, size.y * 0.5f), new Vector3(size.x, 4f, 0.2f), shell);
            foreach (float side in new[] { -1f, 1f }) Box(room.transform, "Side Wall", new Vector3(side * size.x * 0.5f, 2f, 0f), new Vector3(0.2f, 4f, size.y), shell);
            foreach (float side in new[] { -1f, 1f }) Box(room.transform, "Front Wall", new Vector3(side * (size.x + 2.4f) * 0.25f, 2f, -size.y * 0.5f), new Vector3((size.x - 2.4f) * 0.5f, 4f, 0.2f), shell);
            Box(room.transform, "Door Header", new Vector3(0f, 3.4f, -size.y * 0.5f), new Vector3(2.4f, 1.2f, 0.2f), shell);
            Box(room.transform, "Room Roof", new Vector3(0f, 4f, 0f), new Vector3(size.x, 0.15f, size.y), deck);
        }
        private static GameObject ItemPrefab(string name, Material material, bool coal, float mass, float scale)
        {
            string file = "Assets/Movement/Prefabs/" + name + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file); if (prefab != null) return prefab;
            var item = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/BluePhysicsCube.prefab"));
            PrefabUtility.UnpackPrefabInstance(item, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            item.name = name; item.transform.localScale = Vector3.one * scale; item.GetComponent<MeshRenderer>().sharedMaterial = material;
            item.GetComponent<Rigidbody>().mass = mass; if (coal) item.AddComponent<CoalItem>();
            prefab = PrefabUtility.SaveAsPrefabAsset(item, file); Object.DestroyImmediate(item); return prefab;
        }
        private static Material Material(string name, Color color)
        {
            string file = "Assets/Movement/Materials/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(file);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color }; AssetDatabase.CreateAsset(material, file); }
            return material;
        }
        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name; box.transform.SetParent(parent, false);
            box.transform.localPosition = position; box.transform.localScale = scale; box.GetComponent<Renderer>().sharedMaterial = material;
            box.GetComponent<Collider>().sharedMaterial = contact; return box;
        }
        private static void VisualBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        { GameObject box = Box(parent, name, position, scale, material); Object.DestroyImmediate(box.GetComponent<Collider>()); }
        private static TextMesh Label(Transform parent, string text, Vector3 position, float size)
        {
            var root = new GameObject(text + " Label"); root.transform.SetParent(parent, false); root.transform.localPosition = position;
            var label = root.AddComponent<TextMesh>(); label.text = text; label.characterSize = size; label.fontSize = 48;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); root.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            return label;
        }
    }
}
