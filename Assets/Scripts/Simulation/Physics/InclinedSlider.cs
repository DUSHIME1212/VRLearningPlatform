using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Educational read-out for a block sliding on a ramp: shows the slope angle and the theoretical
    /// acceleration a = g(sin θ − μ cos θ). The actual motion is handled by Unity physics / colliders;
    /// this visualises the numbers.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class InclinedSlider : MonoBehaviour
    {
        [SerializeField] private Transform ramp;                 // ramp whose up axis defines the slope
        [SerializeField] private float frictionCoefficient = 0.2f;
        [SerializeField] private SimpleMachines.MachineLabel readout;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 up = ramp != null ? ramp.up : Vector3.up;
            float theta = Vector3.Angle(up, Vector3.up) * Mathf.Deg2Rad;
            float g = Mathf.Abs(UnityEngine.Physics.gravity.y);
            float a = g * (Mathf.Sin(theta) - frictionCoefficient * Mathf.Cos(theta));

            if (readout != null)
                readout.SetOverrideText($"θ = {theta * Mathf.Rad2Deg:F0}°   a = {Mathf.Max(a, 0f):F2} m/s²   v = {_rb.linearVelocity.magnitude:F2} m/s");
        }
    }
}
