using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// N-body orbital gravity simulator.
    /// Add to all bodies (planets, moons, stars).
    /// Each body attracts every other GravityOrbit body using F = G*m1*m2/r²
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityOrbit : MonoBehaviour
    {
        public static float GravitationalConstant = 6.674f;  // scaled for scene units

        [SerializeField] private float mass       = 1f;
        [SerializeField] private bool  isStationary = false;  // true for the star/sun
        [SerializeField] private TrailRenderer orbitTrail;
        [SerializeField] private SimpleMachines.MachineLabel velocityLabel;

        private Rigidbody _rb;

        private static GravityOrbit[] _allBodies;

        private void OnEnable()
        {
            _allBodies = FindObjectsByType<GravityOrbit>(FindObjectsInactive.Exclude);
        }

        private void Start()
        {
            _rb      = GetComponent<Rigidbody>();
            _rb.mass = mass;
            _rb.useGravity = false;   // we handle gravity ourselves
            if (isStationary) _rb.isKinematic = true;
        }

        private void FixedUpdate()
        {
            if (isStationary) return;

            foreach (var other in _allBodies)
            {
                if (other == this) continue;

                Vector3 dir  = other.transform.position - transform.position;
                float   dist = Mathf.Max(dir.magnitude, 0.5f);   // clamp to avoid singularity
                float   force = GravitationalConstant * (_rb.mass * other._rb.mass) / (dist * dist);
                _rb.AddForce(dir.normalized * force, ForceMode.Force);
            }

            velocityLabel?.SetOverrideText($"v = {_rb.linearVelocity.magnitude:F1} m/s");
        }

        public void SetInitialVelocity(Vector3 velocity)
        {
            _rb = GetComponent<Rigidbody>();
            _rb.linearVelocity = velocity;
        }
    }
}
