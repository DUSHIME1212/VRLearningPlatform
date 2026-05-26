using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace VRLearning.Modules.ARWorksheet
{
    /// <summary>
    /// AR overlay module. Detects printed worksheet markers with ARFoundation.
    /// Spawns 3D concept animations on top of each detected marker.
    /// </summary>
    public class ARWorksheetManager : MonoBehaviour
    {
        [Header("AR")]
        [SerializeField] private ARTrackedImageManager trackedImageManager;

        [Header("Overlays")]
        [SerializeField] private WorksheetOverlay[] overlayPrefabs;

        private void OnEnable()
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }

        private void OnDisable()
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }

        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var img in args.added)   SpawnOverlay(img);
            foreach (var img in args.updated) UpdateOverlay(img);
            foreach (var kvp in args.removed) RemoveOverlay(kvp.Value);
        }

        private void SpawnOverlay(ARTrackedImage img)
        {
            var overlay = System.Array.Find(overlayPrefabs,
                o => o.MarkerName == img.referenceImage.name);
            if (overlay == null) return;

            var instance = Instantiate(overlay, img.transform.position, img.transform.rotation);
            instance.Activate(img.referenceImage.name);
        }

        private void UpdateOverlay(ARTrackedImage img) { /* track position updates */ }
        private void RemoveOverlay(ARTrackedImage img) { /* hide overlay */ }
    }
}
