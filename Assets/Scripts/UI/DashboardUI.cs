using System.Text;
using UnityEngine;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    /// <summary>
    /// In-app teacher dashboard (FR-06/FR-13): lists every learner's progress read from the local
    /// encrypted store (<see cref="DataRepository"/>). Simple functional table — restyle later; a
    /// Firebase-hosted web dashboard can follow once cloud sync is live.
    /// </summary>
    public class DashboardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text tableText;

        private void Start() => Refresh();

        public void Refresh()
        {
            if (tableText == null) return;

            var repo = DataRepository.Instance;
            if (repo == null) { tableText.text = "No data service running."; return; }

            var sb = new StringBuilder();
            sb.AppendLine("Learner              Attempted   Passed   Stars");
            sb.AppendLine("------------------------------------------------");

            var summaries = repo.AllSummaries();
            if (summaries.Count == 0)
                sb.AppendLine("(no sessions recorded yet)");
            else
                foreach (var s in summaries)
                    sb.AppendLine($"{Pad(s.LearnerId, 18)} {s.TotalPuzzlesAttempted,8} {s.TotalPuzzlesPassed,8} {s.TotalStars,7}");

            tableText.text = sb.ToString();
        }

        private static string Pad(string s, int n)
        {
            s ??= "";
            return s.Length >= n ? s.Substring(0, n) : s.PadRight(n);
        }
    }
}
