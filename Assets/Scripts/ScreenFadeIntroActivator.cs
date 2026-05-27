using System.Collections;
using UnityEngine;

public class ScreenFadeIntroActivator : MonoBehaviour
{
    [Header("References")]
    public GameObject introObject;
    public GameObject targetObject;
    public AudioSource audioSource;
    public AudioClip introClip;
    public Movement playerMovement;

    [Header("Timing")]
    [Min(0f)] public float introHoldDuration = 5f;
    [Min(0f)] public float activationDelay = 0f;

    private bool restoredLookStop;
    private bool originalLookStopState;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (introObject != null)
            introObject.SetActive(true);

        if (targetObject != null)
            targetObject.SetActive(false);
    }

    private void Start()
    {
        if (introObject != null)
            introObject.SetActive(true);

        StartCoroutine(BeginIntroSequence());
    }

    private IEnumerator BeginIntroSequence()
    {
        // reklama: bu boshlanish qismi, shu sababli bu joyda ad qo'yilmaydi.
        yield return null;

        LockPlayerControls();

        if (introHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(introHoldDuration);

        if (introObject != null)
            introObject.SetActive(false);

        UnlockPlayerControls();

        PlayIntroSound();
        StartCoroutine(ActivateTargetAfterDelay());
    }

    private void PlayIntroSound()
    {
        if (audioSource == null || introClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = introClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private IEnumerator ActivateTargetAfterDelay()
    {
        if (targetObject == null)
            yield break;

        if (activationDelay > 0f)
            yield return new WaitForSecondsRealtime(activationDelay);

        targetObject.SetActive(true);
    }

    private void LockPlayerControls()
    {
        if (playerMovement != null)
            playerMovement.Freeze();

        originalLookStopState = FPSLook.isStopping;
        FPSLook.isStopping = true;
        restoredLookStop = true;
    }

    private void UnlockPlayerControls()
    {
        if (playerMovement != null)
            playerMovement.Unfreeze();

        if (restoredLookStop)
        {
            FPSLook.isStopping = originalLookStopState;
            restoredLookStop = false;
        }
    }

    private void OnDisable()
    {
        UnlockPlayerControls();
    }
}
