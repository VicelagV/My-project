#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

namespace AllAboard.Movement.Tests
{
    public sealed class AudioAssetTests
    {
        [Test]
        public void CountrysideAmbienceIsLongStereo()
        {
            var clip=UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/CountrysideLoop.wav");
            Assert.That(clip,Is.Not.Null);Assert.That(clip.channels,Is.EqualTo(2));
            Assert.That(clip.length,Is.GreaterThan(100f));
        }
        [Test]
        public void RecordedTrainSignalsAreMonoAndKeepTheirNaturalPhrases()
        {
            foreach(var spec in new[]{("TrainBell",2.612245f),("TrainWhistle",4.148594f)})
            {
                var clip=UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Movement/Audio/"+spec.Item1+".wav");
                Assert.That(clip,Is.Not.Null);Assert.That(clip.channels,Is.EqualTo(1));
                Assert.That(clip.length,Is.EqualTo(spec.Item2).Within(0.01f));
                var data=new float[clip.samples];Assert.That(clip.GetData(data,0),Is.True);
                Assert.That(System.Linq.Enumerable.Max(data,x=>Mathf.Abs(x)),Is.InRange(0.7f,0.9f));
            }
        }
        [Test]
        public void ImportedSoundPaletteIsPlayableAndWiredToBothPrefabs()
        {
            var profile = UnityEditor.AssetDatabase.LoadAssetAtPath<PrototypeAudioSet>(
                "Assets/Movement/Audio/LightPrototypeAudio.asset");
            Assert.That(profile, Is.Not.Null);
            var clips = new System.Collections.Generic.List<AudioClip>();
            clips.AddRange(profile.footsteps);
            clips.AddRange(profile.landings);
            clips.AddRange(profile.impacts);
            clips.Add(profile.pickup);
            clips.Add(profile.jump);
            clips.Add(profile.throwSound);
            Assert.That(clips.Count, Is.EqualTo(13));
            foreach (AudioClip clip in clips)
            {
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.samples, Is.GreaterThan(0), clip.name);
                Assert.That(clip.channels, Is.EqualTo(1), clip.name);
                Assert.That(clip.length, Is.InRange(0.05f, 0.5f), clip.name);
            }
            var player = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/PrototypePlayer.prefab");
            var cube = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/BluePhysicsCube.prefab");
            var playerFeedback = player.GetComponent<PlayerAudioFeedback>();
            var cubeFeedback = cube.GetComponent<ObjectImpactAudio>();
            Assert.That(playerFeedback, Is.Not.Null);
            Assert.That(cubeFeedback, Is.Not.Null);
            Assert.That(new UnityEditor.SerializedObject(playerFeedback).FindProperty("sounds").objectReferenceValue, Is.SameAs(profile));
            Assert.That(new UnityEditor.SerializedObject(cubeFeedback).FindProperty("sounds").objectReferenceValue, Is.SameAs(profile));
        }
    }
}
#endif
