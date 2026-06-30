// Assets/Editor/ExperimentSceneFixes.cs
// One-shot repair utilities for the experiment scenes.
//   Tools/Fix/Repair Back Buttons (All Scenes)
//   Tools/Fix/Fix Pulley Mount
//   Tools/Fix/Add Anatomy Labels (Heart & Lungs)
//   Tools/Fix/Run All
//
// These operate on the already-saved scenes so you don't have to rebuild by hand.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using VRLearning.Simulation.SimpleMachines;

namespace VRLearning.Editor
{
    public static class ExperimentSceneFixes
    {
        const string ScenesFolder = "Assets/Scenes/Experiments";
        const string BasicScene   = "Assets/Scenes/BasicScene.unity";

        static readonly string[] AllExperimentScenes =
        {
            "SimpleMachines_Lever",
            "SimpleMachines_Pulley",
            "SimpleMachines_InclinedPlane",
            "BloodCirc_HeartPump",
            "BloodCirc_ArterialFlow",
            "BloodCirc_Capillary",
            "Breathing_LungExpansion",
            "Breathing_Diaphragm",
            "Breathing_GasExchange",
        };

        [MenuItem("Tools/Fix/Run All")]
        public static void RunAll()
        {
            RepairBackButtons();
            FixPulleyMount();
            AddAnatomyLabels();
            Debug.Log("[Fix] Run All complete.");
        }

        // ─────────────────────────────────────────────────────────
        // 1. BACK BUTTON — swap GraphicRaycaster -> TrackedDeviceGraphicRaycaster
        // ─────────────────────────────────────────────────────────

