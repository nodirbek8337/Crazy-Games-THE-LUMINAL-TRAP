using UnityEngine;
using System.Collections;

public class AmbientColorChanger : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor = new Color32(203, 207, 181, 255);
    public Color darkColor = Color.black;

    [Header("Transition Settings")]
    public float duration = 0.5f;

    private Coroutine currentCoroutine;

    public void SetDark()
    {
        StartColorChange(darkColor);
    }

    public void SetNormal()
    {
        StartColorChange(normalColor);
    }

    private void StartColorChange(Color targetColor)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ChangeAmbientColor(targetColor));
    }

    private IEnumerator ChangeAmbientColor(Color targetColor)
    {
        Color startColor = RenderSettings.ambientLight;
        float elapsed = 0;

        while (elapsed < duration)
        {
            RenderSettings.ambientLight = Color.Lerp(startColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        RenderSettings.ambientLight = targetColor;
    }
}
