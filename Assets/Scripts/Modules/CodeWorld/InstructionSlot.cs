using UnityEngine;
namespace VRLearning.Modules.CodeWorld
{
    public class InstructionSlot : MonoBehaviour
    {
        public int  SlotIndex;
        public bool IsOccupied { get; private set; }
        private InstructionBlock _occupant;
        public void Accept(InstructionBlock block) { _occupant = block; IsOccupied = true; }
        public void Clear() { _occupant = null; IsOccupied = false; }
    }
}
