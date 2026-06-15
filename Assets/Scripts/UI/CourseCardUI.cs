using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    [RequireComponent(typeof(Button))]
    public class CourseCardUI : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Image           cardBackground;
        [SerializeField] private Image           bannerImage;
        [SerializeField] private Image           badgeCircle;
        [SerializeField] private TextMeshProUGUI badgeNumberText;
        [SerializeField] private TextMeshProUGUI unitLabelText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI titleRWText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button          playButton;

        private CourseData _data;
        private System.Action<CourseData> _onClicked;

        public void Bind(CourseData data, System.Action<CourseData> onClicked)
        {
            _data      = data;
            _onClicked = onClicked;

            bool rw = LocalisationManager.Instance != null &&
                      LocalisationManager.Instance.CurrentLanguage == Language.Kinyarwanda;

            titleText.text       = rw ? data.TitleRW       : data.TitleEN;
            titleRWText.text     = rw ? data.TitleEN       : data.TitleRW;
            unitLabelText.text   = data.UnitLabel;
            descriptionText.text = rw ? data.DescriptionRW : data.DescriptionEN;

            Color bg = data.CardColor;
            bg.a = 0.6f;
            cardBackground.color = bg;
            badgeCircle.color    = data.CardColor;
            badgeNumberText.text = data.BadgeNumber.ToString();

            if (bannerImage != null)
            {
                bannerImage.sprite  = data.BannerImage;
                bannerImage.enabled = data.BannerImage != null;
            }

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => _onClicked?.Invoke(_data));

            // Whole card is also clickable
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => _onClicked?.Invoke(_data));
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveAllListeners();
        }
    }
}
