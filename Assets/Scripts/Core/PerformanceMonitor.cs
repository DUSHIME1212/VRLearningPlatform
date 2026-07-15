using UnityEngine;
using UnityEngine.Profiling;

namespace VRLearning.Core
{
    /// <summary>
    /// Console-only performance sampler for VR profiling. Every <see cref="sampleSeconds"/> it logs one
    /// line with FPS, frame time, memory and GC activity, plus a warning on any single frame that
    /// exceeds <see cref="spikeMultiplier"/>× the running-average frame time. Watching the <c>mem=</c>
    /// value climb is the earliest warning the app is heading toward an out-of-memory kill on device.
    ///
    /// Only runs in the Editor or a development build unless <see cref="forceEnable"/> is set, so it
    /// costs nothing in a shipping build. Lives on the persistent Managers object.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Tooltip("Seconds between summary log lines.")]
        [SerializeField] private float sampleSeconds = 5f;
        [Tooltip("A frame longer than this many times the running average is logged as a spike.")]
        [SerializeField] private float spikeMultiplier = 2f;
        [Tooltip("Log even in a non-development build.")]
        [SerializeField] private bool forceEnable = false;

        private int _frames;
        private float _elapsed;
        private float _worstFrame;
        private float _avgFrame = 1f / 30f; // seeded, refined each window
        private readonly int[] _gcAtWindowStart = new int[3];

        private void Start()
        {
            if (!(Application.isEditor || Debug.isDebugBuild || forceEnable))
            {
                enabled = false;
                return;
            }
            SnapshotGc();
            VRLog.Info("Perf", $"monitor active — device={SystemInfo.deviceModel}, " +
                               $"mem={SystemInfo.systemMemorySize}MB, gfx={SystemInfo.graphicsDeviceName}");
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _frames++;
            _elapsed += dt;
            if (dt > _worstFrame) _worstFrame = dt;

            if (_frames > 5 && dt > _avgFrame * spikeMultiplier)
                VRLog.Warn("Perf", $"frame spike {dt * 1000f:F0}ms");

            if (_elapsed >= sampleSeconds)
                Report();
        }

        private void Report()
        {
            float avgFps = _frames / _elapsed;
            _avgFrame = _elapsed / _frames;
            float worstMs = _worstFrame * 1000f;

            long totalMem = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
            long gcMem = System.GC.GetTotalMemory(false) / (1024 * 1024);
            int g0 = System.GC.CollectionCount(0) - _gcAtWindowStart[0];
            int g1 = System.GC.CollectionCount(1) - _gcAtWindowStart[1];
            int g2 = System.GC.CollectionCount(2) - _gcAtWindowStart[2];

            VRLog.Info("Perf",
                $"fps={avgFps:F0} (worst {worstMs:F0}ms) mem={totalMem}MB gcHeap={gcMem}MB " +
                $"gc(g0/g1/g2)={g0}/{g1}/{g2}");

            _frames = 0;
            _elapsed = 0f;
            _worstFrame = 0f;
            SnapshotGc();
        }

        private void SnapshotGc()
        {
            _gcAtWindowStart[0] = System.GC.CollectionCount(0);
            _gcAtWindowStart[1] = System.GC.CollectionCount(1);
            _gcAtWindowStart[2] = System.GC.CollectionCount(2);
        }
    }
}