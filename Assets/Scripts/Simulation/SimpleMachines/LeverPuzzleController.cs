using System.Collections;
using UnityEngine;
using VRLearning.Core;
using VRLearning.UI;

namespace VRLearning.Simulation.SimpleMachines
{
    public class LeverPuzzleController : PuzzleController
    {
        [SerializeField] private LeverController leverController;
        [SerializeField] private float easyAngleThreshold   = 15f;
        [SerializeField] private float mediumAngleThreshold = 20f;
        [SerializeField] private float hardAngleThreshold   = 25f;
        [SerializeField] private LeverController.LeverSide requiredTippedSide = LeverController.LeverSide.Left;

        private float _angleThreshold;

        protected override void InitialisePuzzle()
        {
            var difficulty = DifficultyAdapter.Instance?.Current ?? DifficultyLevel.Easy;
            _angleThreshold = difficulty switch
            {
                DifficultyLevel.Medium => mediumAngleThreshold,
                DifficultyLevel.Hard   => hardAngleThreshold,
                _                      => easyAngleThreshold
            };

            OnPuzzleComplete += result =>
            {
                leverController?.PlaySuccess();
                DifficultyAdapter.Instance?.RecordOutcome(true);
            };

            OnHintRequested += index =>
            {
                if (puzzleData?.Hints == null || index >= puzzleData.Hints.Count) return;
                string text = LocalisationManager.Instance?.Get(puzzleData.Hints[index]) ?? puzzleData.Hints[index];
                UIManager.Instance?.ShowHint(text);
            };

            StartCoroutine(EvaluationLoop());
        }

        protected override bool EvaluateSolution()
        {
            if (leverController == null) return false;
            return Mathf.Abs(leverController.CurrentAngle) >= _angleThreshold
                   && leverController.TippedSide == requiredTippedSide;
        }

        private IEnumerator EvaluationLoop()
        {
            while (!_solved)
            {
                yield return new WaitForSeconds(0.5f);
                if (EvaluateSolution())
                    SubmitSolution();
            }
        }
    }
}
