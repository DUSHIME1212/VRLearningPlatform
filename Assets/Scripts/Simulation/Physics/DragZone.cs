using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Adds extra linear/angular drag to Rigidbodies while they are inside this trigger volume —
    /// water, mud or thick-air resistance. Restores their original drag on exit.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DragZone : MonoBehaviour
    {
        [SerializeField] private float extraLinearDrag = 3f;
        [SerializeField] private float extraAngularDrag = 2f;

        private readonly Dictionary<Rigidbody, Vector2> _previous = new Dictionary<Rigidbody, Vector2>();

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || _previous.ContainsKey(rb)) return;
            _previous[rb] = new Vector2(rb.linearDamping, rb.angularDamping);
            rb.linearDamping  += extraLinearDrag;
            rb.angularDamping += extraAngularDrag;
        }

        private void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null || !_previous.TryGetValue(rb, out var prev)) return;
            rb.linearDamping  = prev.x;
            rb.angularDamping = prev.y;
            _previous.Remove(rb);
        }
    }
}
