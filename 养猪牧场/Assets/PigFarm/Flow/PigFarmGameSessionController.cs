using System;
using System.Collections.Generic;
using PigFarm.Core;
using PigFarm.Pigs;
using UnityEngine;

namespace PigFarm.Flow
{
    public sealed class PigFarmGameSessionController : MonoBehaviour
    {
        [SerializeField] PigFarmGameRulesConfig rules;
        [SerializeField] GameFlowController gameFlow;
        [SerializeField] PigHerdController herd;

        readonly bool[] selectedActions = new bool[4];
        PigFarmActionType currentAction;
        int currentActionRemaining;
        int nutrition;
        int charms;
        int vaccines;
        string lastMessage;
        bool awaitingRoundEnd;
        bool gameComplete;

        public event Action Changed;
        public event Action<string> NoticeRequested;
        public event Action<PigFarmActionType, int> ActionRolled;
        public event Action<int, string> RoundResolved;
        public event Action<int> PlagueResolved;
        public event Action<int> GameCompleted;

        public PigFarmGameRulesConfig Rules { get { return rules; } }
        public GameFlowSnapshot Flow { get { return gameFlow ? gameFlow.Current : default(GameFlowSnapshot); } }
        public IReadOnlyList<PigSnapshot> Pigs { get { return herd ? herd.Pigs : new PigSnapshot[0]; } }
        public int UsedCells { get { return herd ? herd.UsedCells : 0; } }
        public int Capacity { get { return herd ? herd.Capacity : 80; } }
        public int Nutrition { get { return nutrition; } }
        public int Charms { get { return charms; } }
        public int Vaccines { get { return vaccines; } }
        public int CurrentActionRemaining { get { return currentActionRemaining; } }
        public PigFarmActionType CurrentAction { get { return currentAction; } }
        public bool HasRolledAction { get { return currentActionRemaining > 0; } }
        public bool AwaitingRoundEnd { get { return awaitingRoundEnd; } }
        public bool IsGameComplete { get { return gameComplete; } }
        public string LastMessage { get { return lastMessage; } }
        public PigFarmRoundTask CurrentTask { get { return rules ? rules.GetTask(Flow.round) : null; } }

        public void Configure(PigFarmGameRulesConfig value, GameFlowController flow, PigHerdController herdController)
        {
            rules = value;
            gameFlow = flow;
            herd = herdController;
        }

        void Start()
        {
            nutrition = rules ? rules.startingNutrition : 0;
            charms = rules ? rules.startingCharms : 0;
            vaccines = rules ? rules.startingVaccines : 0;
            if (herd)
            {
                herd.HerdChanged += Publish;
                herd.OperationFailed += Fail;
                herd.EnterPenView();
            }
            lastMessage = "请选择 1～3 种行动，再由命运决定本回合行动。";
            Publish();
        }

        void OnDestroy()
        {
            if (!herd) return;
            herd.HerdChanged -= Publish;
            herd.OperationFailed -= Fail;
        }

        public bool IsActionSelected(PigFarmActionType type) { return selectedActions[(int)type]; }

