using UnityEngine;
using System.Collections;

namespace VRLearning
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject sessionWarningPanel;
        [SerializeField] private TMPro.TextMeshProUGUI sessionWarningText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowTimeWarning(float maxMinutes)
        {
            if (sessionWarningPanel == null) return;
            sessionWarningPanel.SetActive(true);
            string key = "ui_session_warning";
            sessionWarningText.text = Core.LocalisationManager.Instance?.Get(key)
                                      ?? $"Session limit: {maxMinutes} min";
            StartCoroutine(HideAfter(sessionWarningPanel, 5f));
        }

        public void ShowHint(string hintText)
        {
            if (hintPanel == null) return;
            hintPanel.SetActive(true);
            var label = hintPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label) label.text = hintText;
        }

        public void ShowSuccess(int stars)
        {
            if (successPanel == null) return;
            successPanel.SetActive(true);
        }

        private IEnumerator HideAfter(GameObject panel, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            panel?.SetActive(false);
        }
    }
}
