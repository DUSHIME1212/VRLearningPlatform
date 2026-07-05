using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Applies a configurable force / impulse to a target Rigidbody when <see cref="Apply"/> is
    /// called — wire it to a VR button or poke to "push", "launch" or "kick" an object.
    /// </summary>
    public class ForceApplier : MonoBehaviour
    {
        [SerializeField] private Rigidbody target;
        [SerializeField] private Vector3 direction = Vector3.up;
        [SerializeField] private float magnitude = 5f;
        [SerializeField] private ForceMode mode = ForceMode.Impulse;
        [SerializeField] private bool localSpace = true;

        /// <summary>Apply the configured force to the target.</summary>
        public void Apply()
        {
            if (target == null) return;
            Vector3 dir = localSpace ? transform.TransformDirection(direction.normalized) : direction.normalized;
            target.AddForce(dir * magnitude, mode);
        }

        /// <summary>Apply with a runtime-supplied strength multiplier (e.g. charge level 0..1).</summary>
        public void Apply(float strength01)
        {
            if (target == null) return;
            Vector3 dir = localSpace ? transform.TransformDirection(direction.normalized) : direction.normalized;
            target.AddForce(dir * magnitude * Mathf.Clamp01(strength01), mode);
        }
    }
}
