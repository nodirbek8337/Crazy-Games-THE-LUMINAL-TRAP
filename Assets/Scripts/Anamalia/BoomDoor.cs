using System.Collections;
using UnityEngine;

public class BoomDoor : MonoBehaviour
{
    [Header("References")]
    public ElevatorController elevatorController;

    [Header("VFX")]
    public GameObject vfxPrefab;
    public Transform vfxSpawnPoint;
    public float vfxLifetime = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip boomClip;

    [Header("Animator")]
    public Animator animator;
    public string goTriggerName = "go";
    public float deathStartDelay = 0.1f;
    public float deathHoldTime = 1f;
    public float respawnDelay = 0f;

    private GameObject spawnedVfx;
    private Coroutine cleanupRoutine;
    private Coroutine sequenceRoutine;
    private bool hasTriggered;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        hasTriggered = false;
        CleanupSpawnedEffects();

        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    private void OnDisable()
    {
        if (cleanupRoutine != null)
            StopCoroutine(cleanupRoutine);

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        cleanupRoutine = null;
        sequenceRoutine = null;
        hasTriggered = false;

        CleanupSpawnedEffects();

        if (triggerCollider != null)
            triggerCollider.enabled = true;

        if (audioSource != null)
            audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || other == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        sequenceRoutine = StartCoroutine(PlayBoomSequence());
    }

    private IEnumerator PlayBoomSequence()
    {
        SpawnVfx();

        if (animator != null)
            animator.SetTrigger(goTriggerName);

        PlayBoomSound();

        if (deathStartDelay > 0f)
            yield return new WaitForSeconds(deathStartDelay);

        if (elevatorController != null)
        {
            yield return StartCoroutine(elevatorController.RespawnDieSequence(deathHoldTime, respawnDelay));
        }
        else if (deathHoldTime > 0f || respawnDelay > 0f)
        {
            yield return new WaitForSeconds(deathHoldTime + respawnDelay);
        }

        ResetTriggerState();
        sequenceRoutine = null;
    }

    private void SpawnVfx()
    {
        if (vfxPrefab == null)
            return;

        Transform spawnTransform = vfxSpawnPoint != null ? vfxSpawnPoint : transform;
        spawnedVfx = Instantiate(vfxPrefab, spawnTransform.position, spawnTransform.rotation);

        if (cleanupRoutine != null)
            StopCoroutine(cleanupRoutine);

        cleanupRoutine = StartCoroutine(DestroyVfxAfterDelay());
    }

    private IEnumerator DestroyVfxAfterDelay()
    {
        yield return new WaitForSeconds(vfxLifetime);

        if (spawnedVfx != null)
        {
            Destroy(spawnedVfx);
            spawnedVfx = null;
        }

        cleanupRoutine = null;
    }

    private void CleanupSpawnedEffects()
    {
        if (spawnedVfx != null)
        {
            Destroy(spawnedVfx);
            spawnedVfx = null;
        }
    }

    private void ResetTriggerState()
    {
        hasTriggered = false;

        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    private void PlayBoomSound()
    {
        if (audioSource == null || boomClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = boomClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void ResolveReferences()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
}
