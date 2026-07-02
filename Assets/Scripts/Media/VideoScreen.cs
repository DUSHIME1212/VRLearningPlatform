using UnityEngine;
using UnityEngine.Video;

namespace VRLearning.Media
{
    /// <summary>
    /// World-space video screen. Renders a VideoClip (or streaming URL) onto a
    /// RenderTexture that is displayed on this object's Renderer, with simple
    /// play / pause / restart controls you can wire to XR buttons or UI events.
    ///
    /// Placeholder-friendly: leave <see cref="clip"/> and <see cref="url"/> empty
    /// and assign a clip later in the Inspector, or at runtime via SetClip / SetUrl.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class VideoScreen : MonoBehaviour
    {
        [Header("Source — leave empty to assign later (placeholder)")]
        [Tooltip("Video asset to play. Takes priority over URL when set.")]
        [SerializeField] private VideoClip clip;
        [Tooltip("Streaming URL, used only when no clip is assigned.")]
        [SerializeField] private string url;

        [Header("Screen")]
        [Tooltip("Renderer that displays the video. Defaults to this object's Renderer.")]
        [SerializeField] private Renderer screenRenderer;
        [SerializeField] private Vector2Int renderTextureSize = new Vector2Int(1280, 720);

        [Header("Playback")]
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private bool loop = true;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.8f;

        private VideoPlayer _player;
        private AudioSource _audio;
        private RenderTexture _rt;

        /// <summary>True while the video is actively playing.</summary>
        public bool IsPlaying => _player != null && _player.isPlaying;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;

            if (screenRenderer == null)
                screenRenderer = GetComponent<Renderer>();

            SetupPlayer();
        }

        private void Start()
        {
            if (playOnStart && HasSource)
                Play();
        }

        private void OnDestroy()
        {
            if (_rt != null)
            {
                _rt.Release();
                Destroy(_rt);
            }
        }

        private bool HasSource => clip != null || !string.IsNullOrEmpty(url);

        private void SetupPlayer()
        {
            _rt = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0)
            {
                name = "VideoScreen_RT"
            };
            _rt.Create();

            if (screenRenderer != null)
                screenRenderer.material.mainTexture = _rt;

            _player = gameObject.GetComponent<VideoPlayer>();
            if (_player == null) _player = gameObject.AddComponent<VideoPlayer>();

            _player.playOnAwake = false;
            _player.isLooping = loop;
            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.targetTexture = _rt;
            _player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _player.SetTargetAudioSource(0, _audio);
            _audio.volume = volume;

            ApplySource();
        }

        private void ApplySource()
        {
            if (_player == null) return;

            if (clip != null)
            {
                _player.source = VideoSource.VideoClip;
                _player.clip = clip;
            }
            else
            {
                _player.source = VideoSource.Url;
                _player.url = url;
            }
        }

        // ---- Public controls (wire these to buttons / UnityEvents) ----

        public void Play()
        {
            if (!HasSource)
            {
                Debug.LogWarning($"{name}: VideoScreen has no clip or URL assigned.", this);
                return;
            }
            _player.Play();
        }

        public void Pause()
        {
            if (_player != null) _player.Pause();
        }

        public void TogglePlayPause()
        {
            if (IsPlaying) Pause();
            else Play();
        }

        public void Restart()
        {
            if (_player == null) return;
            _player.Stop();
            Play();
        }

        public void Stop()
        {
            if (_player != null) _player.Stop();
        }

        /// <summary>Assign a clip at runtime (clears any URL) and refresh the source.</summary>
        public void SetClip(VideoClip newClip)
        {
            clip = newClip;
            url = null;
            ApplySource();
        }

        /// <summary>Assign a streaming URL at runtime (clears any clip) and refresh the source.</summary>
        public void SetUrl(string newUrl)
        {
            url = newUrl;
            clip = null;
            ApplySource();
        }

        public void SetVolume(float value)
        {
            volume = Mathf.Clamp01(value);
            if (_audio != null) _audio.volume = volume;
        }
    }
}
