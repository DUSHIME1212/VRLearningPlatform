using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    public class WeightSnapZone : MonoBehaviour
    {
        [SerializeField] private Transform snapAnchor;

        public bool IsOccupied { get; private set; }
        public WeightBlock Occupant { get; private set; }

        /// <summary>
        /// Signed horizontal distance of this notch from the fulcrum, in the lever plank's
        /// local space (localPosition.x). Sign tells the side (negative = left, positive = right);
        /// the magnitude is the moment arm used for torque = weight * distance.
        /// </summary>
        public float SignedDistance => transform.localPosition.x;

        /// <summary>Absolute moment arm from the fulcrum.</summary>
        public float MomentArm => Mathf.Abs(transform.localPosition.x);

        private void Awake()
        {
            // Create a snapAnchor child if none assigned
            if (snapAnchor == null)
            {
                var anchor = new GameObject("SnapAnchor");
                anchor.transform.SetParent(transform);
                anchor.transform.localPosition = Vector3.zero;
                anchor.transform.localRotation = Quaternion.identity;
                snapAnchor = anchor.transform;
            }
        }

        public void Accept(WeightBlock block)
        {
            if (IsOccupied) return;

            Occupant = block;
            IsOccupied = true;

            var rb = block.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            block.transform.SetParent(snapAnchor);
            block.transform.localPosition = Vector3.zero;
            block.transform.localRotation = Quaternion.identity;

            UnityEngine.Physics.SyncTransforms();
        }

        public void Release()
        {
            if (Occupant == null) return;

            var rb = Occupant.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            Occupant.transform.SetParent(null);
            Occupant = null;
            IsOccupied = false;
        }
    }
}
