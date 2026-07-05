using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Trigger volume that applies a steady directional wind force (with optional gusts) to any
    /// Rigidbody inside it. Put a trigger Collider on this object.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WindForceZone : MonoBehaviour
    {
        [SerializeField] private Vector3 direction = Vector3.forward; // local space
        [SerializeField] private float strength = 5f;
        [SerializeField] private float gustAmount = 0.4f;
        [SerializeField] private float gustSpeed = 1.5f;

        private readonly HashSet<Rigidbody> _inside = new HashSet<Rigidbody>();

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null) _inside.Add(other.attachedRigidbody);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody != null) _inside.Remove(other.attachedRigidbody);
        }

        private void FixedUpdate()
        {
            float gust = 1f + Mathf.Sin(Time.time * gustSpeed) * gustAmount;
            Vector3 force = transform.TransformDirection(direction.normalized) * strength * gust;
            foreach (var rb in _inside)
                if (rb != null && !rb.isKinematic)
                    rb.AddForce(force, ForceMode.Force);
        }
    }
}
