using System.Collections;
using UnityEngine;
using VRLearning.Core;
using VRLearning.UI;

namespace VRLearning.Simulation.SimpleMachines
{
    public class PulleyPuzzleController : PuzzleController
    {
        [SerializeField] private PulleyController pulleyController;
        [SerializeField] private Transform targetZone;
        [SerializeField] private float easyTargetHeight   = 0.5f;
        [SerializeField] private float mediumTargetHeight = 1.0f;
        [SerializeField] private float hardTargetHeight   = 1.5f;
        [SerializeField] private float successTolerance   = 0.15f;

        private float _targetHeight;

        protected override void InitialisePuzzle()
        {
            var difficulty = DifficultyAdapter.Instance?.Current ?? DifficultyLevel.Easy;
            _targetHeight = difficulty switch
            {
                DifficultyLevel.Medium => mediumTargetHeight,
                DifficultyLevel.Hard   => hardTargetHeight,
                _                      => easyTargetHeight
            };

            if (targetZone != null)
            {
                Vector3 p = targetZone.position;
                targetZone.position = new Vector3(p.x, _targetHeight, p.z);
            }

            OnPuzzleComplete += _ => DifficultyAdapter.Instance?.RecordOutcome(true);

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
            if (pulleyController == null) return false;
            return pulleyController.DistanceFromTarget(_targetHeight) <= successTolerance;
        }

        private IEnumerator EvaluationLoop()
        {
            while (!_solved)
            {
                yield return new WaitForSeconds(0.5f);

                if (pulleyController != null)
                    pulleyController.SetDistanceLabelValue(pulleyController.DistanceFromTarget(_targetHeight));

                if (EvaluateSolution())
                    SubmitSolution();
            }
        }
    }
}
