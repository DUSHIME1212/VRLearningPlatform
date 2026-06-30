using UnityEngine;

namespace VRLearning.Simulation.Biology
{
    /// <summary>
    /// Makes the heart (or any organ) pulse with a realistic two-stage beat.
    /// Stage 1 (systole): fast scale-up squeeze.
    /// Stage 2 (diastole): slow relaxation back to rest.
    /// Optionally drives a MachineLabel showing BPM.
    /// </summary>
    public class HeartBeat : MonoBehaviour
    {
        [Header("Beat Pattern")]
        [SerializeField] private float bpm              = 72f;
        [SerializeField] private float systoleScale     = 0.88f;   // squeeze in
        [SerializeField] private float diastoleScale    = 1.05f;   // expand out
        [SerializeField] private float systoleFraction  = 0.25f;   // % of beat that is systole

        [Header("Parts to Animate")]
        [SerializeField] private Transform[] heartParts;           // assign all sub-parts

        [Header("Audio")]
        [SerializeField] private AudioSource beatSource;
        [SerializeField] private AudioClip   lubClip;
        [SerializeField] private AudioClip   dubClip;

        [Header("UI")]
        [SerializeField] private SimpleMachines.MachineLabel bpmLabel;

        private Vector3[] _restScales;
        private float     _beatTimer;
        private float     _beatDuration;
        private bool      _inSystole;

        private void Start()
        {
            // Fallback: if no parts were assigned in the inspector, pulse this object itself.
            if (heartParts == null || heartParts.Length == 0)
                heartParts = new[] { transform };

            _restScales   = new Vector3[heartParts.Length];
            for (int i = 0; i < heartParts.Length; i++)
                _restScales[i] = heartParts[i].localScale;

            SetBPM(bpm);
        }

        public void SetBPM(float newBpm)
        {
            bpm           = Mathf.Clamp(newBpm, 20f, 220f);
            _beatDuration = 60f / bpm;
            bpmLabel?.SetOverrideText($"{Mathf.RoundToInt(bpm)} BPM");
        }

        private void Update()
        {
            _beatTimer += Time.deltaTime;

            float systoleDur  = _beatDuration * systoleFraction;
            float diastoleDur = _beatDuration * (1f - systoleFraction);

            if (_beatTimer < systoleDur)
            {
                // Systole — squeeze toward systoleScale
                float t = _beatTimer / systoleDur;
                ApplyScale(Mathf.SmoothStep(1f, systoleScale, t));

                if (!_inSystole)
                {
                    _inSystole = true;
                    PlayClip(lubClip);
                }
            }
            else if (_beatTimer < _beatDuration)
            {
                // Diastole — expand then settle
                float t = (_beatTimer - systoleDur) / diastoleDur;
                float s = t < 0.3f
                    ? Mathf.SmoothStep(systoleScale, diastoleScale, t / 0.3f)   // quick expand
                    : Mathf.SmoothStep(diastoleScale, 1f, (t - 0.3f) / 0.7f);   // slow settle

                ApplyScale(s);

                if (_inSystole)
                {
                    _inSystole = false;
                    PlayClip(dubClip);
                }
            }
            else
            {
                _beatTimer = 0f;
            }
        }

        private void ApplyScale(float multiplier)
        {
            for (int i = 0; i < heartParts.Length; i++)
                heartParts[i].localScale = _restScales[i] * multiplier;
        }

        private void PlayClip(AudioClip clip)
        {
            if (beatSource != null && clip != null)
                beatSource.PlayOneShot(clip);
        }
    }
}
