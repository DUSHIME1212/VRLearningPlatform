using UnityEngine;

namespace VRLearning.Modules.CodeWorld
{
    /// <summary>
    /// The friendly NPC guide who narrates the Code World town experience.
    /// Plays VO clips based on the current concept key and learner language.
    /// </summary>
    public class TownCharacterNPC : MonoBehaviour
    {
        [SerializeField] private Animator npcAnimator;
        [SerializeField] private NPCSpeechBubble speechBubble;

        [Header("VO Clips")]
        [SerializeField] private AudioClip introClip_EN;
        [SerializeField] private AudioClip introClip_RW;
        [SerializeField] private AudioClip outroClip_EN;
        [SerializeField] private AudioClip outroClip_RW;

        private Core.Language Lang =>
            Core.LocalisationManager.Instance?.CurrentLanguage ?? Core.Language.English;

        public void SpeakIntroduction()
        {
            var clip = Lang == Core.Language.Kinyarwanda ? introClip_RW : introClip_EN;
            Core.AudioManager.Instance?.PlayVO(clip);
            npcAnimator?.SetTrigger("Wave");

            string key = "npc_intro";
            speechBubble?.Show(Core.LocalisationManager.Instance?.Get(key));
        }

        public void SpeakBuildingIntro(string conceptKey)
        {
            string locKey = $"npc_building_{conceptKey}";
            speechBubble?.Show(Core.LocalisationManager.Instance?.Get(locKey));
            npcAnimator?.SetTrigger("Point");
        }

        public void SpeakOutroduction()
        {
            var clip = Lang == Core.Language.Kinyarwanda ? outroClip_RW : outroClip_EN;
            Core.AudioManager.Instance?.PlayVO(clip);
            npcAnimator?.SetTrigger("Celebrate");
        }
    }
}
