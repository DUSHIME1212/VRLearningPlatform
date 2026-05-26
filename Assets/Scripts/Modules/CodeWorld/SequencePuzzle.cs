using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// Sequence puzzle: learner drags instruction blocks into the correct order.
    /// The robot character then executes the sequence visually.
    /// </summary>
    public class SequencePuzzle : Simulation.PuzzleController
    {
        [Header("Sequence Puzzle")]
        [SerializeField] private List<InstructionBlock> availableBlocks;
        [SerializeField] private List<InstructionSlot>  solutionSlots;
        [SerializeField] private RobotCharacter         robot;

        private List<InstructionBlock> _placedBlocks = new();

        protected override void InitialisePuzzle()
        {
            foreach (var block in availableBlocks)
                block.OnBlockPlaced += HandleBlockPlaced;
        }

        protected override bool EvaluateSolution()
        {
            if (_placedBlocks.Count != solutionSlots.Count) return false;
            for (int i = 0; i < solutionSlots.Count; i++)
            {
                if (_placedBlocks[i].InstructionType != puzzleData.CorrectSequence[i])
                    return false;
            }
            return true;
        }

        private void HandleBlockPlaced(InstructionBlock block, int slotIndex)
        {
            if (slotIndex < _placedBlocks.Count) _placedBlocks[slotIndex] = block;
            else _placedBlocks.Add(block);
        }

        public void RunSimulation()
        {
            robot?.ExecuteSequence(_placedBlocks.ConvertAll(b => b.InstructionType));
        }
    }
}
