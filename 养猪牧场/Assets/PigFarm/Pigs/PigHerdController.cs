using System;
using System.Collections.Generic;
using UnityEngine;

namespace PigFarm.Pigs
{
    public sealed class PigHerdController : MonoBehaviour
    {
        [SerializeField] private PigSystemConfig config;
        private PigHerdState state;
        private bool penViewActive;
        private bool crowdingShownThisVisit;

        public event Action HerdChanged;
        public event Action<string> OperationFailed;
        public event Action<PigSnapshot> PigGrew;
        public event Action<PigSnapshot> PigBorn;
        public event Action<PigSnapshot> CrowdingFeedbackRequested;

        public int UsedCells => state == null ? 0 : state.UsedCells;
        public int Capacity => state == null ? 0 : state.Capacity;
        public bool IsCrowded => state != null && state.IsCrowded;
        public bool IsInitialized => state != null;
        public IReadOnlyList<PigSnapshot> Pigs => state == null
            ? (IReadOnlyList<PigSnapshot>)new PigSnapshot[0]
            : state.GetSnapshots();

        public void Configure(PigSystemConfig value)
        {
            config = value;
        }

        private void Awake()
        {
            if (!config)
            {
                Debug.LogError("PigHerdController requires a PigSystemConfig.", this);
                enabled = false;
                return;
            }
            state = new PigHerdState(config);
        }

        private void Start()
        {
            HerdChanged?.Invoke();
        }

        public bool GrowPig(int pigId, bool useNutrition)
        {
            if (state == null)
                return false;
            bool wasCrowded = state.IsCrowded;
            PigSnapshot pig;
            string failure;
            if (!state.TryGrow(pigId, useNutrition ? 2 : 1, out pig, out failure))
                return Fail(failure);
            PigGrew?.Invoke(pig);
            Changed(wasCrowded);
            return true;
        }

        public bool BirthPig(bool useCharm)
        {
            if (state == null)
                return false;
            bool wasCrowded = state.IsCrowded;
            int id;
            string failure;
            if (!state.TryBirth(useCharm, out id, out failure))
                return Fail(failure);
            PigSnapshot newborn = FindSnapshot(id);
            PigBorn?.Invoke(newborn);
            Changed(wasCrowded);
            return true;
        }

        public bool AddBabyPig()
        {
            return AddPig(config ? config.babyStage : null);
        }

        public bool AddPig(PigStageDefinition stage)
        {
            if (state == null) return false;
            bool wasCrowded = state.IsCrowded;
            string failure;
            if (!state.TryAdd(stage, out failure)) return Fail(failure);
            Changed(wasCrowded);
            return true;
        }

        public bool AddPigByStageId(string stageId)
        {
            return AddPig(config ? config.GetStage(stageId) : null);
        }

        public PigStageDefinition ResolveStage(string stageId)
        {
            return config ? config.GetStage(stageId) : null;
        }

        public void ClearHerd()
        {
            if (state == null) return;
            state.Clear();
            HerdChanged?.Invoke();
        }

        public bool RemovePig(int pigId)
        {
            if (state == null)
                return false;
            PigSnapshot removed;
            if (!state.TryRemove(pigId, out removed))
                return Fail("没有找到这坪�?");
            HerdChanged?.Invoke();
            return true;
        }

        public bool VaccinateFirstUnvaccinated()
        {
            if (state == null) return false;
            PigSnapshot pig;
            if (!state.TryVaccinateFirstUnvaccinated(out pig)) return false;
            HerdChanged?.Invoke();
            return true;
        }

        public int CullUnvaccinated()
        {
            if (state == null) return 0;
            int removed = state.CullUnvaccinated();
            if (removed > 0) HerdChanged?.Invoke();
            return removed;
        }

        public void EnterPenView()
        {
            if (state == null || penViewActive)
                return;
            penViewActive = true;
            crowdingShownThisVisit = false;
            TryRequestCrowdingFeedback();
        }

        public void ExitPenView()
        {
            penViewActive = false;
            crowdingShownThisVisit = false;
        }

        private void Changed(bool wasCrowded)
        {
            HerdChanged?.Invoke();
            if (!wasCrowded && state.IsCrowded)
                crowdingShownThisVisit = false;
            TryRequestCrowdingFeedback();
        }

        private void TryRequestCrowdingFeedback()
        {
            if (state == null || !penViewActive || crowdingShownThisVisit || !state.IsCrowded || state.Count == 0)
                return;
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            crowdingShownThisVisit = true;
            CrowdingFeedbackRequested?.Invoke(pigs[UnityEngine.Random.Range(0, pigs.Count)]);
        }

        private PigSnapshot FindSnapshot(int id)
        {
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            for (int i = 0; i < pigs.Count; i++)
                if (pigs[i].id == id)
                    return pigs[i];
            return default(PigSnapshot);
        }

        private bool Fail(string message)
        {
            OperationFailed?.Invoke(message);
            return false;
        }
    }
}
