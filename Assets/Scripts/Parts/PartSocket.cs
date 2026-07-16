using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRLearning.Simulation.Physics;

namespace VRLearning.Parts
{
    /// <summary>
    /// Lets a learner physically drag a displaced anatomy part back into place. Listens for the
    /// part being released (selectExited) and, if it's close enough to its own captured home pose
    /// (from ExplodedView), snaps it precisely into position/rotation/scale and marks it seated.
    ///
    /// Added automatically by GrabbablePartsSetup alongside the XRGrabInteractable it sets up, so
    /// this applies to exactly the same scenes as grab-driven parts — no per-scene authoring.
    /// </summary>
    [RequireComponent(typeof(PartInfo))]
    public class PartSocket : MonoBehaviour
    {
        [Tooltip("Auto-found in a parent if left unassigned.")]
        [SerializeField] private ExplodedView explodedView;
        [SerializeField] private float distanceTolerance = 0.05f; // metres
        [SerializeField] private float angleTolerance = 15f;      // degrees
        [SerializeField] private float snapSpeed = 6f;

        /// <summary>True once this part has been manually dragged into its home socket.</summary>
        public bool IsSeated { get; private set; }

        /// <summary>Raised the moment this part snaps into its home socket.</summary>
        public event System.Action<PartSocket> OnSeated;

        private PartInfo _partInfo;
        private XRBaseInteractable _interactable;
        private Coroutine _snapRoutine;

        private void Awake()
        {
            _partInfo = GetComponent<PartInfo>();
            if (explodedView == null) explodedView = GetComponentInParent<ExplodedView>();
        }

        /// <summary>(Re)binds to whatever XRBaseInteractable currently sits on this GameObject.
        /// Called by GrabbablePartsSetup right after it adds the XRGrabInteractable — mirrors
        /// PartInfo.RefreshInteractable()'s fix for the same OnEnable-before-AddComponent race.</summary>
        public void RefreshInteractable()
        {
            if (_interactable != null)
            {
                _interactable.selectEntered.RemoveListener(OnGrabbed);
                _interactable.selectExited.RemoveListener(OnReleased);
            }
            _interactable = GetComponent<XRBaseInteractable>();
            if (_interactable != null)
            {
                _interactable.selectEntered.AddListener(OnGrabbed);
                _interactable.selectExited.AddListener(OnReleased);
            }
        }

        // Picking a seated part back up un-seats it until the next release is verified again.
        private void OnGrabbed(SelectEnterEventArgs args) => IsSeated = false;

        private void OnReleased(SelectExitEventArgs args)
        {
            if (IsSeated || explodedView == null) return;
            if (!explodedView.TryGetOrigin(transform, out var homeLocalPos, out var homeLocalRot)) return;

            var parent = transform.parent;
            Vector3 homePos = parent != null ? parent.TransformPoint(homeLocalPos) : homeLocalPos;
            Quaternion homeRot = parent != null ? parent.rotation * homeLocalRot : homeLocalRot;

            if (Vector3.Distance(transform.position, homePos) <= distanceTolerance
                && Quaternion.Angle(transform.rotation, homeRot) <= angleTolerance)
            {
                if (_snapRoutine != null) StopCoroutine(_snapRoutine);
                _snapRoutine = StartCoroutine(SnapHome(homePos, homeRot));
            }
        }

        private IEnumerator SnapHome(Vector3 pos, Quaternion rot)
        {
            IsSeated = true;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = _partInfo.OriginalLocalScale;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.MoveTowards(t, 1f, Time.deltaTime * snapSpeed);
                transform.position = Vector3.Lerp(startPos, pos, t);
                transform.rotation = Quaternion.Slerp(startRot, rot, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            _snapRoutine = null;

            _partInfo.SetHighlight(true);
            var soundKit = Object.FindFirstObjectByType<VRLearning.Audio.UISoundKit>();
            soundKit?.PlayAnswered(true);

            OnSeated?.Invoke(this);
        }
    }
}
