using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PigFarm.UI.Screens
{
    public enum PigFarmScreenId
    {
        Main,
        Tutorial,
        SeasonTransition,
        Shop,
        ActionDraw
    }

    public sealed class PigFarmScreenView : MonoBehaviour
    {
        [SerializeField] PigFarmScreenId screenId;
        [SerializeField] PigFarmUIThemeConfig theme;
        [SerializeField] Image[] backgroundTargets;
        [SerializeField] Image[] surfaceTargets;
        [SerializeField] Image[] accentTargets;
        [SerializeField] Text[] primaryTextTargets;
        [SerializeField] Text[] secondaryTextTargets;
        [SerializeField] Button[] navigationButtons;
        [SerializeField] PigFarmScreenId[] navigationTargets;

        Font runtimeFont;
        UnityAction[] navigationActions;

        public PigFarmScreenId ScreenId { get { return screenId; } }
        public event Action<PigFarmScreenId> NavigationRequested;

        public void Configure(
            PigFarmScreenId id,
            PigFarmUIThemeConfig value,
            Image[] backgrounds,
            Image[] surfaces,
            Image[] accents,
            Text[] primaryTexts,
            Text[] secondaryTexts,
            Button[] buttons,
            PigFarmScreenId[] targets)
        {
            screenId = id;
            theme = value;
            backgroundTargets = backgrounds;
            surfaceTargets = surfaces;
            accentTargets = accents;
            primaryTextTargets = primaryTexts;
            secondaryTextTargets = secondaryTexts;
            navigationButtons = buttons;
            navigationTargets = targets;
            navigationActions = null;
            ApplyTheme();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        void Awake()
        {
            ApplyRuntimeFont();
            ApplyTheme();
        }

        void OnEnable()
        {
            ApplyRuntimeFont();
            BindButtons(true);
        }

        void OnDisable()
        {
            BindButtons(false);
        }

        void OnDestroy()
        {
            if (runtimeFont) Destroy(runtimeFont);
        }

        void BindButtons(bool bind)
        {
            if (navigationButtons == null) return;
            if (navigationActions == null || navigationActions.Length != navigationButtons.Length)
            {
                navigationActions = new UnityAction[navigationButtons.Length];
                for (int i = 0; i < navigationActions.Length; i++)
                {
                    int index = i;
                    navigationActions[i] = delegate { RequestNavigation(index); };
                }
            }
            for (int i = 0; i < navigationButtons.Length; i++)
            {
                Button button = navigationButtons[i];
                if (!button) continue;
                if (bind) button.onClick.AddListener(navigationActions[i]);
                else button.onClick.RemoveListener(navigationActions[i]);
            }
        }

        void RequestNavigation(int index)
        {
            if (navigationTargets == null || index < 0 || index >= navigationTargets.Length) return;
            Action<PigFarmScreenId> handler = NavigationRequested;
            if (handler != null) handler(navigationTargets[index]);
        }

        void ApplyRuntimeFont()
        {
            if (!runtimeFont)
            {
                runtimeFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" }, 32);
            }
            if (!runtimeFont) return;
            Text[] labels = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++) labels[i].font = runtimeFont;
        }

        void ApplyTheme()
        {
            if (!theme) return;
            SetImages(backgroundTargets, theme.Background);
            SetImages(surfaceTargets, theme.Surface);
            SetImages(accentTargets, theme.Accent);
            SetTexts(primaryTextTargets, theme.PrimaryText);
            SetTexts(secondaryTextTargets, theme.SecondaryText);
        }

        static void SetImages(Image[] targets, Color color)
        {
            if (targets == null) return;
            for (int i = 0; i < targets.Length; i++) if (targets[i]) targets[i].color = color;
        }

        static void SetTexts(Text[] targets, Color color)
        {
            if (targets == null) return;
            for (int i = 0; i < targets.Length; i++) if (targets[i]) targets[i].color = color;
        }
    }
}
