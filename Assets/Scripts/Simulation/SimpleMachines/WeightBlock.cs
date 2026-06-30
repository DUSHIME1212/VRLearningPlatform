using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRLearning.Simulation.SimpleMachines
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    public class WeightBlock : MonoBehaviour
    {
        [SerializeField] public float weightValue = 1f;
        [SerializeField] private AudioClip impactClip;
        [SerializeField] private float snapRadius = 0.25f;

        public event Action<WeightBlock, Vector3> OnPlaced;
        public event Action<WeightBlock> OnLifted;

        private XRGrabInteractable _grab;
        private Rigidbody _rb;
        private Vector3 _origin;
        private WeightSnapZone _currentZone;

        private void Awake()
        {
            _grab = GetComponent<XRGrabInteractable>();
            _rb = GetComponent<Rigidbody>();
            _origin = transform.position;

            _grab.selectEntered.AddListener(OnGrabbed);
            _grab.selectExited.AddListener(OnReleased);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            if (_currentZone != null)
            {
                _currentZone.Release();
                _currentZone = null;
            }
            OnLifted?.Invoke(this);
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            WeightSnapZone closest = null;
            float minDist = snapRadius;

            var zones = FindObjectsByType<WeightSnapZone>(FindObjectsInactive.Exclude);
            foreach (var zone in zones)
            {
                if (zone.IsOccupied) continue;
                float dist = Vector3.Distance(transform.position, zone.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = zone;
                }
            }

            if (closest != null)
            {
                closest.Accept(this);
                _currentZone = closest;
                OnPlaced?.Invoke(this, closest.transform.position);

                if (impactClip != null)
                    Core.AudioManager.Instance?.PlaySFX(impactClip);
            }
        }

        public void ReturnToOrigin()
        {
            if (_currentZone != null)
            {
                _currentZone.Release();
                _currentZone = null;
            }

            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.position = _origin;
            transform.rotation = Quaternion.identity;
        }

        public bool IsPlaced => _currentZone != null;
        public bool IsGrabbed => _grab.isSelected;
    }
}
