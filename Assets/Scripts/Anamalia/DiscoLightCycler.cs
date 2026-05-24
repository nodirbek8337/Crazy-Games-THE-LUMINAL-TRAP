using System.Collections;
using UnityEngine;

public class DiscoLightCycler : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light[] targetLights;

    [Header("Colors")]
    [SerializeField] private Color colorA = Color.red;
    [SerializeField] private Color colorB = Color.blue;

    [Header("Timing")]
    [Min(0.02f)]
    [SerializeField] private float colorSwapInterval = 0.35f;

    private Color[] originalLightColors;
    private Coroutine colorRoutine;
    private bool showingColorA;

    private void Awake()
    {
        CacheOriginalColors();
    }

    private void OnEnable()
    {
        CacheOriginalColors();
        StartColorSequence();
    }

    private void OnDisable()
    {
        StopColorSequence();
        RestoreOriginalColors();
    }

    private void CacheOriginalColors()
    {
        if (targetLights == null)
        {
            originalLightColors = null;
            return;
        }

        if (originalLightColors == null || originalLightColors.Length != targetLights.Length)
            originalLightColors = new Color[targetLights.Length];

        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] != null)
                originalLightColors[i] = targetLights[i].color;
        }
    }

    private void StartColorSequence()
    {
        if (targetLights == null || targetLights.Length == 0)
            return;

        StopColorSequence();
        showingColorA = false;
        colorRoutine = StartCoroutine(ColorSequenceRoutine());
    }

    private void StopColorSequence()
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = null;
    }

    private IEnumerator ColorSequenceRoutine()
    {
        while (true)
        {
            showingColorA = !showingColorA;
            ApplyCurrentColor();

            if (colorSwapInterval > 0f)
                yield return new WaitForSeconds(colorSwapInterval);
            else
                yield return null;
        }
    }

    private void ApplyCurrentColor()
    {
        if (targetLights == null)
            return;

        Color nextColor = showingColorA ? colorA : colorB;

        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] != null)
                targetLights[i].color = nextColor;
        }
    }

    private void RestoreOriginalColors()
    {
        if (targetLights == null || originalLightColors == null)
            return;

        int count = Mathf.Min(targetLights.Length, originalLightColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (targetLights[i] != null)
                targetLights[i].color = originalLightColors[i];
        }
    }
}
