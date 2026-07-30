using System;
using System.Collections.Generic;
using PigFarm.Audio;
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
        int currentActionTotal;
        int nutrition;
        int charms;
        int vaccines;
        string lastMessage;
        bool awaitingRoundEnd;
        bool gameComplete;
        bool openingShopActive;
        int openingPurchaseCount;
        PigFarmRoundTask currentTask;

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
        public int CurrentActionTotal { get { return currentActionTotal; } }
        public PigFarmActionType CurrentAction { get { return currentAction; } }
        public bool HasRolledAction { get { return currentActionRemaining > 0; } }
        public bool AwaitingRoundEnd { get { return awaitingRoundEnd; } }
        public bool IsGameComplete { get { return gameComplete; } }
        public bool IsOpeningShopActive { get { return openingShopActive; } }
        public int OpeningPurchaseCount { get { return openingPurchaseCount; } }
        public string LastMessage { get { return lastMessage; } }
        public PigFarmRoundTask CurrentTask { get { return currentTask != null ? currentTask : rules ? rules.GetTask(Flow.round) : null; } }

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
            SelectRandomTaskForCurrentStage();
            if (herd)
            {
                herd.ClearHerd();
                herd.HerdChanged += Publish;
                herd.OperationFailed += Fail;
                herd.EnterPenView();
            }
            lastMessage = "请先完成初始采购，再开始本回合行动。";
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
            if (openingShopActive || HasRolledAction || awaitingRoundEnd || gameComplete) return;
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
            if (openingShopActive || count == 0 || HasRolledAction || awaitingRoundEnd || gameComplete)
            {
                Fail("请先选择至少 1 种行动。");
                return;
            }
            var options = new List<PigFarmActionType>(3);
            for (int i = 0; i < selectedActions.Length; i++) if (selectedActions[i]) options.Add((PigFarmActionType)i);
            currentAction = options[UnityEngine.Random.Range(0, options.Count)];
            Vector2Int range = CurrentRollRange;
            currentActionRemaining = UnityEngine.Random.Range(range.x, range.y + 1);
            currentActionTotal = currentActionRemaining;
            lastMessage = "本回合抽到「" + ActionName(currentAction) + "」× " + currentActionRemaining;
            PigFarmAudioService.Play(PigFarmAudioCue.Roll);
            ActionRolled?.Invoke(currentAction, currentActionRemaining);
            Publish();
        }

        public void ExecutePrimaryAction(bool useItem)

        {

            if (!HasRolledAction || awaitingRoundEnd || gameComplete) return;

            if (currentAction == PigFarmActionType.Breed)

            {

                PigSnapshot parent;

                if (!TryFindBreedablePig(out parent)) Fail("当前没有可以繁殖的星星。");

                else ExecuteBreedOnPig(parent.id, useItem);

                return;

            }

            if (currentAction == PigFarmActionType.Feed)

            {

                PigSnapshot target;

                if (!TryFindGrowablePig(out target)) Fail("当前没有可以继续成长的星星。");

                else ExecuteFeedOnPig(target.id, useItem);

                return;

            }

            if (currentAction == PigFarmActionType.Sell)

            {

                PigSnapshot target;

                if (!TryFindHighestValuePig(out target)) { Fail("猪圈里没有可以卖出的星星。"); return; }

                if (!herd || !herd.RemovePig(target.id)) return;

                gameFlow.AddCoins(target.value);

                lastMessage = "卖出「" + StarDisplayNameByStage(target.stageId) + "」，获得 " + target.value + " 金币。";

                PigFarmAudioService.Play(PigFarmAudioCue.Trade);

                ConsumeAction();

            }

        }



        public bool ExecuteFeedOnPig(int pigId, bool useItem)

        {

            if (!HasRolledAction || currentAction != PigFarmActionType.Feed || awaitingRoundEnd || gameComplete) return false;

            PigSnapshot target;

            if (!TryGetPig(pigId, out target)) { Fail("没有找到这颗星星。"); return false; }

            if (!target.canGrow) { Fail("这颗星星已经不能继续成长。"); return false; }

            bool useNutrition = useItem && nutrition > 0;

            if (!herd || !herd.GrowPig(pigId, useNutrition)) return false;

            if (useNutrition) nutrition--;

            lastMessage = "喂养「" + StarDisplayNameByStage(target.stageId) + "」成功" + (useNutrition ? "（使用营养剂）" : "") + "。";

            PigFarmAudioService.Play(PigFarmAudioCue.FeedAndGrow);

            if (useNutrition) PigFarmAudioService.Play(PigFarmAudioCue.ItemAndVaccine);

            ConsumeAction();

            return true;

        }



        public bool ExecuteBreedOnPig(int parentPigId, bool useItem)

        {

            if (!HasRolledAction || currentAction != PigFarmActionType.Breed || awaitingRoundEnd || gameComplete) return false;

            PigSnapshot parent;

            if (!TryGetPig(parentPigId, out parent)) { Fail("没有找到这颗星星。"); return false; }

            if (!parent.canBreed) { Fail("请选择可繁殖的大星星或超大星星。"); return false; }

            bool useCharm = useItem && charms > 0;

            if (!herd || !herd.BirthPig(useCharm)) return false;

            if (useCharm) charms--;

            lastMessage = "「" + StarDisplayNameByStage(parent.stageId) + "」繁殖成功" + (useCharm ? "（使用护符）" : "") + "。";

            PigFarmAudioService.Play(PigFarmAudioCue.Breed);

            if (useCharm) PigFarmAudioService.Play(PigFarmAudioCue.ItemAndVaccine);

            ConsumeAction();

            return true;

        }



        public void BeginOpeningShop()
        {
            openingShopActive = true;
            openingPurchaseCount = 0;
            lastMessage = "使用初始金币购买星星与道具，完成后离开商店。";
            Publish();
        }

        public void EndOpeningShop()
        {
            if (!openingShopActive) return;
            openingShopActive = false;
            lastMessage = "初始采购完成。请选择 1～3 种行动，再抽取本回合行动。";
            Publish();
        }

        public int GetShopItemPrice(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex <= 3)
            {
                PigStageDefinition stage = ResolveStage(GetShopItemStageId(itemIndex));
                return stage ? stage.value : 0;
            }
            return 1;
        }

        public string GetShopItemStageId(int itemIndex)
        {
            if (itemIndex == 0) return "baby";
            if (itemIndex == 1) return "small";
            if (itemIndex == 2) return "medium";
            if (itemIndex == 3) return "large";
            return null;
        }

        public void GetHerdCounts(out int baby, out int small, out int medium, out int large, out int totalValue)
        {
            baby = 0;
            small = 0;
            medium = 0;
            large = 0;
            totalValue = 0;
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            for (int i = 0; i < pigs.Count; i++)
            {
                totalValue += pigs[i].value;
                if (pigs[i].stageId == "baby") baby++;
                else if (pigs[i].stageId == "small") small++;
                else if (pigs[i].stageId == "medium") medium++;
                else if (pigs[i].stageId == "large") large++;
            }
        }

        public void BuyShopItem(int itemIndex)
        {
            bool opening = openingShopActive;
            bool normalShop = HasRolledAction && currentAction == PigFarmActionType.Shop;
            if ((!opening && !normalShop) || awaitingRoundEnd || gameComplete) return;

            int price = GetShopItemPrice(itemIndex);
            if (price <= 0) { Fail("商品无效。"); return; }
            if (Flow.coins < price) { Fail("金币不足。"); return; }

            bool success = true;
            string boughtName;
            if (itemIndex >= 0 && itemIndex <= 3)
            {
                PigStageDefinition stage = ResolveStage(GetShopItemStageId(itemIndex));
                success = herd && herd.AddPig(stage);
                boughtName = StarDisplayName(itemIndex);
            }
            else if (itemIndex == 4)
            {
                nutrition++;
                boughtName = "营养剂";
            }
            else if (itemIndex == 5)
            {
                charms++;
                boughtName = "护符";
            }
            else if (itemIndex == 6)
            {
                vaccines++;
                boughtName = "疫苗";
            }
            else
            {
                Fail("商品无效。");
                return;
            }

            if (!success) return;
            gameFlow.AddCoins(-price);
            lastMessage = "购买「" + boughtName + "」成功，花费 " + price + " 金币。";
            PigFarmAudioService.Play(PigFarmAudioCue.Trade);
            if (opening)
            {
                openingPurchaseCount++;
                Publish();
            }
            else ConsumeAction();
        }

        public void SellPig(int pigId)
        {
            if (!HasRolledAction || currentAction != PigFarmActionType.Sell || awaitingRoundEnd || gameComplete) return;
            PigSnapshot target = default(PigSnapshot);
            bool found = false;
            IReadOnlyList<PigSnapshot> pigs = Pigs;
            for (int i = 0; i < pigs.Count; i++)
            {
                if (pigs[i].id != pigId) continue;
                target = pigs[i];
                found = true;
                break;
            }
            if (!found || !herd || !herd.RemovePig(pigId)) { Fail("这只猪当前无法出售。"); return; }
            gameFlow.AddCoins(target.value);
            lastMessage = "卖出「" + target.stageName + "」，获得 " + target.value + " 金币。";
            PigFarmAudioService.Play(PigFarmAudioCue.Trade);
            ConsumeAction();
        }

        public void VaccinateOnePig()
        {
            if (vaccines <= 0) { Fail("当前没有疫苗。"); return; }
            if (!herd || !herd.VaccinateFirstUnvaccinated()) { Fail("所有猪都已经接种疫苗。"); return; }
            vaccines--;
            lastMessage = "一只猪完成了疫苗接种。";
            PigFarmAudioService.Play(PigFarmAudioCue.ItemAndVaccine);
            Publish();
        }

        public void ResolveRound()
        {
            if (!awaitingRoundEnd || gameComplete) return;
            int completedRound = Flow.round;
            PigFarmRoundTask task = CurrentTask;
            bool completed = task != null && IsTaskComplete(task);
            if (completed)
            {
                GrantReward(task);
                PigFarmAudioService.Play(PigFarmAudioCue.TaskReward);
            }
            PigFarmAudioService.Play(PigFarmAudioCue.RoundTransition);
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
            SelectRandomTaskForCurrentStage();
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

        public bool CanPerformCurrentAction()
        {
            if (!HasRolledAction || awaitingRoundEnd || gameComplete) return false;
            if (currentAction == PigFarmActionType.Feed) return CanFeedAny();
            if (currentAction == PigFarmActionType.Breed) return CanBreedAny();
            if (currentAction == PigFarmActionType.Sell) return Pigs.Count > 0;
            if (currentAction == PigFarmActionType.Shop) return Flow.coins >= 1;
            return false;
        }

        public bool TryAutoEndRoundIfActionImpossible()
        {
            if (!HasRolledAction || awaitingRoundEnd || gameComplete) return false;
            if (CanPerformCurrentAction()) return false;
            string actionName = ActionName(currentAction);
            currentActionRemaining = 0;
            awaitingRoundEnd = true;
            lastMessage = "抽到「" + actionName + "」但本回合无法进行，自动结束回合。";
            Publish();
            ResolveRound();
            return true;
        }

        void ConsumeAction()
        {
            currentActionRemaining = Mathf.Max(0, currentActionRemaining - 1);
            if (currentActionRemaining == 0)
            {
                awaitingRoundEnd = true;
                lastMessage += " 本回合行动已完成，请进行回合结算。";
                Publish();
                return;
            }
            if (!CanPerformCurrentAction())
            {
                string actionName = ActionName(currentAction);
                currentActionRemaining = 0;
                awaitingRoundEnd = true;
                lastMessage += " 剩余「" + actionName + "」已无法继续，自动结束回合。";
                Publish();
                ResolveRound();
                return;
            }
            Publish();
        }

        bool CanFeedAny()
        {
            PigSnapshot target;
            return TryFindGrowablePig(out target);
        }

        bool CanBreedAny()
        {
            PigSnapshot parent;
            if (!TryFindBreedablePig(out parent)) return false;
            PigStageDefinition baby = ResolveStage("baby");
            if (baby && UsedCells + baby.occupiedCells <= Capacity) return true;
            if (charms > 0)
            {
                PigStageDefinition small = ResolveStage("small");
                if (small && UsedCells + small.occupiedCells <= Capacity) return true;
            }
            return false;
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
            currentActionTotal = 0;
        }

        void SelectRandomTaskForCurrentStage()
        {
            if (!rules || rules.roundTasks == null || rules.roundTasks.Length == 0)
            {
                currentTask = null;
                return;
            }
            int stageStart = Mathf.Clamp(Flow.seasonIndex * 4, 0, rules.roundTasks.Length - 1);
            int stageEnd = Mathf.Min(stageStart + 4, rules.roundTasks.Length);
            currentTask = rules.roundTasks[UnityEngine.Random.Range(stageStart, stageEnd)];
        }



        bool TryGetPig(int pigId, out PigSnapshot result)

        {

            IReadOnlyList<PigSnapshot> pigs = Pigs;

            for (int i = 0; i < pigs.Count; i++)

            {

                if (pigs[i].id != pigId) continue;

                result = pigs[i];

                return true;

            }

            result = default(PigSnapshot);

            return false;

        }



        bool TryFindBreedablePig(out PigSnapshot result)

        {

            IReadOnlyList<PigSnapshot> pigs = Pigs;

            for (int i = 0; i < pigs.Count; i++) if (pigs[i].canBreed) { result = pigs[i]; return true; }

            result = default(PigSnapshot);

            return false;

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

        PigStageDefinition ResolveStage(string stageId)
        {
            return herd ? herd.ResolveStage(stageId) : null;
        }

        static string StarDisplayNameByStage(string stageId)

        {

            if (stageId == "baby") return "小星星";

            if (stageId == "small") return "中星星";

            if (stageId == "medium") return "大星星";

            if (stageId == "large") return "超大星星";

            return "星星";

        }



        static string StarDisplayName(int itemIndex)
        {
            if (itemIndex == 0) return "小星星";
            if (itemIndex == 1) return "中星星";
            if (itemIndex == 2) return "大星星";
            if (itemIndex == 3) return "超大星星";
            return "星星";
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
            PigFarmAudioService.Play(PigFarmAudioCue.InvalidAction);
            NoticeRequested?.Invoke(message);
            Publish();
        }

        void Publish() { Changed?.Invoke(); }
    }
}
