using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Turns an object into a physics door / flap on a HingeJoint with angle limits and an optional
    /// auto-close spring. Set a connected body (the frame) in the HingeJoint, or leave it to hinge to
    /// the world. Grab-and-swing it with an XR interactor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HingeDoor : MonoBehaviour
    {
        [SerializeField] private Vector3 hingeAxis = Vector3.up; // local space
        [SerializeField] private float minAngle = -90f;
        [SerializeField] private float maxAngle = 90f;
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float closeSpring = 3f;
        [SerializeField] private float closeDamper = 1f;

        private HingeJoint _hinge;

        public float Angle => _hinge != null ? _hinge.angle : 0f;

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint>();
            if (_hinge == null) _hinge = gameObject.AddComponent<HingeJoint>();

            _hinge.axis = hingeAxis;
            _hinge.useLimits = true;
            _hinge.limits = new JointLimits { min = minAngle, max = maxAngle };

            if (autoClose)
            {
                _hinge.useSpring = true;
                _hinge.spring = new JointSpring { spring = closeSpring, damper = closeDamper, targetPosition = 0f };
            }
        }
    }
}
