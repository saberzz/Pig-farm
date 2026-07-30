using System;
using UnityEngine;
namespace PigFarm.Core {
public sealed class GameFlowController:MonoBehaviour {
 [SerializeField] GameFlowConfig config;
 GameFlowState state;
 public event Action<GameFlowSnapshot> StateChanged;
 public event Action<GameFlowSnapshot> SeasonEnded;
 public event Action<GameFlowSnapshot> GameEnded;
 public bool IsInitialized=>state!=null;
 public GameFlowSnapshot Current=>state!=null?state.Snapshot:default(GameFlowSnapshot);
 public void Configure(GameFlowConfig value){config=value;}
 void Awake(){if(!config){Debug.LogError("GameFlowController requires a GameFlowConfig.",this);enabled=false;return;}state=new GameFlowState(config);}
 void Start(){Publish();}
 public void AdvanceRound(){
  if(state==null||state.Snapshot.isComplete)return;
  var before=state.Snapshot;bool seasonChanged;state.TryAdvance(out seasonChanged);var after=state.Snapshot;
  if(seasonChanged)SeasonEnded?.Invoke(before);Publish();if(after.isComplete)GameEnded?.Invoke(after);
 }
 public void AddCoins(int amount){if(state==null)return;state.AddCoins(amount);Publish();}
 void Publish(){StateChanged?.Invoke(state.Snapshot);}
}}