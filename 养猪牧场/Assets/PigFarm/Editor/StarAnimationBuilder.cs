#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PigFarm.Editor
{
    public static class StarAnimationBuilder
    {
        const string SpriteFolder = "Assets/PigFarm/Sprite";
        const string AnimationFolder = "Assets/PigFarm/Animations/Stars";
        const string PrefabFolder = "Assets/PigFarm/Prefabs/Stars";
        const string ScenePath = "Assets/PigFarm/Scenes/StarAnimationPreview.unity";
        static readonly string[] Names = { "小星星", "中星星", "大星星", "超大星星" };

        [MenuItem("Pig Farm/Build Animated Stars")]
        public static void Build()
        {
            EnsureFolder(AnimationFolder);
            EnsureFolder(PrefabFolder);
            var prefabs = new GameObject[Names.Length];
            for (int i = 0; i < Names.Length; i++) prefabs[i] = BuildStar(Names[i]);
            BuildPreviewScene(prefabs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Built four animated star prefabs and preview scene.");
        }

        static GameObject BuildStar(string name)
        {
            string spritePath = SpriteFolder + "/" + name + ".png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (!sprite) throw new FileNotFoundException("Missing star sprite", spritePath);

            string clipPath = AnimationFolder + "/" + name + "_Float.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (!clip)
            {
                clip = new AnimationClip { name = name + "_Float", frameRate = 30f };
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            ConfigureClip(clip, sprite);

            string controllerPath = AnimationFolder + "/" + name + "_Animator.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            for (int i = stateMachine.states.Length - 1; i >= 0; i--) stateMachine.RemoveState(stateMachine.states[i].state);
            AnimatorState state = stateMachine.AddState("FloatLoop");
            state.motion = clip;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);

            GameObject root = new GameObject(name, typeof(Animator));
            GameObject visual = new GameObject("Visual", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;
            Animator animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            string prefabPath = PrefabFolder + "/" + name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void ConfigureClip(AnimationClip clip, Sprite sprite)
        {
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
                AnimationUtility.SetEditorCurve(clip, binding, null);
            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
            var spriteBinding = EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, new[]
            {
                new ObjectReferenceKeyframe { time = 0f, value = sprite },
                new ObjectReferenceKeyframe { time = 1.2f, value = sprite }
            });

            SetCurve(clip, "Visual", "m_LocalPosition.x", 0f, .10f, 0f);
            SetCurve(clip, "Visual", "m_LocalPosition.y", 0f, .18f, 0f);
            SetCurve(clip, "Visual", "m_LocalPosition.z", 0f, 0f, 0f);
            SetCurve(clip, "Visual", "m_LocalScale.x", 1f, 1.08f, 1f);
            SetCurve(clip, "Visual", "m_LocalScale.y", 1f, 1.08f, 1f);
            SetCurve(clip, "Visual", "m_LocalScale.z", 1f, 1f, 1f);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }

        static void SetCurve(AnimationClip clip, string path, string property, float start, float middle, float end)
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, start),
                new Keyframe(.6f, middle),
                new Keyframe(1.2f, end));
            for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
        }

        static void BuildPreviewScene(GameObject[] prefabs)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.3f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.08f, .14f, .11f, 1f);
            GameObject light = new GameObject("Directional Light", typeof(Light));
            light.GetComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            GameObject parent = new GameObject("AnimatedStars");
            Vector3[] positions =
            {
                new Vector3(-2.4f, .25f, 0f), new Vector3(-.8f, .25f, 0f),
                new Vector3(.8f, .25f, 0f), new Vector3(2.4f, .25f, 0f)
            };
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[i], parent.transform);
                instance.transform.localPosition = positions[i];
                instance.transform.localScale = Vector3.one * .75f;
            }
            EditorSceneManager.SaveScene(scene, ScenePath);
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
