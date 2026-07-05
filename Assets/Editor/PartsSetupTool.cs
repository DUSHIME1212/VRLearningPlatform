using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using VRLearning.Parts;
using VRLearning.Simulation.Physics;
using VRLearning.Audio;

namespace VRLearning.EditorTools
{
    /// <summary>
    /// One-click wiring for the parts display. For the active scene it adds a PartInfo (with an
    /// English name + description looked up from the table below) to each model part, fills the
    /// PartsInfoPanel's parts list, wires its Prev/Next/Explode buttons, and sets up an ExplodedView.
    ///
    /// Usage: select the model group (e.g. "Lungs") — or multi-select the individual parts — in the
    /// Hierarchy, then run VRLearning ▸ Parts ▸ Set Up Parts In Active Scene. Idempotent: safe to re-run
    /// as you add more parts.
    /// </summary>
    public static class PartsSetupTool
    {
        private class Entry
        {
            public readonly string Key, Name, Desc;
            public Entry(string k, string n, string d) { Key = k; Name = n; Desc = d; }
        }

        // Ordered content table. Keys are already-normalised (lowercase, single spaces).
        // Matched most-specific-first (longest key wins); the tour order follows this list.
        private static readonly List<Entry> Table = new List<Entry>
        {
            // Breathing – lungs
            new Entry("trachea",   "Trachea",   "Windpipe carrying air from the throat down toward the lungs."),
            new Entry("bronchi",   "Bronchi",   "Two tubes branching from the trachea, one into each lung."),
            new Entry("right lung","Right Lung","Right breathing organ where oxygen enters the blood."),
            new Entry("left lung", "Left Lung", "Left breathing organ; slightly smaller to make room for the heart."),
            new Entry("rib cage",  "Rib Cage",  "Bony cage protecting the lungs and heart; moves to help you breathe."),
            new Entry("diaphragm", "Diaphragm", "Dome-shaped muscle below the lungs that drives breathing."),
            // Breathing – diaphragm / gas exchange
            new Entry("thoracic",       "Thoracic Cavity",     "The chest space that holds the lungs and heart."),
            new Entry("intercostal",    "Intercostal Muscles", "Muscles between the ribs that lift the chest when you inhale."),
            new Entry("ribs",           "Ribs",                "Curved bones forming the protective chest wall."),
            new Entry("lungs",          "Lungs",               "The pair of organs where gas exchange happens."),
            new Entry("alveolar wall",  "Alveolar Wall",       "The ultra-thin wall where gases cross into the blood."),
            new Entry("alveoli",        "Alveoli",             "Clusters of tiny air sacs where gas exchange happens."),
            new Entry("alveolus",       "Alveolus",            "A tiny air sac in the lungs where gas exchange happens."),
            // Blood – heart
            new Entry("right atrium",    "Right Atrium",     "Upper chamber that receives oxygen-poor blood from the body."),
            new Entry("left atrium",     "Left Atrium",      "Upper chamber that receives oxygen-rich blood from the lungs."),
            new Entry("right ventricle", "Right Ventricle",  "Lower chamber that pumps blood to the lungs."),
            new Entry("left ventricle",  "Left Ventricle",   "Lower chamber that pumps oxygen-rich blood to the whole body."),
            new Entry("aorta",           "Aorta",            "The largest artery; carries blood from the heart to the body."),
            new Entry("pulmonary artery","Pulmonary Artery", "Carries oxygen-poor blood from the heart to the lungs."),
            new Entry("pulmonary",       "Pulmonary Vessel", "Blood vessel connecting the heart and the lungs."),
            new Entry("vena cava",       "Vena Cava",        "Large vein returning oxygen-poor blood to the heart."),
            new Entry("valve",           "Valve",            "Flap that stops blood flowing backward through the heart."),
            new Entry("heart",           "Heart",            "Muscular pump that pushes blood around the body."),
            // Blood – arterial / capillary
            new Entry("artery wall",   "Artery Wall",        "Thick, muscular wall that withstands high blood pressure."),
            new Entry("capillary wall","Capillary Wall",     "A wall one cell thick, so gases pass through easily."),
            new Entry("smooth muscle", "Smooth-Muscle Layer","Muscle layer that lets the artery widen and narrow."),
            new Entry("endothelium",   "Endothelium",        "Smooth inner lining that blood flows against."),
            new Entry("artery",        "Artery",             "Vessel carrying blood away from the heart."),
            new Entry("lumen",         "Lumen",              "The hollow space inside the vessel where blood flows."),
            new Entry("capillary",     "Capillary",          "Tiniest blood vessel, where exchange with tissues happens."),
            new Entry("tissue cell",   "Tissue Cell",        "A body cell that takes in oxygen and gives off carbon dioxide."),
            new Entry("red blood cell","Red Blood Cell",     "Cell that carries oxygen through the blood."),
            new Entry("carbon dioxide","Carbon Dioxide",     "Waste gas that is breathed out of the body."),
            new Entry("oxygen",        "Oxygen",             "Gas the body needs; carried by red blood cells."),
            // Simple machines
            new Entry("fulcrum",      "Fulcrum",          "The fixed pivot point the lever turns around."),
            new Entry("lever plank",  "Lever Arm",        "The rigid bar that turns on the fulcrum."),
            new Entry("lever arm",    "Lever Arm",        "The rigid bar that turns on the fulcrum."),
            new Entry("plank",        "Lever Arm",        "The rigid bar that turns on the fulcrum."),
            new Entry("lever",        "Lever",            "A rigid bar that pivots on a fulcrum to move a load."),
            new Entry("effort",       "Effort",           "The force you apply to move the load."),
            new Entry("pulley wheel", "Pulley Wheel",     "Grooved wheel the rope runs over to redirect force."),
            new Entry("pulley",       "Pulley",           "A wheel and rope that changes the direction of a force."),
            new Entry("rope",         "Rope",             "Flexible line that transfers force over the pulley."),
            new Entry("support beam", "Support Beam",     "Beam that holds the pulley in place."),
            new Entry("support",      "Support",          "Structure that holds the machine in place."),
            new Entry("beam",         "Beam",             "Horizontal bar that supports the pulley."),
            new Entry("ramp",         "Ramp Surface",     "Sloped surface that trades a smaller force for a longer push."),
            new Entry("incline",      "Inclined Plane",   "A sloped surface used to raise a load with less force."),
            new Entry("wedge",        "Wedge",            "A triangular block; two inclined planes back to back."),
            new Entry("friction",     "Friction Surface", "Surface roughness that resists the sliding block."),
            new Entry("weight",       "Load",             "The weight the machine helps you lift."),
            new Entry("load",         "Load",             "The weight the machine helps you lift."),
            new Entry("block",        "Block",            "The load pushed up the ramp."),
            new Entry("base",         "Base",             "The flat bottom the ramp rests on."),
        };

