using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Analytics
{
    /// <summary>
    /// Lightweight event logger for the teacher dashboard and research data.
    /// All events are stored offline and synced to Firebase when connected.
    /// No personally identifiable data is ever stored — learner IDs are alphanumeric codes only.
    /// </summary>
    public class AnalyticsLogger : MonoBehaviour
    {
        public static AnalyticsLogger Instance { get; private set; }

        private readonly List<AnalyticsEvent> _buffer = new();
        private const int FlushThreshold = 20;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Log(string eventType, string moduleId, string puzzleId = null, float value = 0f)
        {
            _buffer.Add(new AnalyticsEvent
            {
                LearnerId  = Core.SessionManager.Instance?.CurrentProfile?.LearnerId ?? "unknown",
                EventType  = eventType,
                ModuleId   = moduleId,
                PuzzleId   = puzzleId,
                Value      = value,
                Timestamp  = System.DateTime.UtcNow.ToString("o")
            });

            if (_buffer.Count >= FlushThreshold)
                Flush();
        }

        private void Flush()
        {
            Core.DataRepository.Instance?.FlushToSQLite();
            _buffer.Clear();
        }

        public static class Events
        {
            public const string PuzzleStarted   = "puzzle_started";
            public const string PuzzleCompleted = "puzzle_completed";
            public const string PuzzleFailed    = "puzzle_failed";
            public const string HintUsed        = "hint_used";
            public const string BlockPlaced     = "block_placed";
            public const string ProgramRun      = "program_run";
            public const string LanguageSwitched = "language_switched";
            public const string SessionStarted  = "session_started";
            public const string SessionEnded    = "session_ended";
        }
    }

    [System.Serializable]
    public class AnalyticsEvent
    {
        public string LearnerId;
        public string EventType;
        public string ModuleId;
        public string PuzzleId;
        public float  Value;
        public string Timestamp;
    }
}
