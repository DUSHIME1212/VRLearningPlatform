using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    public class LeverController : MonoBehaviour
    {
        public enum LeverSide { Left, Balanced, Right }

        [SerializeField] private WeightSnapZone leftZone;
        [SerializeField] private WeightSnapZone rightZone;
        [SerializeField] private MeshRenderer leverRenderer;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private AudioClip creakClip;
        [SerializeField] private AudioClip thudClip;
        [SerializeField] private ForceArrow leftArrow;
        [SerializeField] private ForceArrow rightArrow;

        private HingeJoint _hinge;
        private Rigidbody _rb;
        private float _lastCreakTime;
        private const float CreakCooldown = 0.8f;

        private static readonly Color ColorBalanced = new Color(0.2f, 0.8f, 0.3f);
        private static readonly Color ColorTilting   = new Color(0.9f, 0.8f, 0.1f);
        private static readonly Color ColorTipped    = new Color(0.9f, 0.2f, 0.1f);

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint>();
            _rb    = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdateVisualFeedback();
            CheckCreakSFX();
            UpdateForceArrows();
        }

        public float CurrentAngle => _hinge != null ? _hinge.angle : 0f;

        public LeverSide TippedSide
        {
            get
            {
                float angle = CurrentAngle;
                if (angle < -5f)  return LeverSide.Left;
                if (angle >  5f)  return LeverSide.Right;
                return LeverSide.Balanced;
            }
        }

        private void UpdateVisualFeedback()
        {
            if (leverRenderer == null) return;

            float abs = Mathf.Abs(CurrentAngle);
            Color c;
            if (abs < 2f)        c = ColorBalanced;
            else if (abs < 20f)  c = ColorTilting;
            else                 c = ColorTipped;

            leverRenderer.material.color = c;
        }

        private void CheckCreakSFX()
        {
            if (creakClip == null) return;
            float angularSpeed = Mathf.Abs(_rb.angularVelocity.z);
            if (angularSpeed > 0.5f && Time.time - _lastCreakTime > CreakCooldown)
            {
                _lastCreakTime = Time.time;
                Core.AudioManager.Instance?.PlaySFX(creakClip);
            }
        }

        private void UpdateForceArrows()
        {
            float leftWeight  = leftZone  != null && leftZone.IsOccupied  ? leftZone.Occupant.weightValue  : 0f;
            float rightWeight = rightZone != null && rightZone.IsOccupied ? rightZone.Occupant.weightValue : 0f;
            float maxWeight   = Mathf.Max(leftWeight, rightWeight, 0.01f);

            leftArrow?.SetForce(leftWeight / maxWeight);
            rightArrow?.SetForce(rightWeight / maxWeight);
        }

        public void PlaySuccess()
        {
            successParticles?.Play();
            if (thudClip != null)
                Core.AudioManager.Instance?.PlaySFX(thudClip);
        }
    }
}
