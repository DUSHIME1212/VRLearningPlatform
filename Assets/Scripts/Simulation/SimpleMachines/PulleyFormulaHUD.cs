using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    /// <summary>
    /// Whiteboard for the pulley: shows, in real time, that pulling the rope down lifts the load
    /// the same distance up (a fixed pulley redirects force, MA = 1). Reads the live lift height
    /// from <see cref="PulleyController"/>.
    /// </summary>
    public class PulleyFormulaHUD : FormulaHUD
    {
        [SerializeField] private PulleyController pulley;
        [Tooltip("Colour matching the load block, for the 'Load' term.")]
        [SerializeField] private Color loadColor = new Color(0.9f, 0.3f, 0.2f);

        protected override string Title => "Pulley Power";

        protected override void Refresh()
        {
            if (pulley == null || formulaLabel == null) return;

            float lifted = pulley.LoadLifted;
            formulaLabel.text =
                $"Pull the rope <b>down</b>  →  the {Colorize("Load", loadColor)} goes <b>up</b>!\n" +
                $"<size=120%>Rope pulled  =  Load lifted  =  <b>{lifted:0.00} m</b></size>";

            if (resultLabel != null)
                resultLabel.text = "Mechanical Advantage = 1  (redirects your pull)";
        }
    }
}
