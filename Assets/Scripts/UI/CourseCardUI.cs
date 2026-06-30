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

            // Register listeners first so a missing visual reference can't prevent clicks
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => _onClicked?.Invoke(_data));

            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => _onClicked?.Invoke(_data));

            bool rw = LocalisationManager.Instance != null &&
                      LocalisationManager.Instance.CurrentLanguage == Language.Kinyarwanda;

            if (titleText != null)       titleText.text       = rw ? data.TitleRW       : data.TitleEN;
            if (titleRWText != null)     titleRWText.text     = rw ? data.TitleEN       : data.TitleRW;
            if (unitLabelText != null)   unitLabelText.text   = data.UnitLabel;
            if (descriptionText != null) descriptionText.text = rw ? data.DescriptionRW : data.DescriptionEN;

            Color bg = data.CardColor;
            bg.a = 0.6f;
            if (cardBackground != null)  cardBackground.color = bg;
            if (badgeCircle != null)     badgeCircle.color    = data.CardColor;
            if (badgeNumberText != null) badgeNumberText.text = data.BadgeNumber.ToString();

            if (bannerImage != null)
            {
                bannerImage.sprite  = data.BannerImage;
                bannerImage.enabled = data.BannerImage != null;
            }
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveAllListeners();
        }
    }
}
