using System.Text;
using UnityEngine;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    /// <summary>
    /// Curriculum flow map (FR-15): shows each module and its experiments with lock/complete state,
    /// driven by <see cref="ProgressGate"/> against the persisted scores. Falls back to a built-in
    /// module list if none is assigned in the Inspector.
    /// </summary>
    public class FlowMapUI : MonoBehaviour
    {
        [System.Serializable]
        public class Module
        {
            public string title;
            public string[] experimentTitles;
            public string[] puzzleIds;
        }

        [SerializeField] private TMP_Text mapText;
        [SerializeField] private Module[] modules;

        private void Start() => Refresh();

        public void Refresh()
        {
            if (mapText == null) return;

            string learner = SessionManager.Instance != null && SessionManager.Instance.CurrentProfile != null
                ? SessionManager.Instance.CurrentProfile.LearnerId
                : "guest";

            var sb = new StringBuilder();
            foreach (var m in Modules)
            {
                sb.AppendLine($"<b>{m.title}</b>");
                for (int i = 0; i < m.experimentTitles.Length; i++)
                {
                    string pid = i < m.puzzleIds.Length ? m.puzzleIds[i] : string.Empty;
                    bool unlocked = ProgressGate.IsUnlocked(learner, m.puzzleIds, i);
                    bool done = ProgressGate.HasPassed(learner, pid);
                    string tag = done ? "[done]  " : unlocked ? "[open]  " : "[locked]";
                    sb.AppendLine($"   {tag} {m.experimentTitles[i]}");
                }
                sb.AppendLine();
            }
            mapText.text = sb.ToString();
        }

        private Module[] Modules => (modules != null && modules.Length > 0) ? modules : Default();

        private static Module[] Default() => new[]
        {
            new Module { title = "Simple Machines",   experimentTitles = new[]{"The Lever","The Pulley","Inclined Plane"},          puzzleIds = new[]{"quiz_lever","quiz_pulley","quiz_incline"} },
            new Module { title = "Blood Circulation", experimentTitles = new[]{"The Heart Pump","Arterial Flow","Capillary Exchange"}, puzzleIds = new[]{"quiz_heart","quiz_artery","quiz_capillary"} },
            new Module { title = "Breathing",         experimentTitles = new[]{"Lung Expansion","The Diaphragm","Gas Exchange"},     puzzleIds = new[]{"quiz_breathing","quiz_diaphragm","quiz_gasexchange"} },
        };
    }
}
