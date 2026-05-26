using UnityEngine;

namespace VRLearning
{
    /// <summary>
    /// Scriptable Object — defines an NPC or robot character's personality
    /// and audio assets so characters can be swapped without code changes.
    /// Create via: Assets > Create > VRLearning > Character Definition
    /// </summary>
    [CreateAssetMenu(fileName = "CharDef_New", menuName = "VRLearning/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string CharacterId;
        public string NameKey;
        public Sprite Portrait;

        [Header("VO Clips — English")]
        public AudioClip[] IntroClips_EN;
        public AudioClip[] HintClips_EN;
        public AudioClip[] SuccessClips_EN;
        public AudioClip[] FailureClips_EN;

        [Header("VO Clips — Kinyarwanda")]
        public AudioClip[] IntroClips_RW;
        public AudioClip[] HintClips_RW;
        public AudioClip[] SuccessClips_RW;
        public AudioClip[] FailureClips_RW;

        public AudioClip GetIntro(Core.Language lang)
        {
            var clips = lang == Core.Language.Kinyarwanda ? IntroClips_RW : IntroClips_EN;
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}
