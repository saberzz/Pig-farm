using System;
using UnityEngine;
using UnityEngine.UI;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmTutorialPopup : MonoBehaviour
    {
        [SerializeField] Text titleText;
        [SerializeField] Text bodyText;
        [SerializeField] Text pageText;
        [SerializeField] Button previousButton;
        [SerializeField] Button nextButton;
        [SerializeField] string[] titles;
        [SerializeField] string[] bodies;
        int page;

        public event Action Completed;

        public void Configure(Text title, Text body, Text indicator, Button previous, Button next, string[] pageTitles, string[] pageBodies)
        {
            titleText = title;
            bodyText = body;
            pageText = indicator;
            previousButton = previous;
            nextButton = next;
            titles = pageTitles;
            bodies = pageBodies;
        }

        void OnEnable()
        {
            page = 0;
            if (previousButton) previousButton.onClick.AddListener(Previous);
            if (nextButton) nextButton.onClick.AddListener(Next);
            Refresh();
        }

        void OnDisable()
        {
            if (previousButton) previousButton.onClick.RemoveListener(Previous);
            if (nextButton) nextButton.onClick.RemoveListener(Next);
        }

        void Previous() { page = Mathf.Max(0, page - 1); Refresh(); }

        void Next()
        {
            int count = titles == null ? 0 : titles.Length;
            if (page + 1 < count) { page++; Refresh(); return; }
            gameObject.SetActive(false);
            Completed?.Invoke();
        }

        void Refresh()
        {
            int count = titles == null ? 0 : titles.Length;
            if (count == 0) return;
            page = Mathf.Clamp(page, 0, count - 1);
            if (titleText) titleText.text = titles[page];
            if (bodyText) bodyText.text = bodies != null && bodies.Length > page ? bodies[page] : string.Empty;
            if (pageText) pageText.text = (page + 1) + " / " + count;
            if (previousButton) previousButton.gameObject.SetActive(page > 0);
            if (nextButton)
            {
                Text label = nextButton.GetComponentInChildren<Text>();
                if (label) label.text = page == count - 1 ? "开始经营" : "下一页";
            }
        }
    }
}
