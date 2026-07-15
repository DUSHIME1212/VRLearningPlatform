using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a visible rope through the segments spawned by <see cref="Wire"/>. Wire.cs creates
/// collider-only link GameObjects ("WireSeg_*") with no mesh, so on its own the simulated rope
/// is invisible. This helper traces a LineRenderer through those live segment positions every
/// LateUpdate (after physics has moved them), optionally pinned to the same Start / End points.
/// It reads the segments from the hierarchy — no change to Wire.cs is needed.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class WireLineRenderer : MonoBehaviour
{
    [Tooltip("The 'Segments' container Wire spawns links under. Defaults to this object.")]
    [SerializeField] private Transform segmentsParent;
    [Tooltip("Optional: draw the rope from this point (the top hook) through the segments.")]
    [SerializeField] private Transform start;
    [Tooltip("Optional: end the rope at this point (the load).")]
    [SerializeField] private Transform end;
    [SerializeField] private string segmentPrefix = "WireSeg_";

    private LineRenderer _line;
    private readonly List<Transform> _segments = new();
    private int _lastChildCount = -1;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
    }

    private void LateUpdate()
    {
        Transform parent = segmentsParent != null ? segmentsParent : transform;
        if (parent == null) return;

        // Re-collect segments only when the child set changes (Wire rebuild / first frame).
        if (parent.childCount != _lastChildCount)
        {
            RebuildSegmentList(parent);
            _lastChildCount = parent.childCount;
        }

        int count = _segments.Count + (start != null ? 1 : 0) + (end != null ? 1 : 0);
        if (_segments.Count == 0) { _line.positionCount = 0; return; }

        _line.positionCount = count;
        int i = 0;
        if (start != null) _line.SetPosition(i++, start.position);
        foreach (var seg in _segments)
            if (seg != null) _line.SetPosition(i++, seg.position);
        if (end != null && i < count) _line.SetPosition(i, end.position);
    }

    // Collect "WireSeg_N" children and order them by their numeric suffix (spawn order).
    private void RebuildSegmentList(Transform parent)
    {
        _segments.Clear();
        for (int c = 0; c < parent.childCount; c++)
        {
            Transform child = parent.GetChild(c);
            if (child.name.StartsWith(segmentPrefix)) _segments.Add(child);
        }
        _segments.Sort((a, b) => SegIndex(a).CompareTo(SegIndex(b)));
    }

    private int SegIndex(Transform t)
    {
        return int.TryParse(t.name.Substring(segmentPrefix.Length), out int n) ? n : 0;
    }
}
