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

        private Quaternion _prevRot;
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
                _prevRot = wheelTransform.rotation;
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized || wheelTransform == null || weightBlock == null) return;

            // Signed rotation (deg) about the wheel's axle (world Z) this step. Measured from the
            // quaternion delta rather than eulerAngles, because the wheel sits at X=90° — a gimbal-lock
            // pose where reading eulerAngles.z is unreliable.
            Quaternion cur = wheelTransform.rotation;
            (cur * Quaternion.Inverse(_prevRot)).ToAngleAxis(out float ang, out Vector3 axis);
            if (ang > 180f) ang -= 360f;
            float delta = ang * Mathf.Sign(Vector3.Dot(axis, Vector3.forward));
            _prevRot = cur;

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
