using UnityEngine;

namespace VRLearning
{
    /// <summary>
    /// Scriptable Object — a reusable condition scenario for ConditionPuzzle.
    /// E.g. "Door is locked → IF locked: pick up key, ELSE: walk through"
    /// Create via: Assets > Create > VRLearning > Concept Scenario
    /// </summary>
    [CreateAssetMenu(fileName = "Scenario_New", menuName = "VRLearning/Concept Scenario")]
    public class ConditionScenario : ScriptableObject
    {
        [Header("Scenario")]
        public string ScenarioId;
        public string DescriptionKey;
        public bool   ConditionIsTrue;
        public string ConditionDescriptionKey;

        [Header("Correct Answers")]
        public Modules.CodeWorld.ActionType CorrectIfAction;
        public Modules.CodeWorld.ActionType CorrectElseAction;

        [Header("Visuals")]
        public Sprite SceneIllustration;

        public void Activate() { /* Spawns scene props in world */ }
    }
}
