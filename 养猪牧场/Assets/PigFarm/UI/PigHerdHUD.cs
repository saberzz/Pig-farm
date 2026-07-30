using System.Collections.Generic;
using PigFarm.Pigs;
using UnityEngine;
using UnityEngine.UI;

namespace PigFarm.UI
{
    public sealed class PigHerdHUD : MonoBehaviour
    {
        [SerializeField] private PigHerdController source;
        [SerializeField] private Text capacityText;
        [SerializeField] private Text pigListText;
        [SerializeField] private Text selectedText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button growButton;
        [SerializeField] private Button nutritionButton;
        [SerializeField] private Button birthButton;
        private int selectedIndex;
        private Font runtimeFont;

        public void Configure(Text capacity, Text list, Text selected, Text message, Button previous, Button next, Button grow, Button nutrition, Button birth)
        {
            capacityText = capacity;
            pigListText = list;
            selectedText = selected;
            messageText = message;
            previousButton = previous;
            nextButton = next;
            growButton = grow;
            nutritionButton = nutrition;
            birthButton = birth;
        }

        private void OnEnable()
        {
            ApplyChineseFont();
            if (!source)
                source = FindObjectOfType<PigHerdController>();
            BindButtons(true);
            Subscribe(true);
            if (source && source.IsInitialized)
                source.EnterPenView();
            Refresh();
        }

        private void Start()
        {
            if (source && source.IsInitialized)
            {
                source.EnterPenView();
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (source)
                source.ExitPenView();
            BindButtons(false);
            Subscribe(false);
        }

        private void OnDestroy()
        {
            if (runtimeFont)
                Destroy(runtimeFont);
        }

        private void Subscribe(bool value)
        {
            if (!source)
                return;
            if (value)
            {
                source.HerdChanged += Refresh;
                source.OperationFailed += ShowMessage;
                source.PigGrew += OnPigGrew;
                source.PigBorn += OnPigBorn;
                source.CrowdingFeedbackRequested += OnCrowded;
            }
            else
            {
                source.HerdChanged -= Refresh;
                source.OperationFailed -= ShowMessage;
                source.PigGrew -= OnPigGrew;
                source.PigBorn -= OnPigBorn;
                source.CrowdingFeedbackRequested -= OnCrowded;
            }
        }

        private void BindButtons(bool value)
        {
            SetListener(previousButton, Previous, value);
            SetListener(nextButton, Next, value);
            SetListener(growButton, Grow, value);
            SetListener(nutritionButton, NutritionGrow, value);
            SetListener(birthButton, Birth, value);
        }

        private static void SetListener(Button button, UnityEngine.Events.UnityAction action, bool add)
        {
            if (!button)
                return;
            if (add)
                button.onClick.AddListener(action);
            else
                button.onClick.RemoveListener(action);
        }

        private void Previous() { selectedIndex--; Refresh(); }
        private void Next() { selectedIndex++; Refresh(); }
        private void Grow() { PigSnapshot pig; if (TrySelected(out pig)) source.GrowPig(pig.id, false); }
        private void NutritionGrow() { PigSnapshot pig; if (TrySelected(out pig)) source.GrowPig(pig.id, true); }
        private void Birth() { if (source) source.BirthPig(false); }

        private bool TrySelected(out PigSnapshot pig)
        {
            IReadOnlyList<PigSnapshot> pigs = source ? source.Pigs : null;
            if (pigs == null || pigs.Count == 0)
            {
                pig = default(PigSnapshot);
                ShowMessage("猪圈里还没有猪");
                return false;
            }
            selectedIndex = Mathf.Clamp(selectedIndex, 0, pigs.Count - 1);
            pig = pigs[selectedIndex];
            return true;
        }

        private void Refresh()
        {
            if (!source || !source.IsInitialized)
                return;
            IReadOnlyList<PigSnapshot> pigs = source.Pigs;
            selectedIndex = pigs.Count == 0 ? 0 : Mathf.Clamp(selectedIndex, 0, pigs.Count - 1);
            capacityText.text = "猪圈容量  " + source.UsedCells + " / " + source.Capacity;
            capacityText.color = source.IsCrowded ? new Color(1f, .50f, .25f) : new Color(.95f, .80f, .35f);

            var lines = new List<string>();
            for (int i = 0; i < pigs.Count; i++)
            {
                PigSnapshot pig = pigs[i];
                lines.Add((i == selectedIndex ? "▶ " : "   ") + "#" + pig.id + "  " + pig.stageName + "  " + pig.occupiedCells + "格  价值" + pig.value);
            }
            pigListText.text = lines.Count == 0 ? "猪圈为空" : string.Join("\n", lines.ToArray());
            selectedText.text = pigs.Count == 0 ? "未选择猪只" : "已选择：#" + pigs[selectedIndex].id + " " + pigs[selectedIndex].stageName;
            bool hasPig = pigs.Count > 0;
            previousButton.interactable = hasPig;
            nextButton.interactable = hasPig;
            growButton.interactable = hasPig && pigs[selectedIndex].canGrow;
            nutritionButton.interactable = hasPig && pigs[selectedIndex].canGrow;
        }

        private void OnPigGrew(PigSnapshot pig) { ShowMessage("#" + pig.id + " 成长为 " + pig.stageName); }
        private void OnPigBorn(PigSnapshot pig) { ShowMessage("新的" + pig.stageName + "出生了"); }
        private void OnCrowded(PigSnapshot pig) { ShowMessage("#" + pig.id + "：挤死了！"); }
        private void ShowMessage(string value) { if (messageText) messageText.text = value; }

        private void ApplyChineseFont()
        {
            if (!runtimeFont)
                runtimeFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 26);
            if (!runtimeFont)
                return;
            foreach (Text label in GetComponentsInChildren<Text>(true))
                label.font = runtimeFont;
        }
    }
}
