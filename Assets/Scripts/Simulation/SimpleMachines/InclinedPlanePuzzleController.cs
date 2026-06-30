using UnityEngine;
using VRLearning.Core;
using VRLearning.UI;

namespace VRLearning.Simulation.SimpleMachines
{
    public class InclinedPlanePuzzleController : PuzzleController
    {
        [SerializeField] private InclinedPlaneController planeController;
        [SerializeField] private float targetSlideTimeMedium = 3.0f;
        [SerializeField] private float targetSlideTimeHard   = 2.0f;
        [SerializeField] private float timeTolerance         = 1.0f;

        private bool  _isDemoMode;
        private float _targetSlideTime;

        protected override void InitialisePuzzle()
        {
            var difficulty = DifficultyAdapter.Instance?.Current ?? DifficultyLevel.Easy;

            if (difficulty == DifficultyLevel.Easy)
            {
                _isDemoMode = true;
            }
            else
            {
                _isDemoMode     = false;
                _targetSlideTime = difficulty == DifficultyLevel.Hard
                    ? targetSlideTimeHard
                    : targetSlideTimeMedium;
            }

            OnPuzzleComplete += _ => DifficultyAdapter.Instance?.RecordOutcome(true);

            OnHintRequested += index =>
            {
                if (puzzleData?.Hints == null || index >= puzzleData.Hints.Count) return;
                string text = LocalisationManager.Instance?.Get(puzzleData.Hints[index]) ?? puzzleData.Hints[index];
                UIManager.Instance?.ShowHint(text);
            };

            if (planeController != null)
                planeController.OnBlockReachedBottom += HandleBlockReachedBottom;
        }

        protected override bool EvaluateSolution()
        {
            if (_isDemoMode)
                return planeController != null && planeController.BlockReachedBottom;

            return planeController != null
                   && planeController.BlockReachedBottom
                   && Mathf.Abs(planeController.SlideTime - _targetSlideTime) <= timeTolerance;
        }

        private void HandleBlockReachedBottom(float slideTime)
        {
            if (_solved) return;
            if (EvaluateSolution())
                SubmitSolution();
        }
    }
}
