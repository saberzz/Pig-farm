using UnityEngine;
namespace PigFarm.Core {
[CreateAssetMenu(fileName="GameFlowConfig",menuName="Pig Farm/Config/Game Flow")]
public sealed class GameFlowConfig:ScriptableObject {
 [Min(1)] public int totalRounds=16;
 [Min(1)] public int roundsPerSeason=4;
 [Min(0)] public int startingCoins=18;
 public string[] seasonNames={"春季","夏季","秋季","冬季"};
 public int SeasonCount { get { int n=Mathf.CeilToInt(totalRounds/(float)Mathf.Max(1,roundsPerSeason)); return Mathf.Max(1,Mathf.Min(n,seasonNames==null?0:seasonNames.Length)); } }
 public string GetSeasonName(int index){return seasonNames==null||seasonNames.Length==0?"季节 "+(index+1):seasonNames[Mathf.Clamp(index,0,seasonNames.Length-1)];}
 void OnValidate(){totalRounds=Mathf.Max(1,totalRounds);roundsPerSeason=Mathf.Max(1,roundsPerSeason);if(seasonNames==null||seasonNames.Length==0)seasonNames=new[]{"春季","夏季","秋季","冬季"};}
}}