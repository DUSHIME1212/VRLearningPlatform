using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VRLearning.Core
{
    /// <summary>
    /// Offline data store. Persists learners, sessions and scores to a single **AES-256 encrypted
    /// JSON file** on device (FR-04 offline, NFR-06 encryption, NFR-11 zero-loss). Loads on start and
    /// writes on every change plus on pause/quit, so an interrupted session never loses data.
    ///
    /// The public API is storage-agnostic — swap this file for a SQLite-backed implementation later
    /// (TR-07) without touching any caller. Cloud sync stays in CloudSyncService (FR-10).
    /// </summary>
    public class DataRepository : MonoBehaviour
    {
        public static DataRepository Instance { get; private set; }

        private readonly Dictionary<string, LearnerProfile> _learners = new();
        private readonly List<SessionData>      _sessions = new();
        private readonly List<PerformanceScore> _scores   = new();

        private string FilePath => Path.Combine(Application.persistentDataPath, "vrl_data.enc");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void OnApplicationPause(bool paused) { if (paused) Persist(); }
        private void OnApplicationQuit() => Persist();

        // ---- public API (unchanged signatures) ----

        public LearnerProfile LoadLearner(string id)
        {
            _learners.TryGetValue(id, out var profile);
            return profile;
        }

        public void SaveLearner(LearnerProfile profile) { _learners[profile.LearnerId] = profile; Persist(); }
        public void SaveSession(SessionData session)     { _sessions.Add(session); Persist(); }
        public void SaveScore(PerformanceScore score)    { _scores.Add(score); Persist(); }

        /// <summary>Kept for callers; now flushes the encrypted JSON store.</summary>
        public void FlushToSQLite() => Persist();

        /// <summary>True if this learner has a passing score recorded for the given puzzle/experiment (FR-14).</summary>
        public bool HasPassed(string learnerId, string puzzleId)
            => _scores.Exists(s => s.LearnerId == learnerId && s.PuzzleId == puzzleId && s.Passed);

        /// <summary>Progress summary for every known learner (from profiles + recorded scores) — for the dashboard (FR-06/13).</summary>
        public List<LearnerProgressSummary> AllSummaries()
        {
            var ids = new HashSet<string>();
            foreach (var id in _learners.Keys) ids.Add(id);
            foreach (var s in _scores) if (!string.IsNullOrEmpty(s.LearnerId)) ids.Add(s.LearnerId);

            var list = new List<LearnerProgressSummary>();
            foreach (var id in ids) list.Add(BuildProgressSummary(id));
            return list;
        }

        public LearnerProgressSummary BuildProgressSummary(string learnerId)
        {
            var scores = _scores.FindAll(s => s.LearnerId == learnerId);
            int stars = 0;
            foreach (var s in scores) stars += s.Stars;

            return new LearnerProgressSummary
            {
                LearnerId               = learnerId,
                TotalPuzzlesAttempted   = scores.Count,
                TotalPuzzlesPassed      = scores.FindAll(s => s.Passed).Count,
                TotalStars              = stars,
                ModuleCompletionPercent = new Dictionary<string, float>()
            };
        }

        // ---- persistence ----

        private void Persist()
        {
            try
            {
                var blob = new Blob();
                foreach (var l in _learners.Values) blob.learners.Add(ToDto(l));
                foreach (var s in _sessions)        blob.sessions.Add(ToDto(s));
                foreach (var sc in _scores)         blob.scores.Add(ToDto(sc));

                string json = JsonUtility.ToJson(blob);
                File.WriteAllText(FilePath, EncryptionService.Encrypt(json));
            }
            catch (Exception e) { Debug.LogError("[DataRepository] Persist failed: " + e.Message); }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                string enc = File.ReadAllText(FilePath);
                if (string.IsNullOrEmpty(enc)) return;

                var blob = JsonUtility.FromJson<Blob>(EncryptionService.Decrypt(enc));
                if (blob == null) return;

                _learners.Clear(); _sessions.Clear(); _scores.Clear();
                foreach (var l in blob.learners)  _learners[l.LearnerId] = FromDto(l);
                foreach (var s in blob.sessions)  _sessions.Add(FromDto(s));
                foreach (var sc in blob.scores)   _scores.Add(FromDto(sc));

                Debug.Log($"[DataRepository] Loaded {_learners.Count} learners, {_sessions.Count} sessions, {_scores.Count} scores.");
            }
            catch (Exception e) { Debug.LogError("[DataRepository] Load failed: " + e.Message); }
        }

        // ---- serialisable DTOs (DateTime -> ticks, enums -> int) so JsonUtility round-trips cleanly ----

        [Serializable] private class Blob
        {
            public List<LearnerDto> learners = new();
            public List<SessionDto> sessions = new();
            public List<ScoreDto>   scores   = new();
        }
        [Serializable] private class LearnerDto
        { public string LearnerId, DisplayName; public int AgeGroup, PreferredLanguage, TotalStarsEarned; public long CreatedAt; }
        [Serializable] private class SessionDto
        { public string SessionId, LearnerId, ModuleId; public long StartTime, EndTime; public int Language; public float ScorePercent; public int PuzzlesCompleted, HintsUsed; }
        [Serializable] private class ScoreDto
        { public string LearnerId, SessionId, ModuleId, PuzzleId; public bool Passed; public int Stars; public float TimeTaken; public long RecordedAt; }

        private static LearnerDto ToDto(LearnerProfile l) => new()
        {
            LearnerId = l.LearnerId, DisplayName = l.DisplayName, AgeGroup = (int)l.AgeGroup,
            PreferredLanguage = (int)l.PreferredLanguage, TotalStarsEarned = l.TotalStarsEarned, CreatedAt = l.CreatedAt.Ticks
        };
        private static LearnerProfile FromDto(LearnerDto d) => new()
        {
            LearnerId = d.LearnerId, DisplayName = d.DisplayName, AgeGroup = (AgeGroup)d.AgeGroup,
            PreferredLanguage = (Language)d.PreferredLanguage, TotalStarsEarned = d.TotalStarsEarned, CreatedAt = new DateTime(d.CreatedAt)
        };

        private static SessionDto ToDto(SessionData s) => new()
        {
            SessionId = s.SessionId, LearnerId = s.LearnerId, ModuleId = s.ModuleId,
            StartTime = s.StartTime.Ticks, EndTime = s.EndTime.Ticks, Language = (int)s.Language,
            ScorePercent = s.ScorePercent, PuzzlesCompleted = s.PuzzlesCompleted, HintsUsed = s.HintsUsed
        };
        private static SessionData FromDto(SessionDto d) => new()
        {
            SessionId = d.SessionId, LearnerId = d.LearnerId, ModuleId = d.ModuleId,
            StartTime = new DateTime(d.StartTime), EndTime = new DateTime(d.EndTime), Language = (Language)d.Language,
            ScorePercent = d.ScorePercent, PuzzlesCompleted = d.PuzzlesCompleted, HintsUsed = d.HintsUsed
        };

        private static ScoreDto ToDto(PerformanceScore s) => new()
        {
            LearnerId = s.LearnerId, SessionId = s.SessionId, ModuleId = s.ModuleId, PuzzleId = s.PuzzleId,
            Passed = s.Passed, Stars = s.Stars, TimeTaken = s.TimeTaken, RecordedAt = s.RecordedAt.Ticks
        };
        private static PerformanceScore FromDto(ScoreDto d) => new()
        {
            LearnerId = d.LearnerId, SessionId = d.SessionId, ModuleId = d.ModuleId, PuzzleId = d.PuzzleId,
            Passed = d.Passed, Stars = d.Stars, TimeTaken = d.TimeTaken, RecordedAt = new DateTime(d.RecordedAt)
        };
    }
}
