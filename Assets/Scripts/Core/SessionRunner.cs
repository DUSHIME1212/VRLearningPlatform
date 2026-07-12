using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRLearning.Core
{
    /// <summary>
    /// Drives the learner-session lifecycle in the real scene flow: begins a session when an
    /// experiment scene loads, ends it (saving + trying cloud sync) when leaving, and enforces the
    /// FR-11 cap — a warning at <see cref="warnMinutes"/> and a hard auto-end at <see cref="maxMinutes"/>.
    /// Lives on the persistent Managers object.
    /// </summary>
    public class SessionRunner : MonoBehaviour
    {
        [SerializeField] private float maxMinutes = 30f;   // hard cap (FR-11)
        [SerializeField] private float warnMinutes = 20f;  // warning (FR-11)
        [SerializeField] private string homeScene = "BasicScene";

        private float _timer;
        private bool _active;
        private bool _warned;

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // The scene already open when play starts doesn't raise sceneLoaded — handle it here.
        private void Start() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path.Contains("/Experiments/")) StartSession();
            else StopSession();
        }

        private void StartSession()
        {
            var sm = SessionManager.Instance;
            if (sm != null)
            {
                if (sm.CurrentProfile == null) sm.LoadProfile("guest");
                sm.BeginSession(sm.CurrentProfile);
            }
            _timer = 0f;
            _active = true;
            _warned = false;
        }

        private void StopSession()
        {
            if (!_active) return;
            _active = false;
            SessionManager.Instance?.EndSession(0f);
            ProgressTracker.Instance?.SaveLocalProgress();
            ProgressTracker.Instance?.TrySyncToCloud();
        }

        private void Update()
        {
            if (!_active) return;
            _timer += Time.deltaTime;

            if (!_warned && _timer >= warnMinutes * 60f)
            {
                _warned = true;
                UIManager.Instance?.ShowTimeWarning(maxMinutes - warnMinutes);
            }

            if (_timer >= maxMinutes * 60f)
            {
                StopSession();
                SceneManager.LoadScene(homeScene);
            }
        }
    }
}
