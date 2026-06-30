// Assets/Editor/BreathingSetup.cs
// Wires the BreathingSimulator into the already-built breathing scenes.
//   Tools/Breathing/Setup All Scenes
//   Tools/Breathing/1 - Lung Expansion
//   Tools/Breathing/2 - Diaphragm
//
// SceneBuilder creates the lung / diaphragm props but never attaches the
// simulator or assigns its part references. This closes that gap so the
// lungs actually inflate and the diaphragm drops at runtime.
//
// Re-runnable: each pass rebuilds the "BreathingRig" host and its labels.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using VRLearning.Simulation.Physics;
using VRLearning.Simulation.SimpleMachines;

namespace VRLearning.Editor
{
    public static class BreathingSetup
    {
        const string LungScene = "Assets/Scenes/Experiments/Breathing_LungExpansion.unity";
        const string DiaScene  = "Assets/Scenes/Experiments/Breathing_Diaphragm.unity";

        // ─────────────────────────────────────────────────────────
        // MENU ITEMS
        // ─────────────────────────────────────────────────────────

        [MenuItem("Tools/Breathing/Setup All Scenes")]
        public static void SetupAll()
        {
            SetupLungExpansion();
            SetupDiaphragm();
            Debug.Log("[BreathingSetup] Breathing scenes wired up.");
        }

        [MenuItem("Tools/Breathing/1 - Lung Expansion")]
        public static void SetupLungExpansion()
        {
            var scene = EditorSceneManager.OpenScene(LungScene, OpenSceneMode.Single);

            var lungL = FindInScene(scene, "LungLeft");
            var lungR = FindInScene(scene, "LungRight");
            if (lungL == null && lungR == null)
            {
                Debug.LogError("[BreathingSetup] No LungLeft/LungRight in Lung Expansion scene. Run 'VRLearning > Build Test Scenes' first.");
                return;
            }

            // Centre the rig between whichever lungs exist.
            Vector3 centre = lungL != null && lungR != null
                ? (lungL.transform.position + lungR.transform.position) * 0.5f
                : (lungL != null ? lungL.transform.position : lungR.transform.position);

            WireSimulator(scene, centre,
                leftLung:  lungL ? lungL.transform : null,
                rightLung: lungR ? lungR.transform : null,
                diaphragm: FindTransform(scene, "Diaphragm"));

            Save(scene);
        }

        [MenuItem("Tools/Breathing/2 - Diaphragm")]
        public static void SetupDiaphragm()
        {
            var scene = EditorSceneManager.OpenScene(DiaScene, OpenSceneMode.Single);

            var dia = FindInScene(scene, "Diaphragm");
            if (dia == null)
            {
                Debug.LogError("[BreathingSetup] No 'Diaphragm' in Diaphragm scene. Run 'VRLearning > Build Test Scenes' first.");
                return;
            }

            WireSimulator(scene, dia.transform.position + Vector3.up * 0.4f,
                leftLung:  FindTransform(scene, "LungLeft"),
                rightLung: FindTransform(scene, "LungRight"),
                diaphragm: dia.transform);

            Save(scene);
        }

        // ─────────────────────────────────────────────────────────
        // CORE WIRING
        // ─────────────────────────────────────────────────────────

        static void WireSimulator(Scene scene, Vector3 centre,
            Transform leftLung, Transform rightLung, Transform diaphragm)
        {
            // Fresh rig each run.
            var old = FindInScene(scene, "BreathingRig");
            if (old != null) Object.DestroyImmediate(old);

            var rig = new GameObject("BreathingRig");
            rig.transform.position = centre;

            var sim = rig.AddComponent<BreathingSimulator>();

            var phase = MakeLabel(rig.transform, "PhaseLabel", centre + new Vector3(0f, 0.55f, 0f), "Inhale");
            var rr    = MakeLabel(rig.transform, "RRLabel",    centre + new Vector3(0f, 0.40f, 0f), "15 breaths/min");

            // BreathingSimulator's part references are private [SerializeField]s.
            var so = new SerializedObject(sim);
            SetRef(so, "leftLung",   leftLung);
            SetRef(so, "rightLung",  rightLung);
            SetRef(so, "diaphragm",  diaphragm);
            SetRef(so, "phaseLabel", phase);
            SetRef(so, "rrLabel",    rr);
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[BreathingSetup] Wired BreathingSimulator in '{scene.name}' " +
                      $"(L:{(leftLung ? "✓" : "–")} R:{(rightLung ? "✓" : "–")} Dia:{(diaphragm ? "✓" : "–")}).");
        }

        // A world-space TMP label whose text is driven at runtime via
        // MachineLabel.SetOverrideText (called by BreathingSimulator).
        static MachineLabel MakeLabel(Transform parent, string name, Vector3 pos, string initial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            // Face roughly toward the player (who stands at z = -2).
            go.transform.rotation = Quaternion.LookRotation(pos - new Vector3(pos.x, 1.6f, pos.z - 2f), Vector3.up);

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text      = initial;
            tmp.fontSize  = 1.4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 1f);

            return go.AddComponent<MachineLabel>();
        }

        // ─────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────

        static void SetRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p == null) { Debug.LogWarning($"[BreathingSetup] Property '{prop}' not found on BreathingSimulator."); return; }
            p.objectReferenceValue = value;
        }

        static void Save(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static Transform FindTransform(Scene scene, string name)
        {
            var go = FindInScene(scene, name);
            return go != null ? go.transform : null;
        }

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
    }
}
