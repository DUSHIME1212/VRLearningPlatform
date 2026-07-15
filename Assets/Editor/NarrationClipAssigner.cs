using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRLearning.Parts;

namespace VRLearning.Editor
{
    /// <summary>
    /// Editor convenience: assigns each <see cref="PartInfo"/>'s NarrationClip from
    /// Assets/Audio/VO/English/&lt;candidate name&gt;.(aac|wav|mp3|ogg) by naming convention, so
    /// after generating the audio the user drops the files in one folder and clicks a menu item.
    /// Tries several candidate names per part (PartId, the raw display name, and an
    /// underscored version) since exported clips aren't always named consistently.
    /// Never overwrites a clip that's already set.
    /// </summary>
    public static class NarrationClipAssigner
    {
        const string NarrationDir = "Assets/Audio/VO/MP3";
        static readonly string[] Extensions = { ".aac", ".wav", ".mp3", ".ogg" };

        [MenuItem("Tools/Narration/Assign Clips To Open Scene Parts")]
        public static void AssignOpenScene()
        {
            var parts = UnityEngine.Object.FindObjectsByType<PartInfo>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int assigned = 0;
            var missing = new List<string>();
            foreach (var p in parts)
            {
                if (TryAssign(p)) assigned++;
                else if (p != null && p.NarrationClip == null) missing.Add(p.PartId ?? p.NameEN);
            }
            Debug.Log($"[NarrationClipAssigner] Open scene: assigned {assigned}/{parts.Length} clips from {NarrationDir}.");
            if (missing.Count > 0)
                Debug.LogWarning($"[NarrationClipAssigner] No clip found for: {string.Join(", ", missing)}");
        }

        [MenuItem("Tools/Narration/Assign Clips To Part Prefabs")]
        public static void AssignPrefabs()
        {
            int assigned = 0, total = 0;
            var missing = new List<string>();
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
                    else if (p.NarrationClip == null) missing.Add(p.PartId ?? p.NameEN);
                }
                if (dirty) EditorUtility.SetDirty(go);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[NarrationClipAssigner] Prefabs: assigned {assigned}/{total} clips from {NarrationDir}.");
            if (missing.Count > 0)
                Debug.LogWarning($"[NarrationClipAssigner] No clip found for: {string.Join(", ", missing)}");
        }

        static bool TryAssign(PartInfo part)
        {
            if (part == null || part.NarrationClip != null) return false; // never overwrite

            AudioClip clip = null;
            foreach (var key in CandidateKeys(part))
            {
                clip = LoadClip(key);
                if (clip != null) break;
            }
            if (clip == null) return false;

            var so = new SerializedObject(part);
            so.FindProperty("NarrationClip").objectReferenceValue = clip;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(part);
            return true;
        }

        // Exported clips aren't named consistently (PartId, "Display Name", or Display_Name),
        // so try each plausible form in priority order.
        static IEnumerable<string> CandidateKeys(PartInfo part)
        {
            if (!string.IsNullOrEmpty(part.PartId)) yield return part.PartId;
            if (!string.IsNullOrEmpty(part.NameEN))
            {
                yield return part.NameEN;                      // "Vena Cava"
                yield return part.NameEN.Replace(" ", "_");     // "Vena_Cava"
                yield return part.NameEN.Replace(" ", "");      // "VenaCava"
            }
        }

        static AudioClip LoadClip(string key)
        {
            foreach (var ext in Extensions)
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
    }
}
