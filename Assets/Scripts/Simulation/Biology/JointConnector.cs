using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRLearning.Simulation.Biology
{
    /// <summary>
    /// Add this to any anatomy sub-part.
    /// It auto-creates a SpringJoint back to the root body at Start,
    /// and detaches/reattaches when the player grabs/releases the part.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class JointConnector : MonoBehaviour
    {
        public enum JointMode { Spring, Hinge, Fixed }

        [Header("Root")]
        [SerializeField] private Rigidbody anchorBody;   // assign HeartRoot Rigidbody

        [Header("Joint Settings")]
        [SerializeField] private JointMode mode = JointMode.Spring;
        [SerializeField] private float springForce   = 800f;
        [SerializeField] private float springDamper  = 40f;
        [SerializeField] private float breakForce    = Mathf.Infinity;   // Infinity = unbreakable
        [SerializeField] private Vector3 hingeAxis   = Vector3.up;
        [SerializeField] private Vector2 hingeLimits = new Vector2(-60f, 60f);

        [Header("Grab Response")]
        [SerializeField] private bool detachOnGrab   = false;  // true → fully detach when grabbed
        [SerializeField] private bool reattachOnRelease = true;

        private Rigidbody _rb;
        private Joint     _joint;
        private Vector3   _localAnchorOffset;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            var grab = GetComponent<XRGrabInteractable>();
            grab.selectEntered.AddListener(OnGrabbed);
            grab.selectExited.AddListener(OnReleased);
        }

        private void Start()
        {
            if (anchorBody == null)
            {
                Debug.LogWarning($"[JointConnector] {name}: anchorBody not assigned.", this);
                return;
            }
            _localAnchorOffset = anchorBody.transform.InverseTransformPoint(transform.position);
            CreateJoint();
        }

        private void CreateJoint()
        {
            switch (mode)
            {
                case JointMode.Spring:
                    var spring = gameObject.AddComponent<SpringJoint>();
                    spring.connectedBody    = anchorBody;
                    spring.spring           = springForce;
                    spring.damper           = springDamper;
                    spring.autoConfigureConnectedAnchor = false;
                    spring.connectedAnchor  = _localAnchorOffset;
                    spring.breakForce       = breakForce;
                    spring.enableCollision  = true;
                    _joint = spring;
                    break;

                case JointMode.Hinge:
                    var hinge = gameObject.AddComponent<HingeJoint>();
                    hinge.connectedBody   = anchorBody;
                    hinge.axis            = hingeAxis;
                    hinge.useLimits       = true;
                    hinge.limits          = new JointLimits { min = hingeLimits.x, max = hingeLimits.y };
                    hinge.breakForce      = breakForce;
                    hinge.enableCollision = true;
                    _joint = hinge;
                    break;

                case JointMode.Fixed:
                    var fixed_ = gameObject.AddComponent<FixedJoint>();
                    fixed_.connectedBody = anchorBody;
                    fixed_.breakForce    = breakForce;
                    _joint = fixed_;
                    break;
            }
        }

        private void OnGrabbed(SelectEnterEventArgs _)
        {
            if (detachOnGrab && _joint != null)
            {
                Destroy(_joint);
                _joint = null;
            }
        }

        private void OnReleased(SelectExitEventArgs _)
        {
            if (reattachOnRelease && _joint == null && anchorBody != null)
                CreateJoint();
        }

        public void Detach()
        {
            if (_joint != null) { Destroy(_joint); _joint = null; }
        }

        public void Reattach()
        {
            if (_joint == null && anchorBody != null) CreateJoint();
        }
    }
}
