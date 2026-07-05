using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Drops an object from rest, times its fall to a target height and computes g from
    /// g = 2h / t². Call <see cref="Drop"/> (e.g. from a button) to run the experiment.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FreeFallTimer : MonoBehaviour
    {
        [SerializeField] private SimpleMachines.MachineLabel label;
        [SerializeField] private float stopHeight = 0f; // world Y counted as "landed"

        private Rigidbody _rb;
        private float _startY;
        private float _time;
        private bool _running;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>Release the object and start timing.</summary>
        public void Drop()
        {
            _rb.useGravity = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _startY = transform.position.y;
            _time = 0f;
            _running = true;
        }

        private void FixedUpdate()
        {
            if (!_running) return;
            _time += Time.fixedDeltaTime;

            if (transform.position.y <= stopHeight)
            {
                _running = false;
                float h = Mathf.Max(_startY - stopHeight, 0f);
                float g = _time > 0f ? 2f * h / (_time * _time) : 0f;
                if (label != null)
                    label.SetOverrideText($"h {h:F2} m   t {_time:F2} s   g ≈ {g:F1} m/s²");
            }
        }
    }
}
