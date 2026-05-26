using UnityEngine;
using TMPro;
using System.Collections;
namespace VRLearning.Modules.CodeWorld
{
    public class NPCSpeechBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float hideAfter = 4f;
        public void Show(string text) { if (label) label.text = text; gameObject.SetActive(true); StopAllCoroutines(); StartCoroutine(Hide()); }
        private IEnumerator Hide() { yield return new WaitForSeconds(hideAfter); gameObject.SetActive(false); }
    }
}
