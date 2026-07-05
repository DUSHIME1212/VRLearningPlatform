using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Draws the predicted projectile arc for a launch, using p = p0 + v0·t + ½·g·t². Great paired
    /// with a launcher so students can aim. By default it uses this object's forward × launchSpeed;
    /// call <see cref="DrawArc"/> to preview any velocity.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        [SerializeField] private float launchSpeed = 8f;
        [SerializeField] private int points = 30;
        [SerializeField] private float timeStep = 0.1f;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            DrawArc(transform.forward * launchSpeed);
        }

        /// <summary>Draw the arc for a given initial velocity (world space).</summary>
        public void DrawArc(Vector3 initialVelocity)
        {
            _line.positionCount = points;
            Vector3 g = UnityEngine.Physics.gravity;
            for (int i = 0; i < points; i++)
            {
                float t = i * timeStep;
                Vector3 p = transform.position + initialVelocity * t + 0.5f * g * t * t;
                _line.SetPosition(i, p);
            }
        }
    }
}
