using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    /// <summary>
    /// Whiteboard for the lever: shows "Load × Distance = Effort × Distance" with the words and
    /// numbers colour-matched to the weight blocks, updating live as the child moves weights or
    /// changes the notch (distance). Reads everything from <see cref="LeverController"/>.
    /// Left arm = Effort, right arm = Load (matches the scene's static labels).
    /// </summary>
    public class LeverFormulaHUD : FormulaHUD
    {
        [SerializeField] private LeverController lever;

        protected override string Title => "Balance Rule";

        protected override void Refresh()
        {
            if (lever == null || formulaLabel == null) return;

            string load = lever.RightOccupied
                ? $"{Colorize($"{lever.RightWeight:0.#}", lever.RightColor)} × {lever.RightDistance:0.0}"
                : "__ × __";
            string effort = lever.LeftOccupied
                ? $"{Colorize($"{lever.LeftWeight:0.#}", lever.LeftColor)} × {lever.LeftDistance:0.0}"
                : "__ × __";

            formulaLabel.text =
                $"{Colorize("Load", lever.RightColor)} × Distance  =  {Colorize("Effort", lever.LeftColor)} × Distance\n" +
                $"<size=120%>{load}  =  {effort}</size>";

            if (resultLabel == null) return;

            if (lever.IsBalanced)
                resultLabel.text = "<color=#33CC55>⚖ Balanced!</color>";
            else if (lever.TorqueRight > lever.TorqueLeft)
                resultLabel.text = $"{Colorize("Load", lever.RightColor)} side wins ↓";
            else
                resultLabel.text = $"{Colorize("Effort", lever.LeftColor)} side wins ↓";
        }
    }
}
