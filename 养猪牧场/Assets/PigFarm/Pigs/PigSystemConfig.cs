using System.Collections.Generic;
using UnityEngine;

namespace PigFarm.Pigs
{
    [CreateAssetMenu(fileName = "PigSystemConfig", menuName = "Pig Farm/Config/Pig System")]
    public sealed class PigSystemConfig : ScriptableObject
    {
        [Min(1)] public int penCapacity = 80;
        [Min(0)] public int crowdingThreshold = 70;
        public PigStageDefinition babyStage;
        public PigStageDefinition smallStage;
        public PigStageDefinition mediumStage;
        public PigStageDefinition largeStage;
        public List<PigStageDefinition> startingPigs = new List<PigStageDefinition>();

        public PigStageDefinition BirthStage(bool useCharm)
        {
            return useCharm && smallStage ? smallStage : babyStage;
        }

        public PigStageDefinition GetStage(string stageId)
        {
            if (stageId == "baby") return babyStage;
            if (stageId == "small") return smallStage;
            if (stageId == "medium") return mediumStage;
            if (stageId == "large") return largeStage;
            return null;
        }

        private void OnValidate()
        {
            penCapacity = Mathf.Max(1, penCapacity);
            crowdingThreshold = Mathf.Clamp(crowdingThreshold, 0, penCapacity);
        }
    }
}
