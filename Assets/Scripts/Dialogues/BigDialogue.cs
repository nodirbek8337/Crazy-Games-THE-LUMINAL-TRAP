using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class BigDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLineData
    {
        public DialogueLineKey lineKey = DialogueLineKey.None;
        public DialogueSpeakerKey speakerKey = DialogueSpeakerKey.None;
        public float duration = 3f;
        public AudioClip voiceClip;
    }

    [Header("UI")]
    public TextMeshProUGUI speakerTextComponent;
    public TextMeshProUGUI messageTextComponent;
    public TextMeshProUGUI skipTextComponent;

    [Header("Dialogue")]
    public DialogueLineData[] dialogueLines;
    public bool useClick = false;
    public bool allowSkip = false;
    public bool restrictPlayerMovement = true;
    public bool allowCameraLookWhileMovementRestricted = false;
    public float fadeDuration = 0.5f;
    public float skipAdvanceDelay = 1f;

    [Header("Runtime References")]
    public AudioSource audioSource;
    public Movement playerMovement;

    private int index = 0;
    private bool dialogueStarted = false;
    private bool dialogueFinished = false;
    private bool appliedGameplayLock = false;
    private bool isSkippingLine = false;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
        ResetDialogueState();
        StartCoroutine(BeginDialogueRoutine());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;

        if (audioSource != null)
            audioSource.Stop();

        if (dialogueStarted && !dialogueFinished && restrictPlayerMovement && playerMovement != null)
            playerMovement.Unfreeze();

        HideSkipPrompt();
        ReleaseGameplayLockIfNeeded();
    }

    private void Update()
    {
        if (PauseGame.isPaused)
            return;

        if (dialogueStarted && !dialogueFinished && !isSkippingLine && useClick && Input.GetMouseButtonDown(0))
            NextLine();

        if (dialogueStarted && !dialogueFinished && !isSkippingLine && allowSkip && Input.GetKeyDown(KeyCode.Q))
            SkipDialogue();
    }

    private IEnumerator BeginDialogueRoutine()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (speakerTextComponent != null)
            speakerTextComponent.text = string.Empty;

        if (messageTextComponent != null)
            messageTextComponent.text = string.Empty;

        HideSkipPrompt();

        if (GetDialogueLineCount() == 0)
        {
            Debug.LogWarning($"{name}: BigDialogue has no configured lines.");
            Destroy(gameObject);
            yield break;
        }

        StartDialogue();
    }

    private void StartDialogue()
    {
        if (dialogueStarted || dialogueFinished)
            return;

        dialogueStarted = true;
        index = 0;
        UpdateSkipPrompt();

        if (restrictPlayerMovement && !allowCameraLookWhileMovementRestricted)
        {
            SetDialogueGameplayLock(true);
            appliedGameplayLock = true;
        }

        if (restrictPlayerMovement && playerMovement != null)
            playerMovement.Freeze();

        if (audioSource != null)
            audioSource.Stop();

        StartCoroutine(FadeInLine());
    }

    private IEnumerator FadeInLine()
    {
        float safeFadeDuration = Mathf.Max(0.0001f, fadeDuration);

        if (speakerTextComponent != null)
            speakerTextComponent.alpha = 0f;

        if (messageTextComponent != null)
            messageTextComponent.alpha = 0f;

        yield return LocalizationSettings.InitializationOperation;

        if (speakerTextComponent != null)
            speakerTextComponent.text = GetSpeakerText(index);

        if (messageTextComponent != null)
            messageTextComponent.text = GetMessageText(index);

        AudioClip currentClip = GetLineVoiceClip(index);
        if (audioSource != null && currentClip != null)
        {
            audioSource.clip = currentClip;
            audioSource.Play();
        }

        isSkippingLine = false;

        float elapsedTime = 0f;
        while (elapsedTime < safeFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            if (speakerTextComponent != null)
                speakerTextComponent.alpha = Mathf.Clamp01(elapsedTime / safeFadeDuration);

            if (messageTextComponent != null)
                messageTextComponent.alpha = Mathf.Clamp01(elapsedTime / safeFadeDuration);

            yield return null;
        }

        if (!useClick)
        {
            yield return new WaitForSeconds(GetLineDuration(index));
            StartCoroutine(FadeOutLine());
        }
    }

    private IEnumerator FadeOutLine()
    {
        float safeFadeDuration = Mathf.Max(0.0001f, fadeDuration);
        float elapsedTime = 0f;

        while (elapsedTime < safeFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            if (speakerTextComponent != null)
                speakerTextComponent.alpha = Mathf.Clamp01(1f - (elapsedTime / safeFadeDuration));

            if (messageTextComponent != null)
                messageTextComponent.alpha = Mathf.Clamp01(1f - (elapsedTime / safeFadeDuration));

            yield return null;
        }

        NextLine();
    }

    private void NextLine()
    {
        index++;

        if (index < GetDialogueLineCount())
        {
            StartCoroutine(FadeInLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueFinished = true;
        dialogueStarted = false;

        if (audioSource != null)
            audioSource.Stop();

        if (restrictPlayerMovement && playerMovement != null)
            playerMovement.Unfreeze();

        HideSkipPrompt();
        ReleaseGameplayLockIfNeeded();
        Destroy(gameObject);
    }

    private void SkipDialogue()
    {
        if (!dialogueStarted || dialogueFinished)
            return;

        StopAllCoroutines();
        StartCoroutine(SkipCurrentLineRoutine());
    }

    private IEnumerator SkipCurrentLineRoutine()
    {
        isSkippingLine = true;

        if (audioSource != null)
            audioSource.Stop();

        if (speakerTextComponent != null)
        {
            speakerTextComponent.alpha = 0f;
            speakerTextComponent.text = string.Empty;
        }

        if (messageTextComponent != null)
        {
            messageTextComponent.alpha = 0f;
            messageTextComponent.text = string.Empty;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, skipAdvanceDelay));

        index++;
        isSkippingLine = false;

        if (index < GetDialogueLineCount())
        {
            StartCoroutine(FadeInLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private void HandleLocaleChanged(Locale _)
    {
        if (!dialogueStarted || dialogueFinished)
            return;

        if (index < 0 || index >= GetDialogueLineCount())
            return;

        if (speakerTextComponent != null)
            speakerTextComponent.text = GetSpeakerText(index);

        if (messageTextComponent != null)
            messageTextComponent.text = GetMessageText(index);

        UpdateSkipPrompt();
    }

    private int GetDialogueLineCount()
    {
        return dialogueLines != null ? dialogueLines.Length : 0;
    }

    private string GetSpeakerText(int lineIndex)
    {
        DialogueLineData line = GetDialogueLine(lineIndex);
        if (line == null)
            return string.Empty;

        if (line.speakerKey == DialogueSpeakerKey.None || line.speakerKey == DialogueSpeakerKey.DOT)
            return string.Empty;

        string localizationKey = GetSpeakerLocalizationKey(line.speakerKey);
        if (!string.IsNullOrEmpty(localizationKey))
            return GetLocalizedText(localizationKey, string.Empty);

        return string.Empty;
    }

    private string GetMessageText(int lineIndex)
    {
        DialogueLineData line = GetDialogueLine(lineIndex);
        if (line == null)
            return string.Empty;

        if (line.lineKey != DialogueLineKey.None)
            return GetLocalizedText(line.lineKey.ToString(), line.lineKey.ToString());

        return string.Empty;
    }

    private float GetLineDuration(int lineIndex)
    {
        DialogueLineData line = GetDialogueLine(lineIndex);
        if (line != null)
            return Mathf.Max(0f, line.duration);

        return 0f;
    }

    private AudioClip GetLineVoiceClip(int lineIndex)
    {
        DialogueLineData line = GetDialogueLine(lineIndex);
        if (line != null)
            return line.voiceClip;

        return null;
    }

    private DialogueLineData GetDialogueLine(int lineIndex)
    {
        if (dialogueLines == null || lineIndex < 0 || lineIndex >= dialogueLines.Length)
            return null;

        return dialogueLines[lineIndex];
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (string.IsNullOrEmpty(key))
            return fallback;

        if (LocalizationSettings.StringDatabase == null)
            return fallback;

        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(DialogueLocalizationKeys.TableName, key);
        if (string.IsNullOrEmpty(localized))
            return fallback;

        if (localized.StartsWith("No translation found for '", System.StringComparison.Ordinal))
            return fallback;

        return localized;
    }

    private void UpdateSkipPrompt()
    {
        if (skipTextComponent == null)
            return;

        bool shouldShowSkip = allowSkip && dialogueStarted && !dialogueFinished;
        skipTextComponent.gameObject.SetActive(shouldShowSkip);

        if (!shouldShowSkip)
            return;

        skipTextComponent.text = GetLocalizedText("DIALOGUE_SKIP_PROMPT", "Q - Skip");
    }

    private void HideSkipPrompt()
    {
        if (skipTextComponent == null)
            return;

        skipTextComponent.text = string.Empty;
        skipTextComponent.gameObject.SetActive(false);
    }

    private static string GetSpeakerLocalizationKey(DialogueSpeakerKey speakerKey)
    {
        switch (speakerKey)
        {
            case DialogueSpeakerKey.SPEAKER_YOU:
                return "SPEAKER_YOU";
            case DialogueSpeakerKey.SPEAKER_XAYRULLA:
                return "SPEAKER_INNER_VOICE";
            default:
                return string.Empty;
        }
    }

    private static void SetDialogueGameplayLock(bool locked)
    {
        PauseGame.SetGameplayLock(locked);
        FPSLook.isStopping = locked;
    }

    private void ReleaseGameplayLockIfNeeded()
    {
        if (!appliedGameplayLock)
            return;

        SetDialogueGameplayLock(false);
        appliedGameplayLock = false;
    }

    private void ResetDialogueState()
    {
        StopAllCoroutines();
        index = 0;
        dialogueStarted = false;
        dialogueFinished = false;
        appliedGameplayLock = false;
        isSkippingLine = false;
        HideSkipPrompt();
    }
}
