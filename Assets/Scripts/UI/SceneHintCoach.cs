using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRLearning.Core;
using VRLearning.Parts;
using VRLearning.Simulation.SimpleMachines;

namespace VRLearning.UI
{
    /// <summary>
    /// Gentle, contextual onboarding for kids who've never used VR. On entering an experiment it
    /// shows a short first-time instruction, then — only while the child is idle (not interacting)
    /// — cycles a few "what to do" tips through the existing <see cref="UIManager"/> hint panel.
    /// The idle timer resets on real activity (selecting a part, placing/lifting a weight), so busy
    /// kids aren't nagged. Self-bootstraps once and re-initialises on every scene load, so no scene
    /// or prefab needs editing. Fully defensive (null-checked, no per-frame allocation).
    /// </summary>
    public class SceneHintCoach : MonoBehaviour
    {
        private const float IdleSeconds = 15f; // no-activity gap before the next tip
        private const float FirstDelay  = 2.5f; // grace period before the entry instruction

        private static SceneHintCoach _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("SceneHintCoach");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SceneHintCoach>();
        }

        private string[] _hintKeys;
        private int _next;
        private float _idleTimer;

        private PartsInfoPanel _parts;
        private readonly List<WeightBlock> _weights = new();

        private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void Start() => InitForScene(SceneManager.GetActiveScene().name);

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InitForScene(scene.name);

        private void InitForScene(string sceneName)
        {
            Unsubscribe();
            _hintKeys   = DefaultKeysForScene(sceneName);
            _next       = 0;
            _idleTimer  = IdleSeconds - FirstDelay; // entry instruction shows shortly after load
            if (_hintKeys == null) return;          // non-experiment scene (e.g. the hub): stay quiet

            _parts = FindFirstObjectByType<PartsInfoPanel>(FindObjectsInactive.Include);
            if (_parts != null) _parts.PartChanged += ResetIdle;

            _weights.Clear();
            foreach (var w in FindObjectsByType<WeightBlock>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (w == null) continue;
                _weights.Add(w);
                w.OnPlaced += OnWeightPlaced;
                w.OnLifted += OnWeightLifted;
            }
        }

        private void Unsubscribe()
        {
            if (_parts != null) _parts.PartChanged -= ResetIdle;
            _parts = null;
            foreach (var w in _weights)
            {
                if (w == null) continue;
                w.OnPlaced -= OnWeightPlaced;
                w.OnLifted -= OnWeightLifted;
            }
            _weights.Clear();
        }

        private void Update()
        {
            if (_hintKeys == null || _next >= _hintKeys.Length) return;

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= IdleSeconds)
            {
                ShowTip(_hintKeys[_next]);
                _next++;
                _idleTimer = 0f;
            }
        }

        private void ShowTip(string key)
        {
            string text = LocalisationManager.Instance != null ? LocalisationManager.Instance.Get(key) : key;
            UIManager.Instance?.ShowHint(text);
        }

        private void OnWeightPlaced(WeightBlock w, Vector3 p) => ResetIdle();
        private void OnWeightLifted(WeightBlock w) => ResetIdle();
        private void ResetIdle() => _idleTimer = 0f;

        // First key = the entry instruction; the rest are idle "stuck" tips, shown once each.
        private static string[] DefaultKeysForScene(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            if (s.StartsWith("SimpleMachines_Lever"))        return new[] { "coach_lever_1", "coach_lever_2" };
            if (s.StartsWith("SimpleMachines_Pulley"))       return new[] { "coach_pulley_1", "coach_pulley_2" };
            if (s.StartsWith("SimpleMachines_Incl"))         return new[] { "coach_incline_1", "coach_incline_2" };
            if (s.StartsWith("BloodCirc") || s.StartsWith("Breathing"))
                                                             return new[] { "coach_anatomy_1", "coach_anatomy_2" };
            return null; // hub / menu scenes: no coaching
        }
    }
}
