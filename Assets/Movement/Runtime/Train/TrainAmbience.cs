using UnityEngine;

namespace AllAboard.Train
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class TrainAmbience : MonoBehaviour
    {
        [SerializeField] private AudioClip countryside;
        [SerializeField, Range(0f,1f)] private float volume=0.08f;
        private AudioSource source;
        public void Configure(AudioClip clip) { countryside=clip;volume=0.03f; }
        private void Awake()
        {
            source=GetComponent<AudioSource>();source.clip=countryside;
            source.loop=true;source.playOnAwake=false;source.spatialBlend=0f;
            source.volume=0f;source.dopplerLevel=0f;
            if(countryside!=null)source.Play();
        }
        private void Update()
        {
            if(source!=null)source.volume=Mathf.MoveTowards(source.volume,volume,Time.deltaTime*0.04f);
        }
        private void OnDisable(){if(source!=null)source.Pause();}
        private void OnEnable(){if(source!=null)source.UnPause();}
    }
}
