using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    /// <summary>
    /// Whiteboard for the inclined plane: shows the ramp's mechanical advantage
    /// (MA = ramp length ÷ height, a geometric constant) plus the LIVE friction μ and slide
    /// state from <see cref="InclinedPlaneController"/> as the child turns the friction dial.
    /// </summary>
    public class InclinedPlaneFormulaHUD : FormulaHUD
    {
        [SerializeField] private InclinedPlaneController plane;
        [Tooltip("Ramp slope length in metres (for MA = length ÷ height).")]
        [SerializeField] private float rampLength = 2f;
        [Tooltip("Ramp vertical height in metres.")]
        [SerializeField] private float rampHeight = 1f;

        protected override string Title => "Ramp Rule";

        protected override void Refresh()
        {
            if (formulaLabel == null) return;

            float ma = rampHeight > 0.001f ? rampLength / rampHeight : 1f;
            formulaLabel.text =
                "MA = Ramp length ÷ Height\n" +
                $"<size=120%>MA = {rampLength:0.0} ÷ {rampHeight:0.0} = <b>{ma:0.0}× easier</b></size>";

            if (resultLabel != null && plane != null)
                resultLabel.text = $"Friction  μ = <b>{plane.CurrentFriction:0.00}</b>"
                                   + (plane.BlockMoving ? "   (sliding…)" : "");
        }
    }
}
