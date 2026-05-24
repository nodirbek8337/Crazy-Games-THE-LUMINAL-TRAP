using UnityEngine;

public class LightColorSwitcher : MonoBehaviour
{
    [Header("Lights")]
    public Light[] targetLights;

    [Header("Colors")]
    public Color enabledColor = Color.red;

    [Header("Timing")]
    public float colorChangeDelay = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip switchClip;

    private Color[] originalColors;
    private Coroutine delayedRoutine;

    private void Awake()
    {
        CacheOriginalColors();
    }

    private void OnEnable()
    {
        CacheOriginalColors();

        if (delayedRoutine != null)
            StopCoroutine(delayedRoutine);

        delayedRoutine = StartCoroutine(DelayedSwitchRoutine());
    }

    private void OnDisable()
    {
        if (delayedRoutine != null)
            StopCoroutine(delayedRoutine);

        delayedRoutine = null;
        RestoreOriginalColors();
    }

    private System.Collections.IEnumerator DelayedSwitchRoutine()
    {
        if (colorChangeDelay > 0f)
            yield return new WaitForSeconds(colorChangeDelay);

        ApplyEnabledColor();
        PlaySwitchSound();
        delayedRoutine = null;
    }

    private void CacheOriginalColors()
    {
        if (targetLights == null)
        {
            originalColors = null;
            return;
        }

        if (originalColors == null || originalColors.Length != targetLights.Length)
            originalColors = new Color[targetLights.Length];

        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] != null)
                originalColors[i] = targetLights[i].color;
        }
    }

    private void ApplyEnabledColor()
    {
        if (targetLights == null)
            return;

        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] != null)
                targetLights[i].color = enabledColor;
        }
    }

    private void RestoreOriginalColors()
    {
        if (targetLights == null || originalColors == null)
            return;

        int count = Mathf.Min(targetLights.Length, originalColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (targetLights[i] != null)
                targetLights[i].color = originalColors[i];
        }
    }

    private void PlaySwitchSound()
    {
        if (audioSource == null || switchClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = switchClip;
        audioSource.loop = false;
        audioSource.Play();
    }
}
