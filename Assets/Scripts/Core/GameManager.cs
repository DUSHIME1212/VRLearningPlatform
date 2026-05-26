using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace VRLearning.Core
{
    /// <summary>
    /// Singleton. Bootstraps the app, owns the session lifecycle,
    /// and routes between scenes. Survives scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SessionManager sessionManager;
        [SerializeField] private LocalisationManager localisationManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ProgressTracker progressTracker;

        [Header("Config")]
        [SerializeField] private float vrSessionMaxMinutes = 25f;

        private float _sessionTimer;
        private bool _sessionActive;

        public AppState CurrentState { get; private set; } = AppState.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            TransitionTo(AppState.MainMenu);
        }

        private void Update()
        {
            if (!_sessionActive) return;
            _sessionTimer += Time.deltaTime;
            if (_sessionTimer >= vrSessionMaxMinutes * 60f)
                TriggerSessionTimeWarning();
        }

        public void TransitionTo(AppState newState)
        {
            CurrentState = newState;
            switch (newState)
            {
                case AppState.MainMenu:
                    SceneManager.LoadScene(SceneNames.MainMenu);
                    break;
                case AppState.CodeWorld:
                    StartSession();
                    SceneManager.LoadScene(SceneNames.CodeWorld);
                    break;
                case AppState.SimulationBuilder:
                    StartSession();
                    SceneManager.LoadScene(SceneNames.SimulationBuilder);
                    break;
                case AppState.ARWorksheet:
                    StartSession();
                    SceneManager.LoadScene(SceneNames.ARWorksheet);
                    break;
                case AppState.TeacherDashboard:
                    SceneManager.LoadScene(SceneNames.TeacherDashboard);
                    break;
            }
        }

        private void StartSession()
        {
            _sessionTimer = 0f;
            _sessionActive = true;
            sessionManager.BeginSession(sessionManager.CurrentProfile);
        }

        public void EndSession()
        {
            _sessionActive = false;
            progressTracker.SaveLocalProgress();
            progressTracker.TrySyncToCloud();
        }

        private void TriggerSessionTimeWarning()
        {
            UIManager.Instance?.ShowTimeWarning(vrSessionMaxMinutes);
        }
    }

    public enum AppState { Boot, MainMenu, CodeWorld, SimulationBuilder, ARWorksheet, TeacherDashboard }

    public static class SceneNames
    {
        public const string MainMenu          = "MainMenu";
        public const string CodeWorld         = "CodeWorld";
        public const string SimulationBuilder = "SimulationBuilder";
        public const string ARWorksheet       = "ARWorksheet";
        public const string TeacherDashboard  = "TeacherDashboard";
    }
}
