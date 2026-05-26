using UnityEngine;
using System.Collections.Generic;

namespace VRLearning
{
    /// <summary>
    /// Scriptable Object — one learning module (e.g. Code World).
    /// Holds ordered puzzle list, curriculum tags, and age-group lock.
    /// Create via: Assets > Create > VRLearning > Module Definition
    /// </summary>
    [CreateAssetMenu(fileName = "ModuleDef_New", menuName = "VRLearning/Module Definition")]
    public class ModuleDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string ModuleId;
        public string TitleKey;
        public string DescriptionKey;
        public Sprite ModuleIcon;

        [Header("Curriculum")]
        public string CurriculumAlignment;  // e.g. "Rwanda ICT P3–P4 – Sequencing"
        public Core.AgeGroup MinAgeGroup;
        public List<string> LearningObjectiveKeys;

        [Header("Puzzles")]
        public List<Simulation.PuzzleDefinition> Puzzles;

        [Header("Completion")]
        [Range(0f, 1f)] public float PassThreshold = 0.6f;
        public Sprite CompletionBadge;
        public AudioClip CompletionJingle;
    }
}
