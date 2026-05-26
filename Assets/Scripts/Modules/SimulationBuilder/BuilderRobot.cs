using UnityEngine;
using System.Collections.Generic;
using System;
namespace VRLearning.Modules.SimulationBuilder
{
    public class BuilderRobot : MonoBehaviour
    {
        public void ExecuteProgram(List<Modules.CodeWorld.InstructionType> instructions, Action onFinished) => onFinished?.Invoke();
        public void ResetPosition() => transform.localPosition = Vector3.zero;
    }
}
