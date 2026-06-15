using System.Collections.Generic;
using UnityEngine;

namespace VRLearning
{
    [CreateAssetMenu(fileName = "CourseDef_New", menuName = "VRLearning/Course Data")]
    public class CourseData : ScriptableObject
    {
        [Header("Identity")]
        public string CourseId;
        public string TitleEN;
        public string TitleRW;
        public string UnitLabel;
        public string DescriptionEN;
        public string DescriptionRW;

        [Header("Visuals")]
        public Color  CardColor;
        public int    BadgeNumber;
        public Sprite BannerImage;

        [Header("Experiments")]
        public List<SimulationData> Simulations;
    }
}
