using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRLearning.UI
{
    /// <summary>
    /// Attach to a manager object or canvas, then link <see cref="LoadSceneByName"/> to a Button's
    /// OnClick. Loads scenes **asynchronously** and frees memory before and after the load — this
    /// avoids the memory spikes and long main-thread stalls that make memory-limited devices (Quest)
    /// kill the app during scene transitions. Make sure target scenes are in Build Settings.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Scene name provided to SceneNavigator is empty!");
                return;
            }
            SceneLoadRunner.Load(sceneName);
        }
    }

    /// <summary>
    /// Persistent (DontDestroyOnLoad) helper so the async load and the post-load memory cleanup
    /// survive the scene swap — the SceneNavigator that started the load is destroyed with its scene.
    /// </summary>
    internal class SceneLoadRunner : MonoBehaviour
    {
        public static void Load(string sceneName)
        {
            var go = new GameObject("SceneLoadRunner");
            DontDestroyOnLoad(go);
            go.AddComponent<SceneLoadRunner>().StartCoroutine(Run(go, sceneName));
        }

        private static IEnumerator Run(GameObject host, string sceneName)
        {
            // Free assets the outgoing scene no longer needs before we spike memory again.
            yield return Resources.UnloadUnusedAssets();

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"SceneNavigator: could not load '{sceneName}' — is it added to Build Settings?");
            }
            else
            {
                while (!op.isDone) yield return null;
            }

            // Reclaim the memory the old scene left behind (critical on Quest).
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();

            Destroy(host);
        }
    }
}
