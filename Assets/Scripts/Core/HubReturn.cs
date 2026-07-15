using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace VRLearning.Core
{
    /// <summary>
    /// Universal "back to the hub" for VR. While inside an experiment scene (path contains
    /// "/Experiments/"), pressing the left controller's menu button — or Escape in the Editor — returns
    /// to the hub scene through the async, memory-safe <see cref="UI.SceneLoadRunner"/>. Lives on the
    /// persistent Managers object, so it works in every experiment without adding a button to each scene.
    /// Any world-space UI back button can also just call <see cref="ReturnToHub"/>.
    /// </summary>
    public class HubReturn : MonoBehaviour
    {
        [SerializeField] private string hubScene = "BasicScene";
        private InputAction _back;

        private void OnEnable()
        {
            _back = new InputAction("HubReturn", InputActionType.Button);
            _back.AddBinding("<XRController>{LeftHand}/menuButton");
            _back.AddBinding("<Keyboard>/escape");
            _back.performed += OnBack;
            _back.Enable();
        }

        private void OnDisable()
        {
            if (_back == null) return;
            _back.performed -= OnBack;
            _back.Disable();
            _back.Dispose();
            _back = null;
        }

        private void OnBack(InputAction.CallbackContext _)
        {
            if (SceneManager.GetActiveScene().path.Contains("/Experiments/"))
                ReturnToHub();
        }

        /// <summary>Load the hub scene (unless already there). Safe to wire to a UI Button.</summary>
        public void ReturnToHub()
        {
            if (SceneManager.GetActiveScene().name == hubScene) return;
            VRLog.Info("HubReturn", $"returning to '{hubScene}'");
            UI.SceneLoadRunner.Load(hubScene);
        }
    }
}