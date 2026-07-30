using System;
using UnityEngine;

namespace PigFarm.UI.Screens
{
    public sealed class PigFarmUIScreenHost : MonoBehaviour
    {
        [SerializeField] PigFarmScreenId initialScreen = PigFarmScreenId.Main;
        [SerializeField] PigFarmScreenView[] screens;

        public PigFarmScreenId CurrentScreen { get; private set; }
        public event Action<PigFarmScreenId> ScreenChanged;

        public void Configure(PigFarmScreenId initial, PigFarmScreenView[] views)
        {
            initialScreen = initial;
            screens = views;
        }

        void Start()
        {
            Subscribe(true);
            Show(initialScreen);
        }

        void OnDestroy()
        {
            Subscribe(false);
        }

        public void Show(PigFarmScreenId id)
        {
            if (screens == null) return;
            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i]) screens[i].SetVisible(screens[i].ScreenId == id);
            }
            CurrentScreen = id;
            Action<PigFarmScreenId> handler = ScreenChanged;
            if (handler != null) handler(id);
        }

        void Subscribe(bool subscribe)
        {
            if (screens == null) return;
            for (int i = 0; i < screens.Length; i++)
            {
                if (!screens[i]) continue;
                if (subscribe) screens[i].NavigationRequested += Show;
                else screens[i].NavigationRequested -= Show;
            }
        }
    }
}
