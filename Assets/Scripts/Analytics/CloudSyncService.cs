using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRLearning
{
    /// <summary>
    /// Pushes buffered scores and analytics to Firebase Firestore
    /// when an internet connection is available. Silently skips if offline.
    /// </summary>
    public class CloudSyncService : MonoBehaviour
    {
        public static CloudSyncService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PushPendingScores(List<Core.PerformanceScore> scores)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;
            StartCoroutine(UploadScores(scores));
        }

        private IEnumerator UploadScores(List<Core.PerformanceScore> scores)
        {
            // TODO: Replace with Firebase SDK calls
            // FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            // foreach (var score in scores)
            // {
            //     var docRef = db.Collection("scores").Document(score.PuzzleId + "_" + score.LearnerId);
            //     yield return docRef.SetAsync(score).YieldWait();
            // }
            Debug.Log($"[CloudSync] Would upload {scores.Count} scores (Firebase stub)");
            yield return null;
        }
    }
}
