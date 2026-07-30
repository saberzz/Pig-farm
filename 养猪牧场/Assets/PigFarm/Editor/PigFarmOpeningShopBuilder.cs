#if UNITY_EDITOR

using PigFarm.Pigs;

using PigFarm.UI.Flow;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.UI;

namespace PigFarm.Editor

{

    public static class PigFarmOpeningShopBuilder

    {

        const string HudPrefabPath = "Assets/PigFarm/Prefabs/Flow/GameplayFlowHUD.prefab";

        const string PigConfigPath = "Assets/PigFarm/Config/PigSystemConfig.asset";

        const string FontPath = "Assets/Fonts/WenYuanRoundedSCVF.ttf";

        const string CoinSpritePath = "Assets/PigFarm/Sprite/Generated/coin_icon.png";

        static readonly Color Ink = new Color(.14f, .12f, .09f, 1f);

        static readonly Color Paper = new Color(1f, .97f, .86f, 1f);

        static readonly Color Orange = new Color(.88f, .39f, .14f, 1f);

        static readonly Color Green = new Color(.19f, .36f, .25f, 1f);

        static readonly Color Cream = new Color(.96f, .90f, .72f, 1f);

        static readonly Color Panel = new Color(.90f, .88f, .82f, 1f);

        [MenuItem("Pig Farm/Build Opening Shop UI")]

        public static void Build()

        {

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);

            if (!font)

            {

                Debug.LogError("Missing font: " + FontPath);

                return;

            }

            Sprite smallStar = LoadSprite("Assets/PigFarm/Sprite/小星星.png");

            Sprite midStar = LoadSprite("Assets/PigFarm/Sprite/中星星.png");

            Sprite bigStar = LoadSprite("Assets/PigFarm/Sprite/大星星.png");

            Sprite hugeStar = LoadSprite("Assets/PigFarm/Sprite/超大星星.png");

            Sprite nutrition = LoadSprite("Assets/PigFarm/Sprite/medichine.png");

            Sprite charm = LoadSprite("Assets/PigFarm/Sprite/energy.png");

            Sprite vaccine = LoadSprite("Assets/PigFarm/Sprite/zhen.png");

            Sprite coin = EnsureCoinSprite();

            UpdatePigConfig();

            GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);

            try

            {

                PigFarmGameplayView view = root.GetComponent<PigFarmGameplayView>();

                DestroyChild(root.transform, "ShopScreen");

                DestroyChild(root.transform, "OpeningShopIntro");

                GameObject intro = BuildIntro(root.transform, font);

                GameObject shop;

                Button close;

                Button[] buttons;

                Text header, vaccineCount, goldCount, task, purchase, starCount, totalValue;

                Text[] names;

                UnityEngine.UI.Image[] icons;

                Text[] prices;

                BuildShop(root.transform, font, smallStar, midStar, bigStar, hugeStar, nutrition, charm, vaccine, coin,

                    out shop, out close, out buttons, out header, out vaccineCount, out goldCount,

                    out task, out purchase, out starCount, out totalValue, out names, out icons, out prices);

                // Keep existing sell/roll wiring; refresh shop refs on view via Configure helpers.

                var so = new SerializedObject(view);

                so.FindProperty("shopScreen").objectReferenceValue = shop;

                so.FindProperty("shopCloseButton").objectReferenceValue = close;

                SerializedProperty shopButtons = so.FindProperty("shopButtons");

                shopButtons.arraySize = buttons.Length;

                for (int i = 0; i < buttons.Length; i++)

                    shopButtons.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];

                so.FindProperty("openingIntro").objectReferenceValue = intro.GetComponent<PigFarmOpeningShopIntro>();

                so.FindProperty("shopHeaderText").objectReferenceValue = header;

                so.FindProperty("shopVaccineCountText").objectReferenceValue = vaccineCount;

                so.FindProperty("shopGoldCountText").objectReferenceValue = goldCount;

                so.FindProperty("shopTaskText").objectReferenceValue = task;

                so.FindProperty("shopPurchaseCountText").objectReferenceValue = purchase;

                so.FindProperty("shopStarCountText").objectReferenceValue = starCount;

                so.FindProperty("shopTotalValueText").objectReferenceValue = totalValue;

                so.FindProperty("smallStarSprite").objectReferenceValue = smallStar;

                so.FindProperty("mediumStarSprite").objectReferenceValue = midStar;

                so.FindProperty("largeStarSprite").objectReferenceValue = bigStar;

                so.FindProperty("hugeStarSprite").objectReferenceValue = hugeStar;

                SerializedProperty nameProps = so.FindProperty("shopProductNameTexts");

                SerializedProperty iconProps = so.FindProperty("shopProductIcons");