        [MenuItem("Tools/Fix/Repair Back Buttons (All Scenes)")]
        public static void RepairBackButtons()
        {
            int fixedCount = 0;
            var buildPaths = new List<string> { BasicScene };

            foreach (var name in AllExperimentScenes)
            {
                string path = $"{ScenesFolder}/{name}.unity";
                buildPaths.Add(path);

                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"[Fix] Scene missing: {path}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var canvas = FindInScene(scene, "BackButtonCanvas");
                if (canvas == null)
                {
                    Debug.LogWarning($"[Fix] No BackButtonCanvas in {name}");
                    continue;
                }

                if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                {
                    var old = canvas.GetComponent<GraphicRaycaster>();
                    if (old != null) Object.DestroyImmediate(old);
                    canvas.AddComponent<TrackedDeviceGraphicRaycaster>();
                    fixedCount++;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            EnsureInBuildSettings(buildPaths);
            Debug.Log($"[Fix] Back buttons repaired in {fixedCount} scene(s); build settings verified.");
        }

        // ─────────────────────────────────────────────────────────
        // 2. PULLEY MOUNT — un-parent the static bracket from the spinning Wheel
        // ─────────────────────────────────────────────────────────

        [MenuItem("Tools/Fix/Fix Pulley Mount")]
        public static void FixPulleyMount()
        {
            string path = $"{ScenesFolder}/SimpleMachines_Pulley.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var mount = FindInScene(scene, "Pulley_Mount");
            var wheel = FindInScene(scene, "Wheel");
            if (mount == null || wheel == null)
            {
                Debug.LogError("[Fix] Pulley_Mount or Wheel not found.");
                return;
            }

            // Static rig parent that does NOT rotate.
            var rig = FindInScene(scene, "PulleyRig");
            if (rig == null) rig = new GameObject("PulleyRig");

            // Re-parent the mount out of the wheel, preserving its world transform,
            // so only the wheel (and its disc/rope children) spins.
            mount.transform.SetParent(rig.transform, worldPositionStays: true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Fix] Pulley_Mount detached from Wheel; mount stays fixed while wheel turns.");
        }

        // ─────────────────────────────────────────────────────────
        // 3. ANATOMY LABELS — always-on labels with leader lines
        // ─────────────────────────────────────────────────────────

        // (partKeyEN, keyRW, radial direction from organ centre, distance multiplier)
        static readonly (string en, string rw, Vector3 dir, float dist)[] HeartParts =
        {
            ("lbl_right_atrium",     "lbl_right_atrium",     new Vector3( 0.9f,  0.7f, 0f), 1.0f),
            ("lbl_left_atrium",      "lbl_left_atrium",      new Vector3(-0.9f,  0.7f, 0f), 1.0f),
            ("lbl_right_ventricle",  "lbl_right_ventricle",  new Vector3( 0.9f, -0.6f, 0f), 1.0f),
            ("lbl_left_ventricle",   "lbl_left_ventricle",   new Vector3(-0.9f, -0.6f, 0f), 1.0f),
            ("lbl_aorta",            "lbl_aorta",            new Vector3( 0.2f,  1.2f, 0f), 1.2f),
            ("lbl_pulmonary_artery", "lbl_pulmonary_artery", new Vector3(-0.4f,  1.1f, 0f), 1.2f),
        };

        static readonly (string en, string rw, Vector3 dir, float dist)[] LungParts =
        {
            ("lbl_trachea",    "lbl_trachea",    new Vector3( 0.0f,  1.3f, 0f), 1.2f),
            ("lbl_bronchi",    "lbl_bronchi",    new Vector3( 0.5f,  0.8f, 0f), 1.0f),
            ("lbl_left_lung",  "lbl_left_lung",  new Vector3(-1.1f,  0.2f, 0f), 1.0f),
            ("lbl_right_lung", "lbl_right_lung", new Vector3( 1.1f,  0.2f, 0f), 1.0f),
            ("lbl_diaphragm",  "lbl_diaphragm",  new Vector3( 0.0f, -1.1f, 0f), 1.1f),
            ("lbl_alveoli",    "lbl_alveoli",    new Vector3(-0.6f, -0.6f, 0f), 1.0f),
        };

        [MenuItem("Tools/Fix/Add Anatomy Labels (Heart & Lungs)")]
        public static void AddAnatomyLabels()
        {
            int scenesTouched = 0;
            foreach (var name in AllExperimentScenes)
            {
                string path = $"{ScenesFolder}/{name}.unity";
                if (!System.IO.File.Exists(path)) continue;

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                var heart = FindInScene(scene, "Heart");
                var lungL = FindInScene(scene, "LungLeft");
                var lungR = FindInScene(scene, "LungRight");

                bool any = false;

                if (heart != null)
                {
                    BuildLabelSet(scene, "AnatomyLabels_Heart", heart.transform, HeartParts);
                    any = true;
                }
                if (lungL != null || lungR != null)
                {
                    // Centre between the two lungs (or whichever exists).
                    Vector3 c = lungL != null && lungR != null
                        ? (lungL.transform.position + lungR.transform.position) * 0.5f
                        : (lungL != null ? lungL.transform.position : lungR.transform.position);
                    BuildLabelSet(scene, "AnatomyLabels_Lungs", c, LungParts);
                    any = true;
                }

                if (any)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    scenesTouched++;
                }
            }
            Debug.Log($"[Fix] Anatomy labels added to {scenesTouched} scene(s).");
        }

        static void BuildLabelSet(Scene scene, string rootName, Transform organ,
            (string en, string rw, Vector3 dir, float dist)[] parts)
        {
            BuildLabelSet(scene, rootName, organ.position, parts);
        }

        static void BuildLabelSet(Scene scene, string rootName, Vector3 organCentre,
            (string en, string rw, Vector3 dir, float dist)[] parts)
        {
            // Re-runnable: clear any previous bake.
            var existing = FindInScene(scene, rootName);
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject(rootName);
            root.transform.position = organCentre;

            // Player roughly stands at z = -2, eye height ~1.6.
            Vector3 viewer = new Vector3(organCentre.x, 1.6f, organCentre.z - 2f);

            const float spread = 0.45f;   // metres per unit of dir
            foreach (var p in parts)
            {
                Vector3 anchor   = organCentre + p.dir.normalized * (spread * 0.35f);
                Vector3 labelPos = organCentre + p.dir.normalized * (spread * p.dist) +
                                   p.dir.normalized * 0.25f;

                // ── Label ──
                var go  = new GameObject(p.en);
                go.transform.SetParent(root.transform, false);
                go.transform.position = labelPos;
                // Face the viewer so text is readable.
                go.transform.rotation = Quaternion.LookRotation(labelPos - viewer, Vector3.up);

                var tmp = go.AddComponent<TextMeshPro>();
                tmp.text          = p.en;            // refreshed at runtime by MachineLabel
                tmp.fontSize      = 1.4f;
                tmp.alignment     = TextAlignmentOptions.Center;
                tmp.color         = Color.white;
                var rt = tmp.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(4f, 1f);

                var ml = go.AddComponent<MachineLabel>();
                var so = new SerializedObject(ml);
                so.FindProperty("keyEN").stringValue = p.en;
                so.FindProperty("keyRW").stringValue = p.rw;
                so.ApplyModifiedProperties();

                // ── Leader line: label -> organ anchor ──
                var lineGo = new GameObject(p.en + "_Leader");
                lineGo.transform.SetParent(go.transform, false);
                var lr = lineGo.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = 2;
                lr.SetPosition(0, labelPos);
                lr.SetPosition(1, anchor);
                lr.startWidth = 0.01f;
                lr.endWidth   = 0.01f;
                lr.numCapVertices = 2;
                var sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                var mat = new Material(sh) { color = new Color(1f, 1f, 1f, 0.7f) };
                lr.sharedMaterial = mat;
            }
        }

        // ─────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────

        // Finds a root-or-nested object by name in the open scene, including inactive ones.
        static GameObject FindInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var t = FindChild(root.transform, name);
                if (t != null) return t.gameObject;
            }
            return null;
        }

        static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform c in parent)
            {
                if (c.name == name) return c;
                var r = FindChild(c, name);
                if (r != null) return r;
            }
            return null;
        }

        static void EnsureInBuildSettings(IEnumerable<string> paths)
        {
            var list = EditorBuildSettings.scenes.ToList();
            foreach (var path in paths)
                if (!list.Any(s => s.path == path))
                    list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
