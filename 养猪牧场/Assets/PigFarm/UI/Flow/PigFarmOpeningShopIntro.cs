using System;
using UnityEngine;
using UnityEngine.UI;
using PigFarm.Audio;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmOpeningShopIntro : MonoBehaviour
    {
        [SerializeField] Text bodyText;
        [SerializeField] Button enterShopButton;

        public event Action EnterShopRequested;

        public void Configure(Text body, Button enterButton)
        {
            bodyText = body;
            enterShopButton = enterButton;
        }

        public void Show(string body)
        {
            if (bodyText) bodyText.text = body;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (enterShopButton) enterShopButton.onClick.AddListener(OnEnterClicked);
        }

        void OnDisable()
        {
            if (enterShopButton) enterShopButton.onClick.RemoveListener(OnEnterClicked);
        }

        void OnEnterClicked()
        {
            PigFarmAudioService.Play(PigFarmAudioCue.UiClick);
            Hide();
            EnterShopRequested?.Invoke();
        }
    }
}