        private static readonly HashSet<string> Noise = new HashSet<string>
            { "meshy", "ai", "image", "texture", "fbx", "glb", "gltf", "obj", "the", "anatomy", "dia" };

        [MenuItem("VRLearning/Parts/Set Up Parts In Active Scene")]
        public static void SetUp()
        {
            var panel = Object.FindFirstObjectByType<PartsInfoPanel>(FindObjectsInactive.Include);
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Parts Setup", "No PartsInfoPanel found in the active scene.", "OK");
                return;
            }

            // ---- resolve the parts + explode root from the current selection ----
            var selected = Selection.gameObjects ?? new GameObject[0];
            var parts = new List<Transform>();
            Transform root;

            if (selected.Length > 1)
            {
                foreach (var go in selected)
                    if (HasRenderer(go.transform)) parts.Add(go.transform);
                root = selected[0].transform.parent != null ? selected[0].transform.parent : selected[0].transform;
            }
            else
            {
                root = selected.Length == 1 ? selected[0].transform : GetPartsRoot(panel);
                if (root == null)
                {
                    EditorUtility.DisplayDialog("Parts Setup",
                        "Select the model group (e.g. \"Lungs\") — or multi-select the individual parts — in the Hierarchy, then run again.",
                        "OK");
                    return;
                }
                foreach (Transform c in root)
                    if (HasRenderer(c)) parts.Add(c);
                if (parts.Count == 0 && HasRenderer(root)) parts.Add(root);
            }

