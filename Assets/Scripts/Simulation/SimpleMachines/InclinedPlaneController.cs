using System;
using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    public class InclinedPlaneController : MonoBehaviour
    {
        [SerializeField] private Rigidbody slidingBlock;
        [SerializeField] private PhysicsMaterial rampMaterial;
        [SerializeField] private PhysicsMaterial blockMaterial;
        [SerializeField] private Transform rampTopSpawnPoint;
        [SerializeField] private MachineLabel frictionValueLabel;
        [SerializeField] private AudioSource blockSlideSource;
        [SerializeField] private float minFriction = 0.05f;
        [SerializeField] private float maxFriction = 0.95f;
        [SerializeField] private float bottomDetectionY = 0.05f;

        public bool BlockReachedBottom { get; private set; }
        public float SlideTime => BlockReachedBottom ? _slideEndTime - _slideStartTime : Time.time - _slideStartTime;

        public event Action<float> OnBlockReachedBottom;

        private float _slideStartTime;
        private float _slideEndTime;
        private bool _sliding;

        private void Start()
        {
            _slideStartTime = Time.time;
            SetFriction(0.5f);
        }

        public void SetFriction(float normalizedValue)
        {
            float f = Mathf.Lerp(minFriction, maxFriction, Mathf.Clamp01(normalizedValue));

            if (rampMaterial != null)
            {
                rampMaterial.dynamicFriction = f;
                rampMaterial.staticFriction  = f;
            }
            if (blockMaterial != null)
            {
                blockMaterial.dynamicFriction = f;
                blockMaterial.staticFriction  = f;
            }

            frictionValueLabel?.SetOverrideText($"μ = {f:F2}");
        }

        private void FixedUpdate()
        {
            if (BlockReachedBottom || slidingBlock == null) return;

            float speed = slidingBlock.linearVelocity.magnitude;

            // Slide audio
            if (blockSlideSource != null)
            {
                bool shouldPlay = speed > 0.05f;
                if (shouldPlay && !blockSlideSource.isPlaying)
                {
                    blockSlideSource.Play();
                    _sliding = true;
                    if (!_sliding) _slideStartTime = Time.time;
                }
                else if (!shouldPlay && blockSlideSource.isPlaying)
                {
                    blockSlideSource.Stop();
                }
                if (blockSlideSource.isPlaying)
                    blockSlideSource.pitch = Mathf.Clamp(speed * 0.5f, 0.8f, 2.0f);
            }

            // Bottom detection
            if (slidingBlock.position.y <= bottomDetectionY)
            {
                BlockReachedBottom = true;
                _slideEndTime = Time.time;
                blockSlideSource?.Stop();
                OnBlockReachedBottom?.Invoke(SlideTime);
            }
        }

        public void ResetBlock()
        {
            if (slidingBlock == null || rampTopSpawnPoint == null) return;

            slidingBlock.linearVelocity  = Vector3.zero;
            slidingBlock.angularVelocity = Vector3.zero;
            slidingBlock.position        = rampTopSpawnPoint.position;
            slidingBlock.rotation        = rampTopSpawnPoint.rotation;

            BlockReachedBottom = false;
            _sliding           = false;
            _slideStartTime    = Time.time;
        }
    }
}
