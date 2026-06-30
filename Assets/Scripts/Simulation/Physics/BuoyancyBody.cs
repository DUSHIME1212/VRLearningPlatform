using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Makes a Rigidbody float in a virtual fluid volume.
    /// Place this on any object. Set fluidSurface to the Y position of the water surface.
    /// Archimedes' principle: upthrust = ρ_fluid × V_submerged × g
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyancyBody : MonoBehaviour
    {
        [Header("Fluid")]
        [SerializeField] private float fluidSurfaceY   = 0f;
        [SerializeField] private float fluidDensity    = 1000f;  // kg/m³ — water = 1000
        [SerializeField] private float fluidDrag       = 3f;
        [SerializeField] private float fluidAngularDrag = 1f;

        [Header("Object")]
        [SerializeField] private float objectDensity   = 500f;   // < 1000 → floats, > 1000 → sinks

        [Header("Visual")]
        [SerializeField] private Renderer objectRenderer;
        [SerializeField] private Color    aboveWaterColor;
        [SerializeField] private Color    belowWaterColor = new Color(0.2f, 0.4f, 0.8f, 0.6f);

        private Rigidbody _rb;
        private float     _volume;
        private float     _defaultDrag;
        private float     _defaultAngularDrag;

        private void Start()
        {
            _rb                = GetComponent<Rigidbody>();
            _defaultDrag       = _rb.linearDamping;
            _defaultAngularDrag = _rb.angularDamping;

            // Estimate volume from mass and density: V = m / ρ
            _volume = _rb.mass / objectDensity;

            if (objectRenderer)
                aboveWaterColor = objectRenderer.material.color;
        }

        private void FixedUpdate()
        {
            float objectBottom = transform.position.y - GetComponent<Collider>().bounds.extents.y;
            float objectTop    = transform.position.y + GetComponent<Collider>().bounds.extents.y;
            float height       = objectTop - objectBottom;

            float submergedFraction = Mathf.Clamp01((fluidSurfaceY - objectBottom) / height);
            bool  isSubmerged       = submergedFraction > 0f;

            if (isSubmerged)
            {
                float submergedVolume = _volume * submergedFraction;
                float upthrust        = fluidDensity * submergedVolume * Mathf.Abs(UnityEngine.Physics.gravity.y);
                _rb.AddForce(Vector3.up * upthrust, ForceMode.Force);

                _rb.linearDamping  = fluidDrag;
                _rb.angularDamping = fluidAngularDrag;
            }
            else
            {
                _rb.linearDamping  = _defaultDrag;
                _rb.angularDamping = _defaultAngularDrag;
            }

            if (objectRenderer)
                objectRenderer.material.color = Color.Lerp(aboveWaterColor, belowWaterColor, submergedFraction);
        }

        public void SetFluidDensity(float density)  => fluidDensity   = density;
        public void SetObjectDensity(float density) => objectDensity  = density;
        public void SetSurfaceY(float y)            => fluidSurfaceY  = y;
    }
}
