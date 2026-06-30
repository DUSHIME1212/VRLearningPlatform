using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    public class WeightSnapZone : MonoBehaviour
    {
        [SerializeField] private Transform snapAnchor;

        public bool IsOccupied { get; private set; }
        public WeightBlock Occupant { get; private set; }

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
