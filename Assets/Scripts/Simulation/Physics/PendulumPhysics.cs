using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Realistic pendulum via HingeJoint + Rigidbody.
    /// Optionally shows period label and trails.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PendulumPhysics : MonoBehaviour
    {
        [SerializeField] private Transform pivotPoint;
        [SerializeField] private float    armLength  = 1.5f;
        [SerializeField] private float    bobMass    = 1f;
        [SerializeField] private float    damping    = 0.05f;
        [SerializeField] private float    startAngle = 30f;   // degrees
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private SimpleMachines.MachineLabel periodLabel;

        private Rigidbody  _rb;
        private HingeJoint _hinge;

        private void Start()
        {
            _rb      = GetComponent<Rigidbody>();
            _rb.mass = bobMass;
            _rb.linearDamping  = damping;
            _rb.angularDamping = damping;

            // Position bob at end of arm
            transform.position = pivotPoint != null
                ? pivotPoint.position + new Vector3(Mathf.Sin(startAngle * Mathf.Deg2Rad), -Mathf.Cos(startAngle * Mathf.Deg2Rad), 0f) * armLength
                : transform.position;

            // HingeJoint connecting to pivot
            _hinge               = gameObject.AddComponent<HingeJoint>();
            _hinge.anchor        = Vector3.zero;
            _hinge.axis          = Vector3.forward;
            _hinge.autoConfigureConnectedAnchor = false;
            _hinge.connectedAnchor = pivotPoint != null ? pivotPoint.position : Vector3.up * armLength;

            // Theoretical period label: T = 2π√(L/g)
            float period = 2f * Mathf.PI * Mathf.Sqrt(armLength / Mathf.Abs(UnityEngine.Physics.gravity.y));
            periodLabel?.SetOverrideText($"T = {period:F2} s");
        }

        public void SetArmLength(float length)
        {
            armLength = length;
            float period = 2f * Mathf.PI * Mathf.Sqrt(armLength / Mathf.Abs(UnityEngine.Physics.gravity.y));
            periodLabel?.SetOverrideText($"T = {period:F2} s");
        }

        public void Release(float angle)
        {
            if (pivotPoint == null) return;
            transform.position = pivotPoint.position +
                new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad), 0f) * armLength;
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
}
