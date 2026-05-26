using UnityEngine;
using System;
namespace VRLearning.Modules.SimulationBuilder
{
    public class RunButton : MonoBehaviour
    {
        public event Action OnPressed;
        public void Press() => OnPressed?.Invoke();
        public void SetInteractable(bool v) { }
    }
}
