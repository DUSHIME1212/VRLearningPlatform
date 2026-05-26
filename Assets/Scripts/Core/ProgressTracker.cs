using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Core
{
    /// <summary>
    /// Writes progress to local SQLite via DataRepository.
    /// Attempts Firebase cloud sync whenever connectivity is available.
    /// </summary>
    public class ProgressTracker : MonoBehaviour
    {
        public static ProgressTracker Instance { get; private set; }

        private readonly List<PerformanceScore> _pendingScores = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RecordPuzzleResult(string moduleId, string puzzleId, bool passed, int stars, float timeSeconds)
        {
            var score = new PerformanceScore
            {
                LearnerId    = SessionManager.Instance.CurrentProfile?.LearnerId,
                SessionId    = SessionManager.Instance.ActiveSession?.SessionId,
                ModuleId     = moduleId,
                PuzzleId     = puzzleId,
                Passed       = passed,
                Stars        = stars,
                TimeTaken    = timeSeconds,
                RecordedAt   = System.DateTime.UtcNow
            };

            DataRepository.Instance.SaveScore(score);
            _pendingScores.Add(score);
        }

        public void SaveLocalProgress()
        {
            DataRepository.Instance.FlushToSQLite();
        }

        public void TrySyncToCloud()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;
            CloudSyncService.Instance?.PushPendingScores(_pendingScores);
            _pendingScores.Clear();
        }

        public LearnerProgressSummary GetSummary(string learnerId)
        {
            return DataRepository.Instance.BuildProgressSummary(learnerId);
        }
    }

    [System.Serializable]
    public class PerformanceScore
    {
        public string   LearnerId;
        public string   SessionId;
        public string   ModuleId;
        public string   PuzzleId;
        public bool     Passed;
        public int      Stars;        // 0-3
        public float    TimeTaken;
        public System.DateTime RecordedAt;
    }

    [System.Serializable]
    public class LearnerProgressSummary
    {
        public string LearnerId;
        public int    TotalPuzzlesAttempted;
        public int    TotalPuzzlesPassed;
        public int    TotalStars;
        public Dictionary<string, float> ModuleCompletionPercent;
    }
}
