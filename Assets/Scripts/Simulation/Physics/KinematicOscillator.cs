using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Drives a kinematic platform back and forth along an axis with simple-harmonic motion — a moving
    /// platform / piston / elevator. Rigidbodies resting on it are carried along.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicOscillator : MonoBehaviour
    {
        [SerializeField] private Vector3 axis = Vector3.up; // local space at start
        [SerializeField] private float amplitude = 1f;
        [SerializeField] private float speed = 1f;

        private Rigidbody _rb;
        private Vector3 _center;
        private Vector3 _worldDir;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _center = transform.position;
            _worldDir = transform.TransformDirection(axis.normalized);
        }

        private void FixedUpdate()
        {
            Vector3 target = _center + _worldDir * (Mathf.Sin(Time.time * speed) * amplitude);
            _rb.MovePosition(target);
        }
    }
}
