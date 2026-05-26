using UnityEngine;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// Condition puzzle: learner picks IF / ELSE branches for a scenario.
    /// E.g. "IF the door is locked → pick up key, ELSE → walk through".
    /// </summary>
    public class ConditionPuzzle : Simulation.PuzzleController
    {
        [Header("Condition Puzzle")]
        [SerializeField] private ConditionScenario scenario;
        [SerializeField] private BranchSelector    ifBranchSelector;
        [SerializeField] private BranchSelector    elseBranchSelector;
        [SerializeField] private RobotCharacter    robot;

        protected override void InitialisePuzzle()
        {
            scenario?.Activate();
        }

        protected override bool EvaluateSolution()
        {
            return ifBranchSelector.SelectedAction   == scenario.CorrectIfAction
                && elseBranchSelector.SelectedAction == scenario.CorrectElseAction;
        }

        public void PreviewBranch()
        {
            robot?.ExecuteConditional(
                scenario.ConditionIsTrue,
                ifBranchSelector.SelectedAction,
                elseBranchSelector.SelectedAction);
        }
    }
}