                SerializedProperty priceProps = so.FindProperty("shopProductPriceTexts");

                nameProps.arraySize = names.Length;

                iconProps.arraySize = icons.Length;

                priceProps.arraySize = prices.Length;

                for (int i = 0; i < names.Length; i++)

                {

                    nameProps.GetArrayElementAtIndex(i).objectReferenceValue = names[i];

                    iconProps.GetArrayElementAtIndex(i).objectReferenceValue = icons[i];

                    priceProps.GetArrayElementAtIndex(i).objectReferenceValue = prices[i];

                }

                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);

            }

            finally

            {

                PrefabUtility.UnloadPrefabContents(root);

            }

            // Sync scene instance if present.

            var sceneView = Object.FindObjectOfType<PigFarmGameplayView>();

            if (sceneView)

            {

                PrefabUtility.RevertPrefabInstance(sceneView.gameObject, InteractionMode.AutomatedAction);
                var session = Object.FindObjectOfType<PigFarm.Flow.PigFarmGameSessionController>();
                if (session)
                {
                    var soScene = new SerializedObject(sceneView);
                    soScene.FindProperty("session").objectReferenceValue = session;
                    soScene.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorSceneManager.MarkSceneDirty(sceneView.gameObject.scene);

                EditorSceneManager.SaveOpenScenes();

            }

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log("Opening shop UI rebuilt.");

        }

        static void UpdatePigConfig()

        {

            PigSystemConfig config = AssetDatabase.LoadAssetAtPath<PigSystemConfig>(PigConfigPath);

            if (!config) return;

            config.babyStage = AssetDatabase.LoadAssetAtPath<PigStageDefinition>("Assets/PigFarm/Config/Pigs/Baby.asset");

            config.smallStage = AssetDatabase.LoadAssetAtPath<PigStageDefinition>("Assets/PigFarm/Config/Pigs/Small.asset");

            config.mediumStage = AssetDatabase.LoadAssetAtPath<PigStageDefinition>("Assets/PigFarm/Config/Pigs/Medium.asset");

            config.largeStage = AssetDatabase.LoadAssetAtPath<PigStageDefinition>("Assets/PigFarm/Config/Pigs/Large.asset");

            config.startingPigs.Clear();

            EditorUtility.SetDirty(config);

        }

        static GameObject BuildIntro(Transform parent, Font font)

        {

            GameObject root = ScreenRoot("OpeningShopIntro", typeof(PigFarmOpeningShopIntro));

            root.transform.SetParent(parent, false);

            MakeImage(root.transform, "Dim", new Color(0, 0, 0, .72f), Vector2.zero, Vector2.one);

            UnityEngine.UI.Image card = MakeImage(root.transform, "Card", Paper, new Vector2(.22f, .28f), new Vector2(.78f, .72f));

            Outline(card.gameObject, Ink, 4);

            Text body = Label(card.transform, "Body", "", 28, TextAnchor.MiddleCenter, Ink, new Vector2(.08f, .28f), new Vector2(.92f, .90f), false, font);

            Button enter = Button(card.transform, "EnterShopButton", "进入商店", Orange, new Vector2(.30f, .08f), new Vector2(.70f, .24f), font);

            root.GetComponent<PigFarmOpeningShopIntro>().Configure(body, enter);

            root.SetActive(false);

            return root;

        }

        static void BuildShop(Transform parent, Font font,

            Sprite smallStar, Sprite midStar, Sprite bigStar, Sprite hugeStar,

            Sprite nutrition, Sprite charm, Sprite vaccine, Sprite coin,

            out GameObject root, out Button close, out Button[] items,

            out Text header, out Text vaccineCount, out Text goldCount,

            out Text task, out Text purchase, out Text starCount, out Text totalValue,

            out Text[] names, out UnityEngine.UI.Image[] icons, out Text[] prices)

        {

            root = ScreenRoot("ShopScreen");

            root.transform.SetParent(parent, false);

            MakeImage(root.transform, "Background", new Color(.86f, .84f, .78f, 1f), Vector2.zero, Vector2.one);

            UnityEngine.UI.Image top = MakeImage(root.transform, "TopBar", Darkish(), new Vector2(0, .86f), Vector2.one);

            header = Label(top.transform, "HeaderText", "初始采购", 40, TextAnchor.MiddleLeft, Color.white, new Vector2(.03f, .08f), new Vector2(.40f, .92f), true, font);

            UnityEngine.UI.Image vaccineBox = MakeImage(top.transform, "VaccineOwned", Panel, new Vector2(.62f, .12f), new Vector2(.78f, .88f));

            SpriteImage(vaccineBox.transform, "Icon", vaccine, new Vector2(.06f, .15f), new Vector2(.42f, .85f));

            vaccineCount = Label(vaccineBox.transform, "Count", "0", 28, TextAnchor.MiddleCenter, Ink, new Vector2(.42f, .1f), new Vector2(.94f, .9f), true, font);

            UnityEngine.UI.Image goldBox = MakeImage(top.transform, "GoldOwned", Panel, new Vector2(.80f, .12f), new Vector2(.97f, .88f));

            SpriteImage(goldBox.transform, "Icon", coin, new Vector2(.06f, .15f), new Vector2(.42f, .85f));

            goldCount = Label(goldBox.transform, "Count", "0", 28, TextAnchor.MiddleCenter, Ink, new Vector2(.42f, .1f), new Vector2(.94f, .9f), true, font);

            UnityEngine.UI.Image left = MakeImage(root.transform, "LeftPanel", Cream, new Vector2(.025f, .12f), new Vector2(.30f, .84f));

            Outline(left.gameObject, Ink, 3);

            Label(left.transform, "TaskTitle", "当前任务", 24, TextAnchor.MiddleLeft, Ink, new Vector2(.06f, .88f), new Vector2(.94f, .98f), true, font);

            task = Label(left.transform, "TaskBody", "", 18, TextAnchor.UpperLeft, Ink, new Vector2(.06f, .58f), new Vector2(.94f, .88f), false, font);

            purchase = Label(left.transform, "PurchaseCount", "购买次数  0", 22, TextAnchor.MiddleLeft, Ink, new Vector2(.06f, .48f), new Vector2(.94f, .58f), true, font);

            Label(left.transform, "StarTitle", "当前拥有", 22, TextAnchor.MiddleLeft, Ink, new Vector2(.06f, .38f), new Vector2(.94f, .48f), true, font);

            starCount = Label(left.transform, "StarCounts", "", 20, TextAnchor.UpperLeft, Ink, new Vector2(.06f, .12f), new Vector2(.94f, .38f), false, font);

            totalValue = Label(left.transform, "TotalValue", "总价值  0", 22, TextAnchor.MiddleLeft, Orange, new Vector2(.06f, .02f), new Vector2(.94f, .12f), true, font);

            UnityEngine.UI.Image products = MakeImage(root.transform, "Products", Paper, new Vector2(.32f, .14f), new Vector2(.975f, .84f));

            Outline(products.gameObject, Ink, 3);

            Label(products.transform, "StarSectionTitle", "星星", 26, TextAnchor.MiddleLeft, Ink, new Vector2(.03f, .90f), new Vector2(.30f, .98f), true, font);

            Label(products.transform, "ItemSectionTitle", "道具", 26, TextAnchor.MiddleLeft, Ink, new Vector2(.03f, .42f), new Vector2(.30f, .50f), true, font);

            items = new Button[7];

            names = new Text[7];

            icons = new Image[7];

            prices = new Text[7];

            Sprite[] productSprites = { smallStar, midStar, bigStar, hugeStar, nutrition, charm, vaccine };

            string[] productNames = { "小星星", "中星星", "大星星", "超大星星", "营养剂", "护符", "疫苗" };

            int[] productPrices = { 3, 6, 10, 15, 1, 1, 1 };

            for (int i = 0; i < 4; i++)

            {

                float x = .03f + i * .24f;

                BuildProductCard(products.transform, "BuyStar" + i, productNames[i], productSprites[i], productPrices[i],

                    new Vector2(x, .52f), new Vector2(x + .22f, .88f), font,

                    out items[i], out names[i], out icons[i], out prices[i]);

            }

            for (int i = 0; i < 3; i++)

            {

                float x = .03f + i * .24f;

                int index = 4 + i;

                BuildProductCard(products.transform, "BuyItem" + i, productNames[index], productSprites[index], productPrices[index],

                    new Vector2(x, .08f), new Vector2(x + .22f, .40f), font,

                    out items[index], out names[index], out icons[index], out prices[index]);

            }

            close = Button(root.transform, "CloseShopButton", "完成采购", Orange, new Vector2(.72f, .02f), new Vector2(.96f, .10f), font);

            root.SetActive(false);

        }

        static void BuildProductCard(Transform parent, string name, string title, Sprite icon, int price,

            Vector2 min, Vector2 max, Font font,

            out Button button, out Text nameText, out UnityEngine.UI.Image iconImage, out Text priceText)

        {

            UnityEngine.UI.Image card = MakeImage(parent, name, new Color(.78f, .78f, .74f, 1f), min, max);

            button = card.gameObject.AddComponent<Button>();

            button.targetGraphic = card;

            nameText = Label(card.transform, "Name", title, 20, TextAnchor.MiddleCenter, Ink, new Vector2(.05f, .78f), new Vector2(.95f, .98f), true, font);

            iconImage = SpriteImage(card.transform, "Icon", icon, new Vector2(.18f, .28f), new Vector2(.82f, .76f));

            priceText = Label(card.transform, "Price", price + " 金币", 18, TextAnchor.MiddleCenter, Orange, new Vector2(.05f, .04f), new Vector2(.95f, .26f), true, font);

        }

        static Color Darkish() { return new Color(.35f, .35f, .35f, 1f); }

        static Sprite EnsureCoinSprite()

        {

            Sprite existing = LoadSprite(CoinSpritePath);

            if (existing) return existing;

            EnsureFolder("Assets/PigFarm/Sprite/Generated");

            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);

            Color gold = new Color(1f, .78f, .18f, 1f);

            Color edge = new Color(.72f, .48f, .08f, 1f);

            for (int y = 0; y < 64; y++)

            for (int x = 0; x < 64; x++)

            {

                float dx = x - 31.5f;

                float dy = y - 31.5f;

                float d = Mathf.Sqrt(dx * dx + dy * dy);

                if (d > 30f) tex.SetPixel(x, y, Color.clear);

                else if (d > 26f) tex.SetPixel(x, y, edge);

                else tex.SetPixel(x, y, Color.Lerp(gold, Color.white, 1f - d / 30f));

            }

            tex.Apply();

            System.IO.File.WriteAllBytes(CoinSpritePath, tex.EncodeToPNG());

            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(CoinSpritePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(CoinSpritePath) as TextureImporter;

            if (importer)

            {

                importer.textureType = TextureImporterType.Sprite;

                importer.spritePixelsPerUnit = 100;

                importer.SaveAndReimport();

            }

            return LoadSprite(CoinSpritePath);

        }

        static Sprite LoadSprite(string path)

        {

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);

        }

        static void DestroyChild(Transform parent, string name)

        {

            Transform child = parent.Find(name);

            if (child) Object.DestroyImmediate(child.gameObject);

        }

        static GameObject ScreenRoot(string name, params System.Type[] extras)

        {

            var go = new GameObject(name, typeof(RectTransform));

            foreach (var t in extras) go.AddComponent(t);

            var rt = go.GetComponent<RectTransform>();

            rt.anchorMin = Vector2.zero;

            rt.anchorMax = Vector2.one;

            rt.offsetMin = Vector2.zero;

            rt.offsetMax = Vector2.zero;

            return go;

        }

        static UnityEngine.UI.Image MakeImage(Transform parent, string name, Color color, Vector2 min, Vector2 max)

        {

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));

            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();

            image.color = color;

            Rect(go.GetComponent<RectTransform>(), min, max);

            return image;

        }

        static UnityEngine.UI.Image SpriteImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max)

        {

            UnityEngine.UI.Image image = MakeImage(parent, name, Color.white, min, max);

            image.sprite = sprite;

            image.preserveAspect = true;

            image.type = Image.Type.Simple;

            return image;

        }

        static Text Label(Transform parent, string name, string value, int size, TextAnchor anchor, Color color,

            Vector2 min, Vector2 max, bool bestFit, Font font)

        {

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));

            go.transform.SetParent(parent, false);

            Text text = go.GetComponent<Text>();

            text.font = font;

            text.text = value;

            text.fontSize = size;

            text.color = color;

            text.alignment = anchor;

            text.horizontalOverflow = HorizontalWrapMode.Wrap;

            text.verticalOverflow = VerticalWrapMode.Overflow;

            text.resizeTextForBestFit = bestFit;

            if (bestFit)

            {

                text.resizeTextMinSize = Mathf.Max(10, size / 2);

                text.resizeTextMaxSize = size;

            }

            Rect(go.GetComponent<RectTransform>(), min, max);

            return text;

        }

        static Button Button(Transform parent, string name, string label, Color color, Vector2 min, Vector2 max, Font font)

        {

            UnityEngine.UI.Image image = MakeImage(parent, name, color, min, max);

            Button button = image.gameObject.AddComponent<Button>();

            button.targetGraphic = image;

            Label(image.transform, "Label", label, 24, TextAnchor.MiddleCenter, Color.white, new Vector2(.05f, .1f), new Vector2(.95f, .9f), true, font);

            return button;

        }

        static void Outline(GameObject go, Color color, int distance)

        {

            var outline = go.AddComponent<Outline>();

            outline.effectColor = color;

            outline.effectDistance = new Vector2(distance, -distance);

        }

        static void Rect(RectTransform rt, Vector2 min, Vector2 max)

        {

            rt.anchorMin = min;

            rt.anchorMax = max;

            rt.offsetMin = Vector2.zero;

            rt.offsetMax = Vector2.zero;

        }

        static void EnsureFolder(string path)

        {

            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");

            string name = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);

        }

    }

}

#endif

