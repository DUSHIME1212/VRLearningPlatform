using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using VRLearning.Core;

namespace VRLearning.Tests.EditMode
{
    public class ProgressGateTests
    {
        private static readonly FieldInfo ScoresField =
            typeof(DataRepository).GetField("_scores", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo InstanceProperty =
            typeof(DataRepository).GetProperty(nameof(DataRepository.Instance));

        private GameObject _go;
        private DataRepository _repo;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("FakeDataRepository");
            _repo = _go.AddComponent<DataRepository>();

            // AppBootstrap's [RuntimeInitializeOnLoadMethod] instantiates a real, DontDestroyOnLoad
            // "Managers" DataRepository whenever anything triggers a scene load (including, in
            // practice, an Editor session that has entered Play Mode at some point) — that instance
            // can already hold the static Instance slot, which would make Awake()'s self-claim
            // check silently self-destruct our fake and leave the real one in place. Force it
            // directly so these tests are hermetic regardless of what else is loaded.
            InstanceProperty.SetValue(null, _repo);

            // Also clear any on-disk save data Awake()/Load() may have picked up (Application.
            // persistentDataPath is shared with actual Play Mode runs), so tests are hermetic
            // regardless of what's saved on this machine.
            ScoresField.SetValue(_repo, new List<PerformanceScore>());
        }

        [TearDown]
        public void TearDown()
        {
            InstanceProperty.SetValue(null, null);
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private void AddScore(string learnerId, string puzzleId, bool passed)
        {
            var scores = (List<PerformanceScore>)ScoresField.GetValue(_repo);
            scores.Add(new PerformanceScore { LearnerId = learnerId, PuzzleId = puzzleId, Passed = passed });
        }

        [Test]
        public void IsUnlocked_FirstInOrderedList_AlwaysUnlocked()
        {
            var ids = new[] { "p1", "p2", "p3" };
            Assert.IsTrue(ProgressGate.IsUnlocked("learner1", ids, 0));
        }

        [Test]
        public void IsUnlocked_NullOrderedList_AlwaysUnlocked()
        {
            Assert.IsTrue(ProgressGate.IsUnlocked("learner1", null, 2));
        }

        [Test]
        public void IsUnlocked_IndexBeyondList_ReturnsFalse()
        {
            var ids = new[] { "p1", "p2" };
            Assert.IsFalse(ProgressGate.IsUnlocked("learner1", ids, 5));
        }

        [Test]
        public void IsUnlocked_PreviousNotPassed_StaysLocked()
        {
            var ids = new[] { "p1", "p2" };
            Assert.IsFalse(ProgressGate.IsUnlocked("learner1", ids, 1), "p2 should stay locked until p1 is passed.");
        }

        [Test]
        public void IsUnlocked_PreviousPassed_Unlocks()
        {
            AddScore("learner1", "p1", true);
            var ids = new[] { "p1", "p2" };

            Assert.IsTrue(ProgressGate.IsUnlocked("learner1", ids, 1));
        }

        [Test]
        public void IsUnlocked_PreviousOnlyFailed_StaysLocked()
        {
            AddScore("learner1", "p1", false);
            var ids = new[] { "p1", "p2" };

            Assert.IsFalse(ProgressGate.IsUnlocked("learner1", ids, 1), "A failing attempt must not unlock the next experiment.");
        }

        [Test]
        public void IsUnlocked_NullPreviousPuzzleId_AlwaysUnlocked()
        {
            Assert.IsTrue(ProgressGate.IsUnlocked("learner1", (string)null));
        }

        [Test]
        public void HasPassed_IsScopedPerLearner()
        {
            AddScore("learnerA", "p1", true);

            Assert.IsFalse(ProgressGate.HasPassed("learnerB", "p1"), "Passing scores must not leak across learners.");
        }
    }
}
