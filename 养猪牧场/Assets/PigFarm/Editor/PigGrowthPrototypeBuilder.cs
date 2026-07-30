#if UNITY_EDITOR
using System.Collections.Generic;
using PigFarm.Pigs;
using PigFarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigFarm.Editor
{
    public static class PigGrowthPrototypeBuilder
    {
        private const string Root = "Assets/PigFarm";
        private const string PigConfigFolder = Root + "/Config/Pigs";
        private const string PigConfigPath = Root + "/Config/PigSystemConfig.asset";
        private const string PrefabPath = Root + "/Prefabs/PigHerdHUD.prefab";
        private const string ScenePath = Root + "/Scenes/PigGrowthPrototype.unity";

        [MenuItem("Pig Farm/Build Pig Growth Prototype")]
        public static void Build()
        {
            EnsureFolder(PigConfigFolder);
            EnsureFolder(Root + "/Prefabs");
            EnsureFolder(Root + "/Scenes");

            PigStageDefinition baby = Stage("Baby", "baby", "猪宝宝", 3, 4, false);
            PigStageDefinition small = Stage("Small", "small", "小猪", 6, 6, false);
            PigStageDefinition medium = Stage("Medium", "medium", "中猪", 10, 9, true);
            PigStageDefinition large = Stage("Large", "large", "大猪", 15, 12, true);
            baby.nextStage = small;
            small.nextStage = medium;
            medium.nextStage = large;
            large.nextStage = null;
            EditorUtility.SetDirty(baby);
            EditorUtility.SetDirty(small);
            EditorUtility.SetDirty(medium);
            EditorUtility.SetDirty(large);

            PigSystemConfig config = AssetDatabase.LoadAssetAtPath<PigSystemConfig>(PigConfigPath);
            if (!config)
            {
                config = ScriptableObject.CreateInstance<PigSystemConfig>();
                AssetDatabase.CreateAsset(config, PigConfigPath);
            }
            config.penCapacity = 80;
            config.crowdingThreshold = 70;
            config.babyStage = baby;
            config.smallStage = small;
            config.startingPigs = new List<PigStageDefinition> { baby, small, medium };
            EditorUtility.SetDirty(config);

            GameObject hudPrefab = BuildHudPrefab();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Camera.main.backgroundColor = new Color(.025f, .075f, .055f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            var system = new GameObject("PigHerdSystem");
            system.AddComponent<PigHerdController>().Configure(config);

            var canvasObject = new GameObject("PigGrowthCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            CreateImage("Background", canvasObject.transform, new Color(.025f, .075f, .055f), Vector2.zero, Vector2.one);
            Text title = CreateText("Title", canvasObject.transform, "养猪牧场 · 猪只与成长系统", 42, TextAnchor.MiddleLeft, new Color(.95f, .78f, .30f));
            SetRect(title.rectTransform, new Vector2(.05f, .88f), new Vector2(.95f, .97f));
            Text rule = CreateText("Rule", canvasObject.transform, "容量 80 格 · 超过 70 格触发拥挤反馈 · 容量不足时操作不消耗资源", 22, TextAnchor.MiddleLeft, new Color(.72f, .82f, .76f));
            SetRect(rule.rectTransform, new Vector2(.05f, .82f), new Vector2(.95f, .88f));
            PrefabUtility.InstantiatePrefab(hudPrefab, canvasObject.transform);
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Pig growth prototype created: " + ScenePath);
        }

        private static GameObject BuildHudPrefab()
        {
            var root = new GameObject("PigHerdHUD", typeof(RectTransform), typeof(Image), typeof(PigHerdHUD));
            SetRect(root.GetComponent<RectTransform>(), new Vector2(.05f, .08f), new Vector2(.95f, .80f));
            root.GetComponent<Image>().color = new Color(.075f, .20f, .135f, .98f);

            Text capacity = CreateText("Capacity", root.transform, "猪圈容量 19 / 80", 32, TextAnchor.MiddleLeft, new Color(.95f, .80f, .35f));
            SetRect(capacity.rectTransform, new Vector2(.05f, .84f), new Vector2(.95f, .96f));
            GameObject divider = CreateImage("Divider", root.transform, new Color(.20f, .38f, .27f), new Vector2(.04f, .80f), new Vector2(.96f, .805f));

            Text list = CreateText("PigList", root.transform, "猪只列表", 25, TextAnchor.UpperLeft, Color.white);
            SetRect(list.rectTransform, new Vector2(.05f, .31f), new Vector2(.60f, .76f));
            Text selected = CreateText("Selected", root.transform, "已选择", 22, TextAnchor.MiddleLeft, new Color(.80f, .88f, .82f));
            SetRect(selected.rectTransform, new Vector2(.05f, .22f), new Vector2(.60f, .30f));
            Text message = CreateText("Message", root.transform, "选择猪只并执行成长操作", 21, TextAnchor.MiddleLeft, new Color(.98f, .70f, .32f));
            SetRect(message.rectTransform, new Vector2(.05f, .08f), new Vector2(.60f, .19f));

            Button previous = CreateButton("Previous", root.transform, "上一只", new Color(.20f, .42f, .30f), new Vector2(.65f, .67f), new Vector2(.78f, .78f));
            Button next = CreateButton("Next", root.transform, "下一只", new Color(.20f, .42f, .30f), new Vector2(.81f, .67f), new Vector2(.94f, .78f));
            Button grow = CreateButton("Grow", root.transform, "喂养成长", new Color(.20f, .55f, .30f), new Vector2(.65f, .48f), new Vector2(.94f, .62f));
            Button nutrition = CreateButton("NutritionGrow", root.transform, "营养剂成长 2 级", new Color(.38f, .38f, .68f), new Vector2(.65f, .29f), new Vector2(.94f, .43f));
            Button birth = CreateButton("Birth", root.transform, "生育猪宝宝", new Color(.72f, .37f, .16f), new Vector2(.65f, .10f), new Vector2(.94f, .24f));
            root.GetComponent<PigHerdHUD>().Configure(capacity, list, selected, message, previous, next, grow, nutrition, birth);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static PigStageDefinition Stage(string fileName, string id, string displayName, int value, int cells, bool canBreed)
        {
            string path = PigConfigFolder + "/" + fileName + ".asset";
            PigStageDefinition stage = AssetDatabase.LoadAssetAtPath<PigStageDefinition>(path);
            if (!stage)
            {
                stage = ScriptableObject.CreateInstance<PigStageDefinition>();
                AssetDatabase.CreateAsset(stage, path);
            }
            stage.id = id;
            stage.displayName = displayName;
            stage.value = value;
            stage.occupiedCells = cells;
            stage.canBreed = canBreed;
            return stage;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = CreateImage(name, parent, color, min, max);
            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, .18f);
            colors.pressedColor = Color.Lerp(color, Color.black, .25f);
            button.colors = colors;
            Text text = CreateText("Label", go.transform, label, 23, TextAnchor.MiddleCenter, Color.white);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
        }

        private static GameObject CreateImage(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), min, max);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
#endif
