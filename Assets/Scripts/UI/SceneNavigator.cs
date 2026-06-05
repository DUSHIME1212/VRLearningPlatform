using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRLearning.UI
{
    /// <summary>
    /// Attach this script to a manager object or canvas in your scene, 
    /// then link the LoadSceneByName method to your UI Button's OnClick event.
    /// </summary>
    public class SceneNavigator : MonoBehaviour
    {
        /// <summary>
        /// Loads a scene by its name. 
        /// Make sure the target scene is added to File -> Build Settings -> Scenes in Build.
        /// </summary>
        /// <param name="sceneName">The exact name of the scene to load.</param>
        public void LoadSceneByName(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"Loading Scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene name provided to SceneNavigator is empty!");
            }
        }
    }
}
