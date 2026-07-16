using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRLearning.Parts
{
    /// <summary>
    /// Makes the anatomy parts pick-up-able. On entering an anatomy scene, every <see cref="PartInfo"/>-tagged
    /// object gets a gravity-free kinematic <see cref="Rigidbody"/> + an <see cref="XRGrabInteractable"/>, so a
    /// learner can grab a part and leave it floating anywhere — it stays exactly where released (no gravity, no
    /// drift, no throw). Because <see cref="PartInfo"/> auto-hooks any XR interactable, grabbing a part also
    /// highlights it and drives the info panel for free.
    ///
    /// Scoped to the anatomy scenes only; Simple Machines (own grabbables) and the rig-animated Breathing lungs
    /// are left untouched. Lives on the persistent Managers object and re-runs on every scene load.
    /// </summary>
    public class GrabbablePartsSetup : MonoBehaviour
    {
        static readonly HashSet<string> AnatomyScenes = new()
        {
            "BloodCirc_HeartPump", "BloodCirc_ArterialFlow", "BloodCirc_Capillary", "Breathing_GasExchange"
        };

        void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // The scene already open when play starts doesn't raise sceneLoaded — handle it here.
        void Start() => SetupIfAnatomy(SceneManager.GetActiveScene());

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SetupIfAnatomy(scene);

        void SetupIfAnatomy(Scene scene)
        {
            if (!AnatomyScenes.Contains(scene.name)) return;
            int done = 0;
            foreach (var part in Object.FindObjectsByType<PartInfo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (MakeGrabbable(part.gameObject)) done++;
            VRLearning.Core.VRLog.Info("Parts", $"{scene.name}: made {done} part(s) grabbable (no gravity).");
        }

        static bool MakeGrabbable(GameObject go)
        {
            if (go.GetComponent<XRGrabInteractable>() != null) return false; // already set up

            if (go.GetComponent<Collider>() == null)
                go.AddComponent<BoxCollider>(); // safety — parts already carry a BoxCollider

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;               // floats and stays where released
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var grab = go.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;          // no fling on release
            grab.useDynamicAttach = true;        // grab from where you touch it

            // PartInfo's own OnEnable already ran (and cached a null interactable) before this
            // method runs, since it's called from SceneManager.sceneLoaded — fires after every
            // scene object's OnEnable. Re-bind now that the interactable actually exists.
            go.GetComponent<PartInfo>()?.RefreshInteractable();

            if (go.GetComponent<PartSocket>() == null) go.AddComponent<PartSocket>();
            go.GetComponent<PartSocket>().RefreshInteractable();

            return true;
        }
    }
}
