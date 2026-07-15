using TMPro;
using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    /// <summary>
    /// Base for the floating, child-friendly "whiteboard" that shows a machine's formula
    /// updating in real time. Subclasses fill in the machine-specific values. Text is rebuilt
    /// on a fixed cadence (not every frame) to avoid string/GC churn in VR, and the board can
    /// billboard toward the player so it's always readable.
    /// </summary>
    public abstract class FormulaHUD : MonoBehaviour
    {
        [Header("Whiteboard text")]
        [SerializeField] protected TMP_Text titleLabel;
        [SerializeField] protected TMP_Text formulaLabel;
        [SerializeField] protected TMP_Text resultLabel;

        [Header("Behaviour")]
        [Tooltip("Seconds between text refreshes. 0 = every frame.")]
        [SerializeField] protected float refreshInterval = 0.1f;
        [Tooltip("Turn the board to face the player each frame.")]
        [SerializeField] protected bool billboard = true;
        [Tooltip("Flip 180° if the text reads mirrored/backwards.")]
        [SerializeField] protected bool flipFacing = false;
        [Tooltip("Optional explicit face target; falls back to the main camera.")]
        [SerializeField] protected Transform faceTarget;

        private float _nextRefresh;
        private Transform _cam;

        protected virtual void Start()
        {
            if (titleLabel != null) titleLabel.text = Title;
            Refresh();
        }

        protected virtual void Update()
        {
            if (Time.time >= _nextRefresh)
            {
                _nextRefresh = Time.time + Mathf.Max(0f, refreshInterval);
                Refresh();
            }
            if (billboard) FaceCamera();
        }

        /// <summary>Whiteboard heading, shown once.</summary>
        protected abstract string Title { get; }

        /// <summary>Rebuild the live formula + result text from the machine's current state.</summary>
        protected abstract void Refresh();

        /// <summary>Wrap text in a TMP rich-text colour tag (used for colour-matched terms).</summary>
        protected static string Colorize(string text, Color c)
            => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text}</color>";

        private void FaceCamera()
        {
            if (_cam == null)
            {
                Transform t = faceTarget != null ? faceTarget
                            : (Camera.main != null ? Camera.main.transform : null);
                if (t == null) return;   // camera not ready yet — try again next frame
                _cam = t;
            }

            Vector3 dir = transform.position - _cam.position;
            if (flipFacing) dir = -dir;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
