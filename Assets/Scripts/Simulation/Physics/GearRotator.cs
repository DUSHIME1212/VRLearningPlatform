using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Rotates like a meshing gear. If a driver gear is set, this gear turns the opposite way at a
    /// speed scaled by the tooth ratio (driverTeeth / thisTeeth). Leave driver empty to be the driver.
    /// </summary>
    public class GearRotator : MonoBehaviour
    {
        [SerializeField] private GearRotator driver;            // null = this is the driver
        [SerializeField] private float teeth = 12f;
        [SerializeField] private Vector3 axis = Vector3.forward; // local space
        [SerializeField] private float driverSpeed = 60f;        // deg/s, used only when no driver

        /// <summary>Current angular speed in degrees/second (read by driven gears).</summary>
        public float AngularSpeed { get; private set; }
        public float Teeth => teeth;

        private void Update()
        {
            AngularSpeed = driver == null
                ? driverSpeed
                : -driver.AngularSpeed * (driver.Teeth / Mathf.Max(teeth, 1f));

            transform.Rotate(axis.normalized, AngularSpeed * Time.deltaTime, Space.Self);
        }
    }
}
