using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmStartScreen : MonoBehaviour
    {
        [SerializeField] string gameplaySceneName = "PigFarmGame";
        [SerializeField] Button startButton;

        public void Configure(Button button, string sceneName)
        {
            startButton = button;
            gameplaySceneName = sceneName;
        }

        void OnEnable() { if (startButton) startButton.onClick.AddListener(StartGame); }
        void OnDisable() { if (startButton) startButton.onClick.RemoveListener(StartGame); }
        void StartGame() { SceneManager.LoadScene(gameplaySceneName); }
    }
}
