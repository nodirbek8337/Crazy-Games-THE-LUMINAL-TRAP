using System.Collections;
using UnityEngine;

public static class ScreenCanvasFader
{
    private const string ScreenCanvasObjectName = "ScreenCanvas";
    private const float DefaultFadeDuration = 0.5f;

    private static CanvasGroup cachedCanvasGroup;
    private static bool warnedMissingCanvas;

    public static IEnumerator FadeIn(float duration = DefaultFadeDuration)
    {
        return FadeTo(1f, duration);
    }

    public static IEnumerator FadeOut(float duration = DefaultFadeDuration)
    {
        return FadeTo(0f, duration);
    }

    public static void SetHidden()
    {
        CanvasGroup canvasGroup = ResolveCanvasGroup();
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public static void SetVisible()
    {
        CanvasGroup canvasGroup = ResolveCanvasGroup();
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private static IEnumerator FadeTo(float targetAlpha, float duration)
    {
        CanvasGroup canvasGroup = ResolveCanvasGroup();
        if (canvasGroup == null)
            yield break;

        targetAlpha = Mathf.Clamp01(targetAlpha);

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha > 0f;
            canvasGroup.blocksRaycasts = targetAlpha > 0f;
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            canvasGroup.alpha = currentAlpha;
            canvasGroup.interactable = currentAlpha > 0.001f;
            canvasGroup.blocksRaycasts = currentAlpha > 0.001f;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
    }

    private static CanvasGroup ResolveCanvasGroup()
    {
        if (cachedCanvasGroup != null)
        {
            if (cachedCanvasGroup.gameObject != null)
                return cachedCanvasGroup;

            cachedCanvasGroup = null;
        }

        GameObject screenCanvasObject = GameObject.Find(ScreenCanvasObjectName);
        if (screenCanvasObject == null)
        {
            if (!warnedMissingCanvas)
            {
                Debug.LogWarning("ScreenCanvasFader: GameObject named 'ScreenCanvas' was not found.");
                warnedMissingCanvas = true;
            }

            return null;
        }

        cachedCanvasGroup = screenCanvasObject.GetComponent<CanvasGroup>();
        if (cachedCanvasGroup == null)
            cachedCanvasGroup = screenCanvasObject.AddComponent<CanvasGroup>();

        cachedCanvasGroup.interactable = false;
        cachedCanvasGroup.blocksRaycasts = false;

        return cachedCanvasGroup;
    }
}
