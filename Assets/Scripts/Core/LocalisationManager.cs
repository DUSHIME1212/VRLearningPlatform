using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace VRLearning.Core
{
    /// <summary>
    /// Loads JSON string tables for English and Kinyarwanda.
    /// All UI text fetched via Get(key). Language can be swapped at runtime.
    /// </summary>
    public class LocalisationManager : MonoBehaviour
    {
        public static LocalisationManager Instance { get; private set; }

        private Dictionary<string, string> _strings = new();
        private Language _currentLanguage = Language.Kinyarwanda;

        public Language CurrentLanguage => _currentLanguage;
        public event System.Action OnLanguageChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(_currentLanguage);
        }

        public void SetLanguage(Language lang)
        {
            if (_currentLanguage == lang) return;
            _currentLanguage = lang;
            LoadLanguage(lang);
            OnLanguageChanged?.Invoke();
        }

        public string Get(string key)
        {
            return _strings.TryGetValue(key, out var val) ? val : $"[{key}]";
        }

        private void LoadLanguage(Language lang)
        {
            string file = lang == Language.Kinyarwanda ? "strings_rw" : "strings_en";
            TextAsset asset = Resources.Load<TextAsset>($"Localisation/{file}");
            if (asset == null) { Debug.LogWarning($"[Localisation] Missing file: {file}"); return; }

            _strings.Clear();
            var data = JsonUtility.FromJson<LocalisationTable>(asset.text);
            foreach (var entry in data.entries)
                _strings[entry.key] = entry.value;

            Debug.Log($"[Localisation] Loaded {_strings.Count} strings for {lang}");
        }
    }

    [System.Serializable]
    public class LocalisationTable
    {
        public List<LocalisationEntry> entries;
    }

    [System.Serializable]
    public class LocalisationEntry
    {
        public string key;
        public string value;
    }
}
