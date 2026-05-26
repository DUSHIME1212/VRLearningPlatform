using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Modules.SimulationBuilder
{
    /// <summary>
    /// Ages 10-12 module. Free-form drag-and-drop VR programming environment.
    /// Children assemble block programs and watch them execute on a virtual robot.
    /// </summary>
    public class SimulationBuilderManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuilderPalette    palette;
        [SerializeField] private ProgramCanvas     canvas;
        [SerializeField] private BuilderRobot      robot;
        [SerializeField] private RunButton         runButton;
        [SerializeField] private ClearButton       clearButton;

        [Header("Config")]
        [SerializeField] private int maxBlocks = 12;

        private List<BuilderBlock> _program = new();
        private bool _running;

        private void Start()
        {
            runButton.OnPressed   += RunProgram;
            clearButton.OnPressed += ClearProgram;
            canvas.OnProgramChanged += OnProgramChanged;
        }

        private void OnProgramChanged(List<BuilderBlock> blocks)
        {
            _program = blocks;
            runButton.SetInteractable(blocks.Count > 0 && blocks.Count <= maxBlocks && !_running);
        }

        private void RunProgram()
        {
            if (_running || _program.Count == 0) return;
            _running = true;
            runButton.SetInteractable(false);

            var instructions = _program.ConvertAll(b => b.InstructionType);
            robot.ExecuteProgram(instructions, OnProgramFinished);

            Core.ProgressTracker.Instance?.RecordPuzzleResult(
                "simulation_builder", "free_build_" + System.DateTime.UtcNow.Ticks,
                true, 1, Time.time);
        }

        private void OnProgramFinished()
        {
            _running = false;
            runButton.SetInteractable(true);
        }

        private void ClearProgram()
        {
            canvas.Clear();
            robot.ResetPosition();
        }
    }
}
