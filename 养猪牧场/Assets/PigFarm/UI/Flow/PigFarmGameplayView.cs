using PigFarm.Flow;
using PigFarm.Pigs;
using PigFarm.Audio;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmGameplayView : MonoBehaviour
    {
        [SerializeField] PigFarmGameSessionController session;
        [SerializeField] PigFarmTutorialPopup tutorial;
        [SerializeField] PigFarmOpeningShopIntro openingIntro;
        [SerializeField] Text stageText;
        [SerializeField] Text taskText;
        [SerializeField] Text rewardText;
        [SerializeField] Text resourceText;
        [SerializeField] Text capacityText;
        [SerializeField] Text herdText;
        [SerializeField] Text rollRangeText;
        [SerializeField] Text messageText;
        [SerializeField] Image[] stars;
        [SerializeField] Sprite smallStarSprite;
        [SerializeField] Sprite mediumStarSprite;
        [SerializeField] Sprite largeStarSprite;
        [SerializeField] Sprite hugeStarSprite;
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
        [SerializeField] GameObject roundTipPanel;
        [SerializeField] Text roundTipStageText;
        [SerializeField] Text roundTipTaskText;
        [SerializeField] Button tutorialButton;
        [SerializeField] GameObject stageTaskSection;
        [SerializeField] GameObject taskContextSection;
        [SerializeField] GameObject contextCounterPanel;
        [SerializeField] Text contextCounterLabel;
        [SerializeField] Text contextCounterValue;
        [SerializeField] Text contextGuideText;
        [SerializeField] Button penActionButton;
        [SerializeField] GameObject rollResultPanel;
        [SerializeField] Text rollActionText;
        [SerializeField] Text rollNumberText;
        [SerializeField] GameObject shopScreen;
        [SerializeField] Button shopCloseButton;
        [SerializeField] GameObject sellScreen;
        [SerializeField] Button sellCloseButton;
        [SerializeField] Button[] sellPigButtons;
        [SerializeField] Text shopHeaderText;
        [SerializeField] Text shopVaccineCountText;
        [SerializeField] Text shopGoldCountText;
        [SerializeField] Text shopTaskText;
        [SerializeField] Text shopPurchaseCountText;
        [SerializeField] Text shopStarCountText;
        [SerializeField] Text shopTotalValueText;
        [SerializeField] Text[] shopProductNameTexts;
        [SerializeField] Image[] shopProductIcons;
        [SerializeField] Text[] shopProductPriceTexts;

        [SerializeField] PigFarmPenStarSlot[] starSlots;



        Coroutine roundTipRoutine;

        Coroutine starAnimRoutine;
        Coroutine rollRoutine;
        bool initialTutorialCompleted;
        bool openingShopCompleted;
        bool roundUiReady;
        bool rolling;

        bool resultRevealed;

        bool starTargetingActive;

        bool pendingUseItem;
        readonly Color selectedColor = new Color(.91f, .48f, .13f, 1f);
        readonly Color normalColor = new Color(.25f, .40f, .31f, 1f);

        public void SetSession(PigFarmGameSessionController value) { session = value; }

        public void Configure(PigFarmGameSessionController value, PigFarmTutorialPopup tutorialPopup,
            Text stage, Text task, Text reward, Text resources, Text capacity, Text herd, Text rollRange, Text message,
            Image[] starImages, Button[] actions, Button confirm, GameObject actionModal, Text actionTitle, Text actionBody,
            Button primary, Button item, GameObject shop, Button[] shopItems, Button settle, Button vaccinate,
            GameObject transition, Text transitionLabel, Button transitionContinue, GameObject final, Text finalLabel, Button restart,
            GameObject roundTip, Text roundTipStage, Text roundTipTask)
        {
            session = value; tutorial = tutorialPopup; stageText = stage; taskText = task; rewardText = reward;
            resourceText = resources; capacityText = capacity; herdText = herd; rollRangeText = rollRange; messageText = message;
            stars = starImages; actionButtons = actions; confirmButton = confirm; actionPanel = actionModal;
            actionTitleText = actionTitle; actionBodyText = actionBody; primaryActionButton = primary; itemActionButton = item;
            shopPanel = shop; shopButtons = shopItems; settleRoundButton = settle; vaccinateButton = vaccinate;
            transitionPanel = transition; transitionText = transitionLabel; transitionContinueButton = transitionContinue;
            finalPanel = final; finalText = finalLabel; restartButton = restart;
            roundTipPanel = roundTip; roundTipStageText = roundTipStage; roundTipTaskText = roundTipTask;
        }

        public void ConfigureRoundContext(Button tutorialEntry, GameObject firstSection, GameObject secondSection,
            GameObject counterPanel, Text counterLabel, Text counterValue, Text guide, Button penAction)
        {
            tutorialButton = tutorialEntry; stageTaskSection = firstSection; taskContextSection = secondSection;
            contextCounterPanel = counterPanel; contextCounterLabel = counterLabel; contextCounterValue = counterValue;
            contextGuideText = guide; penActionButton = penAction;
        }

        public void ConfigureActionScreens(GameObject rollPanel, Text rollAction, Text rollNumber,
            GameObject shop, Button shopClose, GameObject sell, Button sellClose, Button[] sellButtons)
        {
            rollResultPanel = rollPanel; rollActionText = rollAction; rollNumberText = rollNumber;
            shopScreen = shop; shopCloseButton = shopClose; sellScreen = sell; sellCloseButton = sellClose;
            sellPigButtons = sellButtons;
        }

        public void ConfigureOpeningShop(PigFarmOpeningShopIntro intro, Text header, Text vaccineCount, Text goldCount,
            Text task, Text purchaseCount, Text starCount, Text totalValue,
            Text[] productNames, Image[] productIcons, Text[] productPrices,
            Sprite smallStar, Sprite mediumStar, Sprite largeStar, Sprite hugeStar)
        {
            openingIntro = intro;
            shopHeaderText = header;
            shopVaccineCountText = vaccineCount;
            shopGoldCountText = goldCount;
            shopTaskText = task;
            shopPurchaseCountText = purchaseCount;
            shopStarCountText = starCount;
            shopTotalValueText = totalValue;
            shopProductNameTexts = productNames;
            shopProductIcons = productIcons;
            shopProductPriceTexts = productPrices;
            smallStarSprite = smallStar;
            mediumStarSprite = mediumStar;
            largeStarSprite = largeStar;
            hugeStarSprite = hugeStar;
        }

        void Start()
        {
            Bind(true);
            if (actionPanel) actionPanel.SetActive(false);
            if (transitionPanel) transitionPanel.SetActive(false);
            if (finalPanel) finalPanel.SetActive(false);
            if (roundTipPanel) roundTipPanel.SetActive(false);
            if (rollResultPanel) rollResultPanel.SetActive(false);
            if (shopScreen) shopScreen.SetActive(false);
            if (sellScreen) sellScreen.SetActive(false);
            if (openingIntro) openingIntro.Hide();
            roundUiReady = false;
            openingShopCompleted = false;
            if (tutorial) tutorial.gameObject.SetActive(true);
            Refresh();
        }

        void OnDestroy() { StopStarIdleAnimations(); Bind(false); }

        void Bind(bool bind)
        {
            if (!session) return;
            if (bind)
            {
                session.Changed += Refresh;
                session.ActionRolled += OnActionRolled;
                session.RoundResolved += OnRoundResolved;
                session.GameCompleted += OnGameCompleted;
                if (tutorial) tutorial.Completed += OnTutorialCompleted;
                if (openingIntro) openingIntro.EnterShopRequested += OpenOpeningShop;
            }
            else
            {
                session.Changed -= Refresh;
                session.ActionRolled -= OnActionRolled;
                session.RoundResolved -= OnRoundResolved;
                session.GameCompleted -= OnGameCompleted;
                if (tutorial) tutorial.Completed -= OnTutorialCompleted;
                if (openingIntro) openingIntro.EnterShopRequested -= OpenOpeningShop;
            }
            if (!bind) return;
            for (int i = 0; actionButtons != null && i < actionButtons.Length; i++)
            {
                int index = i;
                actionButtons[i].onClick.AddListener(delegate { Click(); session.ToggleAction((PigFarmActionType)index); });
            }
            if (confirmButton) confirmButton.onClick.AddListener(ConfirmSelection);
            if (primaryActionButton) primaryActionButton.onClick.AddListener(delegate { Click(); session.ExecutePrimaryAction(false); });
            if (itemActionButton) itemActionButton.onClick.AddListener(OnItemActionClicked);
            for (int i = 0; shopButtons != null && i < shopButtons.Length; i++)
            {
                int index = i;
                shopButtons[i].onClick.AddListener(delegate
                {
                    Click();
                    session.BuyShopItem(index);
                    RefreshShopScreen();
                    if (!session.IsOpeningShopActive && !session.HasRolledAction && shopScreen) shopScreen.SetActive(false);
                });
            }
            if (settleRoundButton) settleRoundButton.onClick.AddListener(delegate { Click(); session.ResolveRound(); });
            if (vaccinateButton) vaccinateButton.onClick.AddListener(delegate { Click(); session.VaccinateOnePig(); });
            if (transitionContinueButton) transitionContinueButton.onClick.AddListener(HideTransition);
            if (restartButton) restartButton.onClick.AddListener(Restart);
            if (tutorialButton) tutorialButton.onClick.AddListener(ShowTutorial);
            if (penActionButton) penActionButton.onClick.AddListener(ExecutePenAction);
            if (shopCloseButton) shopCloseButton.onClick.AddListener(CloseShop);
            if (sellCloseButton) sellCloseButton.onClick.AddListener(delegate { Click(); if (sellScreen) sellScreen.SetActive(false); });
            for (int i = 0; sellPigButtons != null && i < sellPigButtons.Length; i++)
            {
                int index = i;
                sellPigButtons[i].onClick.AddListener(delegate { SellPigAt(index); });
            }
        }

        void Refresh()
        {
            if (!session || !session.Flow.isComplete && session.Flow.totalRounds == 0) return;
            var flow = session.Flow;
            PigFarmRoundTask task = session.CurrentTask;
            if (stageText) stageText.text = "�?" + ChineseNumber(flow.seasonIndex + 1) + "阶段 · 回合" + ChineseNumber(flow.roundInSeason);
            if (taskText) taskText.text = task == null ? "等待任务" : task.title + "\n" + task.description;
            if (rewardText) rewardText.text = task == null ? string.Empty : "本局奖励�?" + RewardName(task.rewardType) + " × " + task.rewardAmount;
            if (resourceText) resourceText.text = "金币 " + flow.coins + "    疫苗 " + session.Vaccines + "    营养�? " + session.Nutrition + "    护符 " + session.Charms;
            if (capacityText) capacityText.text = "猪圈占用 " + session.UsedCells + " / " + session.Capacity;
            if (rollRangeText)
            {
                Vector2Int range = session.CurrentRollRange;
                rollRangeText.text = session.SelectedActionCount == 0 ? "选择 1�?3 种行�?" : "可能次数  " + range.x + "�?" + range.y;
            }
            if (messageText) messageText.text = session.LastMessage;
            RefreshHerd();
            RefreshActions();
            RefreshActionPanel();
            RefreshRoundContext();
            if (shopScreen && shopScreen.activeSelf) RefreshShopScreen();
        }

        void RefreshHerd()

        {

            EnsureStarSlots();

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

            if (herdText) herdText.text = "小星�? " + babies + "   中星�? " + small + "   大星�? " + medium + "   超大星星 " + large + "   已接�? " + vaccinated;

            if (stars != null)

            {

                for (int i = 0; i < stars.Length; i++)

                {

                    bool visible = i < pigs.Count;

                    stars[i].gameObject.SetActive(visible);

                    if (!visible) continue;

                    Sprite sprite = StarSpriteForStage(pigs[i].stageId);

                    if (sprite) stars[i].sprite = sprite;

                    stars[i].color = Color.white;

                    stars[i].preserveAspect = true;

                    stars[i].raycastTarget = true;

                    if (starSlots != null && i < starSlots.Length && starSlots[i] && starSlots[i].Button)

                        starSlots[i].Button.interactable = starTargetingActive;

                }

            }

        }



        Sprite StarSpriteForStage(string stageId)
        {
            if (stageId == "baby") return smallStarSprite;
            if (stageId == "small") return mediumStarSprite;
            if (stageId == "medium") return largeStarSprite;
            if (stageId == "large") return hugeStarSprite;
            return smallStarSprite;
        }

        void RefreshActions()
        {
            bool canSelect = openingShopCompleted && !session.IsOpeningShopActive && !session.HasRolledAction && !session.AwaitingRoundEnd && !session.IsGameComplete;
            for (int i = 0; actionButtons != null && i < actionButtons.Length; i++)
            {
                bool selected = session.IsActionSelected((PigFarmActionType)i);
                Image image = actionButtons[i].GetComponent<Image>();
                if (image) image.color = selected ? selectedColor : normalColor;
                Text label = actionButtons[i].GetComponentInChildren<Text>();
                if (label) label.text = (selected ? "? " : "") + ActionName((PigFarmActionType)i);
                actionButtons[i].interactable = canSelect;
            }
            if (confirmButton)
            {
                confirmButton.interactable = canSelect && session.SelectedActionCount > 0;
                Text label = confirmButton.GetComponentInChildren<Text>();
                if (label) label.text = "抽取（已�? " + session.SelectedActionCount + "/3�?";
            }
        }

        void RefreshActionPanel()
        {
            if (!actionPanel) return;
            bool penAction = resultRevealed && session.HasRolledAction &&
                (session.CurrentAction == PigFarmActionType.Feed || session.CurrentAction == PigFarmActionType.Breed);
            bool show = (resultRevealed && session.HasRolledAction && !penAction && session.CurrentAction != PigFarmActionType.Shop && session.CurrentAction != PigFarmActionType.Sell) || session.AwaitingRoundEnd;
            actionPanel.SetActive(show);
            if (!show) return;
            PigFarmActionType action = session.CurrentAction;
            if (actionTitleText) actionTitleText.text = "行动结果�?" + ActionName(action) + " × " + session.CurrentActionRemaining;
            bool isShop = action == PigFarmActionType.Shop && session.HasRolledAction;
            if (shopPanel) shopPanel.SetActive(false);
            if (primaryActionButton) primaryActionButton.gameObject.SetActive(!isShop && session.HasRolledAction);
            if (itemActionButton) itemActionButton.gameObject.SetActive((action == PigFarmActionType.Breed || action == PigFarmActionType.Feed) && session.HasRolledAction);
            if (settleRoundButton) settleRoundButton.gameObject.SetActive(session.AwaitingRoundEnd);
            if (actionBodyText) actionBodyText.text = ActionDescription(action);
            if (primaryActionButton)
            {
                Text label = primaryActionButton.GetComponentInChildren<Text>();
                if (label) label.text = action == PigFarmActionType.Sell ? "卖出价值最高的�?" : action == PigFarmActionType.Breed ? "普通生�?" : "普通喂�?";
            }
            if (itemActionButton)
            {
                Text label = itemActionButton.GetComponentInChildren<Text>();
                if (label) label.text = action == PigFarmActionType.Breed ? "使用护符生育" : "使用营养剂喂�?";
            }
        }

        void RefreshRoundContext()
        {
            bool tutorialVisible = tutorial && tutorial.gameObject.activeSelf;
            bool tipVisible = roundTipPanel && roundTipPanel.activeSelf;
            bool introVisible = openingIntro && openingIntro.gameObject.activeSelf;
            bool unlocked = roundUiReady && openingShopCompleted && !tutorialVisible && !tipVisible && !introVisible && !session.IsOpeningShopActive;
            if (stageTaskSection) stageTaskSection.SetActive(unlocked);
            if (taskContextSection) taskContextSection.SetActive(unlocked);

            bool selecting = unlocked && !rolling && !session.HasRolledAction && !session.AwaitingRoundEnd && !session.IsGameComplete;
            GameObject actionArea = actionButtons != null && actionButtons.Length > 0 && actionButtons[0]
                ? actionButtons[0].transform.parent.gameObject : null;
            if (actionArea) actionArea.SetActive(selecting);

            bool feed = unlocked && resultRevealed && session.HasRolledAction && session.CurrentAction == PigFarmActionType.Feed;
            bool breed = unlocked && resultRevealed && session.HasRolledAction && session.CurrentAction == PigFarmActionType.Breed;
            bool shop = unlocked && resultRevealed && session.HasRolledAction && session.CurrentAction == PigFarmActionType.Shop;
            bool sell = unlocked && resultRevealed && session.HasRolledAction && session.CurrentAction == PigFarmActionType.Sell;
            bool contextualAction = feed || breed || shop || sell;
            if (contextCounterPanel) contextCounterPanel.SetActive(contextualAction);
            if (contextCounterLabel) contextCounterLabel.text = feed ? "喂养次数" : breed ? "繁殖次数" : shop ? "购买次数" : sell ? "出售次数" : "行动次数";
            if (contextCounterValue) contextCounterValue.text = session.CurrentActionRemaining + "/" + session.CurrentActionTotal;
            if (contextGuideText)
            {
                if (selecting) contextGuideText.text = "根据你的选择的行动数量，可能会获得不一样的次数机会�?";
                else if (feed) contextGuideText.text = "选择你需要喂养的小猪";
                else if (breed) contextGuideText.text = "选择可繁殖的中猪或大�?";
                else if (shop) contextGuideText.text = "进入商店选择需要购买的星星或道�?";
                else if (sell) contextGuideText.text = "进入出售界面选择需要卖出的�?";
                else if (session.AwaitingRoundEnd) contextGuideText.text = "本回合行动完成，请进行回合结�?";
                else contextGuideText.text = "请完成本回合行动";
            }
            if (penActionButton)
            {
                penActionButton.gameObject.SetActive(contextualAction);
                Text label = penActionButton.GetComponentInChildren<Text>();
                if (label) label.text = feed ? "喂养" : breed ? "繁殖" : shop ? "商店" : "出售";
            }
        }

        void RefreshShopScreen()
        {
            if (!session || !shopScreen) return;
            bool opening = session.IsOpeningShopActive;
            PigFarmRoundTask task = session.CurrentTask;
            if (shopHeaderText) shopHeaderText.text = opening ? "初始采购" : "商店";
            if (shopVaccineCountText) shopVaccineCountText.text = session.Vaccines.ToString();
            if (shopGoldCountText) shopGoldCountText.text = session.Flow.coins.ToString();
            if (shopTaskText)
            {
                if (task == null) shopTaskText.text = "当前暂无任务";
                else shopTaskText.text = task.title + "\n" + task.description;
            }
            if (shopPurchaseCountText)
            {
                shopPurchaseCountText.text = opening
                    ? "购买次数  " + session.OpeningPurchaseCount
                    : "购买次数  " + session.CurrentActionRemaining + "/" + session.CurrentActionTotal;
            }
            int baby, small, medium, large, totalValue;
            session.GetHerdCounts(out baby, out small, out medium, out large, out totalValue);
            if (shopStarCountText)
                shopStarCountText.text = "小星�? " + baby + "\n中星�? " + small + "\n大星�? " + medium + "\n超大星星 " + large;
            if (shopTotalValueText) shopTotalValueText.text = "总价�?  " + totalValue;

            string[] names = { "小星�?", "中星�?", "大星�?", "超大星星", "营养�?", "护符", "疫苗" };
            for (int i = 0; shopButtons != null && i < shopButtons.Length; i++)
            {
                int price = session.GetShopItemPrice(i);
                if (shopProductNameTexts != null && i < shopProductNameTexts.Length && shopProductNameTexts[i])
                    shopProductNameTexts[i].text = names[i];
                if (shopProductPriceTexts != null && i < shopProductPriceTexts.Length && shopProductPriceTexts[i])
                    shopProductPriceTexts[i].text = price + " 金币";
                shopButtons[i].interactable = session.Flow.coins >= price &&
                    (opening || (session.HasRolledAction && session.CurrentAction == PigFarmActionType.Shop));
            }
            if (shopCloseButton)
            {
                Text label = shopCloseButton.GetComponentInChildren<Text>();
                if (label) label.text = opening ? "完成采购" : "离开商店";
            }
        }

        void OnItemActionClicked()

        {

            Click();

            if (!session) return;

            if (session.HasRolledAction && (session.CurrentAction == PigFarmActionType.Feed || session.CurrentAction == PigFarmActionType.Breed))

                EnterStarTargeting(true);

            else session.ExecutePrimaryAction(true);

        }



        void ConfirmSelection()
        {
            Click();
            if (rolling || !session || session.SelectedActionCount == 0) return;
            resultRevealed = false;
            session.ConfirmActionSelection();
        }

        void OnActionRolled(PigFarmActionType action, int count)
        {
            if (rollRoutine != null) StopCoroutine(rollRoutine);
            rollRoutine = StartCoroutine(PlayRollResult(action, count));
        }

        IEnumerator PlayRollResult(PigFarmActionType action, int finalCount)
        {
            rolling = true;
            if (rollResultPanel)
            {
                rollResultPanel.transform.SetAsLastSibling();
                rollResultPanel.SetActive(true);
            }
            if (rollActionText) rollActionText.text = "?\n" + ActionName(action);
            float elapsed = 0f;
            Vector2Int range = session.CurrentRollRange;
            while (elapsed < 2f)
            {
                if (rollNumberText) rollNumberText.text = Random.Range(range.x, range.y + 1).ToString();
                yield return new WaitForSecondsRealtime(.08f);
                elapsed += .08f;
            }
            if (rollNumberText) rollNumberText.text = finalCount.ToString();
            yield return new WaitForSecondsRealtime(.25f);
            if (rollResultPanel) rollResultPanel.SetActive(false);
            rolling = false;
            resultRevealed = true;
            if (session && session.TryAutoEndRoundIfActionImpossible())
            {
                resultRevealed = false;
                ClearStarTargeting();
                if (shopScreen) shopScreen.SetActive(false);
                if (sellScreen) sellScreen.SetActive(false);
            }
            else OpenRolledActionScreen();
            Refresh();
            rollRoutine = null;
        }

        void OpenRolledActionScreen()
        {
            if (!session) return;
            if (!session.CanPerformCurrentAction()) return;
            if (session.CurrentAction == PigFarmActionType.Shop && shopScreen)
            {
                shopScreen.transform.SetAsLastSibling();
                shopScreen.SetActive(true);
                RefreshShopScreen();
            }
            else if (session.CurrentAction == PigFarmActionType.Sell && sellScreen)
            {
                RefreshSellCards();
                sellScreen.transform.SetAsLastSibling();
                sellScreen.SetActive(true);
            }
        }

        void RefreshSellCards()
        {
            var pigs = session.Pigs;
            for (int i = 0; sellPigButtons != null && i < sellPigButtons.Length; i++)
            {
                bool visible = i < pigs.Count;
                sellPigButtons[i].gameObject.SetActive(visible);
                if (!visible) continue;
                Text label = sellPigButtons[i].GetComponentInChildren<Text>();
                string starName = StarNameForStage(pigs[i].stageId);
                if (label) label.text = starName + "\n价�? " + pigs[i].value + " 金币";
            }
        }

        static string StarNameForStage(string stageId)
        {
            if (stageId == "baby") return "小星�?";
            if (stageId == "small") return "中星�?";
            if (stageId == "medium") return "大星�?";
            if (stageId == "large") return "超大星星";
            return "星星";
        }

        void SellPigAt(int index)
        {
            if (!session || index < 0 || index >= session.Pigs.Count) return;
            Click();
            session.SellPig(session.Pigs[index].id);
            RefreshSellCards();
            if (!session.HasRolledAction && sellScreen) sellScreen.SetActive(false);
        }

        void OnRoundResolved(int round, string result)
        {
            resultRevealed = false;
            ClearStarTargeting();
            if (shopScreen) shopScreen.SetActive(false);
            if (sellScreen) sellScreen.SetActive(false);
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

        void Click() { PigFarmAudioService.Play(PigFarmAudioCue.UiClick); }
        void OnTutorialCompleted()
        {
            Refresh();
            if (initialTutorialCompleted) return;
            initialTutorialCompleted = true;
            ShowRoundTip();
        }

        void ShowTutorial()
        {
            Click();
            if (tutorial) tutorial.gameObject.SetActive(true);
            Refresh();
        }

        void ExecutePenAction()

        {

            Click();

            if (!session) return;

            if (session.CurrentAction == PigFarmActionType.Shop && shopScreen)

            {

                ClearStarTargeting();

                shopScreen.transform.SetAsLastSibling();

                shopScreen.SetActive(true);

                RefreshShopScreen();

            }

            else if (session.CurrentAction == PigFarmActionType.Sell && sellScreen)

            {

                ClearStarTargeting();

                RefreshSellCards();

                sellScreen.transform.SetAsLastSibling();

                sellScreen.SetActive(true);

            }

            else if (session.CurrentAction == PigFarmActionType.Feed || session.CurrentAction == PigFarmActionType.Breed)

            {

                EnterStarTargeting(false);

            }

            else session.ExecutePrimaryAction(false);

        }



        void EnterStarTargeting(bool useItem)

        {

            if (!session || !session.HasRolledAction) return;

            if (session.CurrentAction != PigFarmActionType.Feed && session.CurrentAction != PigFarmActionType.Breed) return;

            starTargetingActive = true;

            pendingUseItem = useItem;

            if (messageText)

            {

                if (session.CurrentAction == PigFarmActionType.Feed)

                    messageText.text = useItem ? "请选择要喂养的星星（将使用营养剂）" : "请选择要喂养的星星";

                else

                    messageText.text = useItem ? "请选择要繁殖的星星（将使用护符�?" : "请选择要繁殖的大星星或超大星星";

            }

            RefreshHerd();

        }



        void ClearStarTargeting()

        {

            starTargetingActive = false;

            pendingUseItem = false;

        }



        void OnStarClicked(int index)

        {

            Click();

            if (!session || !starTargetingActive) return;

            if (index < 0 || index >= session.Pigs.Count) return;

            int pigId = session.Pigs[index].id;

            bool success = false;

            if (session.CurrentAction == PigFarmActionType.Feed)

                success = session.ExecuteFeedOnPig(pigId, pendingUseItem);

            else if (session.CurrentAction == PigFarmActionType.Breed)

                success = session.ExecuteBreedOnPig(pigId, pendingUseItem);



            if (success && starSlots != null && index < starSlots.Length && starSlots[index])

                starSlots[index].PlayFloat();



            if (!session.HasRolledAction) ClearStarTargeting();

            else

            {

                // keep targeting for remaining actions, but reset item intent after one use

                pendingUseItem = false;

            }

            Refresh();

        }



        void EnsureStarSlots()

        {

            if (stars == null || stars.Length == 0) return;

            if (starSlots == null || starSlots.Length != stars.Length) starSlots = new PigFarmPenStarSlot[stars.Length];

            for (int i = 0; i < stars.Length; i++)

            {

                if (!stars[i]) continue;

                PigFarmPenStarSlot slot = starSlots[i];

                if (!slot) slot = stars[i].GetComponent<PigFarmPenStarSlot>();

                if (!slot) slot = stars[i].gameObject.AddComponent<PigFarmPenStarSlot>();

                Button button = stars[i].GetComponent<Button>();

                if (!button) button = stars[i].gameObject.AddComponent<Button>();

                button.targetGraphic = stars[i];

                stars[i].raycastTarget = true;

                button.transition = Selectable.Transition.None;

                slot.Configure(i, button, stars[i]);

                starSlots[i] = slot;

                button.onClick.RemoveAllListeners();

                int index = i;

                button.onClick.AddListener(delegate { OnStarClicked(index); });

            }

        }



        void StartStarIdleAnimations()

        {

            if (starAnimRoutine != null) StopCoroutine(starAnimRoutine);

            starAnimRoutine = StartCoroutine(StarIdleAnimationLoop());

        }



        void StopStarIdleAnimations()

        {

            if (starAnimRoutine != null)

            {

                StopCoroutine(starAnimRoutine);

                starAnimRoutine = null;

            }

            if (starSlots == null) return;

            for (int i = 0; i < starSlots.Length; i++) if (starSlots[i]) starSlots[i].StopFloat();

        }



        IEnumerator StarIdleAnimationLoop()

        {

            yield return null;

            PlayVisibleStarFloats(true);

            while (true)

            {

                yield return new WaitForSecondsRealtime(5f);

                PlayVisibleStarFloats(false);

            }

        }



        void PlayVisibleStarFloats(bool allVisible)

        {

            if (stars == null || starSlots == null || !session) return;

            var pigs = session.Pigs;

            var candidates = new System.Collections.Generic.List<int>();

            for (int i = 0; i < stars.Length && i < pigs.Count; i++)

            {

                if (!stars[i] || !stars[i].gameObject.activeInHierarchy) continue;

                candidates.Add(i);

            }

            if (candidates.Count == 0) return;

            if (allVisible)

            {

                for (int i = 0; i < candidates.Count; i++)

                {

                    int idx = candidates[i];

                    if (starSlots[idx]) starSlots[idx].PlayFloat();

                }

                return;

            }

            int count = Mathf.Min(3, candidates.Count);

            for (int i = 0; i < count; i++)

            {

                int pick = Random.Range(i, candidates.Count);

                int tmp = candidates[i];

                candidates[i] = candidates[pick];

                candidates[pick] = tmp;

            }

            for (int i = 0; i < count; i++)

            {

                int idx = candidates[i];

                if (starSlots[idx]) starSlots[idx].PlayFloat();

            }

        }



        void ShowRoundTip()
        {
            if (!roundTipPanel || !session || session.IsGameComplete) return;
            roundUiReady = false;
            var flow = session.Flow;
            PigFarmRoundTask task = session.CurrentTask;
            if (roundTipStageText) roundTipStageText.text = "�?" + ChineseNumber(flow.seasonIndex + 1) + "阶段 · 回合" + ChineseNumber(flow.roundInSeason);
            if (roundTipTaskText) roundTipTaskText.text = task == null ? "本回合暂无任�?" : task.title + "\n" + task.description;
            roundTipPanel.transform.SetAsLastSibling();
            roundTipPanel.SetActive(true);
            Refresh();
            if (roundTipRoutine != null) StopCoroutine(roundTipRoutine);
            roundTipRoutine = StartCoroutine(HideRoundTipAfterDelay());
        }

        IEnumerator HideRoundTipAfterDelay()
        {
            yield return new WaitForSecondsRealtime(2f);
            if (roundTipPanel) roundTipPanel.SetActive(false);
            if (!openingShopCompleted)
            {
                ShowOpeningIntro();
            }
            else
            {
                roundUiReady = true;
                Refresh();
                StartStarIdleAnimations();
            }
            roundTipRoutine = null;
        }

        void ShowOpeningIntro()
        {
            roundUiReady = false;
            if (openingIntro)
            {
                openingIntro.Show("欢迎来到养猪牧场！\n请使用初始金币购买星星与道具，\n购买的星星会放入猪圈中�?");
            }
            else OpenOpeningShop();
            Refresh();
        }

        void OpenOpeningShop()
        {
            if (!session) return;
            session.BeginOpeningShop();
            if (shopScreen)
            {
                shopScreen.transform.SetAsLastSibling();
                shopScreen.SetActive(true);
                RefreshShopScreen();
            }
            Refresh();
        }

        void CloseShop()
        {
            Click();
            if (!shopScreen) return;
            if (session && session.IsOpeningShopActive)
            {
                session.EndOpeningShop();
                openingShopCompleted = true;
                roundUiReady = true;
            }
            shopScreen.SetActive(false);
            Refresh();
            if (openingShopCompleted && roundUiReady) StartStarIdleAnimations();
        }

        static string ChineseNumber(int value)
        {
            string[] values = { "�?", "一", "�?", "�?", "�?", "�?", "�?", "�?", "�?", "�?", "�?", "十一", "十二", "十三", "十四", "十五", "十六" };
            return value >= 0 && value < values.Length ? values[value] : value.ToString();
        }

        void HideTransition() { Click(); if (transitionPanel) transitionPanel.SetActive(false); Refresh(); ShowRoundTip(); }
        void Restart() { Click(); SceneManager.LoadScene("PigFarmStart"); }

        static string ActionName(PigFarmActionType type)
        {
            if (type == PigFarmActionType.Breed) return "生育";
            if (type == PigFarmActionType.Feed) return "喂养";
            if (type == PigFarmActionType.Shop) return "商店购买";
            return "卖猪";
        }

        static string ActionDescription(PigFarmActionType type)
        {
            if (type == PigFarmActionType.Breed) return "中猪或大猪可生育小星星；护符可让新生星星直接成为中星星�?";
            if (type == PigFarmActionType.Feed) return "选择可成长的星星提升 1 级；营养剂可以连续成�? 2 级�?";
            if (type == PigFarmActionType.Shop) return "每次购买消耗一次行动。星星按价值售卖，道具均为 1 金币�?";
            return "自动卖出当前价值最高的星星，并立刻获得对应金币�?";
        }

        static string RewardName(PigFarmRewardType type)
        {
            if (type == PigFarmRewardType.Coins) return "金币";
            if (type == PigFarmRewardType.Nutrition) return "营养�?";
            if (type == PigFarmRewardType.Charm) return "护符";
            return "疫苗";
        }
    }
}
