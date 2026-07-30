using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PigFarm.UI.Flow
{
    public sealed class PigFarmPenStarSlot : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image image;
        [SerializeField] RectTransform motionRoot;
        [SerializeField] float floatHeight = 12f;
        [SerializeField] float floatScale = 1.08f;
        [SerializeField] float floatDuration = 1.2f;

        Vector2 baseAnchoredPos;
        Vector3 baseScale;
        Coroutine floatRoutine;
        bool baseCached;

        public Button Button { get { return button; } }
        public Image Image { get { return image; } }
        public int SlotIndex { get; private set; }

        public void Configure(int index, Button slotButton, Image slotImage)
        {
            SlotIndex = index;
            button = slotButton;
            image = slotImage;
            motionRoot = transform as RectTransform;
            CacheBase();
        }

        void Awake()
        {
            if (!button) button = GetComponent<Button>();
            if (!image) image = GetComponent<Image>();
            if (!motionRoot) motionRoot = transform as RectTransform;
            CacheBase();
        }

        void OnEnable()
        {
            CacheBase();
        }

        void OnDisable()
        {
            StopFloat();
            ResetPose();
        }

        public void PlayFloat()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;
            CacheBase();
            StopFloat();
            floatRoutine = StartCoroutine(FloatOnce());
        }

        public void StopFloat()
        {
            if (floatRoutine != null)
            {
                StopCoroutine(floatRoutine);
                floatRoutine = null;
            }
            ResetPose();
        }

        void CacheBase()
        {
            if (!motionRoot) motionRoot = transform as RectTransform;
            if (!motionRoot) return;
            baseAnchoredPos = motionRoot.anchoredPosition;
            baseScale = motionRoot.localScale;
            baseCached = true;
        }

        void ResetPose()
        {
            if (!baseCached || !motionRoot) return;
            motionRoot.anchoredPosition = baseAnchoredPos;
            motionRoot.localScale = baseScale;
        }

        IEnumerator FloatOnce()
        {
            float elapsed = 0f;
            while (elapsed < floatDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / floatDuration);
                float wave = Mathf.Sin(t * Mathf.PI);
                motionRoot.anchoredPosition = baseAnchoredPos + new Vector2(0f, floatHeight * wave);
                float scale = Mathf.Lerp(1f, floatScale, wave);
                motionRoot.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
                yield return null;
            }
            ResetPose();
            floatRoutine = null;
        }
    }
}
