using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// Loop puzzle: learner selects a block and sets a repeat count (1-5).
    /// The robot repeats the action that many times.
    /// Teaches that loops avoid writing the same instruction repeatedly.
    /// </summary>
    public class LoopPuzzle : Simulation.PuzzleController
    {
        [Header("Loop Puzzle")]
        [SerializeField] private InstructionBlock loopBlock;
        [SerializeField] private RepeatCountSelector countSelector;
        [SerializeField] private RobotCharacter robot;
        [SerializeField] private int targetRepeatCount;

        protected override void InitialisePuzzle()
        {
            countSelector.OnCountChanged += OnCountChanged;
        }

        protected override bool EvaluateSolution()
        {
            return countSelector.CurrentCount == targetRepeatCount
                && loopBlock != null;
        }

        private void OnCountChanged(int count)
        {
            robot?.PreviewRepeat(loopBlock?.InstructionType ?? InstructionType.MoveForward, count);
        }
    }
}
