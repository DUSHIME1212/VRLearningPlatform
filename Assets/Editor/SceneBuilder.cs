// Assets/Editor/SceneBuilder.cs
// Run via:  VRLearning > Build Test Scenes
// Creates 9 experiment scenes, each with VR player, concept prop, title UI, and a Back button.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using VRLearning.UI;

public static class SceneBuilder
{
    const string ScenesFolder   = "Assets/Scenes/Experiments";
    const string VRPlayerPrefab = "Assets/Prefabs/Characters/VR player.prefab";
    const string SimulatorPrefab =
        "Assets/Samples/XR Interaction Toolkit/3.3.1/XR Interaction Simulator/XR Interaction Simulator.prefab";

    static readonly (string sceneName, string titleEN, string titleRW, Color theme)[] Scenes =
    {
        ("SimpleMachines_Lever",         "The Lever",          "Inkoni",            new Color(0.42f, 0.49f, 0.27f)),
        ("SimpleMachines_Pulley",        "The Pulley",         "Iguruka",           new Color(0.16f, 0.49f, 0.48f)),
        ("SimpleMachines_InclinedPlane", "Inclined Plane",     "Inzira Yagenewe",   new Color(0.36f, 0.23f, 0.56f)),
        ("BloodCirc_HeartPump",          "The Heart Pump",     "Umutima Ugenda",    new Color(0.65f, 0.12f, 0.12f)),
        ("BloodCirc_ArterialFlow",       "Arterial Flow",      "Inzira y'Amaraso",  new Color(0.55f, 0.08f, 0.18f)),
        ("BloodCirc_Capillary",          "Capillary Exchange", "Guhana Gaz",        new Color(0.48f, 0.08f, 0.22f)),
        ("Breathing_LungExpansion",      "Lung Expansion",     "Ibibuha Bigura",    new Color(0.10f, 0.35f, 0.58f)),
        ("Breathing_Diaphragm",          "Diaphragm Action",   "Inzara Igenda",     new Color(0.10f, 0.25f, 0.65f)),
        ("Breathing_GasExchange",        "Gas Exchange",       "Guhana Umwuka",     new Color(0.05f, 0.45f, 0.48f)),
    };

    // ─────────────────────────────── entry ────────────────────────────────

    [MenuItem("VRLearning/Build Test Scenes")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
            AssetDatabase.CreateFolder("Assets/Scenes", "Experiments");

        var paths = new List<string>();
        foreach (var s in Scenes)
            paths.Add(Build(s.sceneName, s.titleEN, s.titleRW, s.theme));

        AddToBuildSettings(paths);
        AddBasicSceneToBuildSettings();

        EditorSceneManager.OpenScene("Assets/Scenes/BasicScene.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SceneBuilder] {paths.Count} test scenes created and added to Build Settings.");
    }

    // ──────────────────────────── per-scene ───────────────────────────────

