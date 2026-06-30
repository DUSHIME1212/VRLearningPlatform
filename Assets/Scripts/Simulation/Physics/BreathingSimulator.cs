using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Animates lung / diaphragm objects through inhale/exhale cycle.
    /// Lungs scale up on inhale, diaphragm moves down.
    /// Drives labels for tidal volume, RR, and phase.
    /// </summary>
    public class BreathingSimulator : MonoBehaviour
    {
        [Header("Parts")]
        [SerializeField] private Transform   leftLung;
        [SerializeField] private Transform   rightLung;
        [SerializeField] private Transform   diaphragm;
        [SerializeField] private Transform   ribCage;

        [Header("Parameters")]
        [SerializeField] private float breathsPerMinute = 15f;
        [SerializeField] private float inhaleFraction   = 0.4f;  // % of cycle that is inhale

        [Header("Scales")]
        [SerializeField] private Vector3 lungInhaleScale   = new Vector3(1.15f, 1.2f, 1.1f);
        [SerializeField] private Vector3 lungExhaleScale   = Vector3.one;
        [SerializeField] private float   diaphragmDropY    = -0.08f;
        [SerializeField] private float   ribCageExpandX    = 0.06f;

        [Header("Labels")]
        [SerializeField] private SimpleMachines.MachineLabel phaseLabel;
        [SerializeField] private SimpleMachines.MachineLabel rrLabel;

        private Vector3 _leftLungRest, _rightLungRest, _diaphragmRest, _ribCageRest;
        private float   _cycleTimer, _cycleDuration;
        private bool    _inhaling;

        private void Start()
        {
            if (leftLung)   _leftLungRest   = leftLung.localScale;
            if (rightLung)  _rightLungRest  = rightLung.localScale;
            if (diaphragm)  _diaphragmRest  = diaphragm.localPosition;
            if (ribCage)    _ribCageRest    = ribCage.localScale;

            SetBreathRate(breathsPerMinute);
        }

        public void SetBreathRate(float bpm)
        {
            breathsPerMinute = Mathf.Clamp(bpm, 4f, 60f);
            _cycleDuration   = 60f / breathsPerMinute;
            rrLabel?.SetOverrideText($"{Mathf.RoundToInt(breathsPerMinute)} breaths/min");
        }

        private void Update()
        {
            _cycleTimer += Time.deltaTime;
            float inhaleDur  = _cycleDuration * inhaleFraction;
            float exhaleDur  = _cycleDuration * (1f - inhaleFraction);

            float t;
            if (_cycleTimer < inhaleDur)
            {
                t       = _cycleTimer / inhaleDur;
                _inhaling = true;
                phaseLabel?.SetOverrideText("Inhale");
            }
            else if (_cycleTimer < _cycleDuration)
            {
                t       = (_cycleTimer - inhaleDur) / exhaleDur;
                _inhaling = false;
                phaseLabel?.SetOverrideText("Exhale");
            }
            else
            {
                _cycleTimer = 0f;
                return;
            }

            float ease = Mathf.SmoothStep(0f, 1f, t);
            float blend = _inhaling ? ease : 1f - ease;

            if (leftLung)  leftLung.localScale  = Vector3.Lerp(_leftLungRest,   lungInhaleScale, blend);
            if (rightLung) rightLung.localScale  = Vector3.Lerp(_rightLungRest,  lungInhaleScale, blend);
            if (diaphragm) diaphragm.localPosition = _diaphragmRest + Vector3.up * (diaphragmDropY * blend);
            if (ribCage)   ribCage.localScale    = _ribCageRest + new Vector3(ribCageExpandX * blend, 0f, ribCageExpandX * blend);
        }
    }
}
