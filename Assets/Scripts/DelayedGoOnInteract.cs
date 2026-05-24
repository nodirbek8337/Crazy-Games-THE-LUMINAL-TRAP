using TMPro;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class DelayedGoOnInteract : MonoBehaviour
{
    [Header("Prompt")]
    public TextMeshProUGUI reachUIText;

    [Header("UI Toggle")]
    public GameObject uiObjectToToggle;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Go Trigger")]
    [SerializeField] private GoOnPlayerTrigger goOnPlayerTrigger;

    private bool inReach;
    private bool isUiOpen;
    private bool hasTriggered;

    private void OnEnable()
    {
        inReach = false;
        isUiOpen = false;
        SetToggleUiActive(false);
        UpdatePrompt();
    }

    private void OnDisable()
    {
        inReach = false;
        isUiOpen = false;
        SetToggleUiActive(false);
        UpdatePrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || hasTriggered)
            return;

        inReach = true;
        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        inReach = false;
        isUiOpen = false;
        SetToggleUiActive(false);
        UpdatePrompt();
    }

    private void Update()
    {
        if (hasTriggered)
            return;

        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!interactPressed || !inReach)
            return;

        if (!isUiOpen)
        {
            isUiOpen = true;
            SetToggleUiActive(true);
            UpdatePrompt();
            return;
        }

        hasTriggered = true;
        isUiOpen = false;
        SetToggleUiActive(false);
        UpdatePrompt();

        if (goOnPlayerTrigger != null)
            goOnPlayerTrigger.TriggerPlayerEntered();

        enabled = false;
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrWhiteSpace(playerTag) && other.CompareTag(playerTag))
            return true;

        if (other.CompareTag("MainCamera"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private void UpdatePrompt()
    {
        if (reachUIText == null)
            return;

        reachUIText.text = (!hasTriggered && inReach && !isUiOpen)
            ? InteractionPromptLocalization.GetPrompt()
            : string.Empty;
    }

    private void SetToggleUiActive(bool isActive)
    {
        if (uiObjectToToggle != null)
            uiObjectToToggle.SetActive(isActive);
    }
}
