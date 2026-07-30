#if UNITY_EDITOR
using System.Collections.Generic;
using PigFarm.UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigFarm.Editor
{
    public static class PigFarmUIScreenBuilder
    {
        const string Root = "Assets/PigFarm";
        const string PrefabFolder = Root + "/Prefabs/UI";
        const string ConfigFolder = Root + "/Config/UI";
        const string ScenePath = Root + "/Scenes/CoreUIScreenPreview.unity";

        static readonly Color DarkGreen = new Color(0.08f, 0.18f, 0.12f, 1f);
        static readonly Color MidGreen = new Color(0.18f, 0.36f, 0.23f, 1f);
        static readonly Color Cream = new Color(0.95f, 0.88f, 0.69f, 1f);
        static readonly Color PaleCream = new Color(1f, 0.96f, 0.82f, 1f);
        static readonly Color Orange = new Color(0.84f, 0.34f, 0.18f, 1f);
        static readonly Color Brown = new Color(0.28f, 0.19f, 0.11f, 1f);
        static readonly Color Pink = new Color(0.94f, 0.54f, 0.57f, 1f);

        sealed class BuildContext
        {
            public readonly List<Image> backgrounds = new List<Image>();
            public readonly List<Image> surfaces = new List<Image>();
            public readonly List<Image> accents = new List<Image>();
            public readonly List<Text> primaryTexts = new List<Text>();
            public readonly List<Text> secondaryTexts = new List<Text>();
            public readonly List<Button> buttons = new List<Button>();
            public readonly List<PigFarmScreenId> targets = new List<PigFarmScreenId>();
        }

        [MenuItem("Pig Farm/UI/Build Core Screen Prefabs")]
        public static void Build()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ConfigFolder);
            EnsureFolder(Root + "/Scenes");

            PigFarmUIThemeConfig theme = GetOrCreateTheme();
            Dictionary<PigFarmScreenId, GameObject> prefabs = new Dictionary<PigFarmScreenId, GameObject>();
            prefabs.Add(PigFarmScreenId.Main, BuildMain(theme));
            prefabs.Add(PigFarmScreenId.Tutorial, BuildTutorial(theme));
            prefabs.Add(PigFarmScreenId.SeasonTransition, BuildSeasonTransition(theme));
            prefabs.Add(PigFarmScreenId.Shop, BuildShop(theme));
            prefabs.Add(PigFarmScreenId.ActionDraw, BuildActionDraw(theme));
            BuildPreviewScene(prefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Created 5 decoupled Pig Farm UI prefabs and CoreUIScreenPreview scene.");
        }

        static GameObject BuildMain(PigFarmUIThemeConfig theme)
        {
            BuildContext ctx;
            GameObject root = CreateRoot("MainScreen", PigFarmScreenId.Main, theme, out ctx);
            AddTopBar(root.transform, ctx, "怪兽养猪场", "春季 · 第 1 回合", "金币  180");

            Image farm = Panel("FarmOverview", root.transform, new Vector2(.055f, .23f), new Vector2(.68f, .82f), PaleCream, true);
            ctx.surfaces.Add(farm);
            AddLabel(farm.transform, "猪圈近况", 32, TextAnchor.MiddleLeft, new Vector2(.05f, .83f), new Vector2(.55f, .96f), Brown, true, ctx);
            AddLabel(farm.transform, "3 只猪 · 心情良好", 21, TextAnchor.MiddleRight, new Vector2(.52f, .84f), new Vector2(.95f, .95f), Brown, false, ctx);
            AddPigIllustration(farm.transform);
            AddLabel(farm.transform, "小粉猪", 28, TextAnchor.MiddleCenter, new Vector2(.08f, .10f), new Vector2(.42f, .24f), Brown, true, ctx);
            AddLabel(farm.transform, "体重 12kg    成长 65%", 21, TextAnchor.MiddleLeft, new Vector2(.45f, .43f), new Vector2(.91f, .55f), Brown, false, ctx);
            AddProgress(farm.transform, new Vector2(.45f, .34f), new Vector2(.91f, .41f), .65f);
            AddLabel(farm.transform, "饱食  78       健康  92       心情  86", 20, TextAnchor.MiddleLeft, new Vector2(.45f, .18f), new Vector2(.94f, .30f), Brown, false, ctx);

            Image actions = Panel("ActionPanel", root.transform, new Vector2(.71f, .23f), new Vector2(.945f, .82f), Cream, true);
            ctx.surfaces.Add(actions);
            AddLabel(actions.transform, "今天做什么？", 28, TextAnchor.MiddleCenter, new Vector2(.08f, .82f), new Vector2(.92f, .95f), Brown, true, ctx);
            AddNavButton(actions.transform, "ShopButton", "商店", new Vector2(.10f, .60f), new Vector2(.90f, .76f), PigFarmScreenId.Shop, ctx);
            AddNavButton(actions.transform, "ActionDrawButton", "行动抽取", new Vector2(.10f, .39f), new Vector2(.90f, .55f), PigFarmScreenId.ActionDraw, ctx);
            AddNavButton(actions.transform, "TutorialButton", "教程", new Vector2(.10f, .18f), new Vector2(.90f, .34f), PigFarmScreenId.Tutorial, ctx);

            Image footer = Panel("SeasonFooter", root.transform, new Vector2(.055f, .07f), new Vector2(.945f, .17f), MidGreen, false);
            AddLabel(footer.transform, "春季进度", 20, TextAnchor.MiddleLeft, new Vector2(.03f, .08f), new Vector2(.19f, .92f), Color.white, true, ctx);
            AddLabel(footer.transform, "●  ●  ○  ○", 25, TextAnchor.MiddleCenter, new Vector2(.20f, .08f), new Vector2(.53f, .92f), Color.white, false, ctx);
            AddNavButton(footer.transform, "NextRoundButton", "结束回合", new Vector2(.74f, .16f), new Vector2(.97f, .84f), PigFarmScreenId.SeasonTransition, ctx);
            return Save(root, PigFarmScreenId.Main, theme, ctx, "MainScreen.prefab");
        }

        static GameObject BuildTutorial(PigFarmUIThemeConfig theme)
        {
            BuildContext ctx;
            GameObject root = CreateRoot("TutorialScreen", PigFarmScreenId.Tutorial, theme, out ctx);
            AddTopBar(root.transform, ctx, "新手教程", "养好每一只小猪", "1 / 3");
            Image paper = Panel("TutorialCard", root.transform, new Vector2(.14f, .16f), new Vector2(.86f, .82f), PaleCream, true);
            ctx.surfaces.Add(paper);
            AddLabel(paper.transform, "经营你的怪兽猪场", 42, TextAnchor.MiddleCenter, new Vector2(.08f, .78f), new Vector2(.92f, .94f), Brown, true, ctx);
            AddLabel(paper.transform, "每回合可以购买物资、喂养猪只，或抽取一次随机行动。\n让猪健康成长，在季末结算中获得更多金币。", 26, TextAnchor.MiddleCenter, new Vector2(.12f, .47f), new Vector2(.88f, .74f), Brown, false, ctx);
            AddTutorialStep(paper.transform, "01", "查看状态", .17f);
            AddTutorialStep(paper.transform, "02", "选择行动", .43f);
            AddTutorialStep(paper.transform, "03", "结束回合", .69f);
            AddNavButton(paper.transform, "SkipButton", "跳过", new Vector2(.08f, .07f), new Vector2(.27f, .18f), PigFarmScreenId.Main, ctx);
            AddNavButton(paper.transform, "StartButton", "开始养猪", new Vector2(.68f, .07f), new Vector2(.92f, .18f), PigFarmScreenId.Main, ctx);
            return Save(root, PigFarmScreenId.Tutorial, theme, ctx, "TutorialScreen.prefab");
        }

        static GameObject BuildSeasonTransition(PigFarmUIThemeConfig theme)
        {
            BuildContext ctx;
            GameObject root = CreateRoot("SeasonTransitionScreen", PigFarmScreenId.SeasonTransition, theme, out ctx);
            Image dim = root.GetComponent<Image>();
            dim.color = DarkGreen;
            Image card = Panel("SeasonCard", root.transform, new Vector2(.23f, .15f), new Vector2(.77f, .85f), PaleCream, true);
            ctx.surfaces.Add(card);
            AddLabel(card.transform, "回合变化", 24, TextAnchor.MiddleCenter, new Vector2(.30f, .87f), new Vector2(.70f, .96f), Orange, true, ctx);
            AddLabel(card.transform, "春季", 50, TextAnchor.MiddleCenter, new Vector2(.08f, .63f), new Vector2(.38f, .82f), Brown, true, ctx);
            AddLabel(card.transform, "→", 58, TextAnchor.MiddleCenter, new Vector2(.41f, .63f), new Vector2(.59f, .82f), Orange, true, ctx);
            AddLabel(card.transform, "夏季", 50, TextAnchor.MiddleCenter, new Vector2(.62f, .63f), new Vector2(.92f, .82f), Brown, true, ctx);
            AddLabel(card.transform, "季节结算", 26, TextAnchor.MiddleLeft, new Vector2(.12f, .48f), new Vector2(.88f, .59f), Brown, true, ctx);
            AddResultRow(card.transform, "猪只成长", "+ 3", .39f, ctx);
            AddResultRow(card.transform, "本季收入", "+ 46 金币", .29f, ctx);
            AddResultRow(card.transform, "健康奖励", "+ 10 金币", .19f, ctx);
            AddNavButton(card.transform, "ContinueButton", "进入下一回合", new Vector2(.27f, .05f), new Vector2(.73f, .15f), PigFarmScreenId.Main, ctx);
            return Save(root, PigFarmScreenId.SeasonTransition, theme, ctx, "SeasonTransitionScreen.prefab");
        }

        static GameObject BuildShop(PigFarmUIThemeConfig theme)
        {
            BuildContext ctx;
            GameObject root = CreateRoot("ShopScreen", PigFarmScreenId.Shop, theme, out ctx);
            AddTopBar(root.transform, ctx, "猪场商店", "补充本回合所需物资", "金币  180");
            Image tabs = Panel("CategoryTabs", root.transform, new Vector2(.055f, .69f), new Vector2(.945f, .79f), MidGreen, false);
            AddLabel(tabs.transform, "饲料", 23, TextAnchor.MiddleCenter, new Vector2(.04f, .08f), new Vector2(.24f, .92f), Color.white, true, ctx);
            AddLabel(tabs.transform, "药品", 23, TextAnchor.MiddleCenter, new Vector2(.28f, .08f), new Vector2(.48f, .92f), Color.white, false, ctx);
            AddLabel(tabs.transform, "设施", 23, TextAnchor.MiddleCenter, new Vector2(.52f, .08f), new Vector2(.72f, .92f), Color.white, false, ctx);
            AddLabel(tabs.transform, "特殊", 23, TextAnchor.MiddleCenter, new Vector2(.76f, .08f), new Vector2(.96f, .92f), Color.white, false, ctx);
            AddShopCard(root.transform, "基础饲料", "饱食 +20", "20 金币", .055f, ctx);
            AddShopCard(root.transform, "营养饲料", "成长 +15%", "45 金币", .37f, ctx);
            AddShopCard(root.transform, "快乐零食", "心情 +25", "35 金币", .685f, ctx);
            AddNavButton(root.transform, "BackButton", "返回猪场", new Vector2(.055f, .07f), new Vector2(.22f, .16f), PigFarmScreenId.Main, ctx);
            AddLabel(root.transform, "本回合已购买  0 / 3", 22, TextAnchor.MiddleRight, new Vector2(.63f, .07f), new Vector2(.945f, .16f), Color.white, false, ctx);
            return Save(root, PigFarmScreenId.Shop, theme, ctx, "ShopScreen.prefab");
        }

        static GameObject BuildActionDraw(PigFarmUIThemeConfig theme)
        {
            BuildContext ctx;
            GameObject root = CreateRoot("ActionDrawScreen", PigFarmScreenId.ActionDraw, theme, out ctx);
            AddTopBar(root.transform, ctx, "行动抽取", "一次选择，也可能改变整季", "行动点  3 / 3");
            Image info = Panel("DrawInfo", root.transform, new Vector2(.055f, .20f), new Vector2(.30f, .80f), Cream, true);
            ctx.surfaces.Add(info);
            AddLabel(info.transform, "行动牌堆", 30, TextAnchor.MiddleCenter, new Vector2(.08f, .82f), new Vector2(.92f, .95f), Brown, true, ctx);
            AddLabel(info.transform, "剩余  12", 24, TextAnchor.MiddleCenter, new Vector2(.08f, .65f), new Vector2(.92f, .78f), Brown, false, ctx);
            AddLabel(info.transform, "可能获得\n金币 · 饲料 · 随机事件", 22, TextAnchor.MiddleCenter, new Vector2(.10f, .38f), new Vector2(.90f, .60f), Brown, false, ctx);
            AddNavButton(info.transform, "BackButton", "暂不抽取", new Vector2(.15f, .10f), new Vector2(.85f, .24f), PigFarmScreenId.Main, ctx);

            Image cardBack = Panel("CardBack", root.transform, new Vector2(.39f, .24f), new Vector2(.61f, .76f), Orange, true);
            AddLabel(cardBack.transform, "?", 112, TextAnchor.MiddleCenter, new Vector2(.10f, .25f), new Vector2(.90f, .75f), PaleCream, true, ctx);
            AddLabel(cardBack.transform, "命运行动", 24, TextAnchor.MiddleCenter, new Vector2(.10f, .10f), new Vector2(.90f, .25f), PaleCream, true, ctx);

            Image result = Panel("ResultPreview", root.transform, new Vector2(.70f, .20f), new Vector2(.945f, .80f), Cream, true);
            ctx.surfaces.Add(result);
            AddLabel(result.transform, "本回合记录", 28, TextAnchor.MiddleCenter, new Vector2(.08f, .82f), new Vector2(.92f, .95f), Brown, true, ctx);
            AddLabel(result.transform, "尚未抽取行动", 22, TextAnchor.MiddleCenter, new Vector2(.08f, .47f), new Vector2(.92f, .65f), Brown, false, ctx);
            AddNavButton(result.transform, "DrawButton", "抽取行动", new Vector2(.12f, .12f), new Vector2(.88f, .28f), PigFarmScreenId.Main, ctx);
            return Save(root, PigFarmScreenId.ActionDraw, theme, ctx, "ActionDrawScreen.prefab");
        }

        static GameObject CreateRoot(string name, PigFarmScreenId id, PigFarmUIThemeConfig theme, out BuildContext context)
        {
            context = new BuildContext();
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(PigFarmScreenView));
            Rect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            Image image = root.GetComponent<Image>();
            image.color = theme.Background;
            context.backgrounds.Add(image);
            return root;
        }

        static void AddTopBar(Transform parent, BuildContext ctx, string title, string subtitle, string resource)
        {
            Image top = Panel("TopBar", parent, new Vector2(.055f, .85f), new Vector2(.945f, .95f), Cream, true);
            ctx.surfaces.Add(top);
            AddLabel(top.transform, title, 34, TextAnchor.MiddleLeft, new Vector2(.035f, .10f), new Vector2(.38f, .90f), Brown, true, ctx);
            AddLabel(top.transform, subtitle, 21, TextAnchor.MiddleCenter, new Vector2(.36f, .10f), new Vector2(.70f, .90f), Brown, false, ctx);
            AddLabel(top.transform, resource, 24, TextAnchor.MiddleRight, new Vector2(.70f, .10f), new Vector2(.965f, .90f), Brown, true, ctx);
        }

        static void AddPigIllustration(Transform parent)
        {
            Image body = Panel("PigBody", parent, new Vector2(.10f, .28f), new Vector2(.40f, .72f), Pink, true);
            Image leftEar = Panel("LeftEar", body.transform, new Vector2(.08f, .72f), new Vector2(.30f, 1.02f), Pink, true);
            Image rightEar = Panel("RightEar", body.transform, new Vector2(.70f, .72f), new Vector2(.92f, 1.02f), Pink, true);
            Image snout = Panel("Snout", body.transform, new Vector2(.32f, .23f), new Vector2(.68f, .48f), new Color(.96f, .68f, .68f), true);
            AddDot(body.transform, "EyeL", .28f, .64f, Brown);
            AddDot(body.transform, "EyeR", .65f, .64f, Brown);
            AddDot(snout.transform, "NoseL", .30f, .48f, Brown);
            AddDot(snout.transform, "NoseR", .62f, .48f, Brown);
            leftEar.raycastTarget = rightEar.raycastTarget = snout.raycastTarget = false;
        }

        static void AddDot(Transform parent, string name, float x, float y, Color color)
        {
            Image dot = Panel(name, parent, new Vector2(x, y), new Vector2(x + .08f, y + .09f), color, false);
            dot.raycastTarget = false;
        }

        static void AddProgress(Transform parent, Vector2 min, Vector2 max, float value)
        {
            Image back = Panel("GrowthBar", parent, min, max, new Color(.68f, .62f, .48f), false);
            Panel("Fill", back.transform, Vector2.zero, new Vector2(value, 1f), Orange, false);
        }

        static void AddTutorialStep(Transform parent, string number, string title, float x)
        {
            Image circle = Panel("Step" + number, parent, new Vector2(x, .25f), new Vector2(x + .14f, .43f), MidGreen, true);
            Text num = AddLabel(circle.transform, number, 29, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Color.white, true, null);
            num.raycastTarget = false;
            AddLabel(parent, title, 21, TextAnchor.MiddleCenter, new Vector2(x - .02f, .18f), new Vector2(x + .16f, .25f), Brown, true, null);
        }

        static void AddResultRow(Transform parent, string label, string value, float y, BuildContext ctx)
        {
            AddLabel(parent, label, 22, TextAnchor.MiddleLeft, new Vector2(.14f, y), new Vector2(.55f, y + .08f), Brown, false, ctx);
            AddLabel(parent, value, 22, TextAnchor.MiddleRight, new Vector2(.55f, y), new Vector2(.86f, y + .08f), Orange, true, ctx);
        }

        static void AddShopCard(Transform parent, string title, string effect, string price, float x, BuildContext ctx)
        {
            Image card = Panel(title + "Card", parent, new Vector2(x, .22f), new Vector2(x + .26f, .64f), PaleCream, true);
            ctx.surfaces.Add(card);
            Image icon = Panel("ItemIcon", card.transform, new Vector2(.28f, .54f), new Vector2(.72f, .86f), MidGreen, true);
            AddLabel(icon.transform, "物资", 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Color.white, true, ctx);
            AddLabel(card.transform, title, 27, TextAnchor.MiddleCenter, new Vector2(.08f, .38f), new Vector2(.92f, .52f), Brown, true, ctx);
            AddLabel(card.transform, effect, 20, TextAnchor.MiddleCenter, new Vector2(.08f, .27f), new Vector2(.92f, .38f), Brown, false, ctx);
            Button buy = AddButton(card.transform, "BuyButton", price, new Vector2(.15f, .08f), new Vector2(.85f, .22f), Orange, ctx);
            buy.interactable = true;
        }

        static Button AddNavButton(Transform parent, string name, string text, Vector2 min, Vector2 max, PigFarmScreenId target, BuildContext ctx)
        {
            Button button = AddButton(parent, name, text, min, max, Orange, ctx);
            ctx.buttons.Add(button);
            ctx.targets.Add(target);
            return button;
        }

        static Button AddButton(Transform parent, string name, string text, Vector2 min, Vector2 max, Color color, BuildContext ctx)
        {
            Image image = Panel(name, parent, min, max, color, true);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, .48f, .24f, 1f);
            colors.pressedColor = new Color(.66f, .22f, .10f, 1f);
            button.colors = colors;
            AddLabel(image.transform, "Label", 23, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Color.white, true, ctx);
            Text label = image.GetComponentInChildren<Text>();
            label.text = text;
            if (ctx != null) ctx.accents.Add(image);
            return button;
        }

        static Image Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color, bool outline)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), min, max);
            Image image = go.GetComponent<Image>();
            image.color = color;
            if (outline)
            {
                Outline border = go.AddComponent<Outline>();
                border.effectColor = new Color(.20f, .13f, .08f, .65f);
                border.effectDistance = new Vector2(3f, -3f);
            }
            return image;
        }

        static Text AddLabel(Transform parent, string value, int size, TextAnchor alignment, Vector2 min, Vector2 max, Color color, bool bold, BuildContext ctx)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), min, max);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            if (ctx != null)
            {
                if (color == Brown) ctx.primaryTexts.Add(text);
                else if (color == themeSecondaryFallback) ctx.secondaryTexts.Add(text);
            }
            return text;
        }

        static readonly Color themeSecondaryFallback = new Color(0.38f, 0.31f, 0.21f, 1f);

        static GameObject Save(GameObject root, PigFarmScreenId id, PigFarmUIThemeConfig theme, BuildContext ctx, string fileName)
        {
            PigFarmScreenView view = root.GetComponent<PigFarmScreenView>();
            view.Configure(id, theme, ctx.backgrounds.ToArray(), ctx.surfaces.ToArray(), ctx.accents.ToArray(),
                ctx.primaryTexts.ToArray(), ctx.secondaryTexts.ToArray(), ctx.buttons.ToArray(), ctx.targets.ToArray());
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + fileName);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildPreviewScene(Dictionary<PigFarmScreenId, GameObject> prefabs)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DarkGreen;
            GameObject lightGo = new GameObject("Directional Light", typeof(Light));
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            lightGo.GetComponent<Light>().type = LightType.Directional;
            GameObject canvasGo = new GameObject("CoreUIScreenCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PigFarmUIScreenHost));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            List<PigFarmScreenView> views = new List<PigFarmScreenView>();
            foreach (KeyValuePair<PigFarmScreenId, GameObject> pair in prefabs)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(pair.Value, canvasGo.transform);
                Rect(instance.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
                PigFarmScreenView view = instance.GetComponent<PigFarmScreenView>();
                views.Add(view);
                instance.SetActive(pair.Key == PigFarmScreenId.Main);
            }
            canvasGo.GetComponent<PigFarmUIScreenHost>().Configure(PigFarmScreenId.Main, views.ToArray());
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static PigFarmUIThemeConfig GetOrCreateTheme()
        {
            string path = ConfigFolder + "/PigFarmUITheme.asset";
            PigFarmUIThemeConfig theme = AssetDatabase.LoadAssetAtPath<PigFarmUIThemeConfig>(path);
            if (!theme)
            {
                theme = ScriptableObject.CreateInstance<PigFarmUIThemeConfig>();
                AssetDatabase.CreateAsset(theme, path);
            }
            return theme;
        }

        static void Rect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
#endif
