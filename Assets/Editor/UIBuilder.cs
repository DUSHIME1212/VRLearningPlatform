// Assets/Editor/UIBuilder.cs
// Run via:  VRLearning > Build UI Prefabs
// Creates CourseCard, SimulationCard, NPCSpeechBubble, StarDisplay,
// UIManagerCanvas, and CourseSelectionCanvas prefabs under Assets/Prefabs/UI/.

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class UIBuilder
{
    const string PrefabRoot = "Assets/Prefabs/UI";

    // ───────────────────────────── menu entry ──────────────────────────────
    [MenuItem("VRLearning/Build UI Prefabs")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder(PrefabRoot))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        BuildCourseCard();
        BuildSimulationCard();
        BuildNPCSpeechBubble();
        BuildStarDisplay();
        BuildUIManagerCanvas();
        BuildCourseSelectionCanvas();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UIBuilder] All UI prefabs built successfully.");
    }

    // ─────────────────────────── CourseCard ────────────────────────────────
    static void BuildCourseCard()
    {
        var root = NewRect("CourseCard", null, new Vector2(340, 480));
        root.gameObject.AddComponent<Image>().color = Color.clear; // needed for Button raycast
        root.gameObject.AddComponent<Button>();
        root.gameObject.AddComponent<VRLearning.UI.CourseCardUI>();

        // CardBackground
        var bg = NewRect("CardBackground", root, Vector2.zero);
        StretchFull(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.25f, 0.85f);

        // BannerImage (top strip)
        var banner = NewRect("BannerImage", root, new Vector2(340, 140));
        banner.anchorMin = new Vector2(0, 1);
        banner.anchorMax = new Vector2(1, 1);
        banner.pivot     = new Vector2(0.5f, 1);
        banner.anchoredPosition = Vector2.zero;
        var bannerImg = banner.gameObject.AddComponent<Image>();
        bannerImg.preserveAspect = true;

        // ContentLayout
        var content = NewRect("ContentLayout", root, Vector2.zero);
        StretchFull(content);
        content.offsetMin = new Vector2(16, 16);
        content.offsetMax = new Vector2(-16, -148); // leave room for banner
        var vl = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.padding          = new RectOffset(0, 0, 4, 0);
        vl.spacing          = 10;
        vl.childAlignment   = TextAnchor.UpperLeft;
        vl.childForceExpandWidth  = true;
        vl.childForceExpandHeight = false;

        // BadgeRow
        var badgeRow = NewRect("BadgeRow", content, new Vector2(0, 52));
        var hl = badgeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing             = 10;
        hl.childAlignment      = TextAnchor.MiddleLeft;
        hl.childForceExpandHeight = false;
        hl.childForceExpandWidth  = false;
        badgeRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 52;

        var circle = NewRect("BadgeCircle", badgeRow, new Vector2(48, 48));
        circle.gameObject.AddComponent<LayoutElement>().preferredWidth = 48;
        var circleImg = circle.gameObject.AddComponent<Image>();
        circleImg.color = new Color(0.4f, 0.7f, 0.4f);

        var badgeNum = NewRect("BadgeNumber", circle, Vector2.zero);
        StretchFull(badgeNum);
        var badgeTMP = badgeNum.gameObject.AddComponent<TextMeshProUGUI>();
        badgeTMP.text      = "1";
        badgeTMP.fontSize  = 22;
        badgeTMP.alignment = TextAlignmentOptions.Center;
        badgeTMP.color     = Color.white;
        badgeTMP.raycastTarget = false;

        var unitLabel = NewRect("UnitLabel", badgeRow, Vector2.zero);
        var unitLE = unitLabel.gameObject.AddComponent<LayoutElement>();
        unitLE.flexibleWidth = 1;
        var unitTMP = unitLabel.gameObject.AddComponent<TextMeshProUGUI>();
        unitTMP.text       = "SET P6 · Unit 2";
        unitTMP.fontSize   = 13;
        unitTMP.color      = new Color(0.8f, 0.8f, 0.8f);
        unitTMP.raycastTarget = false;

        // Divider
        var divider = NewRect("Divider", content, new Vector2(0, 2));
        var divImg = divider.gameObject.AddComponent<Image>();
        divImg.color = new Color(1, 1, 1, 0.2f);
        divImg.raycastTarget = false;
        divider.gameObject.AddComponent<LayoutElement>().preferredHeight = 2;

        // TitleText
        var titleGO = NewRect("TitleText", content, new Vector2(0, 36));
        titleGO.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;
        var titleTMP = titleGO.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Simple Machines";
        titleTMP.fontSize  = 26;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = Color.white;
        titleTMP.raycastTarget = false;

        // TitleRWText
        var titleRWGO = NewRect("TitleRWText", content, new Vector2(0, 22));
        titleRWGO.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
        var titleRWTMP = titleRWGO.gameObject.AddComponent<TextMeshProUGUI>();
        titleRWTMP.text      = "Imashini Yoroshye";
        titleRWTMP.fontSize  = 15;
        titleRWTMP.fontStyle = FontStyles.Italic;
        titleRWTMP.color     = new Color(0.7f, 0.9f, 0.7f);
        titleRWTMP.raycastTarget = false;

        // DescriptionText
        var descGO = NewRect("DescriptionText", content, Vector2.zero);
        var descLE = descGO.gameObject.AddComponent<LayoutElement>();
        descLE.flexibleHeight = 1;
        var descTMP = descGO.gameObject.AddComponent<TextMeshProUGUI>();
        descTMP.text       = "Explore levers, pulleys, and inclined planes.";
        descTMP.fontSize   = 13;
        descTMP.color      = new Color(0.85f, 0.85f, 0.85f);
        descTMP.textWrappingMode = TextWrappingModes.Normal;
        descTMP.raycastTarget = false;

        // PlayButton (bottom right)
        var playBtn = NewRect("PlayButton", root, new Vector2(120, 44));
        playBtn.anchorMin = new Vector2(1, 0);
        playBtn.anchorMax = new Vector2(1, 0);
        playBtn.pivot     = new Vector2(1, 0);
        playBtn.anchoredPosition = new Vector2(-16, 16);
        var playBtnImg = playBtn.gameObject.AddComponent<Image>();
        playBtnImg.color = new Color(0.25f, 0.6f, 0.25f);
        var playBtnComp = playBtn.gameObject.AddComponent<Button>();
        playBtnComp.targetGraphic = playBtnImg;

        var playLabel = NewRect("PlayLabel", playBtn, Vector2.zero);
        StretchFull(playLabel);
        var playTMP = playLabel.gameObject.AddComponent<TextMeshProUGUI>();
        playTMP.text      = "Play →";
        playTMP.fontSize  = 16;
        playTMP.fontStyle = FontStyles.Bold;
        playTMP.alignment = TextAlignmentOptions.Center;
        playTMP.color     = Color.white;
        playTMP.raycastTarget = false;

        // Wire CourseCardUI serialized fields
        Wire(root.GetComponent<VRLearning.UI.CourseCardUI>(), new[] {
            ("cardBackground",   (Object)bgImg),
            ("bannerImage",      bannerImg),
            ("badgeCircle",      circleImg),
            ("badgeNumberText",  badgeTMP),
            ("unitLabelText",    unitTMP),
            ("titleText",        titleTMP),
            ("titleRWText",      titleRWTMP),
            ("descriptionText",  descTMP),
            ("playButton",       (Object)playBtnComp),
        });

        SavePrefab(root.gameObject, "CourseCard");
        Object.DestroyImmediate(root.gameObject);
    }

    // ─────────────────────────── SimulationCard ────────────────────────────
    static void BuildSimulationCard()
    {
        var root = NewRect("SimulationCard", null, new Vector2(200, 280));
        root.gameObject.AddComponent<Image>().color = Color.clear;
        root.gameObject.AddComponent<Button>();
        root.gameObject.AddComponent<VRLearning.UI.SimulationCardUI>();

        // CardBackground
        var bg = NewRect("CardBackground", root, Vector2.zero);
        StretchFull(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.25f, 0.85f);

        // SelectedHighlight (outline, hidden by default)
        var highlight = NewRect("SelectedHighlight", root, Vector2.zero);
        StretchFull(highlight);
        highlight.offsetMin = new Vector2(-3, -3);
        highlight.offsetMax = new Vector2(3, 3);
        var hlImg = highlight.gameObject.AddComponent<Image>();
        hlImg.color = new Color(0.4f, 0.8f, 1f, 0.8f);
        hlImg.raycastTarget = false;
        highlight.gameObject.SetActive(false);

        // ThumbnailImage (top half)
        var thumb = NewRect("ThumbnailImage", root, new Vector2(0, 130));
        thumb.anchorMin = new Vector2(0, 1);
        thumb.anchorMax = new Vector2(1, 1);
        thumb.pivot     = new Vector2(0.5f, 1);
        thumb.anchoredPosition = Vector2.zero;
        var thumbImg = thumb.gameObject.AddComponent<Image>();
        thumbImg.preserveAspect = true;
        thumbImg.enabled = false;

        // ContentLayout (bottom half)
        var content = NewRect("ContentLayout", root, Vector2.zero);
        content.anchorMin = new Vector2(0, 0);
        content.anchorMax = new Vector2(1, 0.5f);
        content.offsetMin = new Vector2(10, 8);
        content.offsetMax = new Vector2(-10, 0);
        var vl = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.spacing             = 6;
        vl.childAlignment      = TextAnchor.UpperLeft;
        vl.childForceExpandWidth  = true;
        vl.childForceExpandHeight = false;

        // BadgeRow
        var badgeRow = NewRect("BadgeRow", content, new Vector2(0, 32));
        badgeRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
        var hl2 = badgeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl2.spacing              = 8;
        hl2.childAlignment       = TextAnchor.MiddleLeft;
        hl2.childForceExpandHeight = false;
        hl2.childForceExpandWidth  = false;

        var circle = NewRect("BadgeCircle", badgeRow, new Vector2(32, 32));
        circle.gameObject.AddComponent<LayoutElement>().preferredWidth = 32;
        circle.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.6f, 0.4f);

        var badgeTMPGO = NewRect("BadgeText", circle, Vector2.zero);
        StretchFull(badgeTMPGO);
        var badgeTMP = badgeTMPGO.gameObject.AddComponent<TextMeshProUGUI>();
        badgeTMP.text      = "1";
        badgeTMP.fontSize  = 16;
        badgeTMP.alignment = TextAlignmentOptions.Center;
        badgeTMP.color     = Color.white;
        badgeTMP.raycastTarget = false;

        var lockGO = NewRect("LockIcon", badgeRow, new Vector2(24, 24));
        lockGO.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
        var lockImg = lockGO.gameObject.AddComponent<Image>();
        lockImg.color = new Color(1f, 0.6f, 0.1f);
        lockImg.raycastTarget = false;
        lockGO.gameObject.SetActive(false);

        // TitleText
        var titleGO = NewRect("TitleText", content, new Vector2(0, 24));
        titleGO.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
        var titleTMP = titleGO.gameObject.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "The Lever";
        titleTMP.fontSize  = 14;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = Color.white;
        titleTMP.raycastTarget = false;

        // DescriptionText
        var descGO = NewRect("DescriptionText", content, Vector2.zero);
        descGO.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
        var descTMP = descGO.gameObject.AddComponent<TextMeshProUGUI>();
        descTMP.text       = "A simple machine that makes lifting easy.";
        descTMP.fontSize   = 11;
        descTMP.color      = new Color(0.8f, 0.8f, 0.8f);
        descTMP.textWrappingMode = TextWrappingModes.Normal;
        descTMP.raycastTarget = false;

        // Wire SimulationCardUI serialized fields
        Wire(root.GetComponent<VRLearning.UI.SimulationCardUI>(), new[] {
            ("cardBackground",   (Object)bgImg),
            ("selectedHighlight",(Object)hlImg),
            ("badgeText",        badgeTMP),
            ("titleText",        titleTMP),
            ("descriptionText",  descTMP),
            ("lockIcon",         (Object)lockGO),
            ("thumbnailImage",   (Object)thumbImg),
        });

        SavePrefab(root.gameObject, "SimulationCard");
        Object.DestroyImmediate(root.gameObject);
    }

    // ─────────────────────────── NPCSpeechBubble ───────────────────────────
    static void BuildNPCSpeechBubble()
    {
        var root = NewRect("NPCSpeechBubble", null, new Vector2(320, 100));
        root.gameObject.AddComponent<VRLearning.Modules.CodeWorld.NPCSpeechBubble>();
        root.gameObject.SetActive(false);

        var bg = NewRect("Background", root, Vector2.zero);
        StretchFull(bg);
        var bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);
        bgImg.raycastTarget = false;

        var labelGO = NewRect("Label", root, Vector2.zero);
        StretchFull(labelGO);
        labelGO.offsetMin = new Vector2(12, 8);
        labelGO.offsetMax = new Vector2(-12, -8);
        var labelTMP = labelGO.gameObject.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "Hello! I am your guide.";
        labelTMP.fontSize  = 14;
        labelTMP.color     = Color.white;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        labelTMP.textWrappingMode = TextWrappingModes.Normal;
        labelTMP.raycastTarget = false;

        Wire(root.GetComponent<VRLearning.Modules.CodeWorld.NPCSpeechBubble>(), new[] {
            ("label", (Object)labelTMP),
        });

        SavePrefab(root.gameObject, "NPCSpeechBubble");
        Object.DestroyImmediate(root.gameObject);
    }

    // ─────────────────────────── StarDisplay ───────────────────────────────
    static void BuildStarDisplay()
    {
        var root = NewRect("StarDisplay", null, new Vector2(260, 80));
        root.gameObject.AddComponent<VRLearning.UI.StarDisplay>();

        var hl = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing             = 16;
        hl.childAlignment      = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth  = false;
        hl.childForceExpandHeight = false;
        hl.padding             = new RectOffset(8, 8, 4, 4);

        var starObjects = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var starGO = NewRect($"Star_{i}", root, new Vector2(64, 64));
            starGO.gameObject.AddComponent<LayoutElement>().preferredWidth = 64;
            var starImg = starGO.gameObject.AddComponent<Image>();
            starImg.color = new Color(1f, 0.85f, 0.1f);
            starImg.raycastTarget = false;
            starGO.gameObject.SetActive(false);
            starObjects[i] = starGO.gameObject;
        }

        // Wire StarDisplay.stars array
        var so = new SerializedObject(root.GetComponent<VRLearning.UI.StarDisplay>());
        var starsProp = so.FindProperty("stars");
        starsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starObjects[i];
        so.ApplyModifiedProperties();

        SavePrefab(root.gameObject, "StarDisplay");
        Object.DestroyImmediate(root.gameObject);
    }

    // ─────────────────────────── UIManagerCanvas ───────────────────────────
    static void BuildUIManagerCanvas()
    {
        // World-space overlay canvas for HUD panels
        var canvasGO = new GameObject("UIManagerCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta    = new Vector2(1200, 900);
        rt.localScale   = Vector3.one * 0.001f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<VRLearning.UIManager>();

        // Helper: creates a full-stretch semi-transparent panel with a CanvasGroup
        GameObject MakePanel(string name, Color color)
        {
            var p = NewRect(name, rt, Vector2.zero);
            StretchFull(p);
            var img = p.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            p.gameObject.AddComponent<CanvasGroup>();
            p.gameObject.SetActive(false);
            return p.gameObject;
        }

        var mainMenu = MakePanel("MainMenuPanel",       new Color(0, 0, 0, 0.7f));
        var pause    = MakePanel("PausePanel",          new Color(0, 0, 0.1f, 0.8f));
        var hint     = MakePanel("HintPanel",           new Color(0.05f, 0.05f, 0.15f, 0.9f));
        var success  = MakePanel("SuccessPanel",        new Color(0.05f, 0.2f, 0.05f, 0.9f));
        var warning  = MakePanel("SessionWarningPanel", new Color(0.3f, 0.1f, 0f, 0.9f));

        // Hint text
        var hintTxtGO = NewRect("HintText", hint.GetComponent<RectTransform>(), Vector2.zero);
        StretchFull(hintTxtGO);
        hintTxtGO.offsetMin = new Vector2(40, 40);
        hintTxtGO.offsetMax = new Vector2(-40, -40);
        var hintTMP = hintTxtGO.gameObject.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "Hint text here";
        hintTMP.fontSize  = 24;
        hintTMP.alignment = TextAlignmentOptions.Center;
        hintTMP.color     = Color.white;
        hintTMP.raycastTarget = false;

        // Session warning text
        var warnTxtGO = NewRect("WarningText", warning.GetComponent<RectTransform>(), Vector2.zero);
        StretchFull(warnTxtGO);
        warnTxtGO.offsetMin = new Vector2(40, 40);
        warnTxtGO.offsetMax = new Vector2(-40, -40);
        var warnTMP = warnTxtGO.gameObject.AddComponent<TextMeshProUGUI>();
        warnTMP.text      = "Session limit reached!";
        warnTMP.fontSize  = 24;
        warnTMP.alignment = TextAlignmentOptions.Center;
        warnTMP.color     = new Color(1f, 0.8f, 0.2f);
        warnTMP.raycastTarget = false;

        // StarDisplay inside SuccessPanel
        var starDisplayPath = $"{PrefabRoot}/StarDisplay.prefab";
        var starPrefab      = AssetDatabase.LoadAssetAtPath<GameObject>(starDisplayPath);
        if (starPrefab != null)
        {
            var starInst = (GameObject)PrefabUtility.InstantiatePrefab(starPrefab, success.transform);
            var starRT   = starInst.GetComponent<RectTransform>();
            starRT.anchorMin        = new Vector2(0.5f, 0.5f);
            starRT.anchorMax        = new Vector2(0.5f, 0.5f);
            starRT.anchoredPosition = Vector2.zero;
        }

        // Wire UIManager serialized fields
        Wire(canvasGO.GetComponent<VRLearning.UIManager>(), new[] {
            ("mainMenuPanel",       (Object)mainMenu),
            ("pausePanel",          (Object)pause),
            ("hintPanel",           (Object)hint),
            ("successPanel",        (Object)success),
            ("sessionWarningPanel", (Object)warning),
            ("sessionWarningText",  (Object)warnTMP),
        });

        SavePrefab(canvasGO, "UIManagerCanvas");
        Object.DestroyImmediate(canvasGO);
    }

    // ──────────────────────── CourseSelectionCanvas ────────────────────────
    static void BuildCourseSelectionCanvas()
    {
        var canvasGO = new GameObject("CourseSelectionCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rootRT = canvasGO.GetComponent<RectTransform>();
        rootRT.sizeDelta  = new Vector2(1200, 900);
        rootRT.localScale = Vector3.one * 0.001f;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<CanvasGroup>();

        var selUI = canvasGO.AddComponent<VRLearning.UI.CourseSelectionUI>();

        // ── CourseListPanel ──
        var listPanel = NewRect("CourseListPanel", rootRT, Vector2.zero);
        StretchFull(listPanel);
        listPanel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        listPanel.gameObject.AddComponent<CanvasGroup>();

        var listVL = new GameObject("ListLayout").AddComponent<VerticalLayoutGroup>();
        listVL.transform.SetParent(listPanel, false);
        var listVLRT = listVL.GetComponent<RectTransform>();
        StretchFull(listVLRT);
        listVLRT.offsetMin = new Vector2(48, 40);
        listVLRT.offsetMax = new Vector2(-48, -40);
        listVL.spacing             = 20;
        listVL.childAlignment      = TextAnchor.UpperLeft;
        listVL.childForceExpandWidth  = true;
        listVL.childForceExpandHeight = false;

        var listTitle = NewRect("TitleText", listVL.GetComponent<RectTransform>(), new Vector2(0, 56));
        listTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 56;
        var listTitleTMP = listTitle.gameObject.AddComponent<TextMeshProUGUI>();
        listTitleTMP.text      = "Choose a Course";
        listTitleTMP.fontSize  = 42;
        listTitleTMP.fontStyle = FontStyles.Bold;
        listTitleTMP.color     = Color.white;
        listTitleTMP.raycastTarget = false;

        var listSubtitle = NewRect("SubtitleText", listVL.GetComponent<RectTransform>(), new Vector2(0, 30));
        listSubtitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
        var subTMP = listSubtitle.gameObject.AddComponent<TextMeshProUGUI>();
        subTMP.text      = "Select a topic to explore";
        subTMP.fontSize  = 20;
        subTMP.color     = new Color(0.75f, 0.75f, 0.85f);
        subTMP.raycastTarget = false;

        var cardsContainer = NewRect("CourseCardsContainer", listVL.GetComponent<RectTransform>(), Vector2.zero);
        cardsContainer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
        var cardsHL = cardsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        cardsHL.spacing              = 40;
        cardsHL.childAlignment       = TextAnchor.UpperCenter;
        cardsHL.childForceExpandWidth  = false;
        cardsHL.childForceExpandHeight = true;
        cardsContainer.gameObject.AddComponent<ContentSizeFitter>().horizontalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // ── CourseDetailPanel ──
        var detailPanel = NewRect("CourseDetailPanel", rootRT, Vector2.zero);
        StretchFull(detailPanel);
        detailPanel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        detailPanel.gameObject.AddComponent<CanvasGroup>();
        var detailUI = detailPanel.gameObject.AddComponent<VRLearning.UI.CourseDetailUI>();
        detailPanel.gameObject.SetActive(false);

        // Header accent bar
        var accentBar = NewRect("HeaderAccentBar", detailPanel, new Vector2(0, 6));
        accentBar.anchorMin = new Vector2(0, 1);
        accentBar.anchorMax = new Vector2(1, 1);
        accentBar.pivot     = new Vector2(0.5f, 1);
        accentBar.anchoredPosition = Vector2.zero;
        var accentImg = accentBar.gameObject.AddComponent<Image>();
        accentImg.color = new Color(0.4f, 0.7f, 0.4f);
        accentImg.raycastTarget = false;

        // Detail content layout
        var detailContent = NewRect("DetailContent", detailPanel, Vector2.zero);
        StretchFull(detailContent);
        detailContent.offsetMin = new Vector2(48, 24);
        detailContent.offsetMax = new Vector2(-48, -12);
        var detailVL = detailContent.gameObject.AddComponent<VerticalLayoutGroup>();
        detailVL.spacing             = 14;
        detailVL.childAlignment      = TextAnchor.UpperLeft;
        detailVL.childForceExpandWidth  = true;
        detailVL.childForceExpandHeight = false;

        // Back button row
        var topRow = NewRect("TopRow", detailContent, new Vector2(0, 44));
        topRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
        var topHL = topRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        topHL.childAlignment       = TextAnchor.MiddleLeft;
        topHL.childForceExpandHeight = true;
        topHL.childForceExpandWidth  = false;

        var backBtn = NewRect("BackButton", topRow, new Vector2(100, 40));
        backBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 100;
        var backBtnImg = backBtn.gameObject.AddComponent<Image>();
        backBtnImg.color = new Color(0.3f, 0.3f, 0.4f);
        var backBtnComp = backBtn.gameObject.AddComponent<Button>();
        backBtnComp.targetGraphic = backBtnImg;
        var backLabelGO = NewRect("BackLabel", backBtn, Vector2.zero);
        StretchFull(backLabelGO);
        var backLabelTMP = backLabelGO.gameObject.AddComponent<TextMeshProUGUI>();
        backLabelTMP.text      = "← Back";
        backLabelTMP.fontSize  = 16;
        backLabelTMP.alignment = TextAlignmentOptions.Center;
        backLabelTMP.color     = Color.white;
        backLabelTMP.raycastTarget = false;

        // Course title
        var cTitle = NewRect("CourseTitleText", detailContent, new Vector2(0, 48));
        cTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 48;
        var cTitleTMP = cTitle.gameObject.AddComponent<TextMeshProUGUI>();
        cTitleTMP.text      = "Simple Machines";
        cTitleTMP.fontSize  = 36;
        cTitleTMP.fontStyle = FontStyles.Bold;
        cTitleTMP.color     = Color.white;
        cTitleTMP.raycastTarget = false;

        var cTitleRW = NewRect("CourseTitleRWText", detailContent, new Vector2(0, 28));
        cTitleRW.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
        var cTitleRWTMP = cTitleRW.gameObject.AddComponent<TextMeshProUGUI>();
        cTitleRWTMP.text      = "Imashini Yoroshye";
        cTitleRWTMP.fontSize  = 18;
        cTitleRWTMP.fontStyle = FontStyles.Italic;
        cTitleRWTMP.color     = new Color(0.7f, 0.9f, 0.7f);
        cTitleRWTMP.raycastTarget = false;

        var cUnit = NewRect("CourseUnitText", detailContent, new Vector2(0, 22));
        cUnit.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
        var cUnitTMP = cUnit.gameObject.AddComponent<TextMeshProUGUI>();
        cUnitTMP.text     = "SET P6 · Unit 2";
        cUnitTMP.fontSize = 14;
        cUnitTMP.color    = new Color(0.75f, 0.75f, 0.85f);
        cUnitTMP.raycastTarget = false;

        var cDesc = NewRect("CourseDescText", detailContent, new Vector2(0, 44));
        cDesc.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
        var cDescTMP = cDesc.gameObject.AddComponent<TextMeshProUGUI>();
        cDescTMP.text       = "Explore the world of simple machines.";
        cDescTMP.fontSize   = 15;
        cDescTMP.color      = new Color(0.85f, 0.85f, 0.85f);
        cDescTMP.enableWordWrapping = true;
        cDescTMP.raycastTarget = false;

        // Sim cards container
        var simContainer = NewRect("SimCardsContainer", detailContent, Vector2.zero);
        simContainer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
        var simHL = simContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        simHL.spacing              = 24;
        simHL.childAlignment       = TextAnchor.UpperLeft;
        simHL.childForceExpandWidth  = false;
        simHL.childForceExpandHeight = true;

        // Play button row
        var playRow = NewRect("PlayRow", detailContent, new Vector2(0, 52));
        playRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 52;
        var playRowHL = playRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        playRowHL.childAlignment       = TextAnchor.MiddleRight;
        playRowHL.childForceExpandHeight = false;
        playRowHL.childForceExpandWidth  = false;

        var playBtn = NewRect("PlayButton", playRow, new Vector2(160, 48));
        playBtn.gameObject.AddComponent<LayoutElement>().preferredWidth = 160;
        var playBtnImg = playBtn.gameObject.AddComponent<Image>();
        playBtnImg.color = new Color(0.2f, 0.55f, 0.2f);
        var playBtnComp = playBtn.gameObject.AddComponent<Button>();
        playBtnComp.targetGraphic = playBtnImg;
        playBtnComp.interactable  = false;
        var playLabelGO = NewRect("PlayButtonLabel", playBtn, Vector2.zero);
        StretchFull(playLabelGO);
        var playLabelTMP = playLabelGO.gameObject.AddComponent<TextMeshProUGUI>();
        playLabelTMP.text      = "Play";
        playLabelTMP.fontSize  = 18;
        playLabelTMP.fontStyle = FontStyles.Bold;
        playLabelTMP.alignment = TextAlignmentOptions.Center;
        playLabelTMP.color     = Color.white;
        playLabelTMP.raycastTarget = false;

        // Load card prefabs for inspector references
        var courseCardPrefab = AssetDatabase.LoadAssetAtPath<VRLearning.UI.CourseCardUI>($"{PrefabRoot}/CourseCard.prefab");
        var simCardPrefab    = AssetDatabase.LoadAssetAtPath<VRLearning.UI.SimulationCardUI>($"{PrefabRoot}/SimulationCard.prefab");

        // Wire CourseDetailUI
        Wire(detailUI, new[] {
            ("courseTitle",      (Object)cTitleTMP),
            ("courseTitleRW",    cTitleRWTMP),
            ("courseUnit",       cUnitTMP),
            ("courseDesc",       cDescTMP),
            ("headerAccentBar",  accentImg),
            ("simCardsContainer",(Object)simContainer),
            ("simCardPrefab",    (Object)simCardPrefab),
            ("backButton",       (Object)backBtnComp),
            ("playButton",       (Object)playBtnComp),
            ("playButtonLabel",  (Object)playLabelTMP),
        });

        // Wire CourseSelectionUI
        Wire(selUI, new[] {
            ("courseListPanel",    (Object)listPanel.gameObject),
            ("courseDetailPanel",  (Object)detailPanel.gameObject),
            ("courseCardsContainer",(Object)cardsContainer),
            ("courseCardPrefab",   (Object)courseCardPrefab),
        });

        SavePrefab(canvasGO, "CourseSelectionCanvas");
        Object.DestroyImmediate(canvasGO);
    }

    // ───────────────────────────── helpers ─────────────────────────────────

    static RectTransform NewRect(string name, RectTransform parent, Vector2 size)
    {
        var go = new GameObject(name);
        var rt = go.AddComponent<RectTransform>();
        if (parent != null) go.transform.SetParent(parent, false);
        rt.sizeDelta = size;
        return rt;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    static void Wire(Object target, (string field, Object value)[] bindings)
    {
        var so = new SerializedObject(target);
        foreach (var (field, value) in bindings)
        {
            var prop = so.FindProperty(field);
            if (prop != null)
                prop.objectReferenceValue = value;
            else
                Debug.LogWarning($"[UIBuilder] Field '{field}' not found on {target.GetType().Name}");
        }
        so.ApplyModifiedProperties();
    }

    static void SavePrefab(GameObject go, string name)
    {
        string path = $"{PrefabRoot}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Debug.Log($"[UIBuilder] Saved {path}");
    }
}
