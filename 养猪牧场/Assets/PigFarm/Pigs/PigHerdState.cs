using System;
using System.Collections.Generic;

namespace PigFarm.Pigs
{
    [Serializable]
    public struct PigSnapshot
    {
        public int id;
        public string stageId;
        public string stageName;
        public int value;
        public int occupiedCells;
        public bool canBreed;
        public bool canGrow;
        public bool vaccinated;
    }

    public sealed class PigHerdState
    {
        private sealed class PigRecord
        {
            public int id;
            public PigStageDefinition stage;
            public bool vaccinated;
        }

        private readonly PigSystemConfig config;
        private readonly List<PigRecord> pigs = new List<PigRecord>();
        private int nextId = 1;

        public PigHerdState(PigSystemConfig config)
        {
            this.config = config ? config : throw new ArgumentNullException(nameof(config));
            foreach (PigStageDefinition stage in config.startingPigs)
            {
                if (stage)
                    AddUnchecked(stage);
            }
        }

        public int Capacity => config.penCapacity;
        public int CrowdingThreshold => config.crowdingThreshold;
        public int UsedCells
        {
            get
            {
                int total = 0;
                foreach (PigRecord pig in pigs)
                    total += pig.stage.occupiedCells;
                return total;
            }
        }

        public int Count => pigs.Count;
        public bool IsCrowded => UsedCells > config.crowdingThreshold;

        public List<PigSnapshot> GetSnapshots()
        {
            var result = new List<PigSnapshot>(pigs.Count);
            foreach (PigRecord pig in pigs)
            {
                result.Add(new PigSnapshot
                {
                    id = pig.id,
                    stageId = pig.stage.id,
                    stageName = pig.stage.displayName,
                    value = pig.stage.value,
                    occupiedCells = pig.stage.occupiedCells,
                    canBreed = pig.stage.canBreed,
                    canGrow = pig.stage.CanGrow,
                    vaccinated = pig.vaccinated
                });
            }
            return result;
        }

        public bool TryAdd(PigStageDefinition stage, out string failure)
        {
            if (!stage)
            {
                failure = "��ֻ������Ч";
                return false;
            }
            if (!CanFitDelta(stage.occupiedCells))
            {
                failure = CapacityFailure();
                return false;
            }
            AddUnchecked(stage);
            failure = null;
            return true;
        }

        public void Clear()
        {
            pigs.Clear();
        }

        public bool TryBirth(bool useCharm, out int newbornId, out string failure)
        {
            PigStageDefinition stage = config.BirthStage(useCharm);
            if (!TryAdd(stage, out failure))
            {
                newbornId = 0;
                return false;
            }
            newbornId = pigs[pigs.Count - 1].id;
            return true;
        }

        public bool TryGrow(int pigId, int levels, out PigSnapshot grownPig, out string failure)
        {
            grownPig = default(PigSnapshot);
            PigRecord pig = Find(pigId);
            if (pig == null)
            {
                failure = "没有找到这坪�?";
                return false;
            }

            PigStageDefinition target = pig.stage;
            int remaining = Math.Max(1, levels);
            while (remaining-- > 0 && target.nextStage)
                target = target.nextStage;

            if (target == pig.stage)
            {
                failure = pig.stage.displayName + "已绝丝能继续戝长";
                return false;
            }

            int delta = target.occupiedCells - pig.stage.occupiedCells;
            if (!CanFitDelta(delta))
            {
                failure = CapacityFailure();
                return false;
            }

            pig.stage = target;
            grownPig = Snapshot(pig);
            failure = null;
            return true;
        }

        public bool TryRemove(int pigId, out PigSnapshot removed)
        {
            PigRecord pig = Find(pigId);
            if (pig == null)
            {
                removed = default(PigSnapshot);
                return false;
            }
            removed = Snapshot(pig);
            pigs.Remove(pig);
            return true;
        }

        public bool TryVaccinateFirstUnvaccinated(out PigSnapshot vaccinatedPig)
        {
            PigRecord pig = pigs.Find(item => !item.vaccinated);
            if (pig == null)
            {
                vaccinatedPig = default(PigSnapshot);
                return false;
            }
            pig.vaccinated = true;
            vaccinatedPig = Snapshot(pig);
            return true;
        }

        public int CullUnvaccinated()
        {
            int before = pigs.Count;
            pigs.RemoveAll(pig => !pig.vaccinated);
            return before - pigs.Count;
        }

        private bool CanFitDelta(int delta)
        {
            return UsedCells + Math.Max(0, delta) <= config.penCapacity;
        }

        private string CapacityFailure()
        {
            return "猪圈太挤了，先坖掉几坪猪坧＝";
        }

        private void AddUnchecked(PigStageDefinition stage)
        {
            pigs.Add(new PigRecord { id = nextId++, stage = stage });
        }

        private PigRecord Find(int id)
        {
            return pigs.Find(pig => pig.id == id);
        }

        private static PigSnapshot Snapshot(PigRecord pig)
        {
            return new PigSnapshot
            {
                id = pig.id,
                stageId = pig.stage.id,
                stageName = pig.stage.displayName,
                value = pig.stage.value,
                occupiedCells = pig.stage.occupiedCells,
                canBreed = pig.stage.canBreed,
                canGrow = pig.stage.CanGrow,
                vaccinated = pig.vaccinated
            };
        }
    }
}
