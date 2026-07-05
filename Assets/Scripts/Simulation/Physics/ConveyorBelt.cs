using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Carries Rigidbodies resting on it along a surface direction at a set speed — a conveyor belt.
    /// Needs a solid (non-trigger) Collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] private Vector3 direction = Vector3.forward; // local space
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float grip = 20f; // how quickly objects match belt speed

        private readonly HashSet<Rigidbody> _on = new HashSet<Rigidbody>();

        private void OnCollisionEnter(Collision c) { if (c.rigidbody != null) _on.Add(c.rigidbody); }
        private void OnCollisionExit(Collision c)  { if (c.rigidbody != null) _on.Remove(c.rigidbody); }

        private void FixedUpdate()
        {
            Vector3 beltVel = transform.TransformDirection(direction.normalized) * speed;
            foreach (var rb in _on)
            {
                if (rb == null || rb.isKinematic) continue;
                Vector3 target = beltVel;
                target.y = rb.linearVelocity.y; // don't fight gravity
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, target, grip * Time.fixedDeltaTime);
            }
        }
    }
}
