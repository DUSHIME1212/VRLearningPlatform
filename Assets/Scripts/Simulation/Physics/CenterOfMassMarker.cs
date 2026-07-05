using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Visualises a Rigidbody's centre of mass (and a downward plumb line for tip-over stability).
    /// Shows a gizmo in the editor and, optionally, a small runtime marker sphere in VR.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CenterOfMassMarker : MonoBehaviour
    {
        [SerializeField] private float size = 0.05f;
        [SerializeField] private Color color = Color.yellow;
        [SerializeField] private bool showRuntimeMarker = true;
        [SerializeField] private float plumbLength = 2f;

        private Rigidbody _rb;
        private Transform _marker;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (!showRuntimeMarker) return;
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "CoM Marker";
            var col = s.GetComponent<Collider>();
            if (col != null) Destroy(col);
            s.transform.localScale = Vector3.one * size * 2f;
            var mr = s.GetComponent<MeshRenderer>();
            if (mr != null) mr.material.color = color;
            _marker = s.transform;
        }

        private void Update()
        {
            if (_marker != null && _rb != null)
                _marker.position = _rb.worldCenterOfMass;
        }

        private void OnDrawGizmos()
        {
            var rb = _rb != null ? _rb : GetComponent<Rigidbody>();
            if (rb == null) return;
            Gizmos.color = color;
            Gizmos.DrawSphere(rb.worldCenterOfMass, size);
            Gizmos.DrawLine(rb.worldCenterOfMass, rb.worldCenterOfMass + Vector3.down * plumbLength);
        }
    }
}
