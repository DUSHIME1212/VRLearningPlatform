using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Mass-spring system (Hooke's Law demo).
    /// Attach to a hanging mass; set an anchor transform above it.
    /// SpringJoint drives real physics. Labels show force, extension, period.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpringOscillator : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private float springConstant = 50f;   // N/m — k
        [SerializeField] private float naturalLength  = 1.0f;  // metres
        [SerializeField] private float massMass       = 1f;    // kg
        [SerializeField] private float damping        = 0.3f;
        [SerializeField] private LineRenderer springLine;      // visual coil
        [SerializeField] private SimpleMachines.MachineLabel forceLabel;
        [SerializeField] private SimpleMachines.MachineLabel extensionLabel;
        [SerializeField] private SimpleMachines.MachineLabel periodLabel;

        private Rigidbody   _rb;
        private SpringJoint _joint;

        private void Start()
        {
            _rb      = GetComponent<Rigidbody>();
            _rb.mass = massMass;
            _rb.linearDamping  = damping;

            _joint                   = gameObject.AddComponent<SpringJoint>();
            _joint.spring            = springConstant;
            _joint.damper            = damping * 10f;
            _joint.minDistance       = 0f;
            _joint.maxDistance       = naturalLength;
            _joint.autoConfigureConnectedAnchor = false;
            _joint.connectedAnchor   = anchor != null ? anchor.position : transform.position + Vector3.up * naturalLength;

            // T = 2π√(m/k)
            float period = 2f * Mathf.PI * Mathf.Sqrt(massMass / springConstant);
            periodLabel?.SetOverrideText($"T = {period:F2} s");
        }

        private void Update()
        {
            if (anchor == null) return;

            float ext = Vector3.Distance(transform.position, anchor.position) - naturalLength;
            float f   = springConstant * Mathf.Max(0f, ext);

            forceLabel?.SetOverrideText($"F = {f:F1} N");
            extensionLabel?.SetOverrideText($"x = {ext:F2} m");

            if (springLine != null)
            {
                springLine.positionCount = 2;
                springLine.SetPosition(0, anchor.position);
                springLine.SetPosition(1, transform.position);
            }
        }

        public void SetSpringConstant(float k)
        {
            springConstant = k;
            if (_joint != null) _joint.spring = k;
            float period = 2f * Mathf.PI * Mathf.Sqrt(massMass / k);
            periodLabel?.SetOverrideText($"T = {period:F2} s");
        }
    }
}
