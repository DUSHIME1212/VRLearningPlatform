using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Simulation
{
    /// <summary>
    /// Scriptable Object — defines one puzzle's data.
    /// Create via: Assets > Create > VRLearning > Puzzle Definition
    /// </summary>
    [CreateAssetMenu(fileName = "PuzzleDef_New", menuName = "VRLearning/Puzzle Definition")]
    public class PuzzleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string PuzzleId;
        public string ModuleId;
        public string TitleKey;           // localisation key
        public string DescriptionKey;
        public Simulation.DifficultyLevel Difficulty;

        [Header("Sequence Solution")]
        public List<VRLearning.Modules.CodeWorld.InstructionType> CorrectSequence;

        [Header("Hints")]
        public List<string> Hints;        // localisation keys, shown in order

        [Header("Audio")]
        public AudioClip SuccessClip;
        public AudioClip FailureClip;

        [Header("Stars")]
        [Tooltip("Max time in seconds for 3-star completion")]
        public float ThreeStarTimeLimit = 45f;
    }
}