            if (parts.Count == 0)
            {
                EditorUtility.DisplayDialog("Parts Setup", "No parts with renderers found under the selection.", "OK");
                return;
            }

            string summary = Configure(panel, root, parts);
            EditorUtility.DisplayDialog("Parts Setup",
                $"Done.\n\n{summary}\n\nEnter Play mode and use Next / Previous / Explode.", "OK");
        }

        /// <summary>
        /// Programmatic entry (no dialogs): wire the active scene's PartsInfoPanel using the group
        /// named <paramref name="rootName"/> as the parts root. Returns a one-line-per-item summary.
        /// </summary>
        public static string RunSilent(string rootName)
        {
            var panel = Object.FindFirstObjectByType<PartsInfoPanel>(FindObjectsInactive.Include);
            if (panel == null) return "No PartsInfoPanel in active scene.";

            var rootGo = GameObject.Find(rootName);
            if (rootGo == null) return $"Root '{rootName}' not found.";
            var root = rootGo.transform;

            var parts = new List<Transform>();
            foreach (Transform c in root)
                if (HasRenderer(c)) parts.Add(c);
            if (parts.Count == 0 && HasRenderer(root)) parts.Add(root);
            if (parts.Count == 0) return $"No parts with renderers under '{rootName}'.";

            return Configure(panel, root, parts);
        }

        /// <summary>Core wiring: add PartInfo + content, fill the panel, set up ExplodedView, wire buttons, save.</summary>
        private static string Configure(PartsInfoPanel panel, Transform root, List<Transform> parts)
        {
            var withOrder = new List<KeyValuePair<int, PartInfo>>();
            int matched = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                var t = parts[i];
                var pi = t.GetComponent<PartInfo>();
                if (pi == null) pi = Undo.AddComponent<PartInfo>(t.gameObject);

                var entry = Match(t.name);
                Undo.RecordObject(pi, "Set PartInfo");
                pi.PartId = t.name;
                if (entry != null)
                {
                    pi.NameEN = entry.Name;
                    pi.DescriptionEN = entry.Desc;
                    matched++;
                }
                else if (string.IsNullOrEmpty(pi.NameEN))
                {
                    pi.NameEN = Prettify(t.name);
                }
                EditorUtility.SetDirty(pi);

                int order = entry != null ? Table.IndexOf(entry) : 10000 + i;
                withOrder.Add(new KeyValuePair<int, PartInfo>(order, pi));
            }
            var ordered = withOrder.OrderBy(k => k.Key).Select(k => k.Value).ToList();

            // ---- wire the panel (private serialized fields) ----
            var pso = new SerializedObject(panel);
            var partsProp = pso.FindProperty("parts");
            partsProp.ClearArray();
            for (int i = 0; i < ordered.Count; i++)
            {
                partsProp.InsertArrayElementAtIndex(i);
                partsProp.GetArrayElementAtIndex(i).objectReferenceValue = ordered[i];
            }
            pso.FindProperty("partsRoot").objectReferenceValue = root;
            pso.FindProperty("selectFirstOnStart").boolValue = true;

            // ---- explode support ----
            var ev = root.GetComponent<ExplodedView>();
            if (ev == null) ev = Undo.AddComponent<ExplodedView>(root.gameObject);
            PopulateExplodedView(ev, ordered.Select(p => p.transform).ToList());
            pso.FindProperty("explodedView").objectReferenceValue = ev;
            pso.ApplyModifiedProperties();

