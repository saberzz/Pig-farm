#if UNITY_EDITOR
using PigFarm.Flow;
using PigFarm.UI.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PigFarm.Editor
{
    public static class PigFarmHudPrefabReplacer
    {
        const string HudPath = "Assets/PigFarm/Prefabs/Flow/GameplayFlowHUD.prefab";
        const string GameScenePath = "Assets/PigFarm/Scenes/PigFarmGame.unity";

        static readonly string[][] Replacements =
        {
            new[] { "TopBar", "Assets/PigFarm/Prefabs/UI/TopBar.prefab" },
            new[] { "TaskStage", "Assets/PigFarm/Prefabs/UI/TaskStage.prefab" },
            new[] { "ActionArea", "Assets/PigFarm/Prefabs/UI/ActionArea.prefab" },
            new[] { "ActionExecutionModal", "Assets/PigFarm/Prefabs/UI/ActionExecutionModal.prefab" },
            new[] { "RoundTransitionModal", "Assets/PigFarm/Prefabs/UI/RoundTransitionModal.prefab" },
            new[] { "FinalSettlementModal", "Assets/PigFarm/Prefabs/UI/FinalSettlementModal.prefab" },
            new[] { "RoundStartTip", "Assets/PigFarm/Prefabs/UI/RoundStartTip.prefab" },
            new[] { "RollResultPanel", "Assets/PigFarm/Prefabs/UI/RollResultPanel.prefab" },
            new[] { "SellScreen", "Assets/PigFarm/Prefabs/UI/SellScreen.prefab" },
            new[] { "TutorialPopup", "Assets/PigFarm/Prefabs/Flow/TutorialPopup.prefab" },
            new[] { "OpeningShopIntro", "Assets/PigFarm/Prefabs/UI/OpeningShopIntro.prefab" },
            new[] { "ShopScreen", "Assets/PigFarm/Prefabs/UI/ShopScreen.prefab" },
        };

        [MenuItem("Pig Farm/Replace HUD Children With Prefabs")]
        public static void Replace()
        {
            GameObject hud = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                for (int i = 0; i < Replacements.Length; i++)
                    ReplaceChild(hud.transform, Replacements[i][0], Replacements[i][1]);

                RebindView(hud);
                HideOverlays(hud.transform);
                PrefabUtility.SaveAsPrefabAsset(hud, HudPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(hud);
            }

            SyncScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("GameplayFlowHUD children replaced with same-name prefabs and view bindings restored.");
        }

        static void ReplaceChild(Transform parent, string childName, string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab)
            {
                Debug.LogError("Missing prefab: " + prefabPath);
                return;
            }

            Transform old = parent.Find(childName);
            int sibling = old ? old.GetSiblingIndex() : parent.childCount;
            bool hadRect = false;
            Vector2 aMin = Vector2.zero, aMax = Vector2.one, oMin = Vector2.zero, oMax = Vector2.zero, pivot = new Vector2(.5f, .5f);
            Vector3 scale = Vector3.one;
            Quaternion rot = Quaternion.identity;
            if (old)
            {
                RectTransform oldRt = old.GetComponent<RectTransform>();
                if (oldRt)
                {
                    hadRect = true;
                    aMin = oldRt.anchorMin;
                    aMax = oldRt.anchorMax;
                    oMin = oldRt.offsetMin;
                    oMax = oldRt.offsetMax;
                    pivot = oldRt.pivot;
                    scale = oldRt.localScale;
                    rot = oldRt.localRotation;
                }
                Object.DestroyImmediate(old.gameObject);
            }

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.name = childName;
            inst.transform.SetSiblingIndex(sibling);
            RectTransform rt = inst.GetComponent<RectTransform>();
            if (rt && hadRect)
            {
                rt.anchorMin = aMin;
                rt.anchorMax = aMax;
                rt.offsetMin = oMin;
                rt.offsetMax = oMax;
                rt.pivot = pivot;
                rt.localScale = scale;
                rt.localRotation = rot;
            }
            else if (rt)
            {
                bool overlay = childName.EndsWith("Modal") || childName.EndsWith("Panel") || childName.EndsWith("Tip")
                    || childName.EndsWith("Screen") || childName.EndsWith("Intro") || childName == "TutorialPopup";
                if (overlay)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
            }
        }

        static void RebindView(GameObject hud)
        {
            PigFarmGameplayView view = hud.GetComponent<PigFarmGameplayView>();
            SerializedObject so = new SerializedObject(view);
            Transform root = hud.transform;

            Set(so, "tutorial", Comp<PigFarmTutorialPopup>(root, "TutorialPopup"));
            Set(so, "openingIntro", Comp<PigFarmOpeningShopIntro>(root, "OpeningShopIntro"));
            Set(so, "stageText", Comp<Text>(root, "TaskStage/StageTaskSection/Stage"));
            Set(so, "taskText", Comp<Text>(root, "TaskStage/StageTaskSection/Task"));
            Set(so, "rewardText", Comp<Text>(root, "TaskStage/StageTaskSection/Reward"));
            Set(so, "resourceText", Comp<Text>(root, "TopBar/Resources"));
            Set(so, "herdText", Comp<Text>(root, "Pen/HerdSummary"));
            Set(so, "rollRangeText", Comp<Text>(root, "ActionArea/RollRange"));
            Set(so, "messageText", Comp<Text>(root, "ActionArea/Message"));
            Set(so, "confirmButton", Comp<Button>(root, "ActionArea/ConfirmButton"));
            Set(so, "actionPanel", Go(root, "ActionExecutionModal"));
            Set(so, "actionTitleText", Comp<Text>(root, "ActionExecutionModal/Card/Title"));
            Set(so, "actionBodyText", Comp<Text>(root, "ActionExecutionModal/Card/Body"));
            Set(so, "primaryActionButton", Comp<Button>(root, "ActionExecutionModal/Card/PrimaryButton"));
            Set(so, "itemActionButton", Comp<Button>(root, "ActionExecutionModal/Card/ItemButton"));
            Set(so, "shopPanel", Go(root, "ActionExecutionModal/Card/ShopItems"));
            Set(so, "settleRoundButton", Comp<Button>(root, "ActionExecutionModal/Card/SettleButton"));
            Set(so, "vaccinateButton", Comp<Button>(root, "TopBar/VaccinateButton"));
            Set(so, "transitionPanel", Go(root, "RoundTransitionModal"));
            Set(so, "transitionText", Comp<Text>(root, "RoundTransitionModal/Card/Result"));
            Set(so, "transitionContinueButton", Comp<Button>(root, "RoundTransitionModal/Card/ContinueButton"));
            Set(so, "finalPanel", Go(root, "FinalSettlementModal"));
            Set(so, "finalText", Comp<Text>(root, "FinalSettlementModal/Score"));
            Set(so, "restartButton", Comp<Button>(root, "FinalSettlementModal/RestartButton"));
            Set(so, "roundTipPanel", Go(root, "RoundStartTip"));
            Set(so, "roundTipStageText", Comp<Text>(root, "RoundStartTip/TipCard/StageAndRound"));
            Set(so, "roundTipTaskText", Comp<Text>(root, "RoundStartTip/TipCard/RandomTask"));
            Set(so, "tutorialButton", EnsureButton(Find(root, "TopBar/TutorialIcon")));
            Set(so, "stageTaskSection", Go(root, "TaskStage/StageTaskSection"));
            Set(so, "taskContextSection", Go(root, "TaskStage/TaskContextSection"));
            Set(so, "contextCounterPanel", Go(root, "TaskStage/TaskContextSection/ContextCounter"));
            Set(so, "contextCounterLabel", Comp<Text>(root, "TaskStage/TaskContextSection/ContextCounter/CounterLabel"));
            Set(so, "contextCounterValue", Comp<Text>(root, "TaskStage/TaskContextSection/ContextCounter/CounterValue"));
            Set(so, "contextGuideText", Comp<Text>(root, "TaskStage/TaskContextSection/ContextGuide"));
            Set(so, "penActionButton", Comp<Button>(root, "PenActionButton"));
            Set(so, "rollResultPanel", Go(root, "RollResultPanel"));
            Set(so, "rollActionText", Comp<Text>(root, "RollResultPanel/RollCard/ActionImagePlaceholder/RollingAction"));
            Set(so, "rollNumberText", Comp<Text>(root, "RollResultPanel/RollCard/NumberBlock/RollingNumber"));
            Set(so, "shopScreen", Go(root, "ShopScreen"));
            Set(so, "shopCloseButton", Comp<Button>(root, "ShopScreen/CloseShopButton"));
            Set(so, "sellScreen", Go(root, "SellScreen"));
            Set(so, "sellCloseButton", Comp<Button>(root, "SellScreen/CloseSellButton"));
            Set(so, "shopHeaderText", Comp<Text>(root, "ShopScreen/TopBar/HeaderText"));
            Set(so, "shopVaccineCountText", Comp<Text>(root, "ShopScreen/TopBar/VaccineOwned/Count"));
            Set(so, "shopGoldCountText", Comp<Text>(root, "ShopScreen/TopBar/GoldOwned/Count"));
            Set(so, "shopTaskText", Comp<Text>(root, "ShopScreen/LeftPanel/TaskBody"));
            Set(so, "shopPurchaseCountText", Comp<Text>(root, "ShopScreen/LeftPanel/PurchaseCount"));
            Set(so, "shopStarCountText", Comp<Text>(root, "ShopScreen/LeftPanel/StarCounts"));
            Set(so, "shopTotalValueText", Comp<Text>(root, "ShopScreen/LeftPanel/TotalValue"));

            SerializedProperty starsProp = so.FindProperty("stars");
            SerializedProperty slotsProp = so.FindProperty("starSlots");
            starsProp.arraySize = 24;
            slotsProp.arraySize = 24;
            for (int i = 0; i < 24; i++)
            {
                Transform starT = Find(root, "Pen/Star_" + (i + 1));
                Image img = starT ? starT.GetComponent<Image>() : null;
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = img;
                PigFarmPenStarSlot slot = null;
                if (starT)
                {
                    if (img) img.raycastTarget = true;
                    Button btn = starT.GetComponent<Button>();
                    if (!btn) btn = starT.gameObject.AddComponent<Button>();
                    if (img) btn.targetGraphic = img;
                    btn.transition = Selectable.Transition.None;
                    slot = starT.GetComponent<PigFarmPenStarSlot>();
                    if (!slot) slot = starT.gameObject.AddComponent<PigFarmPenStarSlot>();
                    slot.Configure(i, btn, img);
                }
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slot;
            }

            SerializedProperty actionsProp = so.FindProperty("actionButtons");
            actionsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                actionsProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Button>(root, "ActionArea/Action" + i);

            string[] products = { "BuyStar0", "BuyStar1", "BuyStar2", "BuyStar3", "BuyItem0", "BuyItem1", "BuyItem2" };
            SerializedProperty shopBtnProp = so.FindProperty("shopButtons");
            SerializedProperty nameProp = so.FindProperty("shopProductNameTexts");
            SerializedProperty iconProp = so.FindProperty("shopProductIcons");
            SerializedProperty priceProp = so.FindProperty("shopProductPriceTexts");
            shopBtnProp.arraySize = products.Length;
            nameProp.arraySize = products.Length;
            iconProp.arraySize = products.Length;
            priceProp.arraySize = products.Length;
            for (int i = 0; i < products.Length; i++)
            {
                string basePath = "ShopScreen/Products/" + products[i];
                shopBtnProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Button>(root, basePath);
                nameProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Text>(root, basePath + "/Name");
                iconProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Image>(root, basePath + "/Icon");
                priceProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Text>(root, basePath + "/Price");
            }

            SerializedProperty sellProp = so.FindProperty("sellPigButtons");
            sellProp.arraySize = 8;
            for (int i = 0; i < 8; i++)
                sellProp.GetArrayElementAtIndex(i).objectReferenceValue = Comp<Button>(root, "SellScreen/SellPigList/SellPig" + i);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void HideOverlays(Transform root)
        {
            string[] hide =
            {
                "ActionExecutionModal", "RoundTransitionModal", "FinalSettlementModal",
                "RoundStartTip", "RollResultPanel", "SellScreen", "ShopScreen", "OpeningShopIntro"
            };
            for (int i = 0; i < hide.Length; i++)
            {
                Transform t = Find(root, hide[i]);
                if (t) t.gameObject.SetActive(false);
            }
        }

        static void SyncScene()
        {
            EditorSceneManager.OpenScene(GameScenePath);
            PigFarmGameplayView view = Object.FindObjectOfType<PigFarmGameplayView>();
            PigFarmGameSessionController session = Object.FindObjectOfType<PigFarmGameSessionController>();
            if (!view) return;
            if (PrefabUtility.IsPartOfPrefabInstance(view.gameObject))
                PrefabUtility.RevertPrefabInstance(view.gameObject, InteractionMode.AutomatedAction);
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("session").objectReferenceValue = session;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
        }

        static void Set(SerializedObject so, string name, Object value)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null)
            {
                Debug.LogWarning("Missing property: " + name);
                return;
            }
            p.objectReferenceValue = value;
            if (!value) Debug.LogWarning("Null binding: " + name);
        }

        static Transform Find(Transform root, string path)
        {
            if (!root) return null;
            if (string.IsNullOrEmpty(path)) return root;
            string[] parts = path.Split('/');
            Transform t = root;
            for (int i = 0; i < parts.Length; i++)
            {
                t = t.Find(parts[i]);
                if (!t) return null;
            }
            return t;
        }

        static GameObject Go(Transform root, string path)
        {
            Transform t = Find(root, path);
            return t ? t.gameObject : null;
        }

        static T Comp<T>(Transform root, string path) where T : Component
        {
            Transform t = Find(root, path);
            return t ? t.GetComponent<T>() : null;
        }

        static Button EnsureButton(Transform t)
        {
            if (!t) return null;
            Button btn = t.GetComponent<Button>();
            if (!btn) btn = t.gameObject.AddComponent<Button>();
            Graphic g = t.GetComponent<Graphic>();
            if (g) btn.targetGraphic = g;
            return btn;
        }
    }
}
#endif
