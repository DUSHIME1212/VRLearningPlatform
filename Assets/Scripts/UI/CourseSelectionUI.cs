using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.UI
{
    public class CourseSelectionUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject courseListPanel;
        [SerializeField] private GameObject courseDetailPanel;

        [Header("Course Cards")]
        [SerializeField] private Transform   courseCardsContainer;
        [SerializeField] private CourseCardUI courseCardPrefab;

        [Header("Data")]
        [SerializeField] private List<CourseData> courses;

        private CourseDetailUI _detailUI;

        private void Awake()
        {
            _detailUI = courseDetailPanel.GetComponent<CourseDetailUI>();
        }

        private void Start()
        {
            BuildCourseCards();
            courseListPanel.SetActive(true);
            courseDetailPanel.SetActive(false);
        }

        private void BuildCourseCards()
        {
            foreach (var course in courses)
            {
                var card = Instantiate(courseCardPrefab, courseCardsContainer);
                card.Bind(course, OnCourseCardClicked);
            }
        }

        public void OnCourseCardClicked(CourseData data)
        {
            _detailUI.Bind(data, ShowList);
            StartCoroutine(TransitionToDetail());
        }

        public void ShowList()
        {
            StartCoroutine(TransitionToList());
        }

        private IEnumerator TransitionToDetail()
        {
            yield return StartCoroutine(FadeOut(courseListPanel));
            courseListPanel.SetActive(false);
            courseDetailPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(courseDetailPanel));
        }

        private IEnumerator TransitionToList()
        {
            yield return StartCoroutine(FadeOut(courseDetailPanel));
            courseDetailPanel.SetActive(false);
            courseListPanel.SetActive(true);
            yield return StartCoroutine(FadeIn(courseListPanel));
        }

        private IEnumerator FadeOut(GameObject panel)
        {
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - Mathf.Clamp01(elapsed / 0.2f);
                yield return null;
            }
            cg.alpha = 0f;
        }

        private IEnumerator FadeIn(GameObject panel)
        {
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / 0.2f);
                yield return null;
            }
            cg.alpha = 1f;
        }
    }
}
