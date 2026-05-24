using System.Collections;
using UnityEngine;

public class CameraAnimationTriggerZone : MonoBehaviour
{
    [Header("References")]
    public Movement playerMovement;
    public MainCameraAnimationController mainCameraAnimationController;
    public GameObject objectToActivate;
    public AudioSource audioSource;
    public AudioClip triggerAudioClip;

    [Header("Animator")]
    public string cameraTriggerName;

    [Header("Timing")]
    public float activateObjectDelay = 4f;
    public float postActivationWait = 8f;

    private bool hasTriggered;
    private Coroutine sequenceRoutine;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (triggerCollider != null)
            triggerCollider.enabled = true;

        hasTriggered = false;
        sequenceRoutine = null;
    }

    private void OnDisable()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || other == null || !other.CompareTag("Player"))
            return;

        hasTriggered = true;

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (playerMovement != null)
            playerMovement.Freeze();

        PlayTriggerAudio();

        if (mainCameraAnimationController != null && !string.IsNullOrWhiteSpace(cameraTriggerName))
        {
            mainCameraAnimationController.ResetTrigger(cameraTriggerName);
            mainCameraAnimationController.SetTrigger(cameraTriggerName);
        }

        if (activateObjectDelay > 0f)
            yield return new WaitForSeconds(activateObjectDelay);

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (postActivationWait > 0f)
            yield return new WaitForSeconds(postActivationWait);

        if (playerMovement != null)
            playerMovement.Unfreeze();

        sequenceRoutine = null;
    }

    private void ResolveReferences()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();
    }

    private void PlayTriggerAudio()
    {
        if (audioSource == null || triggerAudioClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = triggerAudioClip;
        audioSource.loop = false;
        audioSource.Play();
    }
}
