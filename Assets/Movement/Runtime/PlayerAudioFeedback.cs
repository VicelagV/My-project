using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(FirstPersonMotor), typeof(AudioSource))]
    public sealed class PlayerAudioFeedback : MonoBehaviour
    {
        [SerializeField] private PrototypeAudioSet sounds;
        [SerializeField, Min(0.1f)] private float distancePerStep = 1.8f;
        private FirstPersonMotor motor;
        private PlayerInteraction interaction;
        private AudioSource steps;
        private AudioSource actions;
        private float stepDistance;
        private int lastStep = -1;
        private int lastLanding = -1;

        public void Configure(PrototypeAudioSet set) => sounds = set;

        private void Awake()
        {
            motor = GetComponent<FirstPersonMotor>();
            interaction = GetComponent<PlayerInteraction>();
            steps = GetComponent<AudioSource>();
            actions = gameObject.AddComponent<AudioSource>();
            foreach (AudioSource source in new[] { steps, actions })
            {
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;
            }
        }

        private void OnEnable()
        {
            if (motor == null) return;
            motor.Jumped += OnJump;
            motor.Landed += OnLanding;
            if (interaction != null)
            {
                interaction.PickedUp += OnPickup;
                interaction.Released += OnRelease;
                interaction.Welded += OnPickup;
            }
        }

        private void OnDisable()
        {
            if (motor == null) return;
            motor.Jumped -= OnJump;
            motor.Landed -= OnLanding;
            if (interaction != null)
            {
                interaction.PickedUp -= OnPickup;
                interaction.Released -= OnRelease;
                interaction.Welded -= OnPickup;
            }
        }

        private void LateUpdate()
        {
            float distance = motor.RelativeGroundDistance;
            if (!motor.Grounded || distance > 4f) { stepDistance = 0f; return; }
            if (distance < 0.0001f || sounds == null) return;
            stepDistance += distance;
            if (stepDistance < distancePerStep) return;
            stepDistance %= distancePerStep;
            Play(steps, PrototypeAudioSet.Choose(sounds.footsteps, ref lastStep),
                sounds.footstepVolume * (motor.IsCrouched ? 0.45f : 1f));
        }

        private void OnJump()
        {
            stepDistance = 0f;
            if (sounds != null) Play(actions, sounds.jump, sounds.interactionVolume * 0.7f);
        }

        private void OnLanding(float speed)
        {
            stepDistance = 0f;
            if (sounds != null) Play(actions, PrototypeAudioSet.Choose(sounds.landings, ref lastLanding),
                sounds.landingVolume * Mathf.Lerp(0.5f, 1f, Mathf.InverseLerp(3f, 12f, speed)));
        }

        private void OnPickup()
        {
            if (sounds != null) Play(actions, sounds.pickup, sounds.interactionVolume);
        }

        private void OnRelease(float throwSpeed)
        {
            // A normal drop stays quiet until the object actually hits something.
            if (sounds != null && throwSpeed > 0.1f) Play(actions, sounds.throwSound, sounds.interactionVolume);
        }

        private void Play(AudioSource source, AudioClip clip, float volume)
        {
            if (clip == null) return;
            source.pitch = Random.Range(0.97f, 1.05f);
            source.PlayOneShot(clip, volume * sounds.masterVolume);
        }
    }
}
