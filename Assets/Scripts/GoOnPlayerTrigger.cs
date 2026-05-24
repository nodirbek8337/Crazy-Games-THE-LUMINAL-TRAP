using UnityEngine;
using System.Collections;

public class GoOnPlayerTrigger : MonoBehaviour, IGoActionReceiver
{
    [Header("Player")]
    public string playerTag = "Player";

    [Header("Camera Swap")]
    public GameObject mainCamera;
    public GameObject animateCamera;
    [Min(0f)] public float cameraSwapDuration = 3f;

    [Header("Pointer Light")]
    public Light pointerLightToDisable;
    [Min(0f)] public float pointerLightDisableDelay = 4f;

    [Header("Player Lock")]
    public Movement playerMovement;
    public FPSLook playerLook;
    [Min(0f)] public float playerLockDuration = 3f;

    [Header("Go Target")]
    [SerializeField] private MonoBehaviour goReceiver;

    [Header("Audio")]
    [SerializeField] private AudioSource startSequenceAudioSource;

    [Header("Options")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;
    private Coroutine cameraSwapRoutine;
    private Coroutine pointerLightRoutine;
    private Coroutine playerLockRoutine;

    private void OnEnable()
    {
        hasTriggered = false;
        ApplyCameraState(mainCameraActive: true, animateCameraActive: false);
        EnablePointerLight(true);
        ReleasePlayerControl();
    }

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (playerLook == null)
            playerLook = FindObjectOfType<FPSLook>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        StartGoSequence();
    }

    public void Go()
    {
        StartGoSequence();
    }

    public void TriggerPlayerEntered()
    {
        StartGoSequence();
    }

    private void StartGoSequence()
    {
        if ((triggerOnlyOnce && hasTriggered) || GoActivationGate.IsLocked())
            return;

        IGoActionReceiver receiver = GetReceiver();
        if (receiver == null || !GoActivationGate.TryLock())
            return;

        hasTriggered = true;

        if (startSequenceAudioSource != null)
            startSequenceAudioSource.Play();

        StartCameraSwap();
        StartPointerLightDisable();
        StartPlayerLock();
        receiver.Go();
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

    private IGoActionReceiver GetReceiver()
    {
        return goReceiver as IGoActionReceiver;
    }

    private void StartCameraSwap()
    {
        if (cameraSwapRoutine != null)
            StopCoroutine(cameraSwapRoutine);

        cameraSwapRoutine = StartCoroutine(SwapCameraTemporarily());
    }

    private void StartPointerLightDisable()
    {
        if (pointerLightRoutine != null)
            StopCoroutine(pointerLightRoutine);

        pointerLightRoutine = StartCoroutine(DisablePointerLightAfterDelay());
    }

    private IEnumerator DisablePointerLightAfterDelay()
    {
        if (pointerLightDisableDelay > 0f)
            yield return new WaitForSeconds(pointerLightDisableDelay);

        EnablePointerLight(false);
        pointerLightRoutine = null;
    }

    private void StartPlayerLock()
    {
        if (playerLockRoutine != null)
            StopCoroutine(playerLockRoutine);

        LockPlayerControl();
        playerLockRoutine = StartCoroutine(UnlockPlayerControlAfterDelay());
    }

    private IEnumerator UnlockPlayerControlAfterDelay()
    {
        if (playerLockDuration > 0f)
            yield return new WaitForSeconds(playerLockDuration);

        ReleasePlayerControl();
        playerLockRoutine = null;
    }

    private IEnumerator SwapCameraTemporarily()
    {
        ApplyCameraState(mainCameraActive: false, animateCameraActive: true);

        if (cameraSwapDuration > 0f)
            yield return new WaitForSeconds(cameraSwapDuration);

        ApplyCameraState(mainCameraActive: true, animateCameraActive: false);

        cameraSwapRoutine = null;
    }

    private void ApplyCameraState(bool mainCameraActive, bool animateCameraActive)
    {
        if (mainCamera != null)
            mainCamera.SetActive(mainCameraActive);

        if (animateCamera != null)
            animateCamera.SetActive(animateCameraActive);
    }

    private void EnablePointerLight(bool isEnabled)
    {
        if (pointerLightToDisable != null)
            pointerLightToDisable.enabled = isEnabled;
    }

    private void LockPlayerControl()
    {
        if (playerMovement != null)
            playerMovement.Freeze();

        FPSLook.isStopping = true;
    }

    private void ReleasePlayerControl()
    {
        if (playerMovement != null)
            playerMovement.Unfreeze();

        FPSLook.isStopping = false;
    }
}
