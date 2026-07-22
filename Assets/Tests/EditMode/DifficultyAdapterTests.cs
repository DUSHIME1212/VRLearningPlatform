using NUnit.Framework;
using UnityEngine;
using VRLearning.Simulation;

namespace VRLearning.Tests.EditMode
{
    public class DifficultyAdapterTests
    {
        private GameObject _go;
        private DifficultyAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("FakeDifficultyAdapter");
            _adapter = _go.AddComponent<DifficultyAdapter>();
        }

        [TearDown]
        public void TearDown()
        {
            // Unity's overridden == treats a destroyed object as null, so the next test's Awake()
            // correctly re-claims the static Instance instead of self-destructing.
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void RecordOutcome_ConsecutivePasses_RampsUpToHardAndCaps()
        {
            Assert.AreEqual(DifficultyLevel.Easy, _adapter.Current);

            _adapter.RecordOutcome(true);
            Assert.AreEqual(DifficultyLevel.Medium, _adapter.Current, "100% pass rate should raise Easy -> Medium.");

            _adapter.RecordOutcome(true);
            Assert.AreEqual(DifficultyLevel.Hard, _adapter.Current, "100% pass rate should raise Medium -> Hard.");

            _adapter.RecordOutcome(true);
            Assert.AreEqual(DifficultyLevel.Hard, _adapter.Current, "Difficulty must not climb past Hard.");
        }

        [Test]
        public void RecordOutcome_ConsecutiveFailuresAfterHard_RampsDownToEasyAndFloors()
        {
            _adapter.RecordOutcome(true);
            _adapter.RecordOutcome(true);
            Assert.AreEqual(DifficultyLevel.Hard, _adapter.Current, "Precondition: should be at Hard.");

            _adapter.RecordOutcome(false); // window [T,T,F] rate 0.67 — no change
            _adapter.RecordOutcome(false); // window [T,T,F,F] rate 0.5 — no change
            Assert.AreEqual(DifficultyLevel.Hard, _adapter.Current);

            _adapter.RecordOutcome(false); // window [T,T,F,F,F] rate 0.4 — drops to Medium
            Assert.AreEqual(DifficultyLevel.Medium, _adapter.Current);

            _adapter.RecordOutcome(false); // window [T,F,F,F,F] rate 0.2 — drops to Easy
            Assert.AreEqual(DifficultyLevel.Easy, _adapter.Current);

            _adapter.RecordOutcome(false); // window [F,F,F,F,F] rate 0 — already floored
            Assert.AreEqual(DifficultyLevel.Easy, _adapter.Current, "Difficulty must not drop below Easy.");
        }

        [Test]
        public void RecordOutcome_SingleFailureAtEasy_StaysAtEasy()
        {
            _adapter.RecordOutcome(false);
            Assert.AreEqual(DifficultyLevel.Easy, _adapter.Current);
        }
    }
}