        public int SelectedActionCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < selectedActions.Length; i++) if (selectedActions[i]) count++;
                return count;
            }
        }

        public Vector2Int CurrentRollRange { get { return rules ? rules.GetRollRange(SelectedActionCount) : new Vector2Int(2, 4); } }

        public void ToggleAction(PigFarmActionType type)
        {
            if (HasRolledAction || awaitingRoundEnd || gameComplete) return;
            int index = (int)type;
            if (!selectedActions[index] && SelectedActionCount >= 3)
            {
                Fail("每回合最多选择 3 种行动。");
                return;
            }
            selectedActions[index] = !selectedActions[index];
            Publish();
        }

        public void ConfirmActionSelection()
        {
            int count = SelectedActionCount;
            if (count == 0 || HasRolledAction || awaitingRoundEnd || gameComplete)
            {
                Fail("请先选择至少 1 种行动。");
                return;
            }
            var options = new List<PigFarmActionType>(3);
            for (int i = 0; i < selectedActions.Length; i++) if (selectedActions[i]) options.Add((PigFarmActionType)i);
            currentAction = options[UnityEngine.Random.Range(0, options.Count)];
            Vector2Int range = CurrentRollRange;
            currentActionRemaining = UnityEngine.Random.Range(range.x, range.y + 1);
            lastMessage = "本回合抽到「" + ActionName(currentAction) + "」× " + currentActionRemaining;
            ActionRolled?.Invoke(currentAction, currentActionRemaining);
            Publish();
        }

        public void ExecutePrimaryAction(bool useItem)
        {
            if (!HasRolledAction || awaitingRoundEnd || gameComplete) return;
            bool success = false;
            if (currentAction == PigFarmActionType.Breed)
            {
                bool useCharm = useItem && charms > 0;
                success = herd && herd.BirthPig(useCharm);
                if (success && useCharm) charms--;
            }
            else if (currentAction == PigFarmActionType.Feed)
            {
                PigSnapshot target;
                if (!TryFindGrowablePig(out target)) Fail("当前没有可以继续成长的猪。");
                else
                {
                    bool useNutrition = useItem && nutrition > 0;
                    success = herd.GrowPig(target.id, useNutrition);
                    if (success && useNutrition) nutrition--;
                }
            }
            else if (currentAction == PigFarmActionType.Sell)
            {
                PigSnapshot target;
                if (!TryFindHighestValuePig(out target)) Fail("猪圈里没有可以卖出的猪。");
                else if (herd.RemovePig(target.id))
                {
                    gameFlow.AddCoins(target.value);
                    lastMessage = "卖出「" + target.stageName + "」，获得 " + target.value + " 金币。";
                    success = true;
                }
            }
            if (success) ConsumeAction();
        }

        public void BuyShopItem(int itemIndex)
        {
            if (!HasRolledAction || currentAction != PigFarmActionType.Shop || awaitingRoundEnd || gameComplete) return;
            int price = itemIndex == 0 ? 3 : 1;
            if (Flow.coins < price) { Fail("金币不足。"); return; }
            bool success = true;
            if (itemIndex == 0) success = herd && herd.AddBabyPig();
            else if (itemIndex == 1) nutrition++;
            else if (itemIndex == 2) charms++;
            else if (itemIndex == 3) vaccines++;
            else success = false;
            if (!success) return;
            gameFlow.AddCoins(-price);
            lastMessage = "购买成功，花费 " + price + " 金币。";
            ConsumeAction();
        }

        public void VaccinateOnePig()
        {
            if (vaccines <= 0) { Fail("当前没有疫苗。"); return; }
            if (!herd || !herd.VaccinateFirstUnvaccinated()) { Fail("所有猪都已经接种疫苗。"); return; }
            vaccines--;
            lastMessage = "一只猪完成了疫苗接种。";
            Publish();
        }

        public void ResolveRound()
        {
            if (!awaitingRoundEnd || gameComplete) return;
            int completedRound = Flow.round;
            PigFarmRoundTask task = CurrentTask;
            bool completed = task != null && IsTaskComplete(task);
            if (completed) GrantReward(task);
            string resolution = completed ? "任务完成，奖励已到账。" : "任务未完成，本回合没有奖励。";

            int culled = 0;
            if (completedRound == 4 || completedRound == 8 || completedRound == 12)
            {
                culled = herd ? herd.CullUnvaccinated() : 0;
                PlagueResolved?.Invoke(culled);
                resolution += " 猪瘟来袭，损失未接种猪 " + culled + " 只。";
            }

            RoundResolved?.Invoke(completedRound, resolution);
            if (completedRound >= 16)
            {
                int total = Flow.coins;
                IReadOnlyList<PigSnapshot> pigs = Pigs;
                for (int i = 0; i < pigs.Count; i++) total += pigs[i].value;
                gameComplete = true;
                lastMessage = "最终金币：现金 " + Flow.coins + " + 猪只价值 " + (total - Flow.coins) + " = " + total;
                GameCompleted?.Invoke(total);
                Publish();
                return;
            }

            gameFlow.AdvanceRound();
            awaitingRoundEnd = false;
            ResetSelection();
            lastMessage = resolution + " 新回合任务已经发布。";
            Publish();
        }

        public bool IsTaskComplete(PigFarmRoundTask task)
        {
            int baby = 0, small = 0, medium = 0, large = 0;
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            for (int i = 0; i < pigs.Count; i++)
            {
                if (pigs[i].stageId == "baby") baby++;
                else if (pigs[i].stageId == "small") small++;
                else if (pigs[i].stageId == "medium") medium++;
                else if (pigs[i].stageId == "large") large++;
            }
            return task != null && task.IsComplete(baby, small, medium, large);
        }

        void ConsumeAction()
        {
            currentActionRemaining = Mathf.Max(0, currentActionRemaining - 1);
            if (currentActionRemaining == 0)
            {
                awaitingRoundEnd = true;
                lastMessage += " 本回合行动已完成，请进行回合结算。";
            }
            Publish();
        }

        void GrantReward(PigFarmRoundTask task)
        {
            if (task.rewardType == PigFarmRewardType.Coins) gameFlow.AddCoins(task.rewardAmount);
            else if (task.rewardType == PigFarmRewardType.Nutrition) nutrition += task.rewardAmount;
            else if (task.rewardType == PigFarmRewardType.Charm) charms += task.rewardAmount;
            else if (task.rewardType == PigFarmRewardType.Vaccine) vaccines += task.rewardAmount;
        }

        void ResetSelection()
        {
            for (int i = 0; i < selectedActions.Length; i++) selectedActions[i] = false;
            currentActionRemaining = 0;
        }

        bool TryFindGrowablePig(out PigSnapshot result)
        {
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            for (int i = 0; i < pigs.Count; i++) if (pigs[i].canGrow) { result = pigs[i]; return true; }
            result = default(PigSnapshot);
            return false;
        }

        bool TryFindHighestValuePig(out PigSnapshot result)
        {
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            result = default(PigSnapshot);
            if (pigs.Count == 0) return false;
            result = pigs[0];
            for (int i = 1; i < pigs.Count; i++) if (pigs[i].value > result.value) result = pigs[i];
            return true;
        }

        static string ActionName(PigFarmActionType type)
        {
            if (type == PigFarmActionType.Breed) return "生育";
            if (type == PigFarmActionType.Feed) return "喂养";
            if (type == PigFarmActionType.Shop) return "商店购买";
            return "卖猪";
        }

        void Fail(string message)
        {
            lastMessage = message;
            NoticeRequested?.Invoke(message);
            Publish();
        }

        void Publish() { Changed?.Invoke(); }
    }
}
