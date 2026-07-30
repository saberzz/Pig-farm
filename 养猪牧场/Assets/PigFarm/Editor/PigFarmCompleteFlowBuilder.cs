#if UNITY_EDITOR
using System.IO;
using PigFarm.Core;
using PigFarm.Flow;
using PigFarm.Pigs;
using PigFarm.UI.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigFarm.Editor
{
    public static class PigFarmCompleteFlowBuilder
    {
        const string Root = "Assets/PigFarm";
        const string FlowPrefabFolder = Root + "/Prefabs/Flow";
        const string RulesPath = Root + "/Config/PigFarmGameRules.asset";
        const string StartScenePath = Root + "/Scenes/PigFarmStart.unity";
        const string GameScenePath = Root + "/Scenes/PigFarmGame.unity";
        const string StarSourcePath = Root + "/Sprite/123.png";
        const string StarPath = Root + "/Sprite/Generated/123_star.png";

        static readonly Color Ink = new Color(.14f, .12f, .09f, 1f);
        static readonly Color Dark = new Color(.065f, .14f, .10f, 1f);
        static readonly Color Green = new Color(.19f, .36f, .25f, 1f);
        static readonly Color Green2 = new Color(.27f, .46f, .32f, 1f);
        static readonly Color Cream = new Color(.96f, .90f, .72f, 1f);
        static readonly Color Paper = new Color(1f, .97f, .86f, 1f);
        static readonly Color Orange = new Color(.88f, .39f, .14f, 1f);
        static readonly Color Gold = new Color(1f, .76f, .22f, 1f);

        [MenuItem("Pig Farm/Build Complete Game Flow")]
        public static void Build()
        {
            EnsureFolder(FlowPrefabFolder);
            EnsureFolder(Root + "/Config");
            EnsureFolder(Root + "/Scenes");
            Sprite star = BuildStarCrop();
            PigFarmGameRulesConfig rules = BuildRules();
            GameObject tutorialPrefab = BuildTutorialPrefab(star);
            GameObject startPrefab = BuildStartPrefab(star);
            GameObject gameplayPrefab = BuildGameplayPrefab(star, tutorialPrefab);
            BuildStartScene(startPrefab);
            BuildGameScene(gameplayPrefab, rules);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(StartScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(StartScenePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
            Debug.Log("Complete Pig Farm flow built: start -> 3 tutorial pages -> 16-round game -> settlement.");
        }

        static PigFarmGameRulesConfig BuildRules()
        {
            PigFarmGameRulesConfig rules = AssetDatabase.LoadAssetAtPath<PigFarmGameRulesConfig>(RulesPath);
            if (!rules)
            {
                rules = ScriptableObject.CreateInstance<PigFarmGameRulesConfig>();
                AssetDatabase.CreateAsset(rules, RulesPath);
            }
            rules.displayStarCount = 24;
            rules.startingNutrition = 1;
            rules.startingCharms = 1;
            rules.startingVaccines = 1;
            rules.actionRollMinimum = new[] { 2, 3, 3 };
            rules.actionRollMaximum = new[] { 4, 6, 8 };
            rules.roundTasks = new[]
            {
                Task("春季 · 小猪评选", "回合结束时，猪圈里至少有 4 只小猪。", PigFarmRewardType.Vaccine, 3, small:4),
                Task("春季 · 育肥巡栏", "小猪和中猪合计至少 4 只。", PigFarmRewardType.Nutrition, 2, smallMedium:4),
                Task("春季 · 新生报喜", "至少拥有 2 只猪宝宝。", PigFarmRewardType.Charm, 1, baby:2),
                Task("春季 · 热闹猪圈", "猪圈里至少有 5 只猪。", PigFarmRewardType.Coins, 10, total:5),
                Task("夏季 · 长膘大赛", "至少拥有 2 只中猪。", PigFarmRewardType.Charm, 1, medium:2),
                Task("夏季 · 青年猪队", "小猪和中猪合计至少 5 只。", PigFarmRewardType.Coins, 20, smallMedium:5),
                Task("夏季 · 重量级新星", "至少拥有 1 只大猪。", PigFarmRewardType.Vaccine, 2, large:1),
                Task("夏季 · 仲夏集市", "猪圈里至少有 7 只猪。", PigFarmRewardType.Nutrition, 2, total:7),
                Task("秋季 · 秋收搬运队", "至少拥有 2 只大猪。", PigFarmRewardType.Vaccine, 2, large:2),
                Task("秋季 · 丰收主力", "大猪和中猪合计至少 5 只。", PigFarmRewardType.Nutrition, 2, mediumLarge:5),
                Task("秋季 · 育肥标兵", "至少拥有 3 只中猪。", PigFarmRewardType.Charm, 1, medium:3),
                Task("秋季 · 满栏丰收", "猪圈里至少有 8 只猪。", PigFarmRewardType.Coins, 30, total:8),
                Task("冬季 · 全家福", "大猪、中猪、小猪和猪宝宝各至少 1 只。", PigFarmRewardType.Coins, 50, all:true),
                Task("冬季 · 冠军展览会", "至少拥有 3 只大猪。", PigFarmRewardType.Nutrition, 3, large:3),
                Task("冬季 · 重量级方阵", "大猪和中猪合计至少 6 只。", PigFarmRewardType.Coins, 60, mediumLarge:6),
                Task("冬季 · 年度猪王", "至少拥有 4 只大猪。", PigFarmRewardType.Coins, 80, large:4)
            };
            EditorUtility.SetDirty(rules);
            return rules;
        }

        static PigFarmRoundTask Task(string title, string description, PigFarmRewardType reward, int rewardAmount,
            int baby = 0, int small = 0, int medium = 0, int large = 0, int total = 0,
            int smallMedium = 0, int mediumLarge = 0, bool all = false)
        {
            return new PigFarmRoundTask
            {
                title = title, description = description, rewardType = reward, rewardAmount = rewardAmount,
                babyMin = baby, smallMin = small, mediumMin = medium, largeMin = large, totalMin = total,
                smallAndMediumMin = smallMedium, mediumAndLargeMin = mediumLarge, requireAllStages = all
            };
        }

        static Sprite BuildStarCrop()
        {
            EnsureFolder(Root + "/Sprite/Generated");
            TextureImporter importer = AssetImporter.GetAtPath(StarSourcePath) as TextureImporter;
            if (!importer) return null;
            bool restoreReadable = importer.isReadable;
            if (!restoreReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(StarSourcePath);
            if (source)
            {
                // 123.png is 1672x941. Its non-transparent star occupies roughly
                // x 656..831 and y 384..558 in Unity's bottom-left texture coordinates.
                int x = Mathf.Clamp(638, 0, source.width - 1);
                int y = Mathf.Clamp(366, 0, source.height - 1);
                int width = Mathf.Min(212, source.width - x);
                int height = Mathf.Min(210, source.height - y);
                Color[] pixels = source.GetPixels(x, y, width, height);
                // The source contains faint purple scan-line pixels around the mascot.
                // Keep the warm star/highlight palette and its dark facial details only.
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color p = pixels[i];
                    bool warm = p.r > .55f && p.g > .18f && p.b < .76f;
                    bool light = p.r > .68f && p.g > .55f && p.b > .42f;
                    bool detail = p.r < .35f && p.g < .30f && p.b < .30f;
                    if (p.a < .05f || (!warm && !light && !detail)) pixels[i] = new Color(0, 0, 0, 0);
                }
                Texture2D crop = new Texture2D(width, height, TextureFormat.RGBA32, false);
                crop.SetPixels(pixels);
                crop.Apply();
                File.WriteAllBytes(StarPath, crop.EncodeToPNG());
                Object.DestroyImmediate(crop);
            }
            if (!restoreReadable) { importer.isReadable = false; importer.SaveAndReimport(); }
            AssetDatabase.ImportAsset(StarPath, ImportAssetOptions.ForceUpdate);
            TextureImporter starImporter = AssetImporter.GetAtPath(StarPath) as TextureImporter;
            if (starImporter)
            {
                starImporter.textureType = TextureImporterType.Sprite;
                starImporter.spriteImportMode = SpriteImportMode.Single;
                starImporter.alphaIsTransparency = true;
                starImporter.mipmapEnabled = false;
                starImporter.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(StarPath);
        }

        static GameObject BuildStartPrefab(Sprite star)
        {
            GameObject root = ScreenRoot("StartGameScreen", typeof(PigFarmStartScreen));
            Image(root.transform, "Background", Dark, Vector2.zero, Vector2.one);
            Image(root.transform, "TopGlow", new Color(.30f, .56f, .34f, .4f), new Vector2(0, .72f), Vector2.one);
            for (int i = 0; i < 7; i++)
            {
                float x = .16f + i * .115f;
                Image icon = SpriteImage(root.transform, "Star" + i, star, new Vector2(x, .67f + (i % 2) * .06f), new Vector2(x + .075f, .80f + (i % 2) * .06f));
                icon.color = new Color(1, 1, 1, .78f);
            }
            Label(root.transform, "怪兽养猪场", 82, TextAnchor.MiddleCenter, Paper, new Vector2(.15f, .53f), new Vector2(.85f, .72f), true);
            Label(root.transform, "16 回合 · 四季经营 · 猪王养成", 28, TextAnchor.MiddleCenter, new Color(.86f, .92f, .82f), new Vector2(.20f, .44f), new Vector2(.80f, .53f), false);
            Button start = Button(root.transform, "StartButton", "开始游戏", Orange, new Vector2(.38f, .26f), new Vector2(.62f, .36f));
            Label(root.transform, "根据最新策划案制作的流程原型", 18, TextAnchor.MiddleCenter, new Color(.66f, .76f, .68f), new Vector2(.30f, .12f), new Vector2(.70f, .20f), false);
            root.GetComponent<PigFarmStartScreen>().Configure(start, "PigFarmGame");
            return SavePrefab(root, "StartGameScreen.prefab");
        }

        static GameObject BuildTutorialPrefab(Sprite star)
        {
            GameObject root = ScreenRoot("TutorialPopup", typeof(PigFarmTutorialPopup));
            Image(root.transform, "Dim", new Color(0, 0, 0, .74f), Vector2.zero, Vector2.one);
            Image card = Image(root.transform, "Card", Paper, new Vector2(.18f, .12f), new Vector2(.82f, .88f));
            Outline(card.gameObject, new Color(.28f, .20f, .12f), 5);
            SpriteImage(card.transform, "StarMascot", star, new Vector2(.42f, .70f), new Vector2(.58f, .91f));
            Text title = Label(card.transform, "Title", "", 40, TextAnchor.MiddleCenter, Ink, new Vector2(.10f, .58f), new Vector2(.90f, .72f), true);
            Text body = Label(card.transform, "Body", "", 25, TextAnchor.MiddleCenter, new Color(.27f, .23f, .16f), new Vector2(.10f, .25f), new Vector2(.90f, .58f), false);
            Text page = Label(card.transform, "Page", "", 20, TextAnchor.MiddleCenter, new Color(.45f, .38f, .27f), new Vector2(.40f, .17f), new Vector2(.60f, .24f), true);
            Button previous = Button(card.transform, "PreviousButton", "上一页", Green, new Vector2(.09f, .07f), new Vector2(.32f, .17f));
            Button next = Button(card.transform, "NextButton", "下一页", Orange, new Vector2(.68f, .07f), new Vector2(.91f, .17f));
            root.GetComponent<PigFarmTutorialPopup>().Configure(title, body, page, previous, next,
                new[] { "经营目标", "猪圈与成长", "回合行动" },
                new[]
                {
                    "你将在 16 个回合中经营猪场，每 4 回合进入下一个季节。\n每回合完成左侧任务可获得奖励；最终金币 = 现金 + 全部猪只价值。",
                    "猪宝宝 → 小猪 → 中猪 → 大猪，每个阶段占用 4 / 6 / 9 / 12 格。\n猪圈隐藏容量为 80 格，超过容量的操作不会消耗行动。",
                    "先从生育、喂养、商店购买、卖猪中选择 1～3 种。\n系统随机决定本回合行动与次数；完成次数后结算任务并进入下一回合。"
                });
            return SavePrefab(root, "TutorialPopup.prefab");
        }

        static GameObject BuildGameplayPrefab(Sprite star, GameObject tutorialPrefab)
        {
            GameObject root = ScreenRoot("GameplayFlowHUD", typeof(PigFarmGameplayView));
            Image(root.transform, "Background", new Color(.72f, .72f, .68f), Vector2.zero, Vector2.one);
            Image(root.transform, "LeftRail", new Color(.015f, .035f, .025f), Vector2.zero, new Vector2(.045f, 1));
            Image(root.transform, "RightRail", new Color(.015f, .035f, .025f), new Vector2(.955f, 0), Vector2.one);

            Image top = Image(root.transform, "TopBar", Dark, new Vector2(.05f, .88f), new Vector2(.95f, .99f));
            Text resources = Label(top.transform, "Resources", "", 24, TextAnchor.MiddleRight, Paper, new Vector2(.40f, .15f), new Vector2(.96f, .85f), true);
            Button vaccinate = Button(top.transform, "VaccinateButton", "使用疫苗", Orange, new Vector2(.05f, .18f), new Vector2(.20f, .82f));

            Image left = Image(root.transform, "TaskStage", Cream, new Vector2(.06f, .32f), new Vector2(.285f, .86f));
            Outline(left.gameObject, Ink, 3);
            Text stage = Label(left.transform, "Stage", "", 32, TextAnchor.MiddleLeft, Ink, new Vector2(.07f, .80f), new Vector2(.93f, .96f), true);
            Text task = Label(left.transform, "Task", "", 23, TextAnchor.UpperLeft, Ink, new Vector2(.07f, .38f), new Vector2(.93f, .80f), false);
            Text reward = Label(left.transform, "Reward", "", 21, TextAnchor.MiddleLeft, Orange, new Vector2(.07f, .22f), new Vector2(.93f, .38f), true);
            Text capacity = Label(left.transform, "Capacity", "", 22, TextAnchor.MiddleLeft, Ink, new Vector2(.07f, .08f), new Vector2(.93f, .22f), true);

            Image pen = Image(root.transform, "Pen", Paper, new Vector2(.31f, .40f), new Vector2(.94f, .86f));
            Outline(pen.gameObject, Ink, 3);
            Label(pen.transform, "养猪区", 30, TextAnchor.UpperCenter, Ink, new Vector2(.35f, .83f), new Vector2(.65f, .98f), true);
            Image[] stars = new Image[24];
            for (int i = 0; i < stars.Length; i++)
            {
                int column = i % 8;
                int row = i / 8;
                float x = .035f + column * .119f + (row % 2) * .018f;
                float y = .16f + row * .215f + (column % 3) * .018f;
                stars[i] = SpriteImage(pen.transform, "Star_" + (i + 1), star, new Vector2(x, y), new Vector2(x + .088f, y + .17f));
                stars[i].color = new Color(1f, 1f, 1f, .92f);
            }
            Text herd = Label(pen.transform, "HerdSummary", "", 21, TextAnchor.LowerCenter, Ink, new Vector2(.05f, .01f), new Vector2(.95f, .13f), true);

            Image actionsArea = Image(root.transform, "ActionArea", new Color(.89f, .89f, .86f), new Vector2(.06f, .055f), new Vector2(.94f, .29f));
            Text range = Label(actionsArea.transform, "RollRange", "", 24, TextAnchor.MiddleLeft, Ink, new Vector2(.03f, .72f), new Vector2(.40f, .96f), true);
            string[] actionNames = { "生育", "喂养", "商店购买", "卖猪" };
            Button[] actionButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float x = .03f + i * .205f;
                actionButtons[i] = Button(actionsArea.transform, "Action" + i, actionNames[i], Green, new Vector2(x, .20f), new Vector2(x + .18f, .68f));
            }
            Button confirm = Button(actionsArea.transform, "ConfirmButton", "确定并抽取", Orange, new Vector2(.84f, .20f), new Vector2(.98f, .68f));
            Text message = Label(actionsArea.transform, "Message", "", 18, TextAnchor.MiddleLeft, Ink, new Vector2(.03f, .01f), new Vector2(.98f, .19f), false);

            GameObject actionModal;
            Text actionTitle, actionBody;
            Button primary, item, settle;
            GameObject shop;
            Button[] shopButtons;
            BuildActionModal(root.transform, out actionModal, out actionTitle, out actionBody, out primary, out item, out shop, out shopButtons, out settle);

            GameObject transition;
            Text transitionText;
            Button transitionContinue;
            BuildTransitionModal(root.transform, out transition, out transitionText, out transitionContinue);

            GameObject final;
            Text finalText;
            Button restart;
            BuildFinalModal(root.transform, out final, out finalText, out restart);

            GameObject tutorialInstance = (GameObject)PrefabUtility.InstantiatePrefab(tutorialPrefab, root.transform);
            Rect(tutorialInstance.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            PigFarmTutorialPopup tutorial = tutorialInstance.GetComponent<PigFarmTutorialPopup>();
            root.GetComponent<PigFarmGameplayView>().Configure(null, tutorial, stage, task, reward, resources, capacity, herd,
                range, message, stars, actionButtons, confirm, actionModal, actionTitle, actionBody, primary, item, shop,
                shopButtons, settle, vaccinate, transition, transitionText, transitionContinue, final, finalText, restart);
            return SavePrefab(root, "GameplayFlowHUD.prefab");
        }

        static void BuildActionModal(Transform parent, out GameObject root, out Text title, out Text body, out Button primary,
            out Button item, out GameObject shop, out Button[] shopButtons, out Button settle)
        {
            root = ScreenRoot("ActionExecutionModal");
            root.transform.SetParent(parent, false);
            Image(root.transform, "Dim", new Color(0, 0, 0, .68f), Vector2.zero, Vector2.one);
            Image card = Image(root.transform, "Card", Paper, new Vector2(.25f, .20f), new Vector2(.75f, .82f));
            Outline(card.gameObject, Ink, 4);
            title = Label(card.transform, "Title", "", 36, TextAnchor.MiddleCenter, Ink, new Vector2(.08f, .80f), new Vector2(.92f, .96f), true);
            body = Label(card.transform, "Body", "", 22, TextAnchor.MiddleCenter, Ink, new Vector2(.10f, .58f), new Vector2(.90f, .80f), false);
            primary = Button(card.transform, "PrimaryButton", "执行一次", Orange, new Vector2(.15f, .14f), new Vector2(.48f, .28f));
            item = Button(card.transform, "ItemButton", "使用道具", Green, new Vector2(.52f, .14f), new Vector2(.85f, .28f));
            settle = Button(card.transform, "SettleButton", "结算本回合", Orange, new Vector2(.30f, .14f), new Vector2(.70f, .28f));
            shop = Image(card.transform, "ShopItems", new Color(.93f, .87f, .69f), new Vector2(.08f, .26f), new Vector2(.92f, .60f)).gameObject;
            string[] names = { "猪宝宝\n3 金币", "营养剂\n1 金币", "护符\n1 金币", "疫苗\n1 金币" };
            shopButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                float x = .03f + i * .245f;
                shopButtons[i] = Button(shop.transform, "ShopItem" + i, names[i], i == 0 ? Orange : Green, new Vector2(x, .14f), new Vector2(x + .215f, .86f));
            }
        }

        static void BuildTransitionModal(Transform parent, out GameObject root, out Text text, out Button continueButton)
        {
            root = ScreenRoot("RoundTransitionModal");
            root.transform.SetParent(parent, false);
            Image(root.transform, "Dim", new Color(.02f, .06f, .04f, .94f), Vector2.zero, Vector2.one);
            Image card = Image(root.transform, "Card", Paper, new Vector2(.27f, .23f), new Vector2(.73f, .77f));
            text = Label(card.transform, "Result", "", 27, TextAnchor.MiddleCenter, Ink, new Vector2(.10f, .24f), new Vector2(.90f, .90f), true);
            continueButton = Button(card.transform, "ContinueButton", "继续", Orange, new Vector2(.31f, .08f), new Vector2(.69f, .21f));
        }

        static void BuildFinalModal(Transform parent, out GameObject root, out Text text, out Button restart)
        {
            root = ScreenRoot("FinalSettlementModal");
            root.transform.SetParent(parent, false);
            Image(root.transform, "Dim", new Color(.02f, .06f, .04f, .98f), Vector2.zero, Vector2.one);
            Label(root.transform, "年度结算", 64, TextAnchor.MiddleCenter, Gold, new Vector2(.20f, .65f), new Vector2(.80f, .82f), true);
            text = Label(root.transform, "Score", "", 30, TextAnchor.MiddleCenter, Paper, new Vector2(.20f, .37f), new Vector2(.80f, .65f), false);
            restart = Button(root.transform, "RestartButton", "返回开始界面", Orange, new Vector2(.39f, .22f), new Vector2(.61f, .32f));
        }

        static void BuildStartScene(GameObject prefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEnvironment();
            GameObject canvas = CreateCanvas("StartCanvas");
            PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, StartScenePath);
        }

        static void BuildGameScene(GameObject prefab, PigFarmGameRulesConfig rules)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEnvironment();
            GameFlowConfig flowConfig = AssetDatabase.LoadAssetAtPath<GameFlowConfig>(Root + "/Config/GameFlowConfig.asset");
            PigSystemConfig pigConfig = AssetDatabase.LoadAssetAtPath<PigSystemConfig>(Root + "/Config/PigSystemConfig.asset");
            GameObject systems = new GameObject("GameSystems");
            GameFlowController flow = systems.AddComponent<GameFlowController>();
            flow.Configure(flowConfig);
            PigHerdController herd = systems.AddComponent<PigHerdController>();
            herd.Configure(pigConfig);
            PigFarmGameSessionController session = systems.AddComponent<PigFarmGameSessionController>();
            session.Configure(rules, flow, herd);
            GameObject canvas = CreateCanvas("GameplayCanvas");
            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            hud.GetComponent<PigFarmGameplayView>().SetSession(session);
            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        static void CreateEnvironment()
        {
            GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Dark;
            GameObject light = new GameObject("Directional Light", typeof(Light));
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.GetComponent<Light>().type = LightType.Directional;
        }

        static GameObject CreateCanvas(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            return go;
        }

        static void CreateEventSystem() { new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }

        static GameObject ScreenRoot(string name, params System.Type[] components)
        {
            System.Type[] types = new System.Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            for (int i = 0; i < components.Length; i++) types[i + 1] = components[i];
            GameObject go = new GameObject(name, types);
            Rect(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            return go;
        }

        static Image Image(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), min, max);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        static Image SpriteImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max)
        {
            Image image = Image(parent, name, Color.white, min, max);
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        static Text Label(Transform parent, string name, string value, int size, TextAnchor anchor, Color color, Vector2 min, Vector2 max, bool bold)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Rect(go.GetComponent<RectTransform>(), min, max);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        static Text Label(Transform parent, string value, int size, TextAnchor anchor, Color color, Vector2 min, Vector2 max, bool bold)
        { return Label(parent, "Text", value, size, anchor, color, min, max, bold); }

        static Button Button(Transform parent, string name, string value, Color color, Vector2 min, Vector2 max)
        {
            Image image = Image(parent, name, color, min, max);
            Outline(image.gameObject, new Color(.18f, .12f, .08f, .75f), 2);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, .18f);
            colors.pressedColor = Color.Lerp(color, Color.black, .25f);
            button.colors = colors;
            Label(image.transform, "Label", value, 22, TextAnchor.MiddleCenter, Color.white, new Vector2(.03f, .05f), new Vector2(.97f, .95f), true);
            return button;
        }

        static void Outline(GameObject go, Color color, float distance)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
        }

        static void Rect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static GameObject SavePrefab(GameObject root, string name)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, FlowPrefabFolder + "/" + name);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
#endif
