using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Explodes a multi-part object outward (educational anatomy/engineering view),
    /// then smoothly reassembles on demand.
    /// Add all sub-parts to the parts list. Call Explode() / Assemble() or toggle.
    /// </summary>
    public class ExplodedView : MonoBehaviour
    {
        [System.Serializable]
        public class Part
        {
            public Transform obj;
            public Vector3   explodeOffset;   // world-space direction × distance
            [HideInInspector] public Vector3 originLocalPos;
            [HideInInspector] public Quaternion originLocalRot;
        }

        [SerializeField] private List<Part> parts = new();
        [SerializeField] private float      explodeSpeed  = 1.5f;
        [SerializeField] private float      assembleSpeed = 2.0f;
        [SerializeField] private float      explodeRadius = 0.4f;  // auto-offset magnitude if offset is zero

        private bool      _exploded;
        private Coroutine _anim;

        private void Start()
        {
            foreach (var p in parts)
            {
                p.originLocalPos = p.obj.localPosition;
                p.originLocalRot = p.obj.localRotation;

                // Auto-compute offset if not manually set
                if (p.explodeOffset == Vector3.zero)
                    p.explodeOffset = (p.obj.position - transform.position).normalized * explodeRadius;
            }
        }

        public void Explode()
        {
            if (_exploded) return;
            _exploded = true;
            Restart(AnimateTo(exploded: true, explodeSpeed));
        }

        public void Assemble()
        {
            if (!_exploded) return;
            _exploded = false;
            Restart(AnimateTo(exploded: false, assembleSpeed));
        }

        public void Toggle() { if (_exploded) Assemble(); else Explode(); }

        private void Restart(IEnumerator routine)
        {
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(routine);
        }

        private IEnumerator AnimateTo(bool exploded, float speed)
        {
            float t = 0f;
            var starts = new Vector3[parts.Count];
            for (int i = 0; i < parts.Count; i++)
                starts[i] = parts[i].obj.localPosition;

            while (t < 1f)
            {
                t = Mathf.MoveTowards(t, 1f, Time.deltaTime * speed);
                for (int i = 0; i < parts.Count; i++)
                {
                    Vector3 target = exploded
                        ? parts[i].originLocalPos + transform.InverseTransformDirection(parts[i].explodeOffset)
                        : parts[i].originLocalPos;
                    parts[i].obj.localPosition = Vector3.Lerp(starts[i], target, Mathf.SmoothStep(0, 1, t));
                }
                yield return null;
            }
        }
    }
}
