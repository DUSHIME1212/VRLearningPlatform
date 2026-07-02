using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRLearning.Core;

namespace VRLearning.Parts
{
    /// <summary>
    /// Attach to a single sub-part of an educational model (a heart chamber, a lever arm,
    /// an alveolus…). Holds a localised name + description and, when the part is hovered
    /// or selected with an XR interactor, highlights itself and raises <see cref="OnSelected"/>
    /// so a <see cref="PartsInfoPanel"/> can display its details.
    ///
    /// Needs a Collider (for XR ray / poke) and an XRSimpleInteractable on the same object.
    /// If no interactable is present the part still works via <see cref="Select"/> (e.g. from
    /// the panel's Next/Previous tour buttons).
    /// </summary>
    [DisallowMultipleComponent]
    public class PartInfo : MonoBehaviour
    {
        [Header("Identity")]
        public string PartId;
        [Tooltip("Display name in English.")]     public string NameEN;
        [Tooltip("Display name in Kinyarwanda.")] public string NameRW;
        [TextArea(2, 4)] public string DescriptionEN;
        [TextArea(2, 4)] public string DescriptionRW;

        [Header("Highlight")]
        [Tooltip("Renderers tinted when this part is hovered/selected. Empty = all child renderers.")]
        [SerializeField] private Renderer[] highlightRenderers;
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
        [Range(0f, 2f)]
        [SerializeField] private float highlightEmission = 0.6f;

        public event Action<PartInfo> OnSelected;
        public event Action<PartInfo> OnHoverEnter;
        public event Action<PartInfo> OnHoverExit;

        private XRBaseInteractable _interactable;
        private MaterialPropertyBlock _mpb;

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");     // URP
        private static readonly int ColorID = Shader.PropertyToID("_Color");             // Built-in
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        public string DisplayName => Localised(NameEN, NameRW);
        public string DisplayDescription => Localised(DescriptionEN, DescriptionRW);

        private void Awake()
        {
            if (highlightRenderers == null || highlightRenderers.Length == 0)
                highlightRenderers = GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _interactable = GetComponent<XRBaseInteractable>();
            if (_interactable != null)
            {
                _interactable.hoverEntered.AddListener(HandleHoverEnter);
                _interactable.hoverExited.AddListener(HandleHoverExit);
                _interactable.selectEntered.AddListener(HandleSelect);
            }
        }

        private void OnDisable()
        {
            if (_interactable != null)
            {
                _interactable.hoverEntered.RemoveListener(HandleHoverEnter);
                _interactable.hoverExited.RemoveListener(HandleHoverExit);
                _interactable.selectEntered.RemoveListener(HandleSelect);
            }
            SetHighlight(false);
        }

        private void HandleHoverEnter(HoverEnterEventArgs _) { SetHighlight(true);  OnHoverEnter?.Invoke(this); }
        private void HandleHoverExit(HoverExitEventArgs _)   { SetHighlight(false); OnHoverExit?.Invoke(this); }
        private void HandleSelect(SelectEnterEventArgs _)    { Select(); }

        /// <summary>Programmatically select this part (also used by the tour buttons).</summary>
        public void Select()
        {
            SetHighlight(true);
            OnSelected?.Invoke(this);
        }

        /// <summary>Tint the part's renderers to show it is hovered/selected.</summary>
        public void SetHighlight(bool on)
        {
            if (highlightRenderers == null) return;
            foreach (var r in highlightRenderers)
            {
                if (r == null) continue;
                if (on)
                {
                    _mpb.Clear();
                    _mpb.SetColor(BaseColorID, highlightColor);
                    _mpb.SetColor(ColorID, highlightColor);
                    _mpb.SetColor(EmissionColorID, highlightColor * highlightEmission);
                    r.SetPropertyBlock(_mpb);
                }
                else
                {
                    r.SetPropertyBlock(null); // clear overrides -> back to the material's look
                }
            }
        }

        private string Localised(string en, string rw)
        {
            var lang = LocalisationManager.Instance != null
                ? LocalisationManager.Instance.CurrentLanguage
                : Language.English;
            string chosen = lang == Language.Kinyarwanda ? rw : en;
            return string.IsNullOrEmpty(chosen) ? en : chosen;
        }
    }
}
