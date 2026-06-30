using UnityEngine;

namespace VRLearning.Simulation.Biology
{
    /// <summary>
    /// Physics valve using HingeJoint spring.
    /// Call Open() on systole, Close() on diastole.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    public class HeartValve : MonoBehaviour
    {
        [SerializeField] private float openAngle       = 75f;
        [SerializeField] private float closedAngle     = 0f;
        [SerializeField] private float springStiffness = 200f;
        [SerializeField] private float springDamper    = 20f;
        [SerializeField] private float openTorque      = 50f;
        [SerializeField] private MeshRenderer valveRenderer;
        [SerializeField] private Color openColor   = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color closedColor = new Color(0.5f, 0.1f, 0.1f);

        private HingeJoint _hinge;
        private Rigidbody  _rb;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint>();
            _rb    = GetComponent<Rigidbody>();
            _hinge.useSpring = true;
            _hinge.useLimits = true;
            _hinge.limits = new JointLimits
            {
                min = Mathf.Min(closedAngle, openAngle),
                max = Mathf.Max(closedAngle, openAngle)
            };
        }

        public void Open()
        {
            IsOpen = true;
            SetSpringTarget(openAngle);
            _rb.AddTorque(transform.right * openTorque, ForceMode.Impulse);
            if (valveRenderer) valveRenderer.material.color = openColor;
        }

        public void Close()
        {
            IsOpen = false;
            SetSpringTarget(closedAngle);
            if (valveRenderer) valveRenderer.material.color = closedColor;
        }

        public void Toggle() { if (IsOpen) Close(); else Open(); }

        private void SetSpringTarget(float angle)
        {
            _hinge.spring = new JointSpring
            {
                spring         = springStiffness,
                damper         = springDamper,
                targetPosition = angle
            };
        }
    }
}
