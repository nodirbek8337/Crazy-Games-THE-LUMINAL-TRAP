using System.Collections;
using TMPro;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PrologueElevatorButtonController : MonoBehaviour
{
    private static PrologueElevatorButtonController activeButton;

    [Header("References")]
    public PrologueElevator prologueElevator;
    public AudioSource audioSource;
    public AudioClip buttonPressSound;

    [Header("Interaction")]
    [SerializeField] private string reachTag = "MainCamera";

    private TextMeshProUGUI reachUIText;
    private bool canInteract;

    private void Start()
    {
        if (prologueElevator == null)
            prologueElevator = GetComponentInParent<PrologueElevator>();
    }

    private void OnEnable()
    {
        StartCoroutine(FindReachUIText());
    }

    private void OnDisable()
    {
        if (activeButton == this)
            activeButton = null;

        canInteract = false;

        if (reachUIText != null)
            reachUIText.text = string.Empty;
    }

    private IEnumerator FindReachUIText()
    {
        yield return new WaitForSeconds(1f);

        GameObject hud = GameObject.Find("HUD");
        if (hud == null)
            yield break;

        Transform reachUI = hud.transform.Find("ReachUI");
        if (reachUI == null)
            yield break;

        reachUIText = reachUI.GetComponentInChildren<TextMeshProUGUI>();
        if (reachUIText != null)
            reachUIText.text = string.Empty;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsReachInteractor(other))
            return;

        if (prologueElevator == null || prologueElevator.isBlockingEvents || !prologueElevator.isDoorOpen)
            return;

        if (!prologueElevator.IsPlayerInsideElevator())
            return;

        activeButton = this;
        canInteract = true;

        if (reachUIText != null)
            reachUIText.text = InteractionPromptLocalization.GetPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsReachInteractor(other))
            return;

        if (activeButton == this)
            activeButton = null;

        canInteract = false;

        if (reachUIText != null)
            reachUIText.text = string.Empty;
    }

    private void Update()
    {
        if (PauseGame.isPaused || PauseGame.IsGameplayLocked)
            return;

        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!interactPressed)
            return;

        if (activeButton != this || !canInteract || prologueElevator == null)
            return;

        if (prologueElevator.isBlockingEvents || !prologueElevator.isDoorOpen)
            return;

        if (!prologueElevator.IsPlayerInsideElevator())
            return;

        if (reachUIText != null)
            reachUIText.text = string.Empty;

        canInteract = false;
        if (activeButton == this)
            activeButton = null;

        prologueElevator.GoPressed();
        audioSource?.PlayOneShot(buttonPressSound);
    }

    private bool IsReachInteractor(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrEmpty(reachTag) && other.CompareTag(reachTag))
            return true;

        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }
}
