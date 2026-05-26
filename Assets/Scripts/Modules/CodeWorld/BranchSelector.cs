using UnityEngine;
namespace VRLearning.Modules.CodeWorld
{
    public class BranchSelector : MonoBehaviour
    {
        public ActionType SelectedAction { get; private set; }
        public void Select(ActionType action) { SelectedAction = action; }
    }
}
