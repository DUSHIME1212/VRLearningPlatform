using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRLearning.Parts;

namespace VRLearning.Editor
{
    /// <summary>
    /// Editor convenience: assigns each <see cref="PartInfo"/>'s NarrationClip from
    /// Assets/Audio/Narration/&lt;PartId or NameEN&gt;.(wav|mp3|ogg) by naming convention, so after
    /// generating the audio the user drops the files in one folder and clicks a menu item.
    /// Never overwrites a clip that's already set.
    /// </summary>
    public static class NarrationClipAssigner
    {
        const string NarrationDir = "Assets/Audio/Narration";

        [MenuItem("Tools/Narration/Assign Clips To Open Scene Parts")]
        public static void AssignOpenScene()
        {
            var parts = UnityEngine.Object.FindObjectsByType<PartInfo>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int assigned = 0;
            foreach (var p in parts)
                if (TryAssign(p)) assigned++;
            Debug.Log($"[NarrationClipAssigner] Open scene: assigned {assigned}/{parts.Length} clips from {NarrationDir}.");
        }

        [MenuItem("Tools/Narration/Assign Clips To Part Prefabs")]
        public static void AssignPrefabs()
        {
            int assigned = 0, total = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                bool dirty = false;
                foreach (var p in go.GetComponentsInChildren<PartInfo>(true))
                {
                    total++;
                    if (TryAssign(p)) { assigned++; dirty = true; }
                }
                if (dirty) EditorUtility.SetDirty(go);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[NarrationClipAssigner] Prefabs: assigned {assigned}/{total} clips from {NarrationDir}.");
        }

        static bool TryAssign(PartInfo part)
        {
            if (part == null || part.NarrationClip != null) return false; // never overwrite
            string key = !string.IsNullOrEmpty(part.PartId) ? part.PartId : Sanitize(part.NameEN);
            if (string.IsNullOrEmpty(key)) return false;

            var clip = LoadClip(key);
            if (clip == null) return false;

            var so = new SerializedObject(part);
            so.FindProperty("NarrationClip").objectReferenceValue = clip;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(part);
            return true;
        }

        static AudioClip LoadClip(string key)
        {
            foreach (var ext in new[] { ".wav", ".mp3", ".ogg" })
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{NarrationDir}/{key}{ext}");
                if (clip != null) return clip;
            }
            if (!AssetDatabase.IsValidFolder(NarrationDir)) return null;

            // Case-insensitive fallback within the narration folder.
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { NarrationDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), key, StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }

        static string Sanitize(string s) => string.IsNullOrEmpty(s) ? s : s.Replace(" ", "_");
    }
}