    static string Build(string sceneName, string titleEN, string titleRW, Color theme)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Lighting
        var sun = new GameObject("Directional Light");
        var light = sun.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.color     = Color.white;
        light.intensity = 1.2f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4, 1, 4);
        SetColor(ground, theme * 0.4f);

        // Concept prop
        CreateProp(sceneName, theme);

        // VR player
        var vrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VRPlayerPrefab);
        if (vrPrefab != null)
        {
            var vr = (GameObject)PrefabUtility.InstantiatePrefab(vrPrefab);
            vr.transform.position = new Vector3(0, 0, -2f);
        }
        else
        {
            // Fallback: plain camera
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = theme * 0.25f;
            camGO.transform.position = new Vector3(0, 1.6f, -2f);
        }

        // XR Interaction Manager (required for VR buttons)
        var xrMgr = new GameObject("XR Interaction Manager");
        xrMgr.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();

        // EventSystem with XR input
        var evtSys = new GameObject("EventSystem");
        evtSys.AddComponent<EventSystem>();
        evtSys.AddComponent<XRUIInputModule>();

        // XR Interaction Simulator (editor play-mode testing)
        var simPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPrefab);
        if (simPrefab != null)
            PrefabUtility.InstantiatePrefab(simPrefab);

        // UI
        CreateSceneUI(titleEN, titleRW, sceneName, theme);

        string path = $"{ScenesFolder}/{sceneName}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // ─────────────────────────── concept props ────────────────────────────

    static void CreateProp(string sceneName, Color theme)
    {
        if (sceneName.Contains("Lever"))
        {
            var fulcrum = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fulcrum.name = "Fulcrum";
            fulcrum.transform.SetPositionAndRotation(new Vector3(0, 0.3f, 2f), Quaternion.Euler(0, 0, 0));
            fulcrum.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            SetColor(fulcrum, theme);

            var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = "LeverPlank";
            plank.transform.SetPositionAndRotation(new Vector3(0, 0.65f, 2f), Quaternion.Euler(0, 0, 8f));
            plank.transform.localScale = new Vector3(2.4f, 0.07f, 0.18f);
            SetColor(plank, theme * 1.3f);
        }
        else if (sceneName.Contains("Pulley"))
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetPositionAndRotation(new Vector3(0, 1f, 2f), Quaternion.identity);
            post.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            SetColor(post, theme);

            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetPositionAndRotation(new Vector3(0, 2.1f, 2f), Quaternion.Euler(90, 0, 0));
            wheel.transform.localScale = new Vector3(0.5f, 0.06f, 0.5f);
            SetColor(wheel, theme * 1.4f);
        }
        else if (sceneName.Contains("Inclined"))
        {
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Ramp";
            ramp.transform.SetPositionAndRotation(new Vector3(0, 0.35f, 2f), Quaternion.Euler(-22f, 0, 0));
            ramp.transform.localScale = new Vector3(1.2f, 0.1f, 2.2f);
            SetColor(ramp, theme * 1.2f);
        }
        else if (sceneName.Contains("Heart"))
        {
            var heart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            heart.name = "Heart";
            heart.transform.position  = new Vector3(0, 1.1f, 2f);
            heart.transform.localScale = new Vector3(0.55f, 0.65f, 0.5f);
            SetColor(heart, theme * 1.5f);
        }
        else if (sceneName.Contains("Arterial") || sceneName.Contains("Capillary"))
        {
            for (int i = 0; i < 3; i++)
            {
                var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tube.name = $"Vessel_{i}";
                tube.transform.SetPositionAndRotation(
                    new Vector3((i - 1) * 0.5f, 0.8f, 2f),
                    Quaternion.Euler(0, 0, 80f));
                tube.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
                SetColor(tube, theme * (1f + i * 0.2f));
            }
        }
        else if (sceneName.Contains("Lung"))
        {
            foreach (int side in new[] { -1, 1 })
            {
                var lung = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lung.name = side < 0 ? "LungLeft" : "LungRight";
                lung.transform.position  = new Vector3(side * 0.38f, 1.15f, 2f);
                lung.transform.localScale = new Vector3(0.4f, 0.6f, 0.35f);
                SetColor(lung, theme * 1.3f);
            }
        }
        else if (sceneName.Contains("Diaphragm"))
        {
            var dia = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dia.name = "Diaphragm";
            dia.transform.SetPositionAndRotation(new Vector3(0, 0.7f, 2f), Quaternion.Euler(90, 0, 0));
            dia.transform.localScale = new Vector3(0.9f, 0.04f, 0.9f);
            SetColor(dia, theme * 1.4f);
        }
        else if (sceneName.Contains("Gas"))
        {
            for (int i = 0; i < 5; i++)
            {
                var bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bubble.name = $"GasBubble_{i}";
                float angle = i * 72f * Mathf.Deg2Rad;
                bubble.transform.position  = new Vector3(Mathf.Cos(angle) * 0.5f, 1f, 2f + Mathf.Sin(angle) * 0.5f);
                bubble.transform.localScale = Vector3.one * (0.12f + i * 0.04f);
                SetColor(bubble, theme * (1f + i * 0.15f));
            }
        }
    }

    // ────────────────────────────── scene UI ──────────────────────────────

    static void CreateSceneUI(string titleEN, string titleRW, string sceneName, Color theme)
    {
        // ── Title canvas ──
        var titleCanvas = new GameObject("TitleCanvas");
        var tc = titleCanvas.AddComponent<Canvas>();
        tc.renderMode = RenderMode.WorldSpace;
        var tcRT = titleCanvas.GetComponent<RectTransform>();
        tcRT.sizeDelta    = new Vector2(900, 220);
        tcRT.localScale   = Vector3.one * 0.001f;
        tcRT.position     = new Vector3(0, 2.4f, 1.8f);
        titleCanvas.AddComponent<GraphicRaycaster>();

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(titleCanvas.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        Stretch(bgRT);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(theme.r * 0.3f, theme.g * 0.3f, theme.b * 0.3f, 0.88f);
        bgImg.raycastTarget = false;

        // EN title
        AddTMP(titleCanvas.transform, "TitleEN", titleEN,   new Vector2(0, 1),   new Vector2(0, 1),   new Vector2(24, -18),  new Vector2(-24, -80),  52, FontStyles.Bold, Color.white);
        // RW title
        AddTMP(titleCanvas.transform, "TitleRW", titleRW,   new Vector2(0, 0.4f),new Vector2(1, 0.7f),new Vector2(24, 0),    new Vector2(-24, 0),    26, FontStyles.Italic, new Color(0.75f, 0.95f, 0.75f));
        // Scene name label
        AddTMP(titleCanvas.transform, "SceneLabel", "TEST · " + sceneName, new Vector2(0, 0), new Vector2(1, 0.38f), new Vector2(24, 6), new Vector2(-24, 0), 18, FontStyles.Normal, new Color(0.6f, 0.6f, 0.6f));

        // ── Back button canvas ──
        var btnCanvas = new GameObject("BackButtonCanvas");
        var bc = btnCanvas.AddComponent<Canvas>();
        bc.renderMode = RenderMode.WorldSpace;
        var bcRT = btnCanvas.GetComponent<RectTransform>();
        bcRT.sizeDelta  = new Vector2(340, 80);
        bcRT.localScale = Vector3.one * 0.001f;
        bcRT.position   = new Vector3(0, 1.35f, 1.8f);
        // XR ray/poke interactors can only click world-space UI through a
        // TrackedDeviceGraphicRaycaster; a plain GraphicRaycaster never registers the click.
        btnCanvas.AddComponent<TrackedDeviceGraphicRaycaster>();

        var nav = btnCanvas.AddComponent<SceneNavigator>();

        var btnGO  = new GameObject("BackButton");
        btnGO.transform.SetParent(btnCanvas.transform, false);
        var btnRT  = btnGO.AddComponent<RectTransform>();
        Stretch(btnRT);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(theme.r * 0.6f, theme.g * 0.6f, theme.b * 0.6f, 1f);
        var btn    = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Wire persistent onclick → SceneNavigator.LoadSceneByName("BasicScene")
        UnityEventTools.AddStringPersistentListener(btn.onClick, nav.LoadSceneByName, "BasicScene");

        var lblGO  = new GameObject("Label");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lblRT  = lblGO.AddComponent<RectTransform>();
        Stretch(lblRT);
        var lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
        lblTMP.text        = "← Back to Menu";
        lblTMP.fontSize    = 34;
        lblTMP.fontStyle   = FontStyles.Bold;
        lblTMP.alignment   = TextAlignmentOptions.Center;
        lblTMP.color       = Color.white;
        lblTMP.raycastTarget = false;
    }

    // ──────────────────────── build settings ──────────────────────────────

    static void AddToBuildSettings(List<string> paths)
    {
        var list = EditorBuildSettings.scenes.ToList();
        foreach (var path in paths)
            if (!list.Any(s => s.path == path))
                list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    static void AddBasicSceneToBuildSettings()
    {
        const string basic = "Assets/Scenes/BasicScene.unity";
        var list = EditorBuildSettings.scenes.ToList();
        if (!list.Any(s => s.path == basic))
            list.Insert(0, new EditorBuildSettingsScene(basic, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ─────────────────────────── helpers ──────────────────────────────────

    static void SetColor(GameObject go, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    static void AddTMP(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        float fontSize, FontStyles style, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text           = text;
        tmp.fontSize       = fontSize;
        tmp.fontStyle      = style;
        tmp.alignment      = TextAlignmentOptions.Center;
        tmp.color          = color;
        tmp.raycastTarget  = false;
    }
}
