using UnityEngine;

namespace VRLearning.Simulation.SimpleMachines
{
    public class ForceArrow : MonoBehaviour
    {
        [SerializeField] private Transform arrowShaft;
        [SerializeField] private Transform arrowHead;
        [SerializeField] private float minScale = 0.1f;
        [SerializeField] private float maxScale = 1.0f;
        [SerializeField] private Gradient colorGradient;

        private MeshRenderer _shaftRenderer;
        private MeshRenderer _headRenderer;

        private void Awake()
        {
            if (arrowShaft != null)  _shaftRenderer = arrowShaft.GetComponent<MeshRenderer>();
            if (arrowHead != null)   _headRenderer  = arrowHead.GetComponent<MeshRenderer>();
        }

        public void SetForce(float normalizedForce)
        {
            normalizedForce = Mathf.Clamp01(normalizedForce);
            float scale = Mathf.Lerp(minScale, maxScale, normalizedForce);

            if (arrowShaft != null)
            {
                Vector3 s = arrowShaft.localScale;
                arrowShaft.localScale = new Vector3(s.x, scale, s.z);
            }

            Color c = colorGradient.Evaluate(normalizedForce);
            if (_shaftRenderer != null) _shaftRenderer.material.color = c;
            if (_headRenderer  != null) _headRenderer.material.color  = c;

            gameObject.SetActive(normalizedForce > 0.01f);
        }

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);
    }
}
