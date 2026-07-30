using System;
using UnityEngine;

namespace PigFarm.Flow
{
    public enum PigFarmActionType { Breed, Feed, Shop, Sell }
    public enum PigFarmRewardType { Coins, Nutrition, Charm, Vaccine }

    [Serializable]
    public sealed class PigFarmRoundTask
    {
        public string title;
        [TextArea(2, 4)] public string description;
        public int babyMin;
        public int smallMin;
        public int mediumMin;
        public int largeMin;
        public int totalMin;
        public int smallAndMediumMin;
        public int mediumAndLargeMin;
        public bool requireAllStages;
        public PigFarmRewardType rewardType;
        [Min(1)] public int rewardAmount = 1;

        public bool IsComplete(int babies, int small, int medium, int large)
        {
            int total = babies + small + medium + large;
            if (requireAllStages && (babies < 1 || small < 1 || medium < 1 || large < 1)) return false;
            return babies >= babyMin && small >= smallMin && medium >= mediumMin && large >= largeMin &&
                   total >= totalMin && small + medium >= smallAndMediumMin && medium + large >= mediumAndLargeMin;
        }
    }

    [CreateAssetMenu(fileName = "PigFarmGameRules", menuName = "Pig Farm/Config/Complete Game Rules")]
    public sealed class PigFarmGameRulesConfig : ScriptableObject
    {
        [Min(1)] public int displayStarCount = 24;
        public int startingNutrition;
        public int startingCharms;
        public int startingVaccines;
        public int[] actionRollMinimum = { 2, 3, 3 };
        public int[] actionRollMaximum = { 4, 6, 8 };
        public PigFarmRoundTask[] roundTasks = new PigFarmRoundTask[16];

        public Vector2Int GetRollRange(int selectedActionCount)
        {
            int index = Mathf.Clamp(selectedActionCount - 1, 0, 2);
            int min = actionRollMinimum != null && actionRollMinimum.Length > index ? actionRollMinimum[index] : 2;
            int max = actionRollMaximum != null && actionRollMaximum.Length > index ? actionRollMaximum[index] : 4;
            return new Vector2Int(Mathf.Max(1, min), Mathf.Max(min, max));
        }

        public PigFarmRoundTask GetTask(int round)
        {
            if (roundTasks == null || roundTasks.Length == 0) return null;
            return roundTasks[Mathf.Clamp(round - 1, 0, roundTasks.Length - 1)];
        }
    }
}
