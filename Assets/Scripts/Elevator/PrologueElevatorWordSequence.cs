using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

[System.Serializable]
public class PrologueWordEntry
{
    public DialogueLineKey lineKey = DialogueLineKey.None;
    public TMP_Text textModel;
}

public class PrologueElevatorWordSequence : MonoBehaviour
{
    [Header("References")]
    public PrologueElevator prologueElevator;
    public MainCameraAnimationController mainCameraAnimationController;
    public Light flickerLight;
    public AudioSource loopAudioSource;
    public AudioSource flickerStartAudioSource;
    public AudioClip flickerStartClip;
    public GameObject dialogueObjectToActivate;

    [Header("Words")]
    public PrologueWordEntry[] wordEntries;

    [Header("Timing")]
    public float sequenceDuration = 30f;
    public float fadeToBlackDuration = 0.5f;

    [Header("Word Flicker")]
    public float minWordAlpha = 0.15f;
    public float maxWordAlpha = 1f;
    public float fastWordFlickerMinInterval = 0.04f;
    public float fastWordFlickerMaxInterval = 0.14f;
    public float wordPauseDarkMinDuration = 0.15f;
    public float wordPauseDarkMaxDuration = 0.55f;
    public int minWordBurstCount = 2;
    public int maxWordBurstCount = 6;

    [Header("Light Flicker")]
    public float minLightIntensity = 0.2f;
    public float maxLightIntensity = 1.2f;
    public float fastFlickerMinInterval = 0.03f;
    public float fastFlickerMaxInterval = 0.12f;
    public float pauseDarkMinDuration = 0.2f;
    public float pauseDarkMaxDuration = 0.8f;
    public int minBurstCount = 2;
    public int maxBurstCount = 6;

    private Coroutine sequenceRoutine;
    private Coroutine lightRoutine;
    private Coroutine wordRoutine;
    private bool isRunning;
    private float originalLightIntensity;
    private bool originalLightEnabled;

