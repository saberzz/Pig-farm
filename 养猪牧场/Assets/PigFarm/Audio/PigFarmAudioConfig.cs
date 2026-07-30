using UnityEngine;

namespace PigFarm.Audio
{
    [CreateAssetMenu(menuName = "Pig Farm/Audio Config", fileName = "PigFarmAudioConfig")]
    public sealed class PigFarmAudioConfig : ScriptableObject
    {
        [Header("Music")]
        public AudioClip gameplayMusic;
        [Range(0f, 1f)] public float musicVolume = .35f;

        [Header("Sound Effects")]
        public AudioClip uiClick;
        public AudioClip invalidAction;
        public AudioClip roll;
        public AudioClip roundTransition;
        public AudioClip trade;
        public AudioClip itemAndVaccine;
        public AudioClip feedAndGrow;
        public AudioClip breed;
        public AudioClip taskReward;
        [Range(0f, 1f)] public float effectsVolume = .8f;
    }
}
