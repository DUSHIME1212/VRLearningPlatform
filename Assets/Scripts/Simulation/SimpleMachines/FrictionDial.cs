using UnityEngine;
using Unity.VRTemplate;

namespace VRLearning.Simulation.SimpleMachines
{
    [RequireComponent(typeof(XRKnob))]
    public class FrictionDial : MonoBehaviour
    {
        [SerializeField] private InclinedPlaneController planeController;

        private void Awake()
        {
            var knob = GetComponent<XRKnob>();
            knob.onValueChange.AddListener(v => planeController?.SetFriction(v));
        }
    }
}
