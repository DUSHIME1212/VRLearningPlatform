using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Launches any Rigidbody that lands on it — a trampoline / bounce pad. Bounces along the pad's
    /// up axis (or the contact normal) with a fixed impulse.
    /// </summary>
    public class BouncePad : MonoBehaviour
    {
        [SerializeField] private float bounceForce = 8f;
        [SerializeField] private bool useSurfaceNormal = true;
        [SerializeField] private AudioClip bounceClip;

        private void OnCollisionEnter(Collision c)
        {
            var rb = c.rigidbody;
            if (rb == null || rb.isKinematic) return;

            Vector3 n = useSurfaceNormal ? transform.up : c.GetContact(0).normal;
            n = n.normalized;

            // Cancel the velocity going into the surface, then add the bounce impulse.
            rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, n);
            rb.AddForce(n * bounceForce, ForceMode.Impulse);

            if (bounceClip != null) Core.AudioManager.Instance?.PlaySFX(bounceClip);
        }
    }
}
