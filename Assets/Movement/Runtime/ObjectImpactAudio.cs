using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(PickupObject), typeof(AudioSource))]
    public sealed class ObjectImpactAudio : MonoBehaviour
    {
        [SerializeField] private PrototypeAudioSet sounds;
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 0.8f;
        [SerializeField, Min(0f)] private float impactCooldown = 0.12f;
        private PickupObject item;
        private AudioSource source;
        private float lastImpactTime = float.NegativeInfinity;
        private int lastClip = -1;

        public void Configure(PrototypeAudioSet set) => sounds = set;

        private void Awake()
        {
            item = GetComponent<PickupObject>();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 1f;
            source.maxDistance = 16f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            float speed = collision.relativeVelocity.magnitude;
            if (sounds == null || item.IsHeld || speed < minimumImpactSpeed || Time.time - lastImpactTime < impactCooldown) return;
            AudioClip clip = PrototypeAudioSet.Choose(sounds.impacts, ref lastClip);
            if (clip == null) return;
            lastImpactTime = Time.time;
            source.pitch = Random.Range(0.95f, 1.08f);
            float strength = Mathf.Lerp(0.25f, 1f, Mathf.InverseLerp(minimumImpactSpeed, 9f, speed));
            source.PlayOneShot(clip, sounds.impactVolume * sounds.masterVolume * strength);
        }
    }
}
