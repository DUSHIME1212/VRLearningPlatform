using UnityEngine;
using System.Collections.Generic;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Procedurally builds a physics chain or rope of N links at runtime.
    /// First link is fixed to this transform. Grab the last link in XR.
    /// </summary>
    public class PhysicsChain : MonoBehaviour
    {
        [SerializeField] private int     linkCount     = 8;
        [SerializeField] private float   linkLength    = 0.15f;
        [SerializeField] private float   linkMass      = 0.1f;
        [SerializeField] private float   linkDamping   = 0.5f;
        [SerializeField] private GameObject linkPrefab;        // optional; falls back to capsule
        [SerializeField] private bool    addGrabToLastLink = true;

        private readonly List<Rigidbody> _links = new();

        private void Start() => BuildChain();

        private void BuildChain()
        {
            Vector3 pos    = transform.position;
            Rigidbody prev = null;

            for (int i = 0; i < linkCount; i++)
            {
                pos += Vector3.down * linkLength;

                GameObject go = linkPrefab != null
                    ? Instantiate(linkPrefab, pos, Quaternion.identity)
                    : CreateCapsuleLink(pos, i);

                go.name = $"ChainLink_{i}";
                go.transform.SetParent(transform);

                var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
                rb.mass            = linkMass;
                rb.linearDamping   = linkDamping;
                rb.angularDamping  = linkDamping;
                rb.interpolation   = RigidbodyInterpolation.Interpolate;

                var joint = go.AddComponent<HingeJoint>();
                if (prev == null)
                {
                    // First link anchors to this transform's position in world space
                    joint.connectedBody  = null;
                    joint.autoConfigureConnectedAnchor = false;
                    joint.connectedAnchor = transform.position;
                }
                else
                {
                    joint.connectedBody = prev;
                    joint.autoConfigureConnectedAnchor = true;
                }
                joint.axis        = Vector3.right;
                joint.useLimits   = true;
                joint.limits      = new JointLimits { min = -45f, max = 45f };

                _links.Add(rb);
                prev = rb;
            }

            if (addGrabToLastLink && _links.Count > 0)
            {
                var lastGo = _links[^1].gameObject;
                if (lastGo.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() == null)
                    lastGo.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            }
        }

        private GameObject CreateCapsuleLink(Vector3 pos, int index)
        {
            var go   = new GameObject($"Link_{index}");
            go.transform.position = pos;
            var mf   = go.AddComponent<MeshFilter>();
            var mr   = go.AddComponent<MeshRenderer>();
            var temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mf.sharedMesh    = temp.GetComponent<MeshFilter>().sharedMesh;
            mr.sharedMaterial = temp.GetComponent<MeshRenderer>().sharedMaterial;
            go.AddComponent<CapsuleCollider>();
            go.transform.localScale = new Vector3(0.06f, linkLength * 0.5f, 0.06f);
            Destroy(temp);
            return go;
        }

        public Rigidbody GetLink(int index) => _links[index];
        public Rigidbody GetLastLink()      => _links.Count > 0 ? _links[^1] : null;
    }
}
