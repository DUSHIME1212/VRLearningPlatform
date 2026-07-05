using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Makes a Rigidbody hover at a target height with a spring-damper (PD) controller, plus a gentle
    /// bob — magnetic-levitation / anti-gravity float. Gravity is disabled while levitating.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Levitator : MonoBehaviour
    {
        [SerializeField] private float hoverHeight = 1.2f;  // above the start height
        [SerializeField] private float stiffness = 40f;
        [SerializeField] private float damping = 8f;
        [SerializeField] private float bobAmplitude = 0.1f;
        [SerializeField] private float bobSpeed = 1.5f;

        private Rigidbody _rb;
        private float _baseY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _baseY = transform.position.y;
        }

        private void FixedUpdate()
        {
            float target = _baseY + hoverHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            float error = target - _rb.position.y;
            float force = error * stiffness - _rb.linearVelocity.y * damping;
            _rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
        }
    }
}
