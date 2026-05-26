using UnityEngine;
namespace VRLearning.Modules.ARWorksheet
{
    public class WorksheetOverlay : MonoBehaviour
    {
        public string MarkerName;
        [SerializeField] private Animator overlayAnimator;
        public void Activate(string markerName) { overlayAnimator?.SetTrigger("Show"); }
    }
}
