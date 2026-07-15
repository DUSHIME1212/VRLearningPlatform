using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRLearning.Simulation.Physics; // ExplodedView

namespace VRLearning.Parts
{
    /// <summary>
    /// World-space panel that shows the name + description of the currently selected
    /// <see cref="PartInfo"/>. Listens to every part's selection event, and offers a
    /// guided tour (Next / Previous) plus an optional Explode toggle.
    ///
    /// Leave <see cref="parts"/> empty to auto-collect: from <see cref="partsRoot"/> if set,
    /// otherwise from the whole scene.
    /// </summary>
    public class PartsInfoPanel : MonoBehaviour
    {
        [Header("Parts")]
        [SerializeField] private List<PartInfo> parts = new List<PartInfo>();
        [Tooltip("If parts is empty, auto-collect all PartInfo under this transform (or the whole scene if null).")]
        [SerializeField] private Transform partsRoot;

        [Header("UI")]
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text counterLabel;

        [Header("Optional")]
        [SerializeField] private ExplodedView explodedView;
        [SerializeField] private bool selectFirstOnStart = false;

        private int _current = -1;
        private int _lastNarratedIndex = -1;

        /// <summary>Raised whenever the displayed part changes. (For audio/FX.)</summary>
        public event System.Action PartChanged;

        /// <summary>The part currently shown, or null. Lets listeners (e.g. UISoundKit) react to it.</summary>
        public PartInfo Current => (_current >= 0 && _current < parts.Count) ? parts[_current] : null;

        private void Awake()
        {
            if (parts == null) parts = new List<PartInfo>();

            if (parts.Count == 0)
            {
                var found = partsRoot != null
                    ? partsRoot.GetComponentsInChildren<PartInfo>(true)
                    : FindObjectsByType<PartInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                parts.AddRange(found);
            }

            foreach (var p in parts)
                if (p != null) p.OnSelected += Display;
        }

        private void OnDestroy()
        {
            foreach (var p in parts)
                if (p != null) p.OnSelected -= Display;
        }

        private void Start()
        {
            if (selectFirstOnStart && parts.Count > 0) SelectIndex(0);
            else ClearPanel();
        }

        /// <summary>Show a part's info and highlight only it.</summary>
        public void Display(PartInfo part)
        {
            if (part == null) return;
            _current = parts.IndexOf(part);

            foreach (var p in parts)
                if (p != null && p != part) p.SetHighlight(false);
            part.SetHighlight(true);

            if (nameLabel != null) nameLabel.text = part.DisplayName;
            if (descriptionLabel != null) descriptionLabel.text = part.DisplayDescription;
            if (counterLabel != null)
                counterLabel.text = _current >= 0 ? $"{_current + 1} / {parts.Count}" : string.Empty;

            // Narrate the newly selected part (only when it actually changes, so re-selecting the
            // same part via hover doesn't restart the clip). PlayVO stops any previous narration.
            if (_current != _lastNarratedIndex)
            {
                _lastNarratedIndex = _current;
                if (part.NarrationClip != null)
                    VRLearning.Core.AudioManager.Instance?.PlayVO(part.NarrationClip);
            }

            PartChanged?.Invoke();
        }

        /// <summary>Advance the guided tour (wraps around).</summary>
        public void Next()
        {
            if (parts.Count == 0) return;
            SelectIndex((_current + 1 + parts.Count) % parts.Count);
        }

        /// <summary>Step the guided tour back (wraps around).</summary>
        public void Previous()
        {
            if (parts.Count == 0) return;
            SelectIndex((_current - 1 + parts.Count) % parts.Count);
        }

        /// <summary>Explode / reassemble the model, if an ExplodedView is linked.</summary>
        public void ToggleExplode()
        {
            if (explodedView != null) explodedView.Toggle();
        }

        private void SelectIndex(int i)
        {
            if (i < 0 || i >= parts.Count || parts[i] == null) return;
            parts[i].Select();
        }

        private void ClearPanel()
        {
            if (nameLabel != null) nameLabel.text = "Select a part";
            if (descriptionLabel != null) descriptionLabel.text = string.Empty;
            if (counterLabel != null) counterLabel.text = string.Empty;
        }
    }
}
