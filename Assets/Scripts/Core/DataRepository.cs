using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Core
{
    /// <summary>
    /// Abstraction layer over SQLite (offline) and Firebase (online).
    /// All reads/writes go through here — no script talks to storage directly.
    /// Swap the backend by changing this class only.
    /// </summary>
    public class DataRepository : MonoBehaviour
    {
        public static DataRepository Instance { get; private set; }

        // In a real build these would be SQLiteConnection / FirebaseFirestore
        private readonly Dictionary<string, LearnerProfile>      _learners  = new();
        private readonly List<SessionData>                        _sessions  = new();
        private readonly List<PerformanceScore>                   _scores    = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSQLite();
        }

        private void InitSQLite()
        {
            // TODO: SQLiteConnection conn = new SQLiteConnection(Application.persistentDataPath + "/vrl.db");
            // conn.CreateTable<LearnerProfile>();
            // conn.CreateTable<SessionData>();
            // conn.CreateTable<PerformanceScore>();
            Debug.Log("[DataRepository] SQLite initialised (stub)");
        }

        public LearnerProfile LoadLearner(string id)
        {
            _learners.TryGetValue(id, out var profile);
            return profile;
        }

        public void SaveLearner(LearnerProfile profile) => _learners[profile.LearnerId] = profile;
        public void SaveSession(SessionData session)     => _sessions.Add(session);
        public void SaveScore(PerformanceScore score)    => _scores.Add(score);

        public void FlushToSQLite()
        {
            // TODO: batch insert _scores and _sessions into SQLite
            Debug.Log($"[DataRepository] Flushed {_scores.Count} scores to SQLite");
        }

        public LearnerProgressSummary BuildProgressSummary(string learnerId)
        {
            var scores = _scores.FindAll(s => s.LearnerId == learnerId);
            return new LearnerProgressSummary
            {
                LearnerId              = learnerId,
                TotalPuzzlesAttempted  = scores.Count,
                TotalPuzzlesPassed     = scores.FindAll(s => s.Passed).Count,
                TotalStars             = scores.ConvertAll(s => s.Stars).Count > 0
                                         ? scores.ConvertAll(s => s.Stars).ToArray()[0] : 0,
                ModuleCompletionPercent = new Dictionary<string, float>()
            };
        }
    }
}
