using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.Data
{
    /// <summary>One multiple-choice question inside a <see cref="QuizDefinition"/>.</summary>
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)] public string Question;
        [Tooltip("Answer choices shown as buttons.")]
        public List<string> Options = new List<string>();
        [Tooltip("Zero-based index into Options of the correct answer.")]
        public int CorrectIndex;
        [TextArea(2, 4)]
        [Tooltip("Shown after the student answers.")]
        public string Explanation;
    }

    /// <summary>
    /// Scriptable Object — a quiz students can answer.
    /// Create via: Assets > Create > VRLearning > Quiz Definition
    /// </summary>
    [CreateAssetMenu(fileName = "Quiz_New", menuName = "VRLearning/Quiz Definition")]
    public class QuizDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string QuizId;
        public string ModuleId;
        [TextArea] public string Title;

        [Header("Questions")]
        public List<QuizQuestion> Questions = new List<QuizQuestion>();

        [Header("Feedback Audio (optional)")]
        public AudioClip CorrectClip;
        public AudioClip WrongClip;
        public AudioClip CompleteClip;

        [Header("Scoring")]
        [Range(0f, 1f)]
        [Tooltip("Fraction of correct answers needed to pass.")]
        public float PassThreshold = 0.6f;
    }
}
