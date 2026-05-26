using UnityEngine;
using System;

namespace VRLearning.Core
{
    /// <summary>
    /// Owns the active LearnerProfile for the current session.
    /// Creates guest profiles if no login is available.
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public LearnerProfile CurrentProfile { get; private set; }
        public SessionData    ActiveSession  { get; private set; }

        public event Action<LearnerProfile> OnProfileLoaded;
        public event Action<SessionData>    OnSessionEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadProfile(string learnerId)
        {
            CurrentProfile = DataRepository.Instance.LoadLearner(learnerId)
                             ?? CreateGuestProfile();
            OnProfileLoaded?.Invoke(CurrentProfile);
        }

        public void BeginSession(LearnerProfile profile)
        {
            ActiveSession = new SessionData
            {
                SessionId  = Guid.NewGuid().ToString(),
                LearnerId  = profile.LearnerId,
                StartTime  = DateTime.UtcNow,
                Language   = profile.PreferredLanguage
            };
        }

        public void EndSession(float scorePercent)
        {
            if (ActiveSession == null) return;
            ActiveSession.EndTime     = DateTime.UtcNow;
            ActiveSession.ScorePercent = scorePercent;
            DataRepository.Instance.SaveSession(ActiveSession);
            OnSessionEnded?.Invoke(ActiveSession);
            ActiveSession = null;
        }

        private LearnerProfile CreateGuestProfile()
        {
            return new LearnerProfile
            {
                LearnerId         = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                AgeGroup          = AgeGroup.Group6to9,
                PreferredLanguage = Language.Kinyarwanda
            };
        }
    }

    [Serializable]
    public class LearnerProfile
    {
        public string   LearnerId;
        public string   DisplayName;
        public AgeGroup AgeGroup;
        public Language PreferredLanguage;
        public int      TotalStarsEarned;
        public DateTime CreatedAt;
    }

    [Serializable]
    public class SessionData
    {
        public string   SessionId;
        public string   LearnerId;
        public DateTime StartTime;
        public DateTime EndTime;
        public Language Language;
        public string   ModuleId;
        public float    ScorePercent;
        public int      PuzzlesCompleted;
        public int      HintsUsed;
    }

    public enum AgeGroup  { Group6to9, Group10to12 }
    public enum Language  { English, Kinyarwanda }
}
