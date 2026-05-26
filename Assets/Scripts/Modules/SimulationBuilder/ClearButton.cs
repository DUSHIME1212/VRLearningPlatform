using UnityEngine;
using System;
namespace VRLearning.Modules.SimulationBuilder
{
    public class ClearButton : MonoBehaviour
    {
        public event Action OnPressed;
        public void Press() => OnPressed?.Invoke();
    }
}
