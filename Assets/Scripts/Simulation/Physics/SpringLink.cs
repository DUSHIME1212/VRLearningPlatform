using UnityEngine;

namespace VRLearning.Simulation.Physics
{
    /// <summary>
    /// Connects this Rigidbody to another with a SpringJoint and (optionally) draws the spring with a
    /// LineRenderer. Grab either body and watch Hooke's law pull them back together.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpringLink : MonoBehaviour
    {
        [SerializeField] private Rigidbody connectedBody;
        [SerializeField] private float spring = 50f;
        [SerializeField] private float damper = 2f;
        [SerializeField] private LineRenderer line;

        private SpringJoint _joint;

        private void Awake()
        {
            _joint = GetComponent<SpringJoint>();
            if (_joint == null) _joint = gameObject.AddComponent<SpringJoint>();
            _joint.spring = spring;
            _joint.damper = damper;
            _joint.autoConfigureConnectedAnchor = true;
            if (connectedBody != null) _joint.connectedBody = connectedBody;
        }

        private void LateUpdate()
        {
            if (line == null || connectedBody == null) return;
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, connectedBody.position);
        }
    }
}
