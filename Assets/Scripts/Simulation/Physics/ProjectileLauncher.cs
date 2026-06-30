using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Fires a Rigidbody projectile at a given angle and speed.
    /// Draws the predicted parabola and shows range/max-height labels.
    /// Respects gravity so it matches physics exactly.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject  projectilePrefab;
        [SerializeField] private float       launchSpeed   = 8f;
        [SerializeField] [Range(0, 90)] private float launchAngle = 45f;
        [SerializeField] private int         trajectorySteps = 60;
        [SerializeField] private LineRenderer trajectoryLine;
        [SerializeField] private SimpleMachines.MachineLabel rangeLabel;
        [SerializeField] private SimpleMachines.MachineLabel heightLabel;
        [SerializeField] private Transform   launchPoint;

        private float _g;

        private void Start()
        {
            _g = Mathf.Abs(UnityEngine.Physics.gravity.y);
            DrawTrajectory();
        }

        private void OnValidate() => DrawTrajectory();

        public void Launch()
        {
            if (projectilePrefab == null) return;
            var origin = launchPoint != null ? launchPoint : transform;
            var go = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
            var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
            rb.linearVelocity = LaunchVelocity();
            Destroy(go, 10f);
        }

        public void SetAngle(float angle)
        {
            launchAngle = Mathf.Clamp(angle, 0f, 90f);
            DrawTrajectory();
        }

        public void SetSpeed(float speed)
        {
            launchSpeed = speed;
            DrawTrajectory();
        }

        private Vector3 LaunchVelocity()
        {
            float rad = launchAngle * Mathf.Deg2Rad;
            Vector3 forward = transform.forward;
            return (forward * Mathf.Cos(rad) + Vector3.up * Mathf.Sin(rad)) * launchSpeed;
        }

        private void DrawTrajectory()
        {
            if (trajectoryLine == null) return;

            trajectoryLine.positionCount = trajectorySteps;
            Vector3 origin = launchPoint != null ? launchPoint.position : transform.position;
            Vector3 vel    = LaunchVelocity();

            float dt = 3f / trajectorySteps;
            for (int i = 0; i < trajectorySteps; i++)
            {
                float t = i * dt;
                Vector3 pos = origin + vel * t + 0.5f * UnityEngine.Physics.gravity * t * t;
                trajectoryLine.SetPosition(i, pos);
            }

            // Analytic range and max height (flat ground assumption)
            float rad    = launchAngle * Mathf.Deg2Rad;
            float range  = (launchSpeed * launchSpeed * Mathf.Sin(2f * rad)) / _g;
            float height = (launchSpeed * launchSpeed * Mathf.Sin(rad) * Mathf.Sin(rad)) / (2f * _g);
            rangeLabel?.SetOverrideText($"Range = {range:F1} m");
            heightLabel?.SetOverrideText($"Max H = {height:F1} m");
        }
    }
}
