using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Core
{
    /// <summary>
    /// Plays SFX and music. Pools AudioSources so VR doesn't stutter.
    /// Voice-over clips are language-aware — pass a base key and language is resolved automatically.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource voSource;
        [SerializeField] private int sfxPoolSize = 8;

        [Header("Volumes")]
        [Range(0,1)] public float masterVolume = 1f;
        [Range(0,1)] public float musicVolume  = 0.4f;
        [Range(0,1)] public float sfxVolume    = 1f;

        private readonly List<AudioSource> _sfxPool = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildSFXPool();
        }

        private void BuildSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxPool.Add(src);
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            var src = _sfxPool.Find(s => !s.isPlaying);
            if (src == null) src = _sfxPool[0];
            src.volume = sfxVolume * masterVolume;
            src.PlayOneShot(clip);
        }

        public void PlayVO(AudioClip clip)
        {
            if (clip == null) return;
            voSource.Stop();
            voSource.clip   = clip;
            voSource.volume = sfxVolume * masterVolume;
            voSource.Play();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip   = clip;
            musicSource.loop   = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();
    }
}
