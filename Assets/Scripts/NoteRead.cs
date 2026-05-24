using System.Collections;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class NoteRead : MonoBehaviour
{
    [Header("UI")]
    public GameObject noteImage;

    [Header("Player")]
    public Movement playerMovement;

    [Header("Pause")]
    public PauseGame pauseGame;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip noteToggleClip;

    private TextMeshProUGUI reachUIText;
    private Collider interactionCollider;
    private bool inReach;
    private bool isReading;
    private bool lockedGameplay;
    private CursorLockMode cachedCursorLockState = CursorLockMode.Locked;
    private bool cachedCursorVisible = false;
    private Coroutine refreshReachRoutine;

    void Awake()
    {
        interactionCollider = GetComponent<Collider>();
        if (interactionCollider == null || !interactionCollider.isTrigger)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                if (col != null && col.isTrigger)
                {
                    interactionCollider = col;
                    break;
                }
            }

            if (interactionCollider == null && colliders.Length > 0)
                interactionCollider = colliders[0];
        }

        HideNoteImage();
    }

    void OnEnable()
    {
        HideNoteImage();
        inReach = false;
        isReading = false;
        lockedGameplay = false;

        if (refreshReachRoutine != null)
            StopCoroutine(refreshReachRoutine);

        refreshReachRoutine = StartCoroutine(RefreshReachStateAfterEnable());
    }

    void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (pauseGame == null)
            pauseGame = FindObjectOfType<PauseGame>();

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
            reachUIText.text = "";
    }

    private IEnumerator RefreshReachStateAfterEnable()
    {
        yield return null;
        RefreshReachState();
        refreshReachRoutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerInteractor(other))
            return;

        inReach = true;
        UpdateReachPrompt();
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsPlayerInteractor(other))
            return;

        if (!inReach)
            inReach = true;

        UpdateReachPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayerInteractor(other))
            return;

        inReach = false;
        UpdateReachPrompt();
    }

    void Update()
    {
        if (PauseGame.isPaused && !isReading)
            return;

        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        bool pausePressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Pause")
            : Input.GetKeyDown(KeyCode.Escape);

        if (interactPressed)
        {
            if (isReading)
            {
                CloseNote();
                return;
            }

            if (inReach)
            {
                OpenNote();
            }
        }

        if (isReading && pausePressed)
        {
            CloseNoteAndOpenPause();
        }
    }

    private void OpenNote()
    {
        PlayToggleSound();
        isReading = true;

        CacheAndHideCursor();

        if (reachUIText != null)
            reachUIText.text = "";

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

    private void OnDisable()
    {
        if (refreshReachRoutine != null)
        {
            StopCoroutine(refreshReachRoutine);
            refreshReachRoutine = null;
        }

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

        inReach = false;
        isReading = false;
        lockedGameplay = false;

        if (reachUIText != null)
            reachUIText.text = "";
    }

    private void CloseNoteAndOpenPause()
    {
        bool wasLocked = lockedGameplay;
        CloseNote();

        if (wasLocked)
        {
            if (pauseGame != null)
                pauseGame.Pause();
        }
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

    private void UpdateReachPrompt()
    {
        if (reachUIText == null)
            return;

        if (isReading)
        {
            reachUIText.text = "";
            return;
        }

        reachUIText.text = inReach ? InteractionPromptLocalization.GetPrompt() : "";
    }

    private void HideNoteImage()
    {
        if (noteImage != null)
            noteImage.SetActive(false);
    }

    private void RefreshReachState()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (playerMovement == null || playerMovement.controller == null || interactionCollider == null)
            return;

        inReach = interactionCollider.bounds.Intersects(playerMovement.controller.bounds);
        UpdateReachPrompt();
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
}
