using UnityEngine;

namespace VRLearning.Core
{
    /// <summary>
    /// Frees unused assets when the OS raises a low-memory signal, giving the app a chance to survive a
    /// spike instead of being killed outright — the most common Quest "crash". Logs the event so it
    /// appears in the console and as a Sentry breadcrumb. Lives on the persistent Managers object.
    /// </summary>
    public class LowMemoryHandler : MonoBehaviour
    {
        private void OnEnable()  => Application.lowMemory += OnLowMemory;
        private void OnDisable() => Application.lowMemory -= OnLowMemory;

        private void OnLowMemory()
        {
            VRLog.Warn("Memory", "low-memory signal — unloading unused assets");
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}