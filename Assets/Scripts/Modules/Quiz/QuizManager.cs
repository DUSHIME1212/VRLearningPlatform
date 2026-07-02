using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using VRLearning.Core;
using VRLearning.Data;

namespace VRLearning.Modules.Quiz
{
    /// <summary>
    /// Drives a multiple-choice quiz from a <see cref="QuizDefinition"/>.
    /// Spawns one button per answer option, scores the run, shows per-question
    /// feedback, then a final result. Wire the UI references in the Inspector
    /// (the QuizPanel prefab does this for you).
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        [System.Serializable] public class QuizCompletedEvent : UnityEvent<int, int> { } // score, total

        [Header("Data")]
        [SerializeField] private QuizDefinition quiz;
        [SerializeField] private bool startOnEnable = true;
        [Tooltip("If true, records the result through ProgressTracker (needs an active learner session).")]
        [SerializeField] private bool recordToProgress = false;

        [Header("Question UI")]
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text questionLabel;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private Transform answerContainer;
        [Tooltip("A disabled Button (with a TMP_Text child) used as the template for answer buttons.")]
        [SerializeField] private Button answerButtonTemplate;
        [SerializeField] private Button nextButton;

        [Header("Panels")]
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultLabel;

        [Header("Answer Layout")]
        [Tooltip("Vertical gap (in canvas units) between spawned answer buttons.")]
        [SerializeField] private float answerSpacing = 16f;

        [Header("Answer Colors")]
        [SerializeField] private Color normalColor  = new Color(0.15f, 0.45f, 0.85f);
        [SerializeField] private Color correctColor = new Color(0.20f, 0.70f, 0.30f);
        [SerializeField] private Color wrongColor   = new Color(0.80f, 0.25f, 0.20f);

        [Header("Events")]
        public QuizCompletedEvent OnQuizCompleted;

        private readonly List<Button> _spawned = new List<Button>();
        private int _index;
        private int _score;
        private bool _answered;

        private void OnEnable()
        {
            if (startOnEnable) StartQuiz();
        }

        /// <summary>(Re)start the quiz from the first question.</summary>
        public void StartQuiz()
        {
            if (quiz == null || quiz.Questions.Count == 0)
            {
                Debug.LogWarning($"{name}: QuizManager has no quiz / questions assigned.", this);
                return;
            }
            _index = 0;
            _score = 0;
            if (resultPanel != null) resultPanel.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(true);
            if (titleLabel != null) titleLabel.text = quiz.Title;
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            _answered = false;
            var q = quiz.Questions[_index];

            if (questionLabel != null) questionLabel.text = q.Question;
            if (progressLabel != null) progressLabel.text = $"Question {_index + 1} / {quiz.Questions.Count}";
            if (feedbackLabel != null) feedbackLabel.text = string.Empty;
            if (nextButton != null) nextButton.gameObject.SetActive(false);

            BuildAnswers(q);
        }

        private void BuildAnswers(QuizQuestion q)
        {
            foreach (var b in _spawned)
                if (b != null) Destroy(b.gameObject);
            _spawned.Clear();

            if (answerButtonTemplate == null || answerContainer == null) return;
            answerButtonTemplate.gameObject.SetActive(false);

            // Manual positioning below is authoritative — disable any layout group on the
            // container so it can't fight the anchored positions we set per button.
            var layout = answerContainer.GetComponent<LayoutGroup>();
            if (layout != null) layout.enabled = false;

            for (int i = 0; i < q.Options.Count; i++)
            {
                int idx = i;
                var btn = Instantiate(answerButtonTemplate, answerContainer);
                btn.gameObject.SetActive(true);
                btn.interactable = true;

                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = q.Options[i];
                SetButtonColor(btn, normalColor);

                // Manual full-width, top-down stacking (robust against layout-group quirks).
                var rt = btn.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot     = new Vector2(0.5f, 1f);
                float h = rt.sizeDelta.y > 1f ? rt.sizeDelta.y : 84f;
                rt.sizeDelta = new Vector2(-20f, h);           // stretch to container minus 10px each side
                rt.anchoredPosition = new Vector2(0f, -i * (h + answerSpacing));

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Answer(idx));
                _spawned.Add(btn);
            }
        }

        /// <summary>Called by an answer button; scores it and reveals feedback.</summary>
        public void Answer(int chosen)
        {
            if (_answered) return;
            _answered = true;

            var q = quiz.Questions[_index];
            bool correct = chosen == q.CorrectIndex;
            if (correct) _score++;

            for (int i = 0; i < _spawned.Count; i++)
            {
                _spawned[i].interactable = false;
                if (i == q.CorrectIndex) SetButtonColor(_spawned[i], correctColor);
                else if (i == chosen)    SetButtonColor(_spawned[i], wrongColor);
            }

            if (feedbackLabel != null)
                feedbackLabel.text = (correct ? "Correct!  " : "Not quite.  ") + q.Explanation;

            AudioManager.Instance?.PlaySFX(correct ? quiz.CorrectClip : quiz.WrongClip);

            if (nextButton != null) nextButton.gameObject.SetActive(true);
        }

        /// <summary>Advance to the next question, or finish if this was the last.</summary>
        public void Next()
        {
            _index++;
            if (_index >= quiz.Questions.Count) Finish();
            else ShowQuestion();
        }

        private void Finish()
        {
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(true);

            int total = quiz.Questions.Count;
            bool passed = total > 0 && (float)_score / total >= quiz.PassThreshold;

            if (resultLabel != null)
                resultLabel.text = $"You scored {_score} / {total}\n" + (passed ? "Passed!" : "Keep practicing!");

            AudioManager.Instance?.PlaySFX(quiz.CompleteClip);

            if (recordToProgress && ProgressTracker.Instance != null)
                ProgressTracker.Instance.RecordPuzzleResult(quiz.ModuleId, quiz.QuizId, passed, StarsFor(_score, total), 0f);

            OnQuizCompleted?.Invoke(_score, total);
        }

        private int StarsFor(int score, int total)
        {
            if (total == 0) return 0;
            float r = (float)score / total;
            if (r >= 0.99f) return 3;
            if (r >= 0.75f) return 2;
            if (r >= quiz.PassThreshold) return 1;
            return 0;
        }

        private void SetButtonColor(Button b, Color c)
        {
            var img = b.GetComponent<Image>();
            if (img != null) img.color = c;
        }
    }
}
