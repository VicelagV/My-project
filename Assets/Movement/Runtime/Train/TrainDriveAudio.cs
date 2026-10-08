using System.Linq;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainDriveAudio : MonoBehaviour
    {
        [SerializeField] private TrainController train;
        [SerializeField] private AudioClip steamLoop;
        [SerializeField] private AudioClip rollingLoop;
        [SerializeField] private AudioClip chuffLoop;
        [SerializeField, Range(0f,1f)] private float chuffVolume=0.22f;
        [SerializeField, Range(0f, 1f)] private float steamVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] private float rollingVolume = 0.18f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.6f;
        [SerializeField, Min(1f)] private float referenceSpeed = 12f;
        private AudioSource steamSource;
        private AudioSource chuffSource;
        private AudioSource[] rollingSources;
        private TrainCar[] rollingCars;
        public float SteamVolume => steamSource != null ? steamSource.volume : 0f;
        public float ChuffVolume => chuffSource != null ? chuffSource.volume : 0f;
        public float RollingVolume => rollingSources != null && rollingSources.Length > 0 ? rollingSources.Max(x => x != null ? x.volume : 0f) : 0f;
        public void Configure(TrainController controller, AudioClip steam, AudioClip rolling,AudioClip chuff=null)
        { train = controller; steamLoop = steam; rollingLoop = rolling;chuffLoop=chuff; }
        private void Start() => Initialize();
        private void Update() => Tick(Time.deltaTime);
        private void Initialize()
        {
            if (steamSource != null || train == null) return;
            rollingCars = train.Consist.ToArray();
            if (rollingCars.Length == 0) return;
            steamSource = Source(rollingCars[0].transform, "Steam Sound", new Vector3(0f, 2f, 2f), steamLoop);
            chuffSource=Source(rollingCars[0].transform,"Locomotive Chuff",new Vector3(0f,1.2f,6f),chuffLoop);
            rollingSources = rollingCars.Select(car => Source(car.transform, "Rolling Sound", new Vector3(0f, 0.7f, 0f), rollingLoop)).ToArray();
        }
        private static AudioSource Source(Transform car, string name, Vector3 position, AudioClip clip)
        {
            var root = new GameObject(name); root.transform.SetParent(car, false); root.transform.localPosition = position;
            var source = root.AddComponent<AudioSource>(); source.clip = clip; source.loop = true; source.playOnAwake = false;
            source.spatialBlend = 1f; source.minDistance = 4f; source.maxDistance = 80f; source.dopplerLevel = 0f; source.volume = 0f;
            if (clip != null) source.Play();
            return source;
        }
        public void Tick(float deltaTime)
        {
            Initialize();
            if (steamSource == null || deltaTime <= 0f) return;
            float rolling = Mathf.Clamp01(Mathf.Abs(train.Speed) / referenceSpeed);
            float steam = train.Resources.Steam > 0f && !train.Braking ? Mathf.Clamp01(train.Throttle) : 0f;
            steamSource.volume = Mathf.MoveTowards(steamSource.volume, steam * steamVolume, deltaTime * steamVolume / fadeSeconds);
            steamSource.pitch = Mathf.Lerp(0.85f, 1.1f, steam);
            float effort=steam*Mathf.Clamp01(Mathf.Abs(train.Speed)/2f);
            chuffSource.volume=Mathf.MoveTowards(chuffSource.volume,effort*chuffVolume,deltaTime*chuffVolume/fadeSeconds);
            chuffSource.pitch=Mathf.Lerp(0.45f,1.55f,rolling);
            for (int i = 0; i < rollingSources.Length; i++)
            {
                if (rollingSources[i] == null) continue;
                float level = train.Contains(rollingCars[i]) ? rolling : 0f;
                rollingSources[i].volume = Mathf.MoveTowards(rollingSources[i].volume, level * rollingVolume, deltaTime * rollingVolume / fadeSeconds);
                rollingSources[i].pitch = Mathf.Lerp(0.65f, 1.25f, rolling);
            }
        }
        private void OnDisable()
        {
            if (steamSource != null) steamSource.volume = 0f;
            if(chuffSource!=null)chuffSource.volume=0f;
            if (rollingSources != null) foreach (var source in rollingSources) if (source != null) source.volume = 0f;
        }
        private void OnDestroy()
        {
            if (steamSource != null) Destroy(steamSource.gameObject);
            if(chuffSource!=null)Destroy(chuffSource.gameObject);
            if (rollingSources != null) foreach (var source in rollingSources) if (source != null) Destroy(source.gameObject);
        }
    }
}
