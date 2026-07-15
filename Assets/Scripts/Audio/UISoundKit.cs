using UnityEngine;
using UnityEngine.UI;
using VRLearning.Modules.Quiz;
using VRLearning.Parts;

namespace VRLearning.Audio
{
    /// <summary>
    /// Self-contained scene audio. Reuses simple click clips at different pitches to give feedback:
    /// quiz correct/wrong/complete chimes, UI button clicks, a click when the parts tour changes part,
    /// and an optional ambient loop. Drop one into a scene — it auto-subscribes to the scene's
    /// <see cref="QuizManager"/> and <see cref="PartsInfoPanel"/> and adds click sounds to the buttons,
    /// so no manual wiring is needed. Independent of the AudioManager singleton, so it works standalone.
    /// </summary>
    public class UISoundKit : MonoBehaviour
    {
        [Header("Clips (reuse a simple click for all — pitch does the rest)")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip wrongClip;
        [SerializeField] private AudioClip completeClip;
        [Tooltip("Optional looping background ambience. Leave empty if you have no ambient clip.")]
        [SerializeField] private AudioClip ambientClip;

        [Header("Pitch")]
        [SerializeField] private float clickPitch    = 1.0f;
        [SerializeField] private float correctPitch  = 1.25f;
        [SerializeField] private float wrongPitch    = 0.7f;
        [SerializeField] private float completePitch = 1.5f;

        [Header("Volume")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume     = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float ambientVolume = 0.25f;

        /// <summary>Name of the last sound played ("click"/"correct"/"wrong"/"complete") — for testing.</summary>
        public string LastPlayed { get; private set; }

        private AudioSource _sfx;
        private AudioSource _ambient;
        private QuizManager _quiz;
        private PartsInfoPanel _parts;

        private void Awake()
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f; // 2D — UI feedback

            _ambient = gameObject.AddComponent<AudioSource>();
            _ambient.playOnAwake = false;
            _ambient.spatialBlend = 0f;
            _ambient.loop = true;
        }

        private void OnEnable()
        {
            _quiz = FindFirstObjectByType<QuizManager>(FindObjectsInactive.Include);
            if (_quiz != null)
            {
                _quiz.Answered += OnAnswered;
                _quiz.Completed += PlayComplete;
            }

            _parts = FindFirstObjectByType<PartsInfoPanel>(FindObjectsInactive.Include);
            if (_parts != null) _parts.PartChanged += OnPartChanged; // "part selected" click (fires on Next/Prev/first show)
        }

        private void OnDisable()
        {
            if (_quiz != null) { _quiz.Answered -= OnAnswered; _quiz.Completed -= PlayComplete; }
            if (_parts != null) _parts.PartChanged -= OnPartChanged;
        }

        // Click when the parts tour changes — but stay silent if the part has narration, so the
        // click doesn't talk over the voice-over.
        private void OnPartChanged()
        {
            if (_parts != null && _parts.Current != null && _parts.Current.NarrationClip != null) return;
            PlayClick();
        }

        private void Start()
        {
            // Click sound on existing buttons. Skip Next/Prev — the parts tour already clicks via PartChanged,
            // and skip answer buttons (they spawn later and give the correct/wrong chime instead).
            foreach (var btn in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (btn.name == "NextButton" || btn.name == "PrevButton") continue;
                btn.onClick.AddListener(PlayClick);
            }

            if (ambientClip != null)
            {
                _ambient.clip = ambientClip;
                _ambient.volume = ambientVolume;
                _ambient.Play();
            }
        }

        private void OnAnswered(bool correct) => PlayAnswered(correct);

        public void PlayClick() => Play(clickClip, clickPitch, "click");
        public void PlayAnswered(bool correct) =>
            Play(correct ? correctClip : wrongClip, correct ? correctPitch : wrongPitch, correct ? "correct" : "wrong");
        public void PlayComplete() => Play(completeClip, completePitch, "complete");

        private void Play(AudioClip clip, float pitch, string label)
        {
            LastPlayed = label;
            if (clip == null || _sfx == null) return;
            _sfx.pitch = pitch;
            _sfx.PlayOneShot(clip, sfxVolume);
        }
    }
}
