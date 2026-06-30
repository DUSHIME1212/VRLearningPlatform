using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// A grabbable VR block representing one programming instruction.
    /// Uses XR Interaction Toolkit grab for natural VR picking.
    /// Snaps to InstructionSlots when released nearby.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class InstructionBlock : MonoBehaviour
    {
        [Header("Instruction")]
        public InstructionType InstructionType;
        [SerializeField] private Sprite blockIcon;
        [SerializeField] private TMPro.TextMeshPro label;

        [Header("Snapping")]
        [SerializeField] private float snapRadius = 0.3f;

        public event Action<InstructionBlock, int> OnBlockPlaced;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
        private Vector3 _origin;

        private void Awake()
        {
            _grab   = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            _origin = transform.position;
            _grab.selectExited.AddListener(OnReleased);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (label == null) return;
            string key = $"instruction_{InstructionType.ToString().ToLower()}";
            label.text = Core.LocalisationManager.Instance?.Get(key) ?? InstructionType.ToString();
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            var slots = FindObjectsByType<InstructionSlot>(FindObjectsInactive.Exclude);
            InstructionSlot closest = null;
            float minDist = snapRadius;

            foreach (var slot in slots)
            {
                float d = Vector3.Distance(transform.position, slot.transform.position);
                if (d < minDist && !slot.IsOccupied) { minDist = d; closest = slot; }
            }

            if (closest != null)
            {
                closest.Accept(this);
                transform.position = closest.transform.position;
                transform.rotation = closest.transform.rotation;
                OnBlockPlaced?.Invoke(this, closest.SlotIndex);
            }
            else
            {
                transform.position = _origin;
            }
        }
    }
}
