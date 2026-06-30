using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Spawns particles that flow along a series of waypoints (blood vessel path, river, etc.).
    /// Particles despawn at the last waypoint and optionally loop back to the first.
    /// No physics rigidbodies — purely transform-based for performance.
    /// </summary>
    public class FluidParticleFlow : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private bool        looping = true;

        [Header("Particles")]
        [SerializeField] private GameObject  particlePrefab;
        [SerializeField] private int         maxParticles  = 20;
        [SerializeField] private float       spawnInterval = 0.15f;
        [SerializeField] private float       particleSpeed = 0.8f;
        [SerializeField] private float       particleScale = 0.06f;
        [SerializeField] private Color       particleColor = new Color(0.8f, 0.1f, 0.1f);

        private struct FlowParticle
        {
            public GameObject go;
            public int        waypointIndex;
            public float      t;
        }

        private FlowParticle[] _particles;
        private int            _activeCount;
        private float          _spawnTimer;
        private int            _nextSpawnWaypoint;

        private void Start()
        {
            // Fallback: if no waypoints were assigned, treat direct children as the path (in order).
            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                    waypoints[i] = transform.GetChild(i);
            }

            _particles = new FlowParticle[maxParticles];
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            // Spawn
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval && _activeCount < maxParticles)
            {
                _spawnTimer = 0f;
                SpawnParticle();
            }

            // Move
            for (int i = 0; i < _activeCount; i++)
            {
                ref var p = ref _particles[i];
                if (p.go == null) continue;

                int next = p.waypointIndex + 1;
                if (next >= waypoints.Length)
                {
                    if (looping)
                    {
                        // Recycle the particle back to the start for a continuous stream
                        // (no allocation, no orphaned GameObjects).
                        p.waypointIndex = 0;
                        p.t             = 0f;
                        p.go.transform.position = waypoints[0].position;
                    }
                    else
                    {
                        Destroy(p.go);
                        RemoveAt(i--);
                    }
                    continue;
                }

                Vector3 from = waypoints[p.waypointIndex].position;
                Vector3 to   = waypoints[next].position;
                float   segLen = Vector3.Distance(from, to);
                p.t += (particleSpeed / Mathf.Max(segLen, 0.01f)) * Time.deltaTime;

                if (p.t >= 1f)
                {
                    p.waypointIndex = next;
                    p.t             = 0f;
                }

                p.go.transform.position = Vector3.Lerp(from, to, p.t);
                p.go.transform.LookAt(to);
            }
        }

        private void SpawnParticle()
        {
            GameObject go = particlePrefab != null
                ? Instantiate(particlePrefab)
                : CreateDefaultParticle();

            go.transform.localScale = Vector3.one * particleScale;
            go.SetActive(true);

            _particles[_activeCount++] = new FlowParticle
            {
                go             = go,
                waypointIndex  = 0,
                t              = 0f
            };
        }

        private GameObject CreateDefaultParticle()
        {
            var go   = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var coll = go.GetComponent<Collider>();
            if (coll) coll.enabled = false;
            var mat = go.GetComponent<MeshRenderer>().material;
            mat.color = particleColor;
            return go;
        }

        private void RemoveAt(int i)
        {
            _particles[i] = _particles[--_activeCount];
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null) return;
            Gizmos.color = particleColor;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] && waypoints[i + 1])
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
