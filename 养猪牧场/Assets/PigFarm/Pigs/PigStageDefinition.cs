using UnityEngine;

namespace PigFarm.Pigs
{
    [CreateAssetMenu(fileName = "PigStage", menuName = "Pig Farm/Config/Pig Stage")]
    public sealed class PigStageDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [Min(0)] public int value;
        [Min(1)] public int occupiedCells = 4;
        public bool canBreed;
        public PigStageDefinition nextStage;

        public bool CanGrow => nextStage != null;
    }
}
