using UnityEngine;

/// <summary>
/// Wire.cs — a physically-simulated rope / wire / cable built from a chain of Rigidbody
/// segments joined by ConfigurableJoints (built-in PhysX; no DOTS/Havok).
///
/// USE CASES: hanging cables, elevator / lift wires, a cord connecting two objects.
///
/// ─────────────────────────────  SCENE SETUP  ─────────────────────────────
///  1. Make two empty GameObjects, "Start" and "End", and place them where the wire
///     should attach (e.g. the top hook and the bottom weight). They may be children
///     of moving objects — the wire will follow them.
///  2. Make one more empty GameObject, "Segments", to hold the spawned links tidy.
///  3. Add this Wire component to any GameObject and drag Start / End / Segments into
///     the matching fields. Set segmentCount, totalLength, totalWeight, etc.
///  4. Press Play — the wire generates and simulates. While playing you can tweak the
///     Inspector values and the wire updates live. In Edit mode, use the component's
///     context-menu ▸ "Rebuild Wire" to (re)build it, and the Scene-view gizmos to preview.
///
///  Notes:
///   • The FIRST segment is a kinematic anchor pinned to Start; the LAST segment is pinned
///     to End (a "close anchor"). If End should physically pull the rope while moving, give
///     the End object a Rigidbody — otherwise the wire still follows End's transform.
///   • usePhysics = false spawns only the placed segment GameObjects (no Rigidbody/collider),
///     handy for previewing spacing with gizmos before enabling simulation.
///   • Unity 6 uses Rigidbody.linearDamping / angularDamping (below). On Unity 2022.3 LTS or
///     older, replace those two with rb.drag / rb.angularDrag.
/// </summary>
[DisallowMultipleComponent]
public class Wire : MonoBehaviour
{
    public enum ColliderShape { Sphere, Capsule }

    // ─────────────────────────────  INSPECTOR FIELDS  ─────────────────────────────

    [Header("Endpoints")]
    [Tooltip("Top / first attachment point. The first segment is pinned here.")]
    [SerializeField] private Transform start;
    [Tooltip("Bottom / second attachment point. The last segment is pinned here.")]
    [SerializeField] private Transform end;
    [Tooltip("Container the spawned segment GameObjects are parented under.")]
    [SerializeField] private Transform segmentsParent;

    [Header("Rope Shape")]
    [Tooltip("Number of links in the chain.")]
    [SerializeField, Min(2)] private int segmentCount = 10;
    [Tooltip("Rest length of the whole rope (drives joint spacing = totalLength / segmentCount).")]
    [SerializeField, Min(0.01f)] private float totalLength = 10f;
    [Tooltip("Total mass shared across all segments (per-segment mass = totalWeight / segmentCount).")]
    [SerializeField, Min(0.001f)] private float totalWeight = 10f;

    [Header("Physics")]
    [Tooltip("When true, segments get a Rigidbody, ConfigurableJoint and a Collider (full simulation). " +
             "When false, only the placeholder segment objects are spawned (preview only).")]
    [SerializeField] private bool usePhysics = true;
    [Tooltip("Collider radius (and capsule radius) for each segment.")]
    [SerializeField, Min(0.001f)] private float radius = 0.1f;
    [Tooltip("Rigidbody linear damping (Unity 6: linearDamping / old: drag).")]
    [SerializeField, Min(0f)] private float drag = 0.5f;
    [Tooltip("Rigidbody angular damping (Unity 6: angularDamping / old: angularDrag).")]
    [SerializeField, Min(0f)] private float angularDrag = 1f;
    [Tooltip("Max bend (degrees) allowed at each joint. Small values (5–15°) let the rope curve " +
             "smoothly across many links without kinking or twisting into itself.")]
    [SerializeField, Range(1f, 45f)] private float swingLimitAngle = 8f;
    [Tooltip("Collider shape per segment.")]
    [SerializeField] private ColliderShape colliderShape = ColliderShape.Capsule;

    // ─────────────────────────────  RUNTIME STATE  ─────────────────────────────

    private const string SegmentPrefix = "WireSeg_";

    private Transform[] _segments;                 // spawned link transforms
    private Rigidbody[] _bodies;                    // per-segment Rigidbody (null when usePhysics == false)
    private ConfigurableJoint[] _chainJoints;       // joint on segment i linking it to segment i-1 (i >= 1)
    private ConfigurableJoint _endJoint;            // extra joint pinning the last segment to End

    // Cached "previous" values so Update() only does work when something actually changed.
    private int _prevSegmentCount = -1;
    private float _prevTotalLength, _prevTotalWeight, _prevDrag, _prevAngularDrag, _prevRadius, _prevSwingLimit;
    private bool _prevUsePhysics;
    private ColliderShape _prevColliderShape;
    private Transform _prevStart, _prevEnd, _prevParent;

    // ─────────────────────────────  LIFECYCLE  ─────────────────────────────

    private void Start()
    {
        Rebuild();
    }

    /// <summary>
    /// Runtime editing: rebuild on structural changes (segment count, physics toggle, endpoint refs),
    /// otherwise update the existing segments in place. Diffs cached values so nothing runs every frame.
    /// </summary>
    private void Update()
    {
        if (!Application.isPlaying) return;               // live edits handled in Play mode; edit mode uses the context menu
        if (start == null || end == null) return;         // silently wait until endpoints are assigned
        if (_segments == null) { Rebuild(); return; }

        bool structuralChange =
            segmentCount != _prevSegmentCount ||
            usePhysics   != _prevUsePhysics ||
            colliderShape != _prevColliderShape ||
            start != _prevStart || end != _prevEnd || segmentsParent != _prevParent;

        if (structuralChange)
        {
            Rebuild();
            return;
        }

        bool valueChange =
            !Mathf.Approximately(totalLength, _prevTotalLength) ||
            !Mathf.Approximately(totalWeight, _prevTotalWeight) ||
            !Mathf.Approximately(drag, _prevDrag) ||
            !Mathf.Approximately(angularDrag, _prevAngularDrag) ||
            !Mathf.Approximately(radius, _prevRadius) ||
            !Mathf.Approximately(swingLimitAngle, _prevSwingLimit);

        if (valueChange)
        {
            UpdateInPlace();
            CacheValues();
        }
    }

    /// <summary>Keeps the kinematic end anchors following the Start / End transforms.</summary>
    private void FixedUpdate()
    {
        if (!Application.isPlaying || _bodies == null || _bodies.Length == 0) return;

        // First segment is the kinematic top anchor — glue it to Start every physics step.
        if (_bodies[0] != null && start != null)
        {
            _bodies[0].MovePosition(start.position);
            _bodies[0].MoveRotation(start.rotation);
        }

        // If End has no Rigidbody, keep the world-space end anchor on the (possibly moving) End transform.
        if (_endJoint != null && _endJoint.connectedBody == null && end != null)
            _endJoint.connectedAnchor = end.position;
    }

    // ─────────────────────────────  BUILD / REBUILD  ─────────────────────────────

    [ContextMenu("Rebuild Wire")]
    public void Rebuild()
    {
        if (start == null || end == null)
        {
            Debug.LogWarning("[Wire] Start and End must both be assigned before building.", this);
            return;
        }

        ClearSegments();       // remove any previously spawned links (no duplicate components)
        GenerateSegments();    // 1–2. spawn evenly spaced empties, store in the Transform[] array
        if (usePhysics)
            SetupPhysics();    // 3–6. Rigidbody + ConfigurableJoint + Collider per segment
        CacheValues();
    }

    [ContextMenu("Clear Wire")]
    public void ClearSegments()
    {
        // Destroy the links we spawned last time.
        if (_segments != null)
            foreach (var seg in _segments)
                if (seg != null) DestroyObject(seg.gameObject);

        // Safety sweep: remove any leftover "WireSeg_" children under the parent (e.g. from a prior session).
        Transform parent = segmentsParent != null ? segmentsParent : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name.StartsWith(SegmentPrefix)) DestroyObject(child.gameObject);
        }

        _segments = null;
        _bodies = null;
        _chainJoints = null;
        _endJoint = null;
    }

    // 1–2. GENERATION: spawn `segmentCount` empties evenly along Start→End, store them in an array.
    private void GenerateSegments()
    {
        Transform parent = segmentsParent != null ? segmentsParent : transform;
        int count = Mathf.Max(2, segmentCount);

        Vector3 direction = end.position - start.position;
        Vector3 forward = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.down;
        // Orient each link so its local +Z runs down the rope — makes joint anchors and capsule
        // colliders line up along the wire. Pick a safe up vector to avoid LookRotation singularities.
        Quaternion rotation = Quaternion.LookRotation(forward, SafeUp(forward));

        _segments = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);                        // 0 at Start … 1 at End
            Vector3 position = Vector3.Lerp(start.position, end.position, t);

            var go = new GameObject(SegmentPrefix + i);
            go.transform.SetParent(parent, worldPositionStays: true);
            go.transform.SetPositionAndRotation(position, rotation);
            _segments[i] = go.transform;
        }
    }

    // 3–6. PHYSICS: Rigidbody + ConfigurableJoint chain + Collider on each segment.
    private void SetupPhysics()
    {
        int count = _segments.Length;
        float spacing = totalLength / count;                          // rest distance between segments
        float segmentMass = totalWeight / count;

        _bodies = new Rigidbody[count];
        _chainJoints = new ConfigurableJoint[count];

        // --- Rigidbodies + colliders ---
        for (int i = 0; i < count; i++)
        {
            GameObject go = _segments[i].gameObject;

            Rigidbody rb = GetOrAdd<Rigidbody>(go);
            rb.mass = segmentMass;
            rb.linearDamping = drag;            // Unity 6 name; older Unity: rb.drag
            rb.angularDamping = angularDrag;    // Unity 6 name; older Unity: rb.angularDrag
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _bodies[i] = rb;

            AddOrUpdateCollider(go, spacing);
        }

        // First segment is the fixed / kinematic top anchor (pinned to Start in FixedUpdate).
        _bodies[0].isKinematic = true;

        // --- Chain joints: each segment i (i >= 1) is joined to the previous one ---
        for (int i = 1; i < count; i++)
        {
            var joint = GetOrAdd<ConfigurableJoint>(_segments[i].gameObject);
            // connectedAnchor lives in the PREVIOUS segment's local space; +Z * spacing puts this
            // segment one rest-length "down the rope" from the previous one.
            ConfigureJoint(joint, _bodies[i - 1], new Vector3(0f, 0f, spacing));
            _chainJoints[i] = joint;
        }

        // --- End attachment: pin the LAST segment to End with a "close" anchor ---
        Rigidbody last = _bodies[count - 1];
        Rigidbody endBody = end.GetComponent<Rigidbody>();
        _endJoint = last.gameObject.AddComponent<ConfigurableJoint>();   // AddComponent: this is a 2nd joint on the last link
        if (endBody != null)
        {
            // Connect straight to End's body; the close anchor is End's local offset to the last segment.
            ConfigureJoint(_endJoint, endBody, end.InverseTransformPoint(last.position));
        }
        else
        {
            // No Rigidbody on End → anchor to End's world position (kept in sync in FixedUpdate).
            ConfigureJoint(_endJoint, null, end.position);
        }

        // Stop adjacent links from colliding with each other (avoids constant jitter).
        for (int i = 1; i < count; i++)
        {
            Collider a = _segments[i - 1].GetComponent<Collider>();
            Collider b = _segments[i].GetComponent<Collider>();
            if (a != null && b != null) Physics.IgnoreCollision(a, b, true);
        }
    }

    // JOINT SETUP: locked linear motion, free twist, small limited swing, no drive springs.
    private void ConfigureJoint(ConfigurableJoint joint, Rigidbody connectedBody, Vector3 connectedAnchor)
    {
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = connectedBody;
        joint.anchor = Vector3.zero;                 // this segment's centre
        joint.connectedAnchor = connectedAnchor;     // manual: spacing offset (or close anchor for End)

        // Linear motion fully locked — segments keep their spacing rather than stretching apart.
        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;

        // Angular motion: twist free, swing limited to a small angle so the rope bends smoothly.
        joint.angularXMotion = ConfigurableJointMotion.Free;      // twist
        joint.angularYMotion = ConfigurableJointMotion.Limited;   // swing 1
        joint.angularZMotion = ConfigurableJointMotion.Limited;   // swing 2
        var swing = new SoftJointLimit { limit = swingLimitAngle };
        joint.angularYLimit = swing;
        joint.angularZLimit = swing;

        // No drive springs/dampers → segments swing freely under physics, not toward a target pose.
        var noDrive = new JointDrive { positionSpring = 0f, positionDamper = 0f, maximumForce = 0f };
        joint.xDrive = joint.yDrive = joint.zDrive = noDrive;
        joint.angularXDrive = joint.angularYZDrive = joint.slerpDrive = noDrive;

        // Stability: preprocessing off + light projection keeps long ropes from exploding / over-stretching.
        joint.enablePreprocessing = false;
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.05f;
        joint.projectionAngle = 5f;
    }

    // ─────────────────────────────  IN-PLACE UPDATE (no regeneration)  ─────────────────────────────

    // Applies changed mass / damping / spacing / radius / swing to the existing segments.
    private void UpdateInPlace()
    {
        if (_segments == null || _bodies == null) return;

        int count = _segments.Length;
        float spacing = totalLength / count;
        float segmentMass = totalWeight / count;
        var swing = new SoftJointLimit { limit = swingLimitAngle };

        for (int i = 0; i < count; i++)
        {
            Rigidbody rb = _bodies[i];
            if (rb != null && !rb.isKinematic)   // skip the pinned anchors
            {
                rb.mass = segmentMass;
                rb.linearDamping = drag;
                rb.angularDamping = angularDrag;
            }

            if (_segments[i] != null)
                AddOrUpdateCollider(_segments[i].gameObject, spacing);

            ConfigurableJoint chain = (_chainJoints != null && i < _chainJoints.Length) ? _chainJoints[i] : null;
            if (chain != null)
            {
                chain.connectedAnchor = new Vector3(0f, 0f, spacing);
                chain.angularYLimit = swing;
                chain.angularZLimit = swing;
            }
        }

        if (_endJoint != null)
        {
            _endJoint.angularYLimit = swing;
            _endJoint.angularZLimit = swing;
        }
    }

    // ─────────────────────────────  COLLIDERS  ─────────────────────────────

    // Adds (or resizes) the chosen collider, removing the other shape so we never leave a duplicate.
    private void AddOrUpdateCollider(GameObject go, float spacing)
    {
        if (colliderShape == ColliderShape.Sphere)
        {
            RemoveComponent<CapsuleCollider>(go);
            var sphere = GetOrAdd<SphereCollider>(go);
            sphere.radius = radius;
            sphere.center = Vector3.zero;
        }
        else
        {
            RemoveComponent<SphereCollider>(go);
            var capsule = GetOrAdd<CapsuleCollider>(go);
            capsule.radius = radius;
            capsule.height = Mathf.Max(spacing, radius * 2f);
            capsule.direction = 2;      // 2 = Z axis, i.e. along the rope
            capsule.center = Vector3.zero;
        }
    }

    // ─────────────────────────────  GIZMOS (preview, even with no physics)  ─────────────────────────────

    private void OnDrawGizmos()
    {
        if (start == null || end == null) return;

        int count = Mathf.Max(2, segmentCount);
        float size = Mathf.Max(0.02f, radius);
        Gizmos.color = Color.cyan;

        // If links are already spawned, draw them where they actually are; otherwise preview the layout.
        if (_segments != null && _segments.Length == count)
        {
            for (int i = 0; i < count; i++)
                if (_segments[i] != null) Gizmos.DrawWireCube(_segments[i].position, Vector3.one * size);
        }
        else
        {
            Vector3 previous = start.position;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = Vector3.Lerp(start.position, end.position, i / (float)(count - 1));
                Gizmos.DrawWireCube(p, Vector3.one * size);
                Gizmos.DrawLine(previous, p);
                previous = p;
            }
        }
    }

    // ─────────────────────────────  HELPERS  ─────────────────────────────

    private void CacheValues()
    {
        _prevSegmentCount = segmentCount;
        _prevTotalLength = totalLength;
        _prevTotalWeight = totalWeight;
        _prevDrag = drag;
        _prevAngularDrag = angularDrag;
        _prevRadius = radius;
        _prevSwingLimit = swingLimitAngle;
        _prevUsePhysics = usePhysics;
        _prevColliderShape = colliderShape;
        _prevStart = start;
        _prevEnd = end;
        _prevParent = segmentsParent;
    }

    // Returns an existing component or adds one — guards against duplicate-component errors on rebuild.
    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void RemoveComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null) return;
        if (Application.isPlaying) Destroy(component);
        else DestroyImmediate(component);
    }

    private static void DestroyObject(GameObject go)
    {
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    // A safe "up" for LookRotation when the rope runs (nearly) vertical.
    private static Vector3 SafeUp(Vector3 forward)
        => Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
}
