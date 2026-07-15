#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRLearning.EditorTools
{
    /// <summary>
    /// Batch-caps Android texture import size to cut Quest memory pressure — without hiding any model.
    /// Textures under the target folders get an Android override capped to 1024 (512 for data maps such
    /// as normal/roughness/metallic, which don't need full colour resolution), using ASTC. Run from
    /// Tools ▸ VRLearning ▸ Cap Model Textures for Quest. Idempotent — safe to re-run.
    ///
    /// NOTE: this reimports many textures and can take a few minutes. Texture streaming (QualitySettings)
    /// is the primary memory cap; this is extra headroom. Run it deliberately, not before a build you
    /// haven't time to verify.
    /// </summary>
    public static class QuestTextureCapper
    {
        private static readonly string[] TargetFolders =
        {
            "Assets/Models", "Assets/MeshyImports", "Assets/Prefabs/Environment"
        };

        [MenuItem("Tools/VRLearning/Cap Model Textures for Quest")]
        public static void CapTextures()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", TargetFolders);
            int changed = 0;
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;

                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Capping textures for Quest", path, (float)i / guids.Length))
                        break;

                    string lower = path.ToLowerInvariant();
                    bool isDataMap = lower.Contains("normal") || lower.Contains("rough") ||
                                     lower.Contains("metallic") || lower.Contains("_mask") ||
                                     lower.Contains("height") || lower.Contains("_ao");
                    int maxSize = isDataMap ? 512 : 1024;

                    var current = importer.GetPlatformTextureSettings("Android");
                    if (current.overridden && current.maxTextureSize <= maxSize) continue;

                    importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        name = "Android",
                        overridden = true,
                        maxTextureSize = maxSize,
                        format = TextureImporterFormat.ASTC_6x6,
                        compressionQuality = 50
                    });
                    importer.SaveAndReimport();
                    changed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log($"[QuestTextureCapper] Capped {changed} of {guids.Length} textures (Android ASTC ≤1024/512).");
        }
    }
}
#endif