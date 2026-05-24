using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.Localization.Components;
using UnityStandardAssets.CrossPlatformInput;

public class ElevatorButtonController : MonoBehaviour
{
    private static ElevatorButtonController activeButton;

    private enum ElevatorDirection
    {
        Auto = 0,
        Top = 1,
        Bottom = 2,
        Ads = 3
    }

    public ElevatorController elevatorController;
    private ObjectTracking objectTracking;
    private TextMeshProUGUI reachUIText;
    private bool isElevatorMoving;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip buttonPressSound;
    [Header("Ads Hint UI")]
    [FormerlySerializedAs("answerText")]
    [SerializeField] private TextMeshProUGUI messageTextComponent;
    [SerializeField] private float answerMessageDuration = 4f;
    [SerializeField]
    private ElevatorDirection elevatorDirection = ElevatorDirection.Auto;
    [SerializeField]
    private string reachTag = "MainCamera";

    private Coroutine answerMessageRoutine;
    private bool isProcessingAdsHint;
    private bool playerInRange;

    private const string LocalizationTableName = "Localization";
    private const string AdsPromptKey = "ADS_HINT_PROMPT";
    private const string RewardedUnavailableKey = "ADS_HINT_REWARDED_UNAVAILABLE";

    void Start()
    {
        isElevatorMoving = false;
        if(elevatorController == null) elevatorController = GetComponentInParent<ElevatorController>();
        if (elevatorController != null)
            objectTracking = elevatorController.GetComponentInChildren<ObjectTracking>();

        ResolveMessageTextComponent();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        StartCoroutine(FindReachUIText());
    }

    IEnumerator FindReachUIText()
    {
        yield return new WaitForSeconds(1f);

        GameObject hud = GameObject.Find("HUD");
        if (hud != null)
        {
            Transform reachUI = hud.transform.Find("ReachUI");
            if (reachUI != null)
            {
                reachUIText = reachUI.GetComponentInChildren<TextMeshProUGUI>();
                if (reachUIText != null)
                    reachUIText.text = "";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        bool isReachInteractor = IsReachInteractor(other);
        bool isInsideElevator = IsPlayerInsideElevator();

        if (isReachInteractor && isInsideElevator)
        {
            if (IsAdsButtonUnavailable() || !CanUseAdsHintHere())
                return;

            playerInRange = true;
            activeButton = this;
            isElevatorMoving = true;

            if (reachUIText != null && elevatorController != null && elevatorController.CanUseButtons())
                reachUIText.text = GetInteractionPromptText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsReachInteractor(other))
        {
            playerInRange = false;
            if (activeButton == this)
            {
                activeButton = null;
                isElevatorMoving = false;
                if (reachUIText != null) reachUIText.text = "";
            }
        }
    }

    private string GetElevatorCommand()
    {
        switch (elevatorDirection)
        {
            case ElevatorDirection.Top:
                return "Top";
            case ElevatorDirection.Bottom:
                return "Bottom";
            case ElevatorDirection.Ads:
                return "Ads";
            default:
                string objectName = gameObject.name.ToLowerInvariant();
                if (objectName.Contains("top"))
                    return "Top";
                if (objectName.Contains("bottom"))
                    return "Bottom";
                if (objectName.Contains("ads"))
                    return "Ads";
                return "Top";
        }
    }

    private string GetInteractionPromptText()
    {
        return GetElevatorCommand() == "Ads"
            ? InteractionPromptLocalization.GetLocalizedString(LocalizationTableName, AdsPromptKey)
            : InteractionPromptLocalization.GetPrompt();
    }

    private bool IsPlayerInsideElevator()
    {
        if (objectTracking != null)
            return objectTracking.IsPlayerInside();

        if (elevatorController != null)
            return elevatorController.IsPlayerInsideElevator();

        return false;
    }

    private bool IsReachInteractor(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrEmpty(reachTag) && other.CompareTag(reachTag))
            return true;

        return IsPlayerInteractor(other);
    }

    private bool IsPlayerInteractor(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    void Update()
    {
        if (PauseGame.isPaused || PauseGame.IsGameplayLocked)
            return;

        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!interactPressed)
            return;

        bool isInsideElevator = IsPlayerInsideElevator();
        bool canInteract = activeButton == this && isElevatorMoving && elevatorController != null && elevatorController.CanUseButtons() && isInsideElevator;

        if (canInteract)
        {
            if (animator != null)
                animator.SetTrigger("Pressing");

            if (reachUIText != null)
                reachUIText.text = "";

            isElevatorMoving = false;
            if (activeButton == this)
                activeButton = null;

            audioSource?.PlayOneShot(buttonPressSound);

            string elevatorCommand = GetElevatorCommand();
            if (elevatorCommand == "Ads")
            {
                if (!CanUseAdsHintHere())
                {
                    RestoreAdsPromptIfPossible();
                    return;
                }

                if (!isProcessingAdsHint)
                {
                    StartCoroutine(HandleAdsHintInteraction());
                }
                return;
            }

            elevatorController.ElevatorButtonPressed(elevatorCommand);
        }
    }

    private IEnumerator HandleAdsHintInteraction()
    {
        if (elevatorController != null && !elevatorController.TryUseAdsHintThisFloor())
        {
            RestoreAdsPromptIfPossible();
            yield break;
        }

        isProcessingAdsHint = true;
        yield return StartCoroutine(YGInterstitialService.ShowInterstitialAndWait(true, 5f));
        isProcessingAdsHint = false;

        if (elevatorController == null)
        {
            ShowTemporaryAnswer(InteractionPromptLocalization.GetLocalizedString(LocalizationTableName, RewardedUnavailableKey));
            RestoreAdsPromptIfPossible();
            yield break;
        }

        ShowPersistentAnswer(elevatorController.GetCurrentAnomalyDescription());
        RestoreAdsPromptIfPossible();
    }

    private void ShowTemporaryAnswer(string message)
    {
        ResolveMessageTextComponent();

        if (messageTextComponent == null)
            return;

        if (answerMessageRoutine != null)
            StopCoroutine(answerMessageRoutine);

        answerMessageRoutine = StartCoroutine(ShowAnswerRoutine(message));
    }

    private void ShowPersistentAnswer(string message)
    {
        ResolveMessageTextComponent();

        if (answerMessageRoutine != null)
        {
            StopCoroutine(answerMessageRoutine);
            answerMessageRoutine = null;
        }

        string resolvedMessage = message ?? string.Empty;

        if (elevatorController != null)
            elevatorController.SetAnomalyHintText(resolvedMessage);

        if (messageTextComponent != null)
            messageTextComponent.text = resolvedMessage;
    }

    private IEnumerator ShowAnswerRoutine(string message)
    {
        messageTextComponent.text = message;

        if (answerMessageDuration > 0f)
            yield return new WaitForSeconds(answerMessageDuration);

        if (messageTextComponent != null)
            messageTextComponent.text = string.Empty;

        answerMessageRoutine = null;
    }

    private void RestoreAdsPromptIfPossible()
    {
        if (!playerInRange || elevatorController == null || !elevatorController.CanUseButtons() || IsAdsButtonUnavailable() || !CanUseAdsHintHere())
            return;

        activeButton = this;
        isElevatorMoving = true;

        if (reachUIText != null)
            reachUIText.text = GetInteractionPromptText();
    }

    private bool IsAdsButtonUnavailable()
    {
        return GetElevatorCommand() == "Ads"
            && elevatorController != null
            && elevatorController.HasUsedAdsHintThisFloor();
    }

    private bool CanUseAdsHintHere()
    {
        if (GetElevatorCommand() != "Ads")
            return true;

        return elevatorController != null && elevatorController.CanUseAdsHint();
    }

    private void ResolveMessageTextComponent()
    {
        if (messageTextComponent == null && elevatorController != null)
            messageTextComponent = elevatorController.anomalyHintText;

        DisableMessageTextLocalizationOverride();
    }

    private void DisableMessageTextLocalizationOverride()
    {
        if (messageTextComponent == null)
            return;

        LocalizeStringEvent localizedStringEvent = messageTextComponent.GetComponent<LocalizeStringEvent>();
        if (localizedStringEvent != null && localizedStringEvent.enabled)
            localizedStringEvent.enabled = false;
    }
}