            // ---- wire buttons ----
            WireButton(panel, "PrevButton",    nameof(PartsInfoPanel.Previous));
            WireButton(panel, "NextButton",    nameof(PartsInfoPanel.Next));
            WireButton(panel, "ExplodeButton", nameof(PartsInfoPanel.ToggleExplode));

            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            EditorSceneManager.SaveScene(panel.gameObject.scene);

            string summary = $"Parts wired: {ordered.Count}\nMatched to content table: {matched}\nRoot: {root.name}";
            Debug.Log("[PartsSetup] " + summary.Replace("\n", "  "));
            return summary;
        }

        [MenuItem("VRLearning/Parts/Set Up Active Scene (Parts + VR UI Fixes)")]
        public static void SetUpActiveSceneFull()
        {
            EditorUtility.DisplayDialog("Scene Setup", ConfigureActiveSceneFull(), "OK");
        }

        /// <summary>Parts wiring (auto-detected model group) + VR UI fixes for the active scene. No dialogs.</summary>
        public static string ConfigureActiveSceneFull()
        {
            var sb = new System.Text.StringBuilder();

            var panel = Object.FindFirstObjectByType<PartsInfoPanel>(FindObjectsInactive.Include);
            if (panel != null)
            {
                var root = GetPartsRoot(panel) ?? BestPartsRoot();
                if (root != null)
                {
                    var parts = new List<Transform>();
                    foreach (Transform c in root)
                        if (HasRenderer(c)) parts.Add(c);
                    if (parts.Count == 0 && HasRenderer(root)) parts.Add(root);
                    sb.AppendLine(parts.Count > 0 ? Configure(panel, root, parts) : "Parts: no renderer children under " + root.name);
                }
                else sb.AppendLine("Parts: no model group found (select the group and use the other menu item).");
            }
            else sb.AppendLine("Parts: no PartsInfoPanel in scene.");

            sb.AppendLine(FixSceneUI());

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return sb.ToString();
        }

        /// <summary>
        /// VR UI fixes: swap GraphicRaycaster → TrackedDeviceGraphicRaycaster on world canvases (so XR
        /// clicks register), rotate our panels to face the player, and turn off right-to-left text.
        /// </summary>
        public static string FixSceneUI()
        {
            int rc = 0, faced = 0, rtl = 0;

            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas.renderMode != RenderMode.WorldSpace) continue;
                if (canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>() == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
                    rc++;
                }
                var grc = canvas.GetComponent<GraphicRaycaster>();
                if (grc != null) Object.DestroyImmediate(grc);
            }

            var refGo = GameObject.Find("BackButtonCanvas");
            Quaternion? r = refGo != null ? refGo.transform.rotation : (Quaternion?)null;
            foreach (var n in new[] { "QuizPanel", "PartsInfoPanel", "VideoScreen" })
            {
                var go = GameObject.Find(n);
                if (go == null) continue;
                if (r.HasValue) { go.transform.rotation = r.Value; faced++; }
                foreach (var t in go.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    if (t.isRightToLeftText) { t.isRightToLeftText = false; EditorUtility.SetDirty(t); rtl++; }
            }
            return $"UI: raycasters={rc}, faced={faced}, RTL-off={rtl}";
        }

        // ---------------- Scene audio ----------------

        private const string ClickClipPath = "Assets/VRTemplateAssets/Audio/Button_22_click.wav";

        [MenuItem("VRLearning/Parts/Add Scene Audio (UISoundKit)")]
        public static void SetupSceneAudioMenu()
        {
            EditorUtility.DisplayDialog("Scene Audio", SetupSceneAudio(), "OK");
        }

        /// <summary>Add/refresh a "SceneAudio" object with a UISoundKit and assign the sample click clip.</summary>
        public static string SetupSceneAudio()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickClipPath);

            var go = GameObject.Find("SceneAudio");
            if (go == null) { go = new GameObject("SceneAudio"); Undo.RegisterCreatedObjectUndo(go, "Create SceneAudio"); }
            var kit = go.GetComponent<UISoundKit>();
            if (kit == null) kit = Undo.AddComponent<UISoundKit>(go);

            var so = new SerializedObject(kit);
            foreach (var field in new[] { "clickClip", "correctClip", "wrongClip", "completeClip" })
            {
                var p = so.FindProperty(field);
                if (p != null) p.objectReferenceValue = clip;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return clip != null ? "UISoundKit added; click clip assigned (pitch handles correct/wrong/complete)."
                                : "UISoundKit added, but click clip not found at " + ClickClipPath;
        }

        // ---------------- Heavy FBX import optimisation ----------------

        [MenuItem("VRLearning/Parts/Optimize Heavy FBX Imports")]
        public static void OptimizeHeavyFbxMenu()
        {
            EditorUtility.DisplayDialog("Optimize Heavy FBX Imports", OptimizeHeavyFbxImports(), "OK");
        }

        /// <summary>Read/Write off, Mesh Compression High, Generate Colliders off on large (&gt;5 MB) FBX models.</summary>
        public static string OptimizeHeavyFbxImports()
        {
            int changed = 0;
            var sb = new System.Text.StringBuilder();
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.ToLowerInvariant().EndsWith(".fbx")) continue;
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                if (new System.IO.FileInfo(path).Length < 5L * 1024 * 1024) continue; // only heavy FBX

                bool dirty = false;
                if (importer.isReadable) { importer.isReadable = false; dirty = true; }
                if (importer.meshCompression != ModelImporterMeshCompression.High) { importer.meshCompression = ModelImporterMeshCompression.High; dirty = true; }
                if (importer.addCollider) { importer.addCollider = false; dirty = true; }
                if (dirty)
                {
                    importer.SaveAndReimport();
                    changed++;
                    sb.Append(System.IO.Path.GetFileName(path)).Append(' ');
                }
            }
            return $"Optimized {changed} heavy FBX importer(s). {sb}";
        }

        private const int HeavyVertexThreshold = 20000;

        /// <summary>
        /// Deactivate every GameObject in the active scene holding a mesh with more than
        /// <see cref="HeavyVertexThreshold"/> vertices (the big Meshy models), so the scene stays light.
        /// Placeholder cubes and normal props are untouched. Re-run the parts setup after you add final models.
        /// </summary>
        [MenuItem("VRLearning/Parts/Deactivate Heavy Models In Active Scene")]
        public static void DeactivateHeavyModels()
        {
            var sb = new System.Text.StringBuilder();
            int count = 0;

            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (mf.sharedMesh == null || mf.sharedMesh.vertexCount < HeavyVertexThreshold) continue;
                var go = mf.gameObject;
                if (!go.activeSelf) continue;
                Undo.RecordObject(go, "Deactivate heavy model");
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                count++;
                sb.AppendLine($"{go.name}  ({mf.sharedMesh.vertexCount:N0} verts)");
            }
            foreach (var smr in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (smr.sharedMesh == null || smr.sharedMesh.vertexCount < HeavyVertexThreshold) continue;
                var go = smr.gameObject;
                if (!go.activeSelf) continue;
                Undo.RecordObject(go, "Deactivate heavy model");
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                count++;
                sb.AppendLine($"{go.name}  ({smr.sharedMesh.vertexCount:N0} verts, skinned)");
            }

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Deactivate Heavy Models",
                count == 0 ? $"No meshes over {HeavyVertexThreshold:N0} verts found in this scene." :
                $"Deactivated {count} heavy object(s):\n\n{sb}", "OK");
        }

        /// <summary>Re-activate objects that <see cref="DeactivateHeavyModels"/> turned off (heavy meshes only).</summary>
        [MenuItem("VRLearning/Parts/Reactivate Heavy Models In Active Scene")]
        public static void ReactivateHeavyModels()
        {
            int count = 0;
            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mf.sharedMesh == null || mf.sharedMesh.vertexCount < HeavyVertexThreshold) continue;
                var go = mf.gameObject;
                if (go.activeSelf) continue;
                Undo.RecordObject(go, "Reactivate heavy model");
                go.SetActive(true);
                EditorUtility.SetDirty(go);
                count++;
            }
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Reactivate Heavy Models", $"Reactivated {count} object(s).", "OK");
        }

        /// <summary>Programmatic (no dialog): deactivate heavy meshes in the active scene, returns a report.</summary>
        public static string DeactivateHeavyModelsSilent()
        {
            int count = 0;
            var sb = new System.Text.StringBuilder();
            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (mf.sharedMesh == null || mf.sharedMesh.vertexCount < HeavyVertexThreshold) continue;
                var go = mf.gameObject;
                if (!go.activeSelf) continue;
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                count++;
                sb.Append(go.name).Append('(').Append(mf.sharedMesh.vertexCount).Append(") ");
            }
            foreach (var smr in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (smr.sharedMesh == null || smr.sharedMesh.vertexCount < HeavyVertexThreshold) continue;
                var go = smr.gameObject;
                if (!go.activeSelf) continue;
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                count++;
                sb.Append(go.name).Append("(skin) ");
            }
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return $"{count}: {sb}";
        }

        /// <summary>Best guess at the model group: the non-UI/non-player root object with the most renderer children.</summary>
        private static Transform BestPartsRoot()
        {
            Transform best = null;
            int bestCount = 0;
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name.Contains("VR player") || go.GetComponent<Canvas>() != null) continue;
                if (go.name == "QuizPanel" || go.name == "PartsInfoPanel" || go.name == "VideoScreen") continue;
                int c = 0;
                foreach (Transform child in go.transform)
                    if (HasRenderer(child)) c++;
                if (c > bestCount) { bestCount = c; best = go.transform; }
            }
            return bestCount >= 2 ? best : null;
        }

        // ---------------- helpers ----------------

        private static Transform GetPartsRoot(PartsInfoPanel panel)
        {
            var so = new SerializedObject(panel);
            var p = so.FindProperty("partsRoot");
            return p != null ? p.objectReferenceValue as Transform : null;
        }

        private static bool HasRenderer(Transform t) => t.GetComponentInChildren<Renderer>(true) != null;

        private static void PopulateExplodedView(ExplodedView ev, List<Transform> transforms)
        {
            var so = new SerializedObject(ev);
            var list = so.FindProperty("parts");
            list.ClearArray();
            for (int i = 0; i < transforms.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                var el = list.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("obj").objectReferenceValue = transforms[i];
                el.FindPropertyRelative("explodeOffset").vector3Value = Vector3.zero; // auto-computed at runtime
            }
            so.ApplyModifiedProperties();
        }

        private static void WireButton(PartsInfoPanel panel, string buttonName, string method)
        {
            var tf = FindDeep(panel.transform, buttonName);
            if (tf == null) return;
            var btn = tf.GetComponent<Button>();
            if (btn == null) return;

            for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(btn.onClick, i);

            UnityAction call =
                method == nameof(PartsInfoPanel.Next)     ? new UnityAction(panel.Next) :
                method == nameof(PartsInfoPanel.Previous) ? new UnityAction(panel.Previous) :
                                                            new UnityAction(panel.ToggleExplode);
            UnityEventTools.AddPersistentListener(btn.onClick, call);
            EditorUtility.SetDirty(btn);
        }

        private static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }

        private static string Norm(string s)
        {
            s = Regex.Replace(s.ToLowerInvariant(), "[^a-z]+", " ");
            var tokens = s.Split(' ')
                          .Where(tok => tok.Length >= 2 && !Noise.Contains(tok));
            return " " + string.Join(" ", tokens) + " ";
        }

        private static Entry Match(string goName)
        {
            string n = Norm(goName);
            foreach (var e in Table.OrderByDescending(x => x.Key.Length))
                if (n.Contains(e.Key)) return e;
            return null;
        }

        private static string Prettify(string goName)
        {
            var n = Norm(goName).Trim();
            if (string.IsNullOrEmpty(n)) return goName;
            var ti = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
            return ti.ToTitleCase(n);
        }
    }
}
