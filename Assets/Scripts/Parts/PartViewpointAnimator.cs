using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRLearning.Simulation.Physics;

namespace VRLearning.Parts
{
    /// <summary>
    /// When a part is selected via the PartsInfoPanel tour, animates it to a centered, scaled-up,
    /// slowly-rotating viewpoint in front of the camera for a closer look — and sends the
    /// previously-focused part back home. The part stays a normal grabbable object the whole time
    /// (never reparented), so grabbing it mid-flight or mid-spin simply hands control to the
    /// learner's hand; letting go elsewhere (or dragging it to its PartSocket) works exactly like
    /// any other displaced part.
    /// </summary>
    public class PartViewpointAnimator : MonoBehaviour
    {
        [Tooltip("Optional. Reused as the source of truth for each part's 'home' pose. If unset, falls back to whatever local pose the part had the first time it was focused.")]
        [SerializeField] private ExplodedView explodedView;
        [SerializeField] private float viewDistance = 0.45f;
        [SerializeField] private float inspectScaleMultiplier = 1.6f;
        [SerializeField] private float spinSpeedDegPerSec = 20f;
        [SerializeField] private float animSpeed = 2.5f;

        private class State
        {
            public bool AtViewpoint;
            public Coroutine FlightRoutine;
            public bool Spinning;
            public bool HookedGrab;
            public Vector3 FallbackPos;
            public Quaternion FallbackRot;
        }

        private readonly Dictionary<PartInfo, State> _states = new Dictionary<PartInfo, State>();

        /// <summary>Called by PartsInfoPanel.Display() whenever the selected part changes.</summary>
        public void FocusPart(PartInfo part, PartInfo previous)
        {
            if (previous != null && previous != part) ReturnHome(previous);
            if (part == null) return;

            var interactable = part.GetComponent<XRBaseInteractable>();
            if (interactable != null && interactable.isSelected) return; // being held — don't fight the grip
            if (explodedView != null && explodedView.IsAnimating) return; // don't fight a live Explode()/Assemble()

            var st = GetState(part);
            HookGrabCancel(part, st);
            if (st.AtViewpoint) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector3 targetPos = cam.transform.position + cam.transform.forward * viewDistance;
            Quaternion targetRot = Quaternion.LookRotation((targetPos - cam.transform.position).normalized, Vector3.up);
            Vector3 targetScale = part.OriginalLocalScale * inspectScaleMultiplier;

            st.AtViewpoint = true;
            RestartFlight(part, st, targetPos, targetRot, targetScale, startSpinAfter: true);
        }

        /// <summary>Sends a displaced part back to its captured home pose/scale.</summary>
        public void ReturnHome(PartInfo part)
        {
            if (part == null || !_states.TryGetValue(part, out var st) || !st.AtViewpoint) return;

            var interactable = part.GetComponent<XRBaseInteractable>();
            if (interactable != null && interactable.isSelected) return; // don't yank it out of a hand
            if (explodedView != null && explodedView.IsAnimating) return;

            Vector3 pos;
            Quaternion rot;
            if (explodedView != null && explodedView.TryGetOrigin(part.transform, out var lp, out var lr))
            {
                var parent = part.transform.parent;
                pos = parent != null ? parent.TransformPoint(lp) : lp;
                rot = parent != null ? parent.rotation * lr : lr;
            }
            else
            {
                pos = st.FallbackPos;
                rot = st.FallbackRot;
            }

            st.AtViewpoint = false;
            RestartFlight(part, st, pos, rot, part.OriginalLocalScale, startSpinAfter: false);
        }

        private State GetState(PartInfo part)
        {
            if (_states.TryGetValue(part, out var st)) return st;
            st = new State { FallbackPos = part.transform.position, FallbackRot = part.transform.rotation };
            _states[part] = st;
            return st;
        }

        private void HookGrabCancel(PartInfo part, State st)
        {
            if (st.HookedGrab) return;
            st.HookedGrab = true;
            part.OnGrabStart += _ => CancelAnimation(part);
        }

        // Grab immediately hands control to the learner's hand — stop any in-flight move and the
        // idle spin so nothing fights the hold.
        private void CancelAnimation(PartInfo part)
        {
            if (!_states.TryGetValue(part, out var st)) return;
            if (st.FlightRoutine != null) { StopCoroutine(st.FlightRoutine); st.FlightRoutine = null; }
            st.Spinning = false;
            st.AtViewpoint = false;
        }

        private void RestartFlight(PartInfo part, State st, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, bool startSpinAfter)
        {
            st.Spinning = false; // stop any current spin while flying
            if (st.FlightRoutine != null) StopCoroutine(st.FlightRoutine);
            st.FlightRoutine = StartCoroutine(FlyTo(part, st, targetPos, targetRot, targetScale, startSpinAfter));
        }

        private IEnumerator FlyTo(PartInfo part, State st, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, bool startSpinAfter)
        {
            var t = part.transform;
            Vector3 startPos = t.position;
            Quaternion startRot = t.rotation;
            Vector3 startScale = t.localScale;
            float progress = 0f;

            while (progress < 1f)
            {
                progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime * animSpeed);
                float e = Mathf.SmoothStep(0f, 1f, progress);
                t.position = Vector3.Lerp(startPos, targetPos, e);
                t.rotation = Quaternion.Slerp(startRot, targetRot, e);
                t.localScale = Vector3.Lerp(startScale, targetScale, e);
                yield return null;
            }

            st.FlightRoutine = null;
            if (startSpinAfter)
            {
                st.Spinning = true;
                StartCoroutine(IdleSpin(part, st));
            }
        }

        private IEnumerator IdleSpin(PartInfo part, State st)
        {
            while (st.Spinning && st.AtViewpoint)
            {
                part.transform.Rotate(Vector3.up, spinSpeedDegPerSec * Time.deltaTime, Space.World);
                yield return null;
            }
        }
    }
}
