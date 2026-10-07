using UnityEngine;

namespace AllAboard.Movement
{
    [CreateAssetMenu(menuName = "All Aboard/Prototype Audio Set")]
    public sealed class PrototypeAudioSet : ScriptableObject
    {
        public AudioClip[] footsteps;
        public AudioClip[] landings;
        public AudioClip[] impacts;
        public AudioClip pickup;
        public AudioClip jump;
        public AudioClip throwSound;
        [Range(0f, 1f)] public float masterVolume = 0.7f;
        [Range(0f, 1f)] public float footstepVolume = 0.2f;
        [Range(0f, 1f)] public float landingVolume = 0.22f;
        [Range(0f, 1f)] public float interactionVolume = 0.14f;
        [Range(0f, 1f)] public float impactVolume = 0.35f;

        public static AudioClip Choose(AudioClip[] clips, ref int previous)
        {
            if (clips == null || clips.Length == 0) return null;
            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && index == previous) index = (index + 1) % clips.Length;
            previous = index;
            return clips[index];
        }
    }
}
