using UnityEngine;

namespace PigFarm.Audio
{
    public enum PigFarmAudioCue
    {
        UiClick,
        InvalidAction,
        Roll,
        RoundTransition,
        Trade,
        ItemAndVaccine,
        FeedAndGrow,
        Breed,
        TaskReward
    }

    public sealed class PigFarmAudioService : MonoBehaviour
    {
        public static PigFarmAudioService Instance { get; private set; }

        [SerializeField] PigFarmAudioConfig config;
        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioSource effectsSource;

        public void Configure(PigFarmAudioConfig value)
        {
            config = value;
            EnsureSources();
            ApplySettings();
        }

        void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
            ApplySettings();
            StartMusic();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Play(PigFarmAudioCue cue)
        {
            if (Instance) Instance.PlayInternal(cue);
        }

        void StartMusic()
        {
            if (!config || !config.gameplayMusic || !musicSource || musicSource.isPlaying) return;
            musicSource.clip = config.gameplayMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        void PlayInternal(PigFarmAudioCue cue)
        {
            if (!config || !effectsSource) return;
            AudioClip clip = GetClip(cue);
            if (clip) effectsSource.PlayOneShot(clip, config.effectsVolume);
        }

        AudioClip GetClip(PigFarmAudioCue cue)
        {
            switch (cue)
            {
                case PigFarmAudioCue.UiClick: return config.uiClick;
                case PigFarmAudioCue.InvalidAction: return config.invalidAction;
                case PigFarmAudioCue.Roll: return config.roll;
                case PigFarmAudioCue.RoundTransition: return config.roundTransition;
                case PigFarmAudioCue.Trade: return config.trade;
                case PigFarmAudioCue.ItemAndVaccine: return config.itemAndVaccine;
                case PigFarmAudioCue.FeedAndGrow: return config.feedAndGrow;
                case PigFarmAudioCue.Breed: return config.breed;
                case PigFarmAudioCue.TaskReward: return config.taskReward;
                default: return null;
            }
        }

        void EnsureSources()
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (!musicSource) musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            if (!effectsSource) effectsSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            effectsSource.playOnAwake = false;
        }

        void ApplySettings()
        {
            if (!config) return;
            if (musicSource) musicSource.volume = config.musicVolume;
            if (effectsSource) effectsSource.volume = 1f;
        }
    }
}
