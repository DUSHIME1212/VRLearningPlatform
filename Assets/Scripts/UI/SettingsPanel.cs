using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using VRLearning.Core;

namespace VRLearning.UI
{
    /// <summary>
    /// Self-contained in-VR settings panel: a Language toggle (English / Kinyarwanda) and Master / Music /
    /// SFX volume controls. Builds its own world-space UI at runtime and lives on the persistent Managers
    /// object, so it is reachable in every scene. Toggle it with the right controller's primary (A) button
    /// or the Escape/Tab key in the Editor. Values persist in PlayerPrefs and drive
    /// <see cref="LocalisationManager"/> and <see cref="AudioManager"/>.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        const string PP_LANG = "vrl_lang";
        const string PP_MASTER = "vrl_vol_master";
        const string PP_MUSIC = "vrl_vol_music";
        const string PP_SFX = "vrl_vol_sfx";

        [SerializeField] float openDistance = 1.5f;
        [SerializeField] Vector2 panelSize = new Vector2(900f, 640f);

        Canvas _canvas;
        GameObject _root;
        TextMeshProUGUI _langLabel;
        bool _open;
        InputAction _toggle;

        void Start()
        {
            LoadAndApply();
            BuildUI();
            Hide();
        }

        void OnEnable()
        {
            _toggle = new InputAction("SettingsToggle", InputActionType.Button);
            _toggle.AddBinding("<XRController>{RightHand}/primaryButton");
            _toggle.AddBinding("<Keyboard>/tab");
            _toggle.performed += OnToggle;
            _toggle.Enable();
        }

        void OnDisable()
        {
            if (_toggle == null) return;
            _toggle.performed -= OnToggle;
            _toggle.Disable();
            _toggle.Dispose();
            _toggle = null;
        }

        void OnToggle(InputAction.CallbackContext _) { if (_open) Hide(); else Show(); }

        // ---- state ----

        void LoadAndApply()
        {
            var loc = LocalisationManager.Instance;
            if (loc != null)
                loc.SetLanguage((Language)PlayerPrefs.GetInt(PP_LANG, (int)loc.CurrentLanguage));

            var am = AudioManager.Instance;
            if (am != null)
            {
                am.masterVolume = PlayerPrefs.GetFloat(PP_MASTER, am.masterVolume);
                am.musicVolume = PlayerPrefs.GetFloat(PP_MUSIC, am.musicVolume);
                am.sfxVolume = PlayerPrefs.GetFloat(PP_SFX, am.sfxVolume);
            }
        }

        void Show()
        {
            _open = true;
            if (_root != null) _root.SetActive(true);
            var cam = Camera.main;
            if (cam != null && _canvas != null)
            {
                var t = _canvas.transform;
                t.position = cam.transform.position + cam.transform.forward * openDistance;
                t.rotation = Quaternion.LookRotation(t.position - cam.transform.position, Vector3.up);
            }
            RefreshLangLabel();
        }

        void Hide() { _open = false; if (_root != null) _root.SetActive(false); }

        void RefreshLangLabel()
        {
            var loc = LocalisationManager.Instance;
            if (_langLabel != null && loc != null)
                _langLabel.text = loc.CurrentLanguage == Language.Kinyarwanda ? "Kinyarwanda" : "English";
        }

        // ---- UI construction ----

        void BuildUI()
        {
            var canvasGO = new GameObject("SettingsCanvas");
            canvasGO.transform.SetParent(transform, false);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3f;
            canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
            var crt = (RectTransform)canvasGO.transform;
            crt.sizeDelta = panelSize;
            canvasGO.transform.localScale = Vector3.one * 0.001f;
            _root = canvasGO;

            var bg = MakeImage(crt, "BG", Vector2.zero, panelSize, new Color(0.08f, 0.09f, 0.12f, 0.95f)).transform as RectTransform;

            float top = panelSize.y / 2f;
            MakeText(bg, "Title", "Settings", new Vector2(0, top - 55f), new Vector2(panelSize.x, 70f), 46f, TextAlignmentOptions.Center);

            // Language row
            MakeText(bg, "LangLabel", "Language", new Vector2(-panelSize.x / 2f + 170f, top - 165f), new Vector2(300f, 55f), 30f, TextAlignmentOptions.Left);
            var langBtn = MakeButton(bg, "LangButton", "English", new Vector2(150f, top - 165f), new Vector2(340f, 66f), ToggleLanguage);
            _langLabel = langBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Volume rows (using - / value / + so they work reliably with a VR pointer)
            MakeVolumeRow(bg, "Master", top - 275f, PP_MASTER, 1.0f, v => { if (AudioManager.Instance != null) AudioManager.Instance.masterVolume = v; });
            MakeVolumeRow(bg, "Music", top - 355f, PP_MUSIC, 0.4f, v => { if (AudioManager.Instance != null) AudioManager.Instance.musicVolume = v; });
            MakeVolumeRow(bg, "SFX", top - 435f, PP_SFX, 1.0f, v => { if (AudioManager.Instance != null) AudioManager.Instance.sfxVolume = v; });

            MakeButton(bg, "CloseButton", "Close", new Vector2(0, -top + 60f), new Vector2(260f, 70f), Hide);
        }

        void ToggleLanguage()
        {
            var loc = LocalisationManager.Instance;
            if (loc == null) return;
            var next = loc.CurrentLanguage == Language.English ? Language.Kinyarwanda : Language.English;
            loc.SetLanguage(next);
            PlayerPrefs.SetInt(PP_LANG, (int)next);
            PlayerPrefs.Save();
            RefreshLangLabel();
        }

        void MakeVolumeRow(RectTransform parent, string name, float y, string ppKey, float def, Action<float> apply)
        {
            float val = Mathf.Clamp01(PlayerPrefs.GetFloat(ppKey, def));
            MakeText(parent, name + "Label", name, new Vector2(-panelSize.x / 2f + 170f, y), new Vector2(240f, 55f), 28f, TextAlignmentOptions.Left);
            var valText = MakeText(parent, name + "Val", Pct(val), new Vector2(150f, y), new Vector2(150f, 55f), 28f, TextAlignmentOptions.Center);
            MakeButton(parent, name + "Minus", "–", new Vector2(40f, y), new Vector2(70f, 66f), () => Adjust(ppKey, def, apply, valText, -0.1f));
            MakeButton(parent, name + "Plus", "+", new Vector2(260f, y), new Vector2(70f, 66f), () => Adjust(ppKey, def, apply, valText, 0.1f));
            apply(val);
        }

        void Adjust(string ppKey, float def, Action<float> apply, TextMeshProUGUI valText, float delta)
        {
            float v = Mathf.Clamp01(Mathf.Round((PlayerPrefs.GetFloat(ppKey, def) + delta) * 10f) / 10f);
            PlayerPrefs.SetFloat(ppKey, v);
            PlayerPrefs.Save();
            apply(v);
            if (valText != null) valText.text = Pct(v);
        }

        static string Pct(float v) => Mathf.RoundToInt(v * 100f) + "%";

        // ---- primitive builders ----

        Image MakeImage(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        TextMeshProUGUI MakeText(RectTransform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = Color.white;
            return t;
        }

        Button MakeButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var img = MakeImage(parent, name, pos, size, new Color(0.22f, 0.28f, 0.42f, 1f));
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var txt = MakeText((RectTransform)img.transform, "Label", label, Vector2.zero, size, 32f, TextAlignmentOptions.Center);
            txt.enableAutoSizing = false;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }
    }
}
