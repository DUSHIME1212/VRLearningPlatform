using UnityEngine;

namespace VRLearning.Core
{
    /// <summary>
    /// Instantiates the persistent "Managers" prefab (localisation, audio, data store, scoring,
    /// session) before the first scene loads, so every experiment scene — entered directly via
    /// SceneNavigator — has the core services available. The managers are DontDestroyOnLoad
    /// singletons, so duplicates from any scene copy self-destruct.
    /// </summary>
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var prefab = Resources.Load<GameObject>("Managers");
            if (prefab == null)
            {
                Debug.LogWarning("[AppBootstrap] Resources/Managers prefab not found — core services will not run.");
                return;
            }
            var go = Object.Instantiate(prefab);
            go.name = "Managers";
        }
    }
}
