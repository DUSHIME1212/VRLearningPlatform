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
                _wheelRb = wheelTransform.GetComponent<Rigidbody>();
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

            if (_wheelRb != null)
                _wheelRb.AddTorque(new Vector3(-deltaY * torqueSensitivity, 0f, 0f), ForceMode.Force);
        }
    }
}
