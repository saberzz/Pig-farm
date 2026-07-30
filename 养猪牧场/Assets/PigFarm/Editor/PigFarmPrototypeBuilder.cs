#if UNITY_EDITOR
using PigFarm.Core;
using PigFarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PigFarm.Editor {
public static class PigFarmPrototypeBuilder {
 const string Root="Assets/PigFarm";
 [MenuItem("Pig Farm/Build Round System Prototype")]
 public static void Build(){
  Folder(Root+"/Config");Folder(Root+"/Prefabs");Folder(Root+"/Scenes");
  var config=AssetDatabase.LoadAssetAtPath<GameFlowConfig>(Root+"/Config/GameFlowConfig.asset");
  if(!config){config=ScriptableObject.CreateInstance<GameFlowConfig>();AssetDatabase.CreateAsset(config,Root+"/Config/GameFlowConfig.asset");}
  var prefab=BuildHud();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
  Camera.main.backgroundColor=new Color(.025f,.075f,.055f);Camera.main.clearFlags=CameraClearFlags.SolidColor;
  var system=new GameObject("GameFlowSystem");system.AddComponent<GameFlowController>().Configure(config);
  var canvasGo=new GameObject("PrototypeCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
  var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
  var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
  Image("Background",canvasGo.transform,new Color(.025f,.075f,.055f),Vector2.zero,Vector2.one);
  var title=Text("PrototypeTitle",canvasGo.transform,"PIG FARM · ROUND SYSTEM",42,TextAnchor.MiddleLeft,new Color(.95f,.78f,.30f));Rect(title.rectTransform,new Vector2(.06f,.78f),new Vector2(.94f,.90f));
  var desc=Text("Description",canvasGo.transform,"A decoupled timeline prototype. Pig, task and shop systems can subscribe to its events.",24,TextAnchor.UpperLeft,new Color(.78f,.86f,.80f));Rect(desc.rectTransform,new Vector2(.06f,.62f),new Vector2(.94f,.77f));
  PrefabUtility.InstantiatePrefab(prefab,canvasGo.transform);
  new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
  EditorSceneManager.SaveScene(scene,Root+"/Scenes/RoundSystemPrototype.unity");
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  Selection.activeObject=AssetDatabase.LoadAssetAtPath<SceneAsset>(Root+"/Scenes/RoundSystemPrototype.unity");
  Debug.Log("Pig Farm round-system prototype created.");
 }
 static GameObject BuildHud(){
  var root=new GameObject("GameFlowHUD",typeof(RectTransform),typeof(Image),typeof(GameFlowHUD));
  Rect(root.GetComponent<RectTransform>(),new Vector2(.06f,.14f),new Vector2(.94f,.53f));root.GetComponent<Image>().color=new Color(.075f,.20f,.135f,.98f);
  var season=Text("Season",root.transform,"春季",38,TextAnchor.MiddleLeft,new Color(.98f,.77f,.27f));Rect(season.rectTransform,new Vector2(.055f,.67f),new Vector2(.42f,.91f));
  var round=Text("Round",root.transform,"第 1 / 16 回合",27,TextAnchor.MiddleLeft,Color.white);Rect(round.rectTransform,new Vector2(.055f,.41f),new Vector2(.67f,.67f));
  var coin=Text("Coins",root.transform,"金币  18",27,TextAnchor.MiddleRight,new Color(.98f,.86f,.50f));Rect(coin.rectTransform,new Vector2(.67f,.67f),new Vector2(.94f,.91f));
  var hint=Text("Hint",root.transform,"结束当前回合，推进时间轴",21,TextAnchor.MiddleLeft,new Color(.72f,.82f,.76f));Rect(hint.rectTransform,new Vector2(.055f,.12f),new Vector2(.64f,.39f));
  var buttonGo=Image("AdvanceButton",root.transform,new Color(.78f,.29f,.10f),new Vector2(.69f,.13f),new Vector2(.94f,.52f));
  var button=buttonGo.AddComponent<Button>();var colors=button.colors;colors.highlightedColor=new Color(.92f,.39f,.13f);colors.pressedColor=new Color(.62f,.20f,.07f);button.colors=colors;
  var label=Text("Label",buttonGo.transform,"结束回合",25,TextAnchor.MiddleCenter,Color.white);Rect(label.rectTransform,Vector2.zero,Vector2.one);
  root.GetComponent<GameFlowHUD>().Configure(season,round,coin,hint,button);
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/GameFlowHUD.prefab");Object.DestroyImmediate(root);return prefab;
 }
 static GameObject Image(string name,Transform parent,Color color,Vector2 min,Vector2 max){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);Rect(go.GetComponent<RectTransform>(),min,max);go.GetComponent<Image>().color=color;return go;}
 static Text Text(string name,Transform parent,string value,int size,TextAnchor align,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.text=value;t.fontSize=size;t.alignment=align;t.color=color;t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=size;return t;}
 static void Rect(RectTransform r,Vector2 min,Vector2 max){r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
 static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;string parent=System.IO.Path.GetDirectoryName(path).Replace("\\","/");Folder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
}}
#endif
