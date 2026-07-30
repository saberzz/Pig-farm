using PigFarm.Flow;
using PigFarm.Pigs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmGameplayView : MonoBehaviour
    {
        [SerializeField] PigFarmGameSessionController session;
        [SerializeField] PigFarmTutorialPopup tutorial;
        [SerializeField] Text stageText;
        [SerializeField] Text taskText;
        [SerializeField] Text rewardText;
        [SerializeField] Text resourceText;
        [SerializeField] Text capacityText;
        [SerializeField] Text herdText;
        [SerializeField] Text rollRangeText;
        [SerializeField] Text messageText;
        [SerializeField] Image[] stars;
        [SerializeField] Button[] actionButtons;
        [SerializeField] Button confirmButton;
        [SerializeField] GameObject actionPanel;
        [SerializeField] Text actionTitleText;
        [SerializeField] Text actionBodyText;
        [SerializeField] Button primaryActionButton;
        [SerializeField] Button itemActionButton;
        [SerializeField] GameObject shopPanel;
        [SerializeField] Button[] shopButtons;
        [SerializeField] Button settleRoundButton;
        [SerializeField] Button vaccinateButton;
        [SerializeField] GameObject transitionPanel;
        [SerializeField] Text transitionText;
        [SerializeField] Button transitionContinueButton;
        [SerializeField] GameObject finalPanel;
        [SerializeField] Text finalText;
        [SerializeField] Button restartButton;
        readonly Color selectedColor = new Color(.91f, .48f, .13f, 1f);
        readonly Color normalColor = new Color(.25f, .40f, .31f, 1f);

        public void SetSession(PigFarmGameSessionController value) { session = value; }

        public void Configure(PigFarmGameSessionController value, PigFarmTutorialPopup tutorialPopup,
            Text stage, Text task, Text reward, Text resources, Text capacity, Text herd, Text rollRange, Text message,
            Image[] starImages, Button[] actions, Button confirm, GameObject actionModal, Text actionTitle, Text actionBody,
            Button primary, Button item, GameObject shop, Button[] shopItems, Button settle, Button vaccinate,
            GameObject transition, Text transitionLabel, Button transitionContinue, GameObject final, Text finalLabel, Button restart)
        {
            session = value; tutorial = tutorialPopup; stageText = stage; taskText = task; rewardText = reward;
            resourceText = resources; capacityText = capacity; herdText = herd; rollRangeText = rollRange; messageText = message;
            stars = starImages; actionButtons = actions; confirmButton = confirm; actionPanel = actionModal;
            actionTitleText = actionTitle; actionBodyText = actionBody; primaryActionButton = primary; itemActionButton = item;
            shopPanel = shop; shopButtons = shopItems; settleRoundButton = settle; vaccinateButton = vaccinate;
            transitionPanel = transition; transitionText = transitionLabel; transitionContinueButton = transitionContinue;
            finalPanel = final; finalText = finalLabel; restartButton = restart;
        }

        void Start()
        {
            Bind(true);
            if (actionPanel) actionPanel.SetActive(false);
            if (transitionPanel) transitionPanel.SetActive(false);
            if (finalPanel) finalPanel.SetActive(false);
            if (tutorial) tutorial.gameObject.SetActive(true);
            Refresh();
        }

        void OnDestroy() { Bind(false); }

        void Bind(bool bind)
        {
            if (!session) return;
            if (bind)
            {
                session.Changed += Refresh;
                session.ActionRolled += OnActionRolled;
                session.RoundResolved += OnRoundResolved;
                session.GameCompleted += OnGameCompleted;
                if (tutorial) tutorial.Completed += Refresh;
            }
            else
            {
                session.Changed -= Refresh;
                session.ActionRolled -= OnActionRolled;
                session.RoundResolved -= OnRoundResolved;
                session.GameCompleted -= OnGameCompleted;
                if (tutorial) tutorial.Completed -= Refresh;
            }
            if (!bind) return;
            for (int i = 0; actionButtons != null && i < actionButtons.Length; i++)
            {
                int index = i;
                actionButtons[i].onClick.AddListener(delegate { session.ToggleAction((PigFarmActionType)index); });
            }
            if (confirmButton) confirmButton.onClick.AddListener(session.ConfirmActionSelection);
            if (primaryActionButton) primaryActionButton.onClick.AddListener(delegate { session.ExecutePrimaryAction(false); });
            if (itemActionButton) itemActionButton.onClick.AddListener(delegate { session.ExecutePrimaryAction(true); });
            for (int i = 0; shopButtons != null && i < shopButtons.Length; i++)
            {
                int index = i;
                shopButtons[i].onClick.AddListener(delegate { session.BuyShopItem(index); });
            }
            if (settleRoundButton) settleRoundButton.onClick.AddListener(session.ResolveRound);
            if (vaccinateButton) vaccinateButton.onClick.AddListener(session.VaccinateOnePig);
            if (transitionContinueButton) transitionContinueButton.onClick.AddListener(HideTransition);
            if (restartButton) restartButton.onClick.AddListener(Restart);
        }

        void Refresh()
        {
            if (!session || !session.Flow.isComplete && session.Flow.totalRounds == 0) return;
            var flow = session.Flow;
            PigFarmRoundTask task = session.CurrentTask;
            if (stageText) stageText.text = flow.seasonName + " · 第 " + flow.round + " 回合";
            if (taskText) taskText.text = task == null ? "等待任务" : task.title + "\n" + task.description;
            if (rewardText) rewardText.text = task == null ? string.Empty : "本轮奖励：" + RewardName(task.rewardType) + " × " + task.rewardAmount;
            if (resourceText) resourceText.text = "金币 " + flow.coins + "    疫苗 " + session.Vaccines + "    营养剂 " + session.Nutrition + "    护符 " + session.Charms;
            if (capacityText) capacityText.text = "猪圈占用 " + session.UsedCells + " / " + session.Capacity;
            if (rollRangeText)
            {
                Vector2Int range = session.CurrentRollRange;
                rollRangeText.text = session.SelectedActionCount == 0 ? "选择 1～3 种行动" : "可能次数  " + range.x + "～" + range.y;
            }
            if (messageText) messageText.text = session.LastMessage;
            RefreshHerd();
            RefreshActions();
            RefreshActionPanel();
        }

        void RefreshHerd()
        {
            int babies = 0, small = 0, medium = 0, large = 0, vaccinated = 0;
            var pigs = session.Pigs;
            for (int i = 0; i < pigs.Count; i++)
            {
                if (pigs[i].stageId == "baby") babies++;
                else if (pigs[i].stageId == "small") small++;
                else if (pigs[i].stageId == "medium") medium++;
                else if (pigs[i].stageId == "large") large++;
                if (pigs[i].vaccinated) vaccinated++;
            }
            if (herdText) herdText.text = "猪宝宝 " + babies + "   小猪 " + small + "   中猪 " + medium + "   大猪 " + large + "   已接种 " + vaccinated;
            if (stars != null)
            {
                int visible = session.Rules ? Mathf.Min(stars.Length, session.Rules.displayStarCount) : stars.Length;
                for (int i = 0; i < stars.Length; i++) stars[i].gameObject.SetActive(i < visible);
            }
        }

        void RefreshActions()
        {
            bool canSelect = !session.HasRolledAction && !session.AwaitingRoundEnd && !session.IsGameComplete;
            for (int i = 0; actionButtons != null && i < actionButtons.Length; i++)
            {
                bool selected = session.IsActionSelected((PigFarmActionType)i);
                Image image = actionButtons[i].GetComponent<Image>();
                if (image) image.color = selected ? selectedColor : normalColor;
                actionButtons[i].interactable = canSelect;
            }
            if (confirmButton) confirmButton.interactable = canSelect && session.SelectedActionCount > 0;
        }

        void RefreshActionPanel()
        {
            if (!actionPanel) return;
            bool show = session.HasRolledAction || session.AwaitingRoundEnd;
            actionPanel.SetActive(show);
            if (!show) return;
            PigFarmActionType action = session.CurrentAction;
            if (actionTitleText) actionTitleText.text = "行动结果：" + ActionName(action) + " × " + session.CurrentActionRemaining;
            bool isShop = action == PigFarmActionType.Shop && session.HasRolledAction;
            if (shopPanel) shopPanel.SetActive(isShop);
            if (primaryActionButton) primaryActionButton.gameObject.SetActive(!isShop && session.HasRolledAction);
            if (itemActionButton) itemActionButton.gameObject.SetActive((action == PigFarmActionType.Breed || action == PigFarmActionType.Feed) && session.HasRolledAction);
            if (settleRoundButton) settleRoundButton.gameObject.SetActive(session.AwaitingRoundEnd);
            if (actionBodyText) actionBodyText.text = ActionDescription(action);
            if (primaryActionButton)
            {
                Text label = primaryActionButton.GetComponentInChildren<Text>();
                if (label) label.text = action == PigFarmActionType.Sell ? "卖出价值最高的猪" : action == PigFarmActionType.Breed ? "普通生育" : "普通喂养";
            }
            if (itemActionButton)
            {
                Text label = itemActionButton.GetComponentInChildren<Text>();
                if (label) label.text = action == PigFarmActionType.Breed ? "使用护符生育" : "使用营养剂喂养";
            }
        }

        void OnActionRolled(PigFarmActionType action, int count) { Refresh(); }

        void OnRoundResolved(int round, string result)
        {
            if (transitionPanel) transitionPanel.SetActive(true);
            if (transitionText) transitionText.text = "第 " + round + " 回合结算\n\n" + result + "\n\n下一回合任务已发布";
        }

        void OnGameCompleted(int total)
        {
            if (actionPanel) actionPanel.SetActive(false);
            if (transitionPanel) transitionPanel.SetActive(false);
            if (finalPanel) finalPanel.SetActive(true);
            if (finalText) finalText.text = "16 回合经营结束\n\n" + session.LastMessage + "\n\n最终得分：" + total;
        }

        void HideTransition() { if (transitionPanel) transitionPanel.SetActive(false); Refresh(); }
        void Restart() { SceneManager.LoadScene("PigFarmStart"); }

        static string ActionName(PigFarmActionType type)
        {
            if (type == PigFarmActionType.Breed) return "生育";
            if (type == PigFarmActionType.Feed) return "喂养";
            if (type == PigFarmActionType.Shop) return "商店购买";
            return "卖猪";
        }

        static string ActionDescription(PigFarmActionType type)
        {
            if (type == PigFarmActionType.Breed) return "中猪或大猪可生育猪宝宝；护符可让新生猪直接成为小猪。";
            if (type == PigFarmActionType.Feed) return "选择可成长的猪提升 1 级；营养剂可以连续成长 2 级。";
            if (type == PigFarmActionType.Shop) return "每次购买消耗一次行动。猪宝宝 3 金币，道具均为 1 金币。";
            return "自动卖出当前价值最高的猪，并立即获得对应金币。";
        }

        static string RewardName(PigFarmRewardType type)
        {
            if (type == PigFarmRewardType.Coins) return "金币";
            if (type == PigFarmRewardType.Nutrition) return "营养剂";
            if (type == PigFarmRewardType.Charm) return "护符";
            return "疫苗";
        }
    }
}
