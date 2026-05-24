using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerActivateObjectAndMusic : MonoBehaviour
{
    [Header("Activation")]
    public GameObject objectToActivate;
    public bool activateOnlyOnce = true;

    [Header("Light Sequence")]
    public Light[] lightsToDisableInOrder;
    [Min(0f)] public float lightDisableInterval = 10f;
    [Min(0f)] public float initialLightDisableDelay = 10f;

    [Header("Death After Activation")]
    public Movement playerMovement;
    public MainCameraAnimationController mainCameraAnimationController;
    [Min(0f)] public float deathDelayAfterActivation = 15f;
    public bool skipDeathIfChaseSequenceExists = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip triggerClip;
    public AudioSource lightAudioSource;
    public AudioClip lightDisableClip;

    [Header("Trigger")]
    [SerializeField] private string targetTag = "Player";

    private bool hasTriggered;
    private Coroutine lightSequenceRoutine;
    private ChasePlayerAndDisablePointLights activatedChaseSequence;
    private Coroutine deathRoutine;
    private bool isCancelled;

    private void Awake()
    {
        isCancelled = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (lightAudioSource == null)
            lightAudioSource = audioSource;

        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();

        if (lightSequenceRoutine != null)
            StopCoroutine(lightSequenceRoutine);

        lightSequenceRoutine = StartCoroutine(DisableLightsInOrder());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCancelled)
            return;

        if (hasTriggered && activateOnlyOnce)
            return;

        if (other == null || !other.CompareTag(targetTag))
            return;

        TriggerActivation();
    }

    private IEnumerator DisableLightsInOrder()
    {
        if (isCancelled)
            yield break;

        if (lightsToDisableInOrder == null || lightsToDisableInOrder.Length == 0)
            yield break;

        if (initialLightDisableDelay > 0f)
            yield return new WaitForSeconds(initialLightDisableDelay);

        for (int i = 0; i < lightsToDisableInOrder.Length; i++)
        {
            if (isCancelled)
                yield break;

            Light lightToDisable = lightsToDisableInOrder[i];
            if (lightToDisable != null)
                lightToDisable.enabled = false;

            if (lightAudioSource != null && lightDisableClip != null)
                lightAudioSource.PlayOneShot(lightDisableClip);

            if (i < lightsToDisableInOrder.Length - 1 && lightDisableInterval > 0f)
                yield return new WaitForSeconds(lightDisableInterval);
        }

        if (!hasTriggered)
            TriggerActivation();

        lightSequenceRoutine = null;
    }

    private void TriggerActivation()
    {
        if (isCancelled)
            return;

        if (hasTriggered && activateOnlyOnce)
            return;

        hasTriggered = true;

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            activatedChaseSequence = objectToActivate.GetComponentInChildren<ChasePlayerAndDisablePointLights>(true);
        }

        if (audioSource != null && triggerClip != null)
            audioSource.PlayOneShot(triggerClip);

        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        deathRoutine = StartCoroutine(DeathAfterActivationRoutine());
    }

    private IEnumerator DeathAfterActivationRoutine()
    {
        if (isCancelled)
            yield break;

        if (skipDeathIfChaseSequenceExists && activatedChaseSequence != null && activatedChaseSequence.isActiveAndEnabled)
            yield break;

        if (deathDelayAfterActivation > 0f)
            yield return new WaitForSeconds(deathDelayAfterActivation);

        if (isCancelled)
            yield break;

        if (!SceneReloadDeathCoordinator.TryBegin())
            yield break;

        if (playerMovement != null)
            playerMovement.Freeze();

        if (mainCameraAnimationController != null)
            mainCameraAnimationController.SetDie(true);

        yield return new WaitForSeconds(1.5f);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void CancelPendingSequence()
    {
        isCancelled = true;

        if (lightSequenceRoutine != null)
        {
            StopCoroutine(lightSequenceRoutine);
            lightSequenceRoutine = null;
        }

        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
            deathRoutine = null;
        }

        activatedChaseSequence = null;
        SceneReloadDeathCoordinator.Reset();
    }
}
