using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Vampire
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private CharacterSelector characterSelector;
        [SerializeField] private Texture2D splashLogo;
        [SerializeField] private float splashFadeInDuration = 0.6f;
        [SerializeField] private float splashHoldDuration = 1.2f;
        [SerializeField] private float splashFadeOutDuration = 0.6f;
        [SerializeField] private Vector2 splashLogoMaxSize = new Vector2(640f, 360f);
        [SerializeField] private Color splashBackgroundColor = Color.black;

        private CanvasGroup splashCanvasGroup;
        private GameObject splashRoot;

        void Start()
        {
            characterSelector.Init();

            if (splashLogo != null)
            {
                CreateSplashScreen();
                StartCoroutine(PlaySplashScreen());
            }
        }

        private void CreateSplashScreen()
        {
            splashRoot = new GameObject("Splash Screen", typeof(RectTransform), typeof(CanvasGroup));
            splashRoot.transform.SetParent(transform, false);
            splashRoot.transform.SetAsLastSibling();

            RectTransform rootRect = splashRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            splashCanvasGroup = splashRoot.GetComponent<CanvasGroup>();
            splashCanvasGroup.alpha = 0f;
            splashCanvasGroup.blocksRaycasts = true;
            splashCanvasGroup.interactable = true;

            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(splashRoot.transform, false);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = splashBackgroundColor;

            GameObject logo = new GameObject("Logo", typeof(RectTransform), typeof(RawImage));
            logo.transform.SetParent(splashRoot.transform, false);
            RectTransform logoRect = logo.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;
            logoRect.sizeDelta = GetLogoSize();

            RawImage logoImage = logo.GetComponent<RawImage>();
            logoImage.texture = splashLogo;
            logoImage.color = Color.white;
        }

        private Vector2 GetLogoSize()
        {
            Vector2 textureSize = new Vector2(splashLogo.width, splashLogo.height);
            if (textureSize.x <= 0f || textureSize.y <= 0f)
            {
                return splashLogoMaxSize;
            }

            float scale = Mathf.Min(splashLogoMaxSize.x / textureSize.x, splashLogoMaxSize.y / textureSize.y, 1f);
            return textureSize * scale;
        }

        private IEnumerator PlaySplashScreen()
        {
            yield return FadeSplash(0f, 1f, splashFadeInDuration);

            if (splashHoldDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(splashHoldDuration);
            }

            yield return FadeSplash(1f, 0f, splashFadeOutDuration);

            Destroy(splashRoot);
            splashRoot = null;
            splashCanvasGroup = null;
        }

        private IEnumerator FadeSplash(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                splashCanvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                splashCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            splashCanvasGroup.alpha = to;
        }
    }
}
