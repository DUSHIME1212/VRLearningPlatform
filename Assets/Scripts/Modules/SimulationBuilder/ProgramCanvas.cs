using UnityEngine;
using System.Collections.Generic;
using System;
namespace VRLearning.Modules.SimulationBuilder
{
    public class ProgramCanvas : MonoBehaviour
    {
        public event Action<List<BuilderBlock>> OnProgramChanged;
        private List<BuilderBlock> _blocks = new();
        public void Clear() { _blocks.Clear(); OnProgramChanged?.Invoke(_blocks); }
    }
}
