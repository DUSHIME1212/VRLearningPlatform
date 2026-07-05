using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Measures and displays a Rigidbody's speed (and optionally acceleration) on a label.
    /// Drop it on any moving body to turn it into a live physics read-out.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VelocityReadout : MonoBehaviour
    {
        [SerializeField] private SimpleMachines.MachineLabel label;
        [SerializeField] private bool showAcceleration = true;

        private Rigidbody _rb;
        private Vector3 _lastVel;

        public float Speed => _rb != null ? _rb.linearVelocity.magnitude : 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            float speed = _rb.linearVelocity.magnitude;
            float accel = (_rb.linearVelocity - _lastVel).magnitude / Mathf.Max(Time.fixedDeltaTime, 1e-5f);
            _lastVel = _rb.linearVelocity;

            if (label != null)
                label.SetOverrideText(showAcceleration
                    ? $"v {speed:F2} m/s   a {accel:F1} m/s²"
                    : $"v {speed:F2} m/s");
        }
    }
}
