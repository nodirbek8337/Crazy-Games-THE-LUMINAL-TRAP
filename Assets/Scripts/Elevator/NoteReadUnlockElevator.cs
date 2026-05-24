using System.Collections;
using TMPro;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class NoteReadUnlockElevator : MonoBehaviour
{
    [Header("UI")]
    public GameObject noteImage;

    [Header("Player")]
    public Movement playerMovement;

    [Header("Pause")]
    public PauseGame pauseGame;

    [Header("Elevator")]
    public ElevatorController elevatorController;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip noteToggleClip;

    private TextMeshProUGUI reachUIText;
    private bool inReach;
    private bool isReading;
    private bool lockedGameplay;
    private bool elevatorUnlocked;
    private CursorLockMode cachedCursorLockState = CursorLockMode.Locked;
    private bool cachedCursorVisible;

    private void Awake()
    {
        HideNoteImage();
        BlockElevator();
    }

    private void OnEnable()
    {
        HideNoteImage();
        BlockElevator();
    }

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (pauseGame == null)
            pauseGame = FindObjectOfType<PauseGame>();

        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();

        EnsureAudioSource();
        StartCoroutine(FindReachUIText());
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
        if (!IsPlayerInteractor(other))
            return;

        inReach = true;
        UpdateReachPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerInteractor(other))
            return;

        inReach = false;
        UpdateReachPrompt();
    }

    private void Update()
    {
        if (PauseGame.isPaused && !isReading)
            return;

        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!interactPressed)
            return;

        if (isReading)
        {
            CloseNote();
            return;
        }

        if (inReach)
            OpenNote();
    }

    private void OpenNote()
    {
        PlayToggleSound();
        isReading = true;

        CacheAndHideCursor();

        if (reachUIText != null)
            reachUIText.text = string.Empty;

        if (noteImage != null)
            noteImage.SetActive(true);

        if (pauseGame != null)
            pauseGame.FullFreeze(true);

        if (playerMovement != null)
            playerMovement.Freeze();

        lockedGameplay = true;
    }

    private void CloseNote()
    {
        PlayToggleSound();
        isReading = false;

        if (noteImage != null)
            noteImage.SetActive(false);

        if (playerMovement != null)
            playerMovement.Unfreeze();

        if (lockedGameplay)
        {
            if (pauseGame != null)
                pauseGame.FullFreeze(false);

            lockedGameplay = false;
        }

        RestoreCursor();
        UpdateReachPrompt();

        if (!elevatorUnlocked)
        {
            UnlockElevator();
            elevatorUnlocked = true;
        }
    }

    private void OnDisable()
    {
        if (noteImage != null)
            noteImage.SetActive(false);

        if (playerMovement != null)
            playerMovement.Unfreeze();

        if (lockedGameplay)
        {
            if (pauseGame != null)
                pauseGame.FullFreeze(false);

            lockedGameplay = false;
        }

        RestoreCursor();

        if (reachUIText != null)
            reachUIText.text = string.Empty;
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

    private void UpdateReachPrompt()
    {
        if (reachUIText == null)
            return;

        if (isReading)
        {
            reachUIText.text = string.Empty;
            return;
        }

        reachUIText.text = inReach ? InteractionPromptLocalization.GetPrompt() : string.Empty;
    }

    private void HideNoteImage()
    {
        if (noteImage != null)
            noteImage.SetActive(false);
    }

    private void CacheAndHideCursor()
    {
        cachedCursorLockState = Cursor.lockState;
        cachedCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestoreCursor()
    {
        Cursor.lockState = cachedCursorLockState;
        Cursor.visible = cachedCursorVisible;
    }

    private void PlayToggleSound()
    {
        if (noteToggleClip == null)
            return;

        EnsureAudioSource();
        if (audioSource == null)
            return;

        audioSource.PlayOneShot(noteToggleClip);
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
    }

    private void BlockElevator()
    {
        if (elevatorUnlocked)
            return;

        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();

        if (elevatorController != null)
        {
            elevatorController.NotAllowElevatorOperate();
            elevatorController.LockButtonsExternally();
        }
    }

    private void UnlockElevator()
    {
        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();

        if (elevatorController != null)
        {
            elevatorController.UnlockButtonsExternally();
            elevatorController.AllowElevatorOperate();
        }
    }
}
