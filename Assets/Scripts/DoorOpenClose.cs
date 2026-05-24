using System.Collections;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class DoorOpenClose : MonoBehaviour
{
    public Animator doorAnimator;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool inReach = false;
    private bool isOpen = false;
    private bool isBlockingEvents = false;

    private TextMeshProUGUI reachUIText;

    void Start()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

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
        if (IsPlayerInteractor(other))
        {
            inReach = true;

            if (!isBlockingEvents && reachUIText != null)
                reachUIText.text = InteractionPromptLocalization.GetPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayerInteractor(other))
        {
            inReach = false;

            if (reachUIText != null)
                reachUIText.text = "";
        }
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
        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked && !isBlockingEvents && inReach && interactPressed)
        {
            isBlockingEvents = true;

            if (!isOpen)
                StartCoroutine(OpenDoor());
            else
                StartCoroutine(CloseDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        isOpen = true;
        doorAnimator.SetBool("OpenDoor", true);
        if (openSound != null) audioSource?.PlayOneShot(openSound);
        yield return new WaitForSeconds(1f);
        isBlockingEvents = false;
    }

    IEnumerator CloseDoor()
    {
        isOpen = false;
        doorAnimator.SetBool("OpenDoor", false);
        if (closeSound != null) audioSource?.PlayOneShot(closeSound);
        yield return new WaitForSeconds(1f);
        isBlockingEvents = false;
    }

    public void ForceClose()
    {
        if (isOpen)
            StartCoroutine(CloseDoor());
    }
}
