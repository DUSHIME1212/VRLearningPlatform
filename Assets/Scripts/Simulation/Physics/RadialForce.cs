using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Pulls (or pushes) nearby Rigidbodies toward this point — a gravity well / repulsor.
    /// Positive strength attracts, negative repels; optional inverse-square falloff.
    /// </summary>
    public class RadialForce : MonoBehaviour
    {
        [SerializeField] private float strength = 20f;      // + attract, - repel
        [SerializeField] private float radius = 5f;
        [SerializeField] private bool inverseSquare = true;
        [SerializeField] private LayerMask affects = ~0;

        private readonly Collider[] _hits = new Collider[32];

        private void FixedUpdate()
        {
            int n = UnityEngine.Physics.OverlapSphereNonAlloc(transform.position, radius, _hits, affects);
            for (int i = 0; i < n; i++)
            {
                var rb = _hits[i].attachedRigidbody;
                if (rb == null || rb.isKinematic) continue;

                Vector3 dir = transform.position - rb.worldCenterOfMass;
                float dist = Mathf.Max(dir.magnitude, 0.1f);
                float mag = strength * (inverseSquare ? 1f / (dist * dist) : 1f);
                rb.AddForce(dir.normalized * mag, ForceMode.Force);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = strength >= 0f ? Color.cyan : Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
