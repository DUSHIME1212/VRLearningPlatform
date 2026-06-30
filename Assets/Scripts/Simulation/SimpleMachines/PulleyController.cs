using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    public class PulleyController : MonoBehaviour
    {
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private Transform weightBlock;
        [SerializeField] private Transform ropeHandle;
        [SerializeField] private LineRenderer ropeRenderer;
        [SerializeField] private MachineLabel distanceLabel;
        [SerializeField] private AudioSource ropeAudioSource;
        [SerializeField] private float wheelRadius = 0.25f;
        [SerializeField] private float weightBaseHeight = 0.2f;
        [SerializeField] private float weightMaxHeight  = 2.0f;

        public float WeightCurrentHeight { get; private set; }

        private float _prevAngle;
        private bool _initialized;

        private void Start()
        {
            WeightCurrentHeight = weightBaseHeight;
            if (weightBlock != null)
            {
                Vector3 p = weightBlock.position;
                weightBlock.position = new Vector3(p.x, WeightCurrentHeight, p.z);
            }
            if (wheelTransform != null)
                _prevAngle = wheelTransform.eulerAngles.x;
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized || wheelTransform == null || weightBlock == null) return;

            float currentAngle = wheelTransform.eulerAngles.x;
            float delta = Mathf.DeltaAngle(_prevAngle, currentAngle);
            _prevAngle = currentAngle;

            float heightDelta = delta * Mathf.Deg2Rad * wheelRadius;
            WeightCurrentHeight = Mathf.Clamp(WeightCurrentHeight + heightDelta, weightBaseHeight, weightMaxHeight);

            Vector3 wp = weightBlock.position;
            weightBlock.position = new Vector3(wp.x, WeightCurrentHeight, wp.z);

            UpdateRope();
            HandleRopeAudio(Mathf.Abs(delta));
        }

        private void UpdateRope()
        {
            if (ropeRenderer == null) return;
            ropeRenderer.positionCount = 3;
            if (ropeHandle != null)
                ropeRenderer.SetPosition(0, ropeHandle.position);
            ropeRenderer.SetPosition(1, wheelTransform.position);
            ropeRenderer.SetPosition(2, weightBlock.position + Vector3.up * 0.12f);
        }

        private void HandleRopeAudio(float deltaAngle)
        {
            if (ropeAudioSource == null) return;
            bool moving = deltaAngle > 0.1f;
            if (moving && !ropeAudioSource.isPlaying)
            {
                ropeAudioSource.Play();
            }
            else if (!moving && ropeAudioSource.isPlaying)
            {
                ropeAudioSource.Stop();
            }
            if (moving)
                ropeAudioSource.pitch = Mathf.Clamp(deltaAngle * 0.05f, 0.8f, 2.0f);
        }

        public void SetDistanceLabelValue(float dist)
        {
            distanceLabel?.SetOverrideText($"{dist:F2} m");
        }

        public float DistanceFromTarget(float targetHeight) =>
            Mathf.Abs(WeightCurrentHeight - targetHeight);
    }
}
