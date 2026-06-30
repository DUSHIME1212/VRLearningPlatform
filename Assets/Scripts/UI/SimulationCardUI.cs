using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    [RequireComponent(typeof(Button))]
    public class SimulationCardUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image           cardBackground;
        [SerializeField] private Image           selectedHighlight;
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject      lockIcon;
        [SerializeField] private Image           thumbnailImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor   = new Color(0.15f, 0.15f, 0.25f, 0.85f);
        [SerializeField] private Color selectedColor = new Color(0.30f, 0.30f, 0.50f, 0.95f);

        public SimulationData BoundData { get; private set; }
        private System.Action<SimulationData> _onSelected;

        public void Bind(SimulationData data, System.Action<SimulationData> onSelected)
        {
            BoundData   = data;
            _onSelected = onSelected;

            bool rw = LocalisationManager.Instance != null &&
                      LocalisationManager.Instance.CurrentLanguage == Language.Kinyarwanda;

            badgeText.text       = data.BadgeNumber.ToString();
            titleText.text       = rw ? data.TitleRW       : data.TitleEN;
            descriptionText.text = rw ? data.DescriptionRW : data.DescriptionEN;

            if (lockIcon != null) lockIcon.SetActive(data.IsLocked);

            if (thumbnailImage != null)
            {
                if (data.Thumbnail != null)
                {
                    thumbnailImage.sprite  = data.Thumbnail;
                    thumbnailImage.enabled = true;
                }
                else
                {
                    thumbnailImage.enabled = false;
                }
            }

            var btn = GetComponent<Button>();
            btn.interactable = !data.IsLocked;
            btn.onClick.RemoveAllListeners();
            if (!data.IsLocked)
                btn.onClick.AddListener(() => _onSelected?.Invoke(BoundData));

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            cardBackground.color = selected ? selectedColor : normalColor;
            if (selectedHighlight != null)
                selectedHighlight.gameObject.SetActive(selected);
        }
    }
}
