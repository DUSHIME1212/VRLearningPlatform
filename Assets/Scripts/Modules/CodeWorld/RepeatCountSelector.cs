using UnityEngine;
using System;
namespace VRLearning.Modules.CodeWorld
{
    public class RepeatCountSelector : MonoBehaviour
    {
        public int CurrentCount { get; private set; } = 1;
        public event Action<int> OnCountChanged;
        public void Increment() { CurrentCount = Mathf.Min(CurrentCount + 1, 5); OnCountChanged?.Invoke(CurrentCount); }
        public void Decrement() { CurrentCount = Mathf.Max(CurrentCount - 1, 1); OnCountChanged?.Invoke(CurrentCount); }
    }
}
