using UnityEngine;
using UnityEngine.UI;

namespace VRLearning.UI
{
    /// <summary>
    /// Rounds the corners of a UIGlass-shaded panel. UGUI's Graphic has no per-instance
    /// MaterialPropertyBlock API (unlike MeshRenderer), so this clones the assigned material once
    /// and pushes this panel's own RectTransform size into it — lets one shared UIGlass.mat asset
    /// still look correctly rounded on panels of very different sizes (e.g. a 440x320 whiteboard
    /// vs a 1200x900 menu panel) without a fixed radius looking wrong on one of them.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class UIGlassRounder : MonoBehaviour
    {
        [SerializeField] private float cornerRadiusPx = 24f;

        private Graphic _graphic;
        private Material _instance;
        private RectTransform _rt;

        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");
        private static readonly int RadiusID = Shader.PropertyToID("_CornerRadiusPx");

        private void OnEnable()
        {
            _graphic = GetComponent<Graphic>();
            _rt = (RectTransform)transform;
            if (_instance == null && _graphic.material != null)
                _instance = Instantiate(_graphic.material);
            if (_instance != null)
                _graphic.material = _instance;
            Push();
        }

        private void OnRectTransformDimensionsChange() => Push();

        private void Push()
        {
            if (_instance == null || _rt == null) return;
            _instance.SetVector(RectSizeID, _rt.rect.size);
            _instance.SetFloat(RadiusID, cornerRadiusPx);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            if (_rt == null) _rt = (RectTransform)transform;
            Push();
        }
#endif
    }
}
