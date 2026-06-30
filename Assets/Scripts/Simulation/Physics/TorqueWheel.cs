using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Lets the player spin a wheel by grabbing a handle and pulling.
    /// Demonstrates torque = Force × lever arm.
    /// Works with any Rigidbody wheel.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TorqueWheel : MonoBehaviour
    {
        [SerializeField] private float friction        = 0.5f;  // angular drag equiv
        [SerializeField] private float maxAngularSpeed = 720f;  // deg/s
        [SerializeField] private SimpleMachines.MachineLabel rpmLabel;
        [SerializeField] private SimpleMachines.MachineLabel torqueLabel;

        private Rigidbody _rb;
        private float     _appliedTorque;

        private void Awake()
        {
            _rb               = GetComponent<Rigidbody>();
            _rb.angularDamping = friction;
            _rb.useGravity     = false;
            _rb.constraints    = RigidbodyConstraints.FreezePositionX
                               | RigidbodyConstraints.FreezePositionY
                               | RigidbodyConstraints.FreezePositionZ
                               | RigidbodyConstraints.FreezeRotationY
                               | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            // Clamp angular speed
            float speed = _rb.angularVelocity.magnitude * Mathf.Rad2Deg;
            if (speed > maxAngularSpeed)
                _rb.angularVelocity = _rb.angularVelocity.normalized * (maxAngularSpeed * Mathf.Deg2Rad);

            float rpm = speed / 6f;   // deg/s → rpm
            rpmLabel?.SetOverrideText($"{rpm:F0} RPM");
            torqueLabel?.SetOverrideText($"τ = {_appliedTorque:F1} N·m");
            _appliedTorque = 0f;
        }

        /// <summary>
        /// Apply torque from a handle. leverArm = distance of force from wheel centre.
        /// </summary>
        public void ApplyHandForce(float force, float leverArm)
        {
            _appliedTorque = force * leverArm;
            _rb.AddTorque(transform.right * _appliedTorque, ForceMode.Force);
        }
    }
}
