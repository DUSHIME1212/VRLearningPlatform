using UnityEngine;

namespace VRLearning
{
    [CreateAssetMenu(fileName = "SimDef_New", menuName = "VRLearning/Simulation Data")]
    public class SimulationData : ScriptableObject
    {
        [Header("Identity")]
        public string SimulationId;
        public string TitleEN;
        public string TitleRW;
        public string DescriptionEN;
        public string DescriptionRW;
        public int    BadgeNumber;

        [Header("Scene")]
        public string SceneName;

        [Header("State")]
        public bool IsLocked;

        [Header("Visuals")]
        public Sprite Thumbnail;
    }
}
