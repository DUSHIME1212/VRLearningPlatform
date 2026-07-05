using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Kinematic spinning platform (turntable / merry-go-round). Rigidbodies resting on it are
    /// carried around through friction. Set the axis and speed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RotatingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 axis = Vector3.up; // local space
        [SerializeField] private float degreesPerSecond = 45f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            Vector3 worldAxis = transform.TransformDirection(axis.normalized);
            Quaternion delta = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, worldAxis);
            _rb.MoveRotation(delta * _rb.rotation);
        }
    }
}
