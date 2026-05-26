using UnityEngine;
using System;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// Attached to each building GameObject in the Code World town.
    /// Handles VR proximity detection, door animation, and puzzle handoff.
    /// </summary>
    public class ConceptBuilding : MonoBehaviour
    {
        [Header("Concept")]
        public string ConceptKey;                    // "sequence" | "loop" | "condition"
        [SerializeField] private Simulation.PuzzleDefinition puzzleDef;

        [Header("Visual")]
        [SerializeField] private Animator           doorAnimator;
        [SerializeField] private GameObject         completionStarDisplay;
        [SerializeField] private ParticleSystem     completionParticles;
        [SerializeField] private MeshRenderer[]     buildingRenderers;

        public event Action<ConceptBuilding>                          OnBuildingEntered;
        public event Action<ConceptBuilding, Simulation.PuzzleResult> OnPuzzleComplete;

        private bool _completed;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            doorAnimator?.SetBool("Open", true);
            OnBuildingEntered?.Invoke(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            doorAnimator?.SetBool("Open", false);
        }

        public void NotifyPuzzleComplete(Simulation.PuzzleResult result)
        {
            OnPuzzleComplete?.Invoke(this, result);
        }

        public void SetCompleted(int stars)
        {
            _completed = true;
            completionParticles?.Play();
            if (completionStarDisplay != null)
                completionStarDisplay.SetActive(true);
            // Swap building material to "completed" tint
            foreach (var r in buildingRenderers)
                r.material.SetFloat("_CompletedBlend", 1f);
        }

        public bool IsCompleted => _completed;
    }
}