    private void Awake()
    {
        ResolveReferences();

        if (flickerLight != null)
        {
            originalLightIntensity = flickerLight.intensity;
            originalLightEnabled = flickerLight.enabled;
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
        HideAllWords();
    }

    private void OnDisable()
    {
        StopAllRunningEffects();
        RestoreLightState();
        HideAllWords();

        if (loopAudioSource != null && loopAudioSource.isPlaying)
            loopAudioSource.Stop();
    }

    public void StartSequence()
    {
        if (isRunning)
            return;

        ResolveReferences();
        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        isRunning = true;

        yield return LocalizationSettings.InitializationOperation;

        if (dialogueObjectToActivate != null)
            dialogueObjectToActivate.SetActive(true);

        ApplyLocalizedTexts();
        ShowAllWords();
        StartLoopAudio();
        PlayFlickerStartAudio();
        StartWordFlicker();
        StartLightFlicker();

        yield return new WaitForSeconds(sequenceDuration);

        if (mainCameraAnimationController != null)
            mainCameraAnimationController.SetDie(true);

        StopWordBlinkVisuals();
        StopWordFlicker();
        StopLightFlicker();

        yield return StartCoroutine(ScreenCanvasFader.FadeIn(fadeToBlackDuration));

        if (prologueElevator != null)
            prologueElevator.ActivatePreloadedScene();

        sequenceRoutine = null;
        isRunning = false;
    }

    private void ApplyLocalizedTexts()
    {
        if (wordEntries == null)
            return;

        foreach (PrologueWordEntry entry in wordEntries)
        {
            if (entry == null || entry.textModel == null || entry.lineKey == DialogueLineKey.None)
                continue;

            entry.textModel.text = GetLocalizedText(entry.lineKey.ToString(), entry.lineKey.ToString());
        }
    }

    private void ShowAllWords()
    {
        if (wordEntries == null)
            return;

        foreach (PrologueWordEntry entry in wordEntries)
        {
            if (entry == null || entry.textModel == null)
                continue;

            entry.textModel.gameObject.SetActive(true);
        }
    }

    private void HideAllWords()
    {
        if (wordEntries == null)
            return;

        foreach (PrologueWordEntry entry in wordEntries)
        {
            if (entry == null || entry.textModel == null)
                continue;

            entry.textModel.gameObject.SetActive(false);
        }
    }

    private void StartWordFlicker()
    {
        StopWordFlicker();
        wordRoutine = StartCoroutine(FlickerWordsRoutine());
    }

    private void StopWordFlicker()
    {
        if (wordRoutine == null)
            return;

        StopCoroutine(wordRoutine);
        wordRoutine = null;
    }

    private IEnumerator FlickerWordsRoutine()
    {
        while (true)
        {
            int burstCount = Random.Range(minWordBurstCount, maxWordBurstCount + 1);

            for (int i = 0; i < burstCount; i++)
            {
                float alpha = Random.value > 0.35f ? Random.Range(minWordAlpha, maxWordAlpha) : 0f;
                SetAllWordAlpha(alpha);
                yield return new WaitForSeconds(Random.Range(fastWordFlickerMinInterval, fastWordFlickerMaxInterval));
            }

            SetAllWordAlpha(0f);
            yield return new WaitForSeconds(Random.Range(wordPauseDarkMinDuration, wordPauseDarkMaxDuration));

            SetAllWordAlpha(Random.Range(minWordAlpha, maxWordAlpha));
            yield return new WaitForSeconds(Random.Range(fastWordFlickerMinInterval, fastWordFlickerMaxInterval));
        }
    }

    private void StopWordBlinkVisuals()
    {
        SetAllWordAlpha(1f);
    }

    private void SetAllWordAlpha(float alpha)
    {
        if (wordEntries == null)
            return;

        foreach (PrologueWordEntry entry in wordEntries)
        {
            if (entry == null || entry.textModel == null)
                continue;

            Color color = entry.textModel.color;
            color.a = alpha;
            entry.textModel.color = color;
        }
    }

    private void StartLoopAudio()
    {
        if (loopAudioSource == null)
            return;

        loopAudioSource.loop = true;
        if (!loopAudioSource.isPlaying)
            loopAudioSource.Play();
    }

    private void PlayFlickerStartAudio()
    {
        if (flickerStartAudioSource == null || flickerStartClip == null)
            return;

        flickerStartAudioSource.Stop();
        flickerStartAudioSource.clip = flickerStartClip;
        flickerStartAudioSource.loop = false;
        flickerStartAudioSource.Play();
    }

    private void StartLightFlicker()
    {
        if (flickerLight == null)
            return;

        StopLightFlicker();
        lightRoutine = StartCoroutine(FlickerLightRoutine());
    }

    private void StopLightFlicker()
    {
        if (lightRoutine == null)
            return;

        StopCoroutine(lightRoutine);
        lightRoutine = null;
    }

    private IEnumerator FlickerLightRoutine()
    {
        flickerLight.enabled = true;

        while (true)
        {
            int burstCount = Random.Range(minBurstCount, maxBurstCount + 1);

            for (int i = 0; i < burstCount; i++)
            {
                bool lightOn = Random.value > 0.35f;
                flickerLight.enabled = lightOn;
                flickerLight.intensity = lightOn ? Random.Range(minLightIntensity, maxLightIntensity) : 0f;
                yield return new WaitForSeconds(Random.Range(fastFlickerMinInterval, fastFlickerMaxInterval));
            }

            flickerLight.enabled = false;
            flickerLight.intensity = 0f;
            yield return new WaitForSeconds(Random.Range(pauseDarkMinDuration, pauseDarkMaxDuration));

            flickerLight.enabled = true;
            flickerLight.intensity = Random.Range(minLightIntensity, maxLightIntensity);
            yield return new WaitForSeconds(Random.Range(fastFlickerMinInterval, fastFlickerMaxInterval));
        }
    }

    private void RestoreLightState()
    {
        if (flickerLight == null)
            return;

        flickerLight.intensity = originalLightIntensity;
        flickerLight.enabled = originalLightEnabled;
    }

    private void StopAllRunningEffects()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        StopWordFlicker();
        StopLightFlicker();

        sequenceRoutine = null;
        isRunning = false;
    }

    private void ResolveReferences()
    {
        if (prologueElevator == null)
            prologueElevator = FindObjectOfType<PrologueElevator>();

        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key) || LocalizationSettings.StringDatabase == null)
            return fallback;

        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(DialogueLocalizationKeys.TableName, key);
        if (string.IsNullOrEmpty(localized))
            return fallback;

        if (localized.StartsWith("No translation found for '", System.StringComparison.Ordinal))
            return fallback;

        return localized;
    }
}
