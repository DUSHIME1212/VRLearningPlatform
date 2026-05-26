using UnityEngine;

namespace VRLearning
{
    /// <summary>
    /// Scriptable Object — global runtime settings.
    /// One instance lives in Resources/AppSettings.
    /// Create via: Assets > Create > VRLearning > App Settings
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettings", menuName = "VRLearning/App Settings")]
    public class AppSettings : ScriptableObject
    {
        [Header("Session")]
        public float MaxVRSessionMinutes   = 25f;
        public float WarningAtMinutesLeft  = 5f;

        [Header("Difficulty")]
        public int   DifficultyWindowSize  = 5;
        public float UpThreshold           = 0.80f;
        public float DownThreshold         = 0.40f;

        [Header("Localisation")]
        public Core.Language DefaultLanguage = Core.Language.Kinyarwanda;

        [Header("Performance")]
        public int   TargetFrameRate       = 60;
        public bool  ReduceTexturesOnLowRam = true;

        [Header("Analytics")]
        public bool  EnableAnalytics       = true;
        public bool  EnableCloudSync       = true;
    }
}
