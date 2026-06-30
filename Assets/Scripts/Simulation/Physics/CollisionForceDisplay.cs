using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Displays impulse / collision force when two objects collide.
    /// Shows force arrow and label. Great for Newton's Third Law demos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionForceDisplay : MonoBehaviour
    {
        [SerializeField] private SimpleMachines.ForceArrow forceArrow;
        [SerializeField] private SimpleMachines.MachineLabel forceLabel;
        [SerializeField] private float displayDuration = 1.5f;
        [SerializeField] private float maxForceDisplay = 50f;

        private float _clearTimer;

        private void OnCollisionEnter(Collision col)
        {
            float impulse = col.impulse.magnitude;
            float norm    = Mathf.Clamp01(impulse / maxForceDisplay);

            forceArrow?.SetForce(norm);
            forceLabel?.SetOverrideText($"F = {impulse:F1} N");
            _clearTimer = displayDuration;
        }

        private void Update()
        {
            if (_clearTimer > 0f)
            {
                _clearTimer -= Time.deltaTime;
                if (_clearTimer <= 0f)
                {
                    forceArrow?.SetForce(0f);
                    forceLabel?.SetOverrideText("F = 0 N");
                }
            }
        }
    }
}
