using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Visualises alveolar gas exchange: the air sacs (alveoli) gently inflate and
    /// deflate on a breathing cycle while O2 molecules diffuse from the air into the
    /// blood and CO2 molecules diffuse from the blood back into the air.
    ///
    /// Purely transform-based (no rigidbodies) for VR performance. Particles are
    /// pooled and recycled, never destroyed. If references are left unassigned the
    /// component builds sensible fallbacks from its own children, so it can be wired
    /// with a single AddComponent.
    /// </summary>
    public class GasExchangeSimulator : MonoBehaviour
    {
        [Header("Alveoli (air sacs)")]
        [Tooltip("Sacs that pulse with the breathing cycle. If empty, direct children are used.")]
        [SerializeField] private Transform[] alveoli;
        [SerializeField] private float inflateScale = 1.18f;

        [Header("Breathing cycle")]
        [SerializeField] private float breathsPerMinute = 15f;
        [SerializeField] private float inhaleFraction   = 0.4f;

        [Header("Diffusion")]
        [Tooltip("Local offset (from this transform) of the blood/capillary side that gases diffuse to and from.")]
        [SerializeField] private Vector3 bloodSideOffset = new Vector3(0f, -0.55f, 0f);
        [SerializeField] private int     o2Count         = 10;
        [SerializeField] private int     co2Count        = 8;
        [SerializeField] private float   travelSpeed     = 0.35f;   // metres / second
        [SerializeField] private float   particleScale   = 0.045f;
        [SerializeField] private Color   o2Color         = new Color(0.95f, 0.25f, 0.2f);   // O2 = red (oxygen-rich)
        [SerializeField] private Color   co2Color        = new Color(0.35f, 0.55f, 0.95f);  // CO2 = blue (waste)

        [Header("Labels (optional)")]
        [SerializeField] private SimpleMachines.MachineLabel phaseLabel;
        [SerializeField] private SimpleMachines.MachineLabel rateLabel;

        private struct Mover
        {
            public Transform tr;
            public Vector3   from;
            public Vector3   to;
            public float     t;
        }

        private Vector3[] _alveoliRest;
        private Mover[]   _o2;
        private Mover[]   _co2;
        private Material  _o2Mat, _co2Mat;
        private Transform _pool;

        private float _cycleTimer, _cycleDuration;

        private Vector3 BloodPoint => transform.TransformPoint(bloodSideOffset);

        private void Start()
        {
            // Fallback: pulse direct children if no alveoli were assigned.
            if (alveoli == null || alveoli.Length == 0)
            {
                alveoli = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                    alveoli[i] = transform.GetChild(i);
            }

            _alveoliRest = new Vector3[alveoli.Length];
            for (int i = 0; i < alveoli.Length; i++)
                if (alveoli[i] != null)
                {
                    if (!alveoli[i].gameObject.activeSelf) alveoli[i].gameObject.SetActive(true);
                    _alveoliRest[i] = alveoli[i].localScale;
                }

            _o2Mat  = MakeUnlit(o2Color);
            _co2Mat = MakeUnlit(co2Color);

            var poolGo = new GameObject("GasParticles");
            _pool = poolGo.transform;
            _pool.SetParent(transform, false);

            _o2  = BuildPool(o2Count,  _o2Mat,  fromAir: true);
            _co2 = BuildPool(co2Count, _co2Mat, fromAir: false);

            SetBreathRate(breathsPerMinute);
        }

        public void SetBreathRate(float bpm)
        {
            breathsPerMinute = Mathf.Clamp(bpm, 4f, 60f);
            _cycleDuration   = 60f / breathsPerMinute;
            rateLabel?.SetOverrideText($"{Mathf.RoundToInt(breathsPerMinute)} breaths/min");
        }

        private Mover[] BuildPool(int count, Material mat, bool fromAir)
        {
            var pool = new Mover[count];
            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = fromAir ? "O2" : "CO2";
                var col = go.GetComponent<Collider>();
                if (col) Destroy(col);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                go.transform.SetParent(_pool, false);
                go.transform.localScale = Vector3.one * particleScale;

                pool[i].tr = go.transform;
                ResetMover(ref pool[i], fromAir, Random.value); // stagger start phase
            }
            return pool;
        }

        private void ResetMover(ref Mover m, bool fromAir, float startT)
        {
            Vector3 air   = RandomAlveolusPoint();
            Vector3 blood = BloodPoint + Random.insideUnitSphere * 0.08f;
            m.from = fromAir ? air   : blood;
            m.to   = fromAir ? blood : air;
            m.t    = startT;
            m.tr.position = Vector3.Lerp(m.from, m.to, m.t);
        }

        private Vector3 RandomAlveolusPoint()
        {
            if (alveoli != null && alveoli.Length > 0)
            {
                var a = alveoli[Random.Range(0, alveoli.Length)];
                if (a != null) return a.position + Random.insideUnitSphere * 0.06f;
            }
            return transform.position + Random.insideUnitSphere * 0.1f;
        }

        private void Update()
        {
            // ── Breathing cycle drives alveoli pulse + phase label ──
            _cycleTimer += Time.deltaTime;
            float inhaleDur = _cycleDuration * inhaleFraction;
            float blend;
            if (_cycleTimer < inhaleDur)
            {
                blend = Mathf.SmoothStep(0f, 1f, _cycleTimer / inhaleDur);
                phaseLabel?.SetOverrideText("Inhale  ·  O₂ in");
            }
            else if (_cycleTimer < _cycleDuration)
            {
                blend = 1f - Mathf.SmoothStep(0f, 1f, (_cycleTimer - inhaleDur) / (_cycleDuration - inhaleDur));
                phaseLabel?.SetOverrideText("Exhale  ·  CO₂ out");
            }
            else
            {
                _cycleTimer = 0f;
                blend = 0f;
            }

            for (int i = 0; i < alveoli.Length; i++)
                if (alveoli[i] != null)
                    alveoli[i].localScale = Vector3.Lerp(_alveoliRest[i], _alveoliRest[i] * inflateScale, blend);

            // ── Diffusing gases ──
            AdvancePool(_o2,  fromAir: true);
            AdvancePool(_co2, fromAir: false);
        }

        private void AdvancePool(Mover[] pool, bool fromAir)
        {
            for (int i = 0; i < pool.Length; i++)
            {
                ref var m = ref pool[i];
                if (m.tr == null) continue;

                float seg = Mathf.Max(Vector3.Distance(m.from, m.to), 0.01f);
                m.t += (travelSpeed / seg) * Time.deltaTime;

                if (m.t >= 1f)
                {
                    ResetMover(ref m, fromAir, 0f); // recycle: pick a fresh sac / blood spot
                    continue;
                }
                m.tr.position = Vector3.Lerp(m.from, m.to, m.t);
            }
        }

        private static Material MakeUnlit(Color c)
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit")
                     ?? Shader.Find("Unlit/Color")
                     ?? Shader.Find("Sprites/Default");
            return new Material(sh) { color = c };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = co2Color;
            Gizmos.DrawWireSphere(BloodPoint, 0.1f);
        }
    }
}
