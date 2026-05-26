using UnityEngine;

namespace VRLearning.Simulation
{
    /// <summary>
    /// Adjusts puzzle difficulty dynamically based on the learner's recent performance.
    /// Uses a rolling window of the last 5 puzzles.
    /// </summary>
    public class DifficultyAdapter : MonoBehaviour
    {
        public static DifficultyAdapter Instance { get; private set; }

        [SerializeField] private int windowSize = 5;
        [SerializeField] private float upThreshold   = 0.8f;  // >80% correct → increase
        [SerializeField] private float downThreshold = 0.4f;  // <40% correct → decrease

        private readonly System.Collections.Generic.Queue<bool> _window = new();
        private DifficultyLevel _current = DifficultyLevel.Easy;

        public DifficultyLevel Current => _current;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RecordOutcome(bool passed)
        {
            _window.Enqueue(passed);
            if (_window.Count > windowSize) _window.Dequeue();

            int passCount = 0;
            foreach (bool b in _window) if (b) passCount++;
            float rate = (float)passCount / _window.Count;

            if (rate >= upThreshold   && _current < DifficultyLevel.Hard)
                _current++;
            else if (rate <= downThreshold && _current > DifficultyLevel.Easy)
                _current--;

            Debug.Log($"[DifficultyAdapter] Pass rate={rate:P0} → {_current}");
        }
    }

    public enum DifficultyLevel { Easy = 0, Medium = 1, Hard = 2 }
}
