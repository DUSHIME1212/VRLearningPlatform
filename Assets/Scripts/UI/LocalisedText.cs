using UnityEngine;
using TMPro;

namespace VRLearning.UI
{
    /// <summary>
    /// Attach to any TextMeshPro component. It will automatically update
    /// its text whenever the language is switched at runtime.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalisedText : MonoBehaviour
    {
        [SerializeField] private string localisationKey;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Core.LocalisationManager.Instance.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (Core.LocalisationManager.Instance != null)
                Core.LocalisationManager.Instance.OnLanguageChanged -= Refresh;
        }

        private void Refresh()
        {
            _text.text = Core.LocalisationManager.Instance?.Get(localisationKey) ?? localisationKey;
        }
    }
}
