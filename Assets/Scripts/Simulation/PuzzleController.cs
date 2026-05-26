using UnityEngine;
using System.Collections.Generic;
using System;

namespace VRLearning.Simulation
{
    /// <summary>
    /// Core puzzle loop used by all three modules.
    /// Concrete modules (CodeWorldPuzzle, etc.) inherit from this.
    /// </summary>
    public abstract class PuzzleController : MonoBehaviour
    {
        [Header("Puzzle Config")]
        [SerializeField] protected PuzzleDefinition puzzleData;
        [SerializeField] protected float hintDelaySeconds = 15f;

        protected int  _hintsUsed;
        protected int  _attempts;
        protected float _startTime;
        protected bool _solved;

        public event Action<PuzzleResult> OnPuzzleComplete;
        public event Action<int>          OnHintRequested;   // hint index
        public event Action               OnPuzzleStarted;

        protected virtual void Start()
        {
            _startTime = Time.time;
            OnPuzzleStarted?.Invoke();
            InitialisePuzzle();
        }

        /// <summary>Override to set up module-specific puzzle state.</summary>
        protected abstract void InitialisePuzzle();

        /// <summary>Override to define correctness logic.</summary>
        protected abstract bool EvaluateSolution();

        public void SubmitSolution()
        {
            if (_solved) return;
            _attempts++;
            bool passed = EvaluateSolution();

            if (passed)
            {
                _solved = true;
                int stars = CalculateStars();
                float elapsed = Time.time - _startTime;

                var result = new PuzzleResult
                {
                    PuzzleId  = puzzleData.PuzzleId,
                    Passed    = true,
                    Stars     = stars,
                    TimeTaken = elapsed,
                    Attempts  = _attempts,
                    HintsUsed = _hintsUsed
                };

                Core.ProgressTracker.Instance?.RecordPuzzleResult(
                    puzzleData.ModuleId, puzzleData.PuzzleId, true, stars, elapsed);

                OnPuzzleComplete?.Invoke(result);
                PlaySuccessFeedback();
            }
            else
            {
                PlayFailureFeedback();
                if (_attempts >= 2) ShowNextHint();
            }
        }

        protected void ShowNextHint()
        {
            if (puzzleData == null || _hintsUsed >= puzzleData.Hints.Count) return;
            OnHintRequested?.Invoke(_hintsUsed);
            _hintsUsed++;
        }

        private int CalculateStars()
        {
            if (_hintsUsed == 0 && _attempts == 1) return 3;
            if (_hintsUsed <= 1 && _attempts <= 2) return 2;
            return 1;
        }

        protected virtual void PlaySuccessFeedback()
        {
            Core.AudioManager.Instance?.PlaySFX(puzzleData?.SuccessClip);
        }

        protected virtual void PlayFailureFeedback()
        {
            Core.AudioManager.Instance?.PlaySFX(puzzleData?.FailureClip);
        }
    }

    [Serializable]
    public class PuzzleResult
    {
        public string PuzzleId;
        public bool   Passed;
        public int    Stars;
        public float  TimeTaken;
        public int    Attempts;
        public int    HintsUsed;
    }
}
