using NUnit.Framework;
using UnityEngine;
using VRLearning.Simulation;

namespace VRLearning.Tests.EditMode
{
    /// <summary>Minimal concrete PuzzleController so tests can exercise the real, shared base-class
    /// logic (SubmitSolution/CalculateStars/hint flow) without depending on any specific machine.</summary>
    public class FakePuzzleController : PuzzleController
    {
        public bool SolveOnNextEvaluate;
        public int InitialiseCallCount;
        public int EvaluateCallCount;

        protected override void InitialisePuzzle() => InitialiseCallCount++;

        protected override bool EvaluateSolution()
        {
            EvaluateCallCount++;
            return SolveOnNextEvaluate;
        }

        // Expose protected state for assertions without changing the production class.
        public bool Solved => (bool)typeof(PuzzleController)
            .GetField("_solved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(this);
    }

    public class PuzzleControllerTests
    {
        private GameObject _go;
        private FakePuzzleController _puzzle;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("FakePuzzle");
            _puzzle = _go.AddComponent<FakePuzzleController>();

            var def = ScriptableObject.CreateInstance<PuzzleDefinition>();
            def.PuzzleId = "test_puzzle";
            def.ModuleId = "test_module";
            def.Hints = new System.Collections.Generic.List<string>();

            var field = typeof(PuzzleController).GetField("puzzleData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_puzzle, def);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void SubmitSolution_NoHintsFirstAttempt_Awards3Stars()
        {
            _puzzle.SolveOnNextEvaluate = true;
            PuzzleResult result = null;
            _puzzle.OnPuzzleComplete += r => result = r;

            _puzzle.SubmitSolution();

            Assert.IsNotNull(result, "OnPuzzleComplete should fire on a successful solve.");
            Assert.AreEqual(3, result.Stars, "First-try, no-hint solve should award 3 stars.");
            Assert.IsTrue(result.Passed);
        }

        [Test]
        public void SubmitSolution_TwoFailuresThenSolve_Awards1Star()
        {
            PuzzleResult result = null;
            _puzzle.OnPuzzleComplete += r => result = r;

            _puzzle.SolveOnNextEvaluate = false;
            _puzzle.SubmitSolution(); // attempt 1, fail
            _puzzle.SubmitSolution(); // attempt 2, fail (also triggers a hint, attempts >= 2)

            _puzzle.SolveOnNextEvaluate = true;
            _puzzle.SubmitSolution(); // attempt 3, succeed

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Stars, "3 attempts should drop the reward to 1 star.");
            Assert.AreEqual(3, result.Attempts);
        }

        [Test]
        public void SubmitSolution_AlreadySolved_DoesNotFireCompleteAgain()
        {
            int completeCount = 0;
            _puzzle.OnPuzzleComplete += _ => completeCount++;

            _puzzle.SolveOnNextEvaluate = true;
            _puzzle.SubmitSolution(); // solves it
            _puzzle.SubmitSolution(); // should be a no-op — already solved

            Assert.AreEqual(1, completeCount, "SubmitSolution must be idempotent once solved.");
            Assert.AreEqual(1, _puzzle.EvaluateCallCount, "EvaluateSolution should not be re-checked after solving.");
        }

        [Test]
        public void SubmitSolution_Failure_DoesNotFireComplete()
        {
            bool fired = false;
            _puzzle.OnPuzzleComplete += _ => fired = true;

            _puzzle.SolveOnNextEvaluate = false;
            _puzzle.SubmitSolution();

            Assert.IsFalse(fired, "A failed attempt must not raise OnPuzzleComplete.");
            Assert.IsFalse(_puzzle.Solved);
        }

        [Test]
        public void SubmitSolution_SecondFailure_RequestsAHint()
        {
            int? hintIndex = null;
            _puzzle.OnHintRequested += i => hintIndex = i;
            var def = ScriptableObject.CreateInstance<PuzzleDefinition>();
            def.Hints = new System.Collections.Generic.List<string> { "hint_1", "hint_2" };
            var field = typeof(PuzzleController).GetField("puzzleData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_puzzle, def);

            _puzzle.SolveOnNextEvaluate = false;
            _puzzle.SubmitSolution(); // attempt 1 — no hint yet
            Assert.IsNull(hintIndex);

            _puzzle.SubmitSolution(); // attempt 2 — hint should fire
            Assert.AreEqual(0, hintIndex, "The first hint (index 0) should be requested after the 2nd failed attempt.");
        }
    }
}
