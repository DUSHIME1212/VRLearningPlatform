using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRLearning.Simulation.SimpleMachines
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class RopeHandle : MonoBehaviour
    {
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private float torqueSensitivity = 5f;
        [SerializeField] private float trackOffsetX = 0.3f;
        [SerializeField] private float trackMinY = 0.3f;
        [SerializeField] private float trackMaxY = 2.0f;

        private Rigidbody _handleRb;
        private Rigidbody _wheelRb;
        private HingeJoint _wheelHinge;
        private bool _isHeld;
        private Vector3 _prevPos;

        private void Awake()
        {
            _handleRb = GetComponent<Rigidbody>();
            _handleRb.isKinematic = true;

            var grab = GetComponent<XRGrabInteractable>();
            grab.selectEntered.AddListener(OnGrabbed);
            grab.selectExited.AddListener(OnReleased);

            if (wheelTransform != null)
            {
                _wheelRb = wheelTransform.GetComponent<Rigidbody>();
                _wheelHinge = wheelTransform.GetComponent<HingeJoint>();
            }
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            _isHeld = true;
            _handleRb.isKinematic = false;
            _prevPos = transform.position;
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            _isHeld = false;
            _handleRb.isKinematic = true;
        }

        private void FixedUpdate()
        {
            if (!_isHeld || wheelTransform == null) return;

            // Clamp handle to vertical track beside the wheel
            Vector3 pos = transform.position;
            float clampedY = Mathf.Clamp(pos.y, trackMinY, trackMaxY);
            transform.position = new Vector3(
                wheelTransform.position.x + trackOffsetX,
                clampedY,
                wheelTransform.position.z
            );

            // Velocity cap to prevent wild swings
            _handleRb.linearVelocity = Vector3.ClampMagnitude(_handleRb.linearVelocity, 2f);

            // Translate handle Y motion to wheel torque
            float deltaY = (_prevPos - transform.position).y;
            _prevPos = transform.position;

            if (_wheelRb != null && _wheelHinge != null)
            {
                // Torque must be applied about the hinge's ACTUAL world-space axis (transformed
                // from its local-space axis by the wheel's rotation) — a hardcoded world axis
                // silently does nothing if it doesn't match the joint's one free rotation axis.
                Vector3 worldAxis = wheelTransform.TransformDirection(_wheelHinge.axis).normalized;
                _wheelRb.AddTorque(worldAxis * (deltaY * torqueSensitivity), ForceMode.Force);
            }
        }
    }
}
