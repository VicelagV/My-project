using System;
using AllAboard.Movement;
using UnityEditor;
using UnityEngine;

namespace AllAboard.Editor
{
    public static class AudioPrototypeBuilder
    {
        private const string Root = "Assets/Movement/Audio/";

        [MenuItem("All Aboard/Set Up Light Prototype Sounds")]
        public static void Apply()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Root.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings;
                settings.preloadAudioData = true;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
            const string profilePath = Root + "LightPrototypeAudio.asset";
            PrototypeAudioSet profile = AssetDatabase.LoadAssetAtPath<PrototypeAudioSet>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PrototypeAudioSet>();
                profile.footsteps = new[] { Clip("footstep_carpet_000"), Clip("footstep_carpet_001"),
                    Clip("footstep_carpet_002"), Clip("footstep_carpet_003"), Clip("footstep_carpet_004") };
                profile.landings = new[] { Clip("impactSoft_medium_000"), Clip("impactSoft_medium_001") };
                profile.impacts = new[] { Clip("impactWood_light_000"), Clip("impactWood_light_001"), Clip("impactWood_light_002") };
                profile.pickup = Clip("pluck_001");
                profile.jump = Clip("pluck_002");
                profile.throwSound = Clip("drop_001");
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            string playerPath = "Assets/Movement/Prefabs/PrototypePlayer.prefab";
            GameObject player = PrefabUtility.LoadPrefabContents(playerPath);
            try
            {
                var feedback = player.GetComponent<PlayerAudioFeedback>();
                if (feedback == null) feedback = player.AddComponent<PlayerAudioFeedback>();
                feedback.Configure(profile);
                PrefabUtility.SaveAsPrefabAsset(player, playerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            string cubePath = "Assets/Movement/Prefabs/BluePhysicsCube.prefab";
            GameObject cube = PrefabUtility.LoadPrefabContents(cubePath);
            try
            {
                var feedback = cube.GetComponent<ObjectImpactAudio>();
                if (feedback == null) feedback = cube.AddComponent<ObjectImpactAudio>();
                feedback.Configure(profile);
                PrefabUtility.SaveAsPrefabAsset(cube, cubePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(cube); }
            AssetDatabase.SaveAssets();
            Debug.Log("Light prototype sounds ready: footsteps, jump, landing, pickup, throw, impacts.");
        }

        private static AudioClip Clip(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + name + ".ogg");
            if (clip == null) throw new InvalidOperationException("Missing prototype sound: " + name);
            return clip;
        }
    }
}
