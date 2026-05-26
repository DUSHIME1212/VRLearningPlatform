using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// Manages the Code World VR town scene.
    /// Each Building maps to one programming concept (Sequence, Loop, Condition).
    /// The learner walks up to a building and interacts to start that concept's puzzle.
    /// </summary>
    public class CodeWorldManager : MonoBehaviour
    {
        [Header("Buildings")]
        [SerializeField] private ConceptBuilding sequenceBuilding;
        [SerializeField] private ConceptBuilding loopBuilding;
        [SerializeField] private ConceptBuilding conditionBuilding;

        [Header("Town")]
        [SerializeField] private TownCharacterNPC guideNPC;
        [SerializeField] private Transform        playerSpawnPoint;

        private readonly List<ConceptBuilding> _buildings = new();
        private int _buildingsCompleted;

        private void Start()
        {
            _buildings.AddRange(new[] { sequenceBuilding, loopBuilding, conditionBuilding });
            foreach (var b in _buildings)
            {
                b.OnBuildingEntered   += HandleBuildingEntered;
                b.OnPuzzleComplete    += HandlePuzzleComplete;
            }
            guideNPC?.SpeakIntroduction();
        }

        private void HandleBuildingEntered(ConceptBuilding building)
        {
            guideNPC?.SpeakBuildingIntro(building.ConceptKey);
        }

        private void HandlePuzzleComplete(ConceptBuilding building, Simulation.PuzzleResult result)
        {
            building.SetCompleted(result.Stars);
            _buildingsCompleted++;

            if (_buildingsCompleted >= _buildings.Count)
                OnAllBuildingsComplete();
        }

        private void OnAllBuildingsComplete()
        {
            guideNPC?.SpeakOutroduction();
            Core.GameManager.Instance?.EndSession();
        }
    }
}
