using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Simulates a magnetic field source that attracts or repels MagneticParticle objects.
    /// Use for electron/ion demos, magnetic separation, etc.
    /// </summary>
    public class MagneticField : MonoBehaviour
    {
        [SerializeField] private float fieldStrength  = 10f;
        [SerializeField] private float fieldRadius    = 3f;
        [SerializeField] private bool  attract        = true;   // false = repel
        [SerializeField] private float falloffPower   = 2f;     // 1=linear, 2=inverse square

        [Header("Visual")]
        [SerializeField] private Light  fieldGlow;
        [SerializeField] private float  glowIntensityMax = 3f;

        private void Update()
        {
            if (fieldGlow)
                fieldGlow.intensity = attract ? glowIntensityMax : glowIntensityMax * 0.5f;
        }

        // Called by MagneticParticle in FixedUpdate
        public Vector3 GetForceOn(Rigidbody rb, float particleCharge)
        {
            Vector3 dir  = transform.position - rb.position;
            float   dist = Mathf.Max(dir.magnitude, 0.1f);
            if (dist > fieldRadius) return Vector3.zero;

            float normalizedDist = dist / fieldRadius;
            float magnitude      = fieldStrength * particleCharge
                                 / Mathf.Pow(normalizedDist, falloffPower);

            return (attract ? dir.normalized : -dir.normalized) * magnitude;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = attract ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(transform.position, fieldRadius);
        }
    }

    /// <summary>
    /// Put on any Rigidbody that should react to MagneticField sources.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MagneticParticle : MonoBehaviour
    {
        [SerializeField] private float charge = 1f;  // positive = attracted by attract-field

        private Rigidbody       _rb;
        private MagneticField[] _fields;

        private void Start()
        {
            _rb     = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _fields = FindObjectsByType<MagneticField>(FindObjectsInactive.Exclude);
        }

        private void FixedUpdate()
        {
            foreach (var field in _fields)
                _rb.AddForce(field.GetForceOn(_rb, charge), ForceMode.Force);
        }
    }
}
