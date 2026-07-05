namespace VRLearning.Core
{
    /// <summary>
    /// Sequential unlocking (FR-14): an experiment in an ordered module list is unlocked when it is
    /// the first one, or when the learner has passed the previous one. UI (SimulationCardUI etc.)
    /// calls <see cref="IsUnlocked"/> to show the locked/unlocked state; the data comes from the
    /// persisted scores in <see cref="DataRepository"/>.
    /// </summary>
    public static class ProgressGate
    {
        /// <summary>Is the experiment at <paramref name="index"/> in this ordered list unlocked?</summary>
        public static bool IsUnlocked(string learnerId, string[] orderedPuzzleIds, int index)
        {
            if (orderedPuzzleIds == null || index <= 0) return true;
            if (index >= orderedPuzzleIds.Length) return false;
            return HasPassed(learnerId, orderedPuzzleIds[index - 1]);
        }

        /// <summary>Is a specific experiment unlocked, given the one that precedes it (null = always).</summary>
        public static bool IsUnlocked(string learnerId, string previousPuzzleId)
            => string.IsNullOrEmpty(previousPuzzleId) || HasPassed(learnerId, previousPuzzleId);

        public static bool HasPassed(string learnerId, string puzzleId)
            => DataRepository.Instance != null && DataRepository.Instance.HasPassed(learnerId, puzzleId);
    }
}
