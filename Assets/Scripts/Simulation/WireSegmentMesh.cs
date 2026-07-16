using UnityEngine;

/// <summary>
/// Gives each segment spawned by Wire.cs a small 3D tube mesh (cylinder or sphere, matching
/// whichever collider shape Wire.cs already configured) instead of drawing the rope as a flat
/// LineRenderer strip. Reads each segment's EXISTING Collider for sizing — Wire.cs itself is
/// untouched. Real geometry gives the rope correct volume/shading from any VR viewing angle,
/// unlike a camera-facing LineRenderer strip.
/// </summary>
public class WireSegmentMesh : MonoBehaviour
{
    [Tooltip("The 'Segments' container Wire spawns links under. Defaults to this object.")]
    [SerializeField] private Transform segmentsParent;
    [SerializeField] private string segmentPrefix = "WireSeg_";
    [Tooltip("Tint for this rope's material (a plain lit color, no texture).")]
    [SerializeField] private Color ropeColor = new Color(0.25f, 0.18f, 0.12f);

    private const string VisualChildName = "Visual";
    private int _lastChildCount = -1;
    private Material _material;

    private static Mesh _cylinderMesh;
    private static Mesh _sphereMesh;

    private void LateUpdate()
    {
        Transform parent = segmentsParent != null ? segmentsParent : transform;
        if (parent == null) return;

        // Only (re)build visuals when the segment set actually changes (Wire rebuild / first frame).
        if (parent.childCount == _lastChildCount) return;
        _lastChildCount = parent.childCount;

        EnsureSharedMeshes();
        EnsureMaterial();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform seg = parent.GetChild(i);
            if (seg.name.StartsWith(segmentPrefix)) BuildVisualFor(seg);
        }
    }

    private void BuildVisualFor(Transform segment)
    {
        Transform visual = segment.Find(VisualChildName);
        if (visual == null)
        {
            var go = new GameObject(VisualChildName);
            go.transform.SetParent(segment, false);
            go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            visual = go.transform;
        }

        var meshFilter = visual.GetComponent<MeshFilter>();
        visual.GetComponent<MeshRenderer>().sharedMaterial = _material;

        var capsule = segment.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            // Native cylinder mesh: height 2 along local Y, diameter 1 (radius 0.5) in X/Z.
            // Rotate Y onto local Z to match the CapsuleCollider's direction=2 (Z) axis, which
            // is also the segment's own "along rope" orientation set up by Wire.cs.
            meshFilter.sharedMesh = _cylinderMesh;
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.localScale = new Vector3(capsule.radius * 2f, capsule.height * 0.5f, capsule.radius * 2f);
            return;
        }

        var sphere = segment.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            meshFilter.sharedMesh = _sphereMesh;
            float d = sphere.radius * 2f;
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(d, d, d);
        }
    }

    private static void EnsureSharedMeshes()
    {
        if (_cylinderMesh == null) _cylinderMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        if (_sphereMesh == null) _sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
    }

    private void EnsureMaterial()
    {
        if (_material != null) return;
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        _material = new Material(shader) { color = ropeColor };
    }
}
