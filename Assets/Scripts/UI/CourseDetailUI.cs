using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    public class CourseDetailUI : MonoBehaviour
    {
        [Header("Course Header")]
        [SerializeField] private TextMeshProUGUI courseTitle;
        [SerializeField] private TextMeshProUGUI courseTitleRW;
        [SerializeField] private TextMeshProUGUI courseUnit;
        [SerializeField] private TextMeshProUGUI courseDesc;
        [SerializeField] private Image           headerAccentBar;

        [Header("Experiments")]
        [SerializeField] private Transform        simCardsContainer;
        [SerializeField] private SimulationCardUI simCardPrefab;

        [Header("Controls")]
        [SerializeField] private Button          backButton;
        [SerializeField] private Button          playButton;
        [SerializeField] private TextMeshProUGUI playButtonLabel;
        [SerializeField] private SceneNavigator  navigator;

        private SimulationData _selectedSim;
        private readonly List<SimulationCardUI> _cards = new();

        private void Awake()
        {
            if (navigator == null)
                navigator = GetComponentInParent<SceneNavigator>(true)
                            ?? gameObject.AddComponent<SceneNavigator>();
        }

        public void Bind(CourseData course, System.Action onBack)
        {
            _selectedSim = null;

            bool rw = LocalisationManager.Instance != null &&
                      LocalisationManager.Instance.CurrentLanguage == Language.Kinyarwanda;

            courseTitle.text   = rw ? course.TitleRW       : course.TitleEN;
            courseTitleRW.text = rw ? course.TitleEN       : course.TitleRW;
            courseUnit.text    = course.UnitLabel;
            courseDesc.text    = rw ? course.DescriptionRW : course.DescriptionEN;

            if (headerAccentBar != null)
                headerAccentBar.color = course.CardColor;

            foreach (var c in _cards)
                if (c != null) Destroy(c.gameObject);
            _cards.Clear();

            foreach (var sim in course.Simulations)
            {
                var card = Instantiate(simCardPrefab, simCardsContainer);
                card.Bind(sim, OnSimSelected);
                _cards.Add(card);
            }

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => onBack?.Invoke());

            playButton.interactable = false;
            playButton.onClick.RemoveAllListeners();
            playButtonLabel.text = rw ? "Kina" : "Play";
        }

        private void OnSimSelected(SimulationData sim)
        {
            _selectedSim = sim;

            foreach (var c in _cards)
                c.SetSelected(c.BoundData == sim);

            playButton.interactable = !sim.IsLocked;
            playButton.onClick.RemoveAllListeners();

            if (!sim.IsLocked && navigator != null)
                playButton.onClick.AddListener(() => navigator.LoadSceneByName(sim.SceneName));
        }
    }
}
