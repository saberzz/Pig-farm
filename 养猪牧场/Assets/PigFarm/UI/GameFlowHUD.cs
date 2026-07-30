using PigFarm.Core;
using UnityEngine;
using UnityEngine.UI;
namespace PigFarm.UI {
public sealed class GameFlowHUD:MonoBehaviour {
 [SerializeField] Text seasonText,roundText,coinText,hintText;
 [SerializeField] Button advanceButton;
 [SerializeField] GameFlowController source;
 bool subscribed;
 Font runtimeFont;
 public void Configure(Text season,Text round,Text coin,Text hint,Button advance){seasonText=season;roundText=round;coinText=coin;hintText=hint;advanceButton=advance;}
 void OnEnable(){ApplyChineseFont();if(!source)source=FindObjectOfType<GameFlowController>();if(advanceButton)advanceButton.onClick.AddListener(OnAdvanceClicked);Subscribe();if(source&&source.IsInitialized)Apply(source.Current);else SetText(hintText,"等待回合系统...");}
 void Start(){Subscribe();if(source&&source.IsInitialized)Apply(source.Current);}
 void OnDisable(){if(advanceButton)advanceButton.onClick.RemoveListener(OnAdvanceClicked);if(source&&subscribed)source.StateChanged-=Apply;subscribed=false;}
 void OnDestroy(){if(runtimeFont)Destroy(runtimeFont);}
 void ApplyChineseFont(){
  if(!runtimeFont)runtimeFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei UI","Microsoft YaHei","SimHei","Arial"},28);
  if(!runtimeFont)return;
  var labels=GetComponentsInChildren<Text>(true);foreach(var label in labels)label.font=runtimeFont;
 }
 void Subscribe(){if(!source||subscribed)return;source.StateChanged+=Apply;subscribed=true;}
 void OnAdvanceClicked(){if(source)source.AdvanceRound();}
 void Apply(GameFlowSnapshot state){
  if(!source||!source.IsInitialized){SetText(hintText,"等待回合系统...");return;}
  SetText(seasonText,state.isComplete?"经营结束":state.seasonName);
  SetText(roundText,state.isComplete?"全部 "+state.totalRounds+" 回合已完成":"第 "+state.round+" / "+state.totalRounds+" 回合  ·  本季 "+state.roundInSeason+" / "+state.roundsPerSeason);
  SetText(coinText,"金币  "+state.coins);
  SetText(hintText,state.isComplete?"已完成一年的经营结算":"结束当前回合，推进时间轴");
  if(advanceButton)advanceButton.interactable=!state.isComplete;
 }
 static void SetText(Text target,string value){if(target)target.text=value;}
}}
