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
        [SerializeField] private float openDistance = 1.2f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // This canvas is a single persistent, DontDestroyOnLoad instance shared across every scene
        // (world-space, static position) — without this, it would only look right in whichever
        // scene it happened to be authored near. Re-centering on the player's current camera each
        // time a panel is shown keeps it readable regardless of which experiment scene is active.
        private void FaceCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.position = cam.transform.position + cam.transform.forward * openDistance;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
        }

        public void ShowTimeWarning(float maxMinutes)
        {
            if (sessionWarningPanel == null) return;
            FaceCamera();
            sessionWarningPanel.SetActive(true);
            string key = "ui_session_warning";
            sessionWarningText.text = Core.LocalisationManager.Instance?.Get(key)
                                      ?? $"Session limit: {maxMinutes} min";
            StartCoroutine(HideAfter(sessionWarningPanel, 5f));
        }

        private Coroutine _hintHide;

        public void ShowHint(string hintText, float autoHideSeconds = 9f)
        {
            if (hintPanel == null) return;
            FaceCamera();
            hintPanel.SetActive(true);
            var label = hintPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label) label.text = hintText;

            if (_hintHide != null) StopCoroutine(_hintHide);
            if (autoHideSeconds > 0f) _hintHide = StartCoroutine(HideAfter(hintPanel, autoHideSeconds));
        }

        public void ShowSuccess(int stars)
        {
            if (successPanel == null) return;
            FaceCamera();
            successPanel.SetActive(true);
        }

        private IEnumerator HideAfter(GameObject panel, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            panel?.SetActive(false);
        }
    }
}
