using System;
namespace PigFarm.Core {
[Serializable] public struct GameFlowSnapshot {
 public int round,totalRounds,roundInSeason,roundsPerSeason,seasonIndex,seasonCount,coins;
 public string seasonName;
 public bool isComplete;
}
public sealed class GameFlowState {
 readonly GameFlowConfig config; int round; int coins;
 public GameFlowState(GameFlowConfig config){this.config=config?config:throw new ArgumentNullException(nameof(config));round=1;coins=config.startingCoins;}
 public GameFlowSnapshot Snapshot { get {
  int seasonIndex=Math.Min((round-1)/config.roundsPerSeason,config.SeasonCount-1);
  return new GameFlowSnapshot{round=round,totalRounds=config.totalRounds,roundInSeason=((round-1)%config.roundsPerSeason)+1,roundsPerSeason=config.roundsPerSeason,seasonIndex=seasonIndex,seasonCount=config.SeasonCount,seasonName=config.GetSeasonName(seasonIndex),coins=coins,isComplete=round>config.totalRounds};
 }}
 public bool TryAdvance(out bool seasonChanged){var before=Snapshot;if(before.isComplete){seasonChanged=false;return false;}round++;var after=Snapshot;seasonChanged=after.isComplete||before.seasonIndex!=after.seasonIndex;return true;}
 public void AddCoins(int amount){coins=Math.Max(0,coins+amount);}
}}