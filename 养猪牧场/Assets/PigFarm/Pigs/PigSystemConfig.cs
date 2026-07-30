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
        public List<PigStageDefinition> startingPigs = new List<PigStageDefinition>();

        public PigStageDefinition BirthStage(bool useCharm)
        {
            return useCharm && smallStage ? smallStage : babyStage;
        }

        private void OnValidate()
        {
            penCapacity = Mathf.Max(1, penCapacity);
            crowdingThreshold = Mathf.Clamp(crowdingThreshold, 0, penCapacity);
        }
    }
}
