using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Generic seesaw / balance beam built on a HingeJoint. Reports its tilt angle and which side is
    /// down, and (optionally) shows it on a label. Reusable for any balance experiment.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    public class BalanceBeam : MonoBehaviour
    {
        [SerializeField] private SimpleMachines.MachineLabel tiltLabel;
        [SerializeField] private float balancedThreshold = 3f; // degrees

        private HingeJoint _hinge;

        public float Angle => _hinge != null ? _hinge.angle : 0f;

        /// <summary>-1 = left down, 0 = balanced, +1 = right down.</summary>
        public int Side => Angle < -balancedThreshold ? -1 : Angle > balancedThreshold ? 1 : 0;

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint>();
        }

        private void Update()
        {
            if (tiltLabel != null)
                tiltLabel.SetOverrideText(Side == 0 ? "Balanced" : $"{Angle:F0}°");
        }
    }
}
