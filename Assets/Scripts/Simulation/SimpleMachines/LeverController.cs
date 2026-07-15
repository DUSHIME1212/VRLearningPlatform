using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    /// <summary>
    /// Drives the lever with LIVE torque physics (no canned animation). Every physics step it
    /// sums the torque each side exerts — torque = weightValue * momentArm for each occupied
    /// snap notch — and applies the net torque to the plank Rigidbody about the hinge axis.
    /// When the two sides balance (|net| &lt; threshold) it eases the plank back to level; when
    /// they don't, the heavier-moment side accelerates down and rests on the HingeJoint limit.
    /// Exposes the per-side torque / weight / distance so a formula whiteboard can read it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    public class LeverController : MonoBehaviour
    {
        public enum LeverSide { Left, Balanced, Right }

        [Header("Snap notches (both arms)")]
        [Tooltip("All weight snap notches along the plank. Each knows its signed distance from the fulcrum.")]
        [SerializeField] private WeightSnapZone[] zones;

        [Header("Torque tuning")]
        [Tooltip("Degrees of target tilt per unit of (weight*distance) imbalance.")]
        [SerializeField] private float anglePerTorque = 28f;
        [Tooltip("Max tilt (deg); a big imbalance rests here — 'the ground'. Keep below the hinge limit.")]
        [SerializeField] private float maxTiltAngle = 40f;
        [Tooltip("Spring strength pulling the beam toward its target tilt.")]
        [SerializeField] private float tiltStiffness = 12f;
        [Tooltip("Damping so the beam settles smoothly instead of oscillating.")]
        [SerializeField] private float tiltDamping = 4f;
        [Tooltip("Below this |net torque| the two sides count as balanced (level).")]
        [SerializeField] private float balanceThreshold = 0.05f;
        [Tooltip("Flip if the heavier-moment side rises instead of dropping.")]
        [SerializeField] private bool invertTilt = false;

        [Header("Visuals / feedback")]
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

        // Live per-side readings, refreshed each FixedUpdate (left = Effort arm, right = Load arm).
        private float _torqueLeft, _torqueRight;
        private float _weightLeft, _weightRight;
        private float _distLeft, _distRight;
        private Color _colorLeft = Color.white, _colorRight = Color.white;
        private bool _hasLeft, _hasRight;

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint>();
            _rb    = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            RecalculateTorque();
            ApplyLeverPhysics();
        }

        private void Update()
        {
            UpdateVisualFeedback();
            CheckCreakSFX();
            UpdateForceArrows();
        }

        // ── Public API for the formula HUD / puzzle controller ───────────────────────────

        public float CurrentAngle => _hinge != null ? _hinge.angle : 0f;

        public float TorqueLeft  => _torqueLeft;   // Effort side (localX &lt; 0)
        public float TorqueRight => _torqueRight;   // Load side   (localX &gt; 0)

        public bool IsBalanced => Mathf.Abs(_torqueRight - _torqueLeft) < balanceThreshold
                                  && (_hasLeft || _hasRight);

        public bool  LeftOccupied  => _hasLeft;
        public float LeftWeight     => _weightLeft;
        public float LeftDistance   => _distLeft;
        public Color LeftColor      => _colorLeft;

        public bool  RightOccupied => _hasRight;
        public float RightWeight    => _weightRight;
        public float RightDistance  => _distRight;
        public Color RightColor     => _colorRight;

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

        // ── Physics ──────────────────────────────────────────────────────────────────────

        private void RecalculateTorque()
        {
            _torqueLeft = _torqueRight = 0f;
            _weightLeft = _weightRight = 0f;
            _distLeft = _distRight = 0f;
            _hasLeft = _hasRight = false;

            if (zones == null) return;

            foreach (var zone in zones)
            {
                if (zone == null || !zone.IsOccupied || zone.Occupant == null) continue;

                float weight = zone.Occupant.weightValue;
                float arm    = zone.MomentArm;
                float torque = weight * arm;

                if (zone.SignedDistance < 0f) // left / Effort arm
                {
                    _torqueLeft += torque;
                    // Keep the reading of the notch furthest out (clearest for the formula).
                    if (!_hasLeft || arm > _distLeft)
                    {
                        _weightLeft = weight; _distLeft = arm; _colorLeft = zone.Occupant.DisplayColor;
                    }
                    _hasLeft = true;
                }
                else                          // right / Load arm
                {
                    _torqueRight += torque;
                    if (!_hasRight || arm > _distRight)
                    {
                        _weightRight = weight; _distRight = arm; _colorRight = zone.Occupant.DisplayColor;
                    }
                    _hasRight = true;
                }
            }
        }

        private void ApplyLeverPhysics()
        {
            if (_rb == null || _hinge == null) return;

            float net = _torqueRight - _torqueLeft;   // >0 => Load (right) side heavier
            float targetAngle = 0f;

            if (Mathf.Abs(net) >= balanceThreshold)
            {
                // Calculate target tilt based on imbalance, clamped to max angle
                targetAngle = net * anglePerTorque;
                
                // Original comment mentioned negative torque direction; however, TippedSide
                // expects a positive angle when the Right side drops. The right-hand rule
                // for HingeJoints ensures that a positive torque increases the angle.
                targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);
            }

            // HingeJoint.angle can read NaN before the solver initialises it — sanitise it.
            float currentAngle = _hinge.angle;
            if (!float.IsFinite(currentAngle)) currentAngle = 0f;

            float angleError = currentAngle - targetAngle;
            float angularVelocity = Vector3.Dot(_rb.angularVelocity, _hinge.axis.normalized);

            // Spring-damper system to pull the lever toward target tilt
            float commandTorque = -(angleError * tiltStiffness) - (angularVelocity * tiltDamping);

            if (invertTilt) commandTorque = -commandTorque;

            // Never feed NaN/Inf to the physics engine (it would corrupt the Rigidbody).
            if (!float.IsFinite(commandTorque)) return;
            _rb.AddTorque(_hinge.axis.normalized * commandTorque, ForceMode.Force);
        }

        // ── Visuals ──────────────────────────────────────────────────────────────────────

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
            if (creakClip == null || _rb == null) return;
            float angularSpeed = Mathf.Abs(_rb.angularVelocity.z);
            if (angularSpeed > 0.5f && Time.time - _lastCreakTime > CreakCooldown)
            {
                _lastCreakTime = Time.time;
                Core.AudioManager.Instance?.PlaySFX(creakClip);
            }
        }

        private void UpdateForceArrows()
        {
            float maxTorque = Mathf.Max(_torqueLeft, _torqueRight, 0.01f);
            leftArrow?.SetForce(_torqueLeft / maxTorque);
            rightArrow?.SetForce(_torqueRight / maxTorque);
        }

        public void PlaySuccess()
        {
            successParticles?.Play();
            if (thudClip != null)
                Core.AudioManager.Instance?.PlaySFX(thudClip);
        }
    }
}
