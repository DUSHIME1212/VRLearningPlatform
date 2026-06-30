using TMPro;
using UnityEngine;
using VRLearning.Core;

namespace VRLearning.Simulation.SimpleMachines
{
    [RequireComponent(typeof(TextMeshPro))]
    public class MachineLabel : MonoBehaviour
    {
        [SerializeField] private string keyEN;
        [SerializeField] private string keyRW;

        private TextMeshPro _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }

        private void OnEnable()
        {
            if (LocalisationManager.Instance != null)
                LocalisationManager.Instance.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (LocalisationManager.Instance != null)
                LocalisationManager.Instance.OnLanguageChanged -= Refresh;
        }

        private void Refresh()
        {
            var lang = SessionManager.Instance?.CurrentProfile?.PreferredLanguage
                       ?? Language.Kinyarwanda;

            string key = lang == Language.English ? keyEN : keyRW;
            _tmp.text = LocalisationManager.Instance != null
                ? LocalisationManager.Instance.Get(key)
                : key;
        }

        public void SetOverrideText(string text)
        {
            if (_tmp != null) _tmp.text = text;
        }
    }
}
