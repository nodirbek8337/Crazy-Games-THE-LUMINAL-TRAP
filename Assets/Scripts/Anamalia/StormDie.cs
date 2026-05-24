using System.Collections;
using UnityEngine;

public class StormDie : MonoBehaviour
{
    [Header("References")]
    public ElevatorController elevatorController;

    [Header("Lights")]
    public Light pointLightA;
    public Light pointLightB;

    [Header("Background Audio")]
    public AudioSource backgroundAudioSource;
    public AudioClip backgroundLoopClip;

    [Header("Death Audio")]
    public AudioSource deathAudioSource;
    public AudioClip deathSoundClip;

    public float respawnDelay = 3f;

    [Header("VFX")]
    public GameObject deathVfxPrefab;
    public Transform vfxSpawnPoint;

    private Coroutine routine;
    private Collider cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ResetState();
        DisablePointLights();
        PlayBackgroundLoop();
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
        ResetState();
        EnablePointLights();
        StopAllAudio();
    }

    private void OnDestroy()
    {
        EnablePointLights();
        StopAllAudio();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (routine != null || other == null || !other.CompareTag("Player"))
            return;

        routine = StartCoroutine(StormDieRoutine());
    }

    private IEnumerator StormDieRoutine()
    {
        SpawnVfx();
        PlayDeathSound();

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnDieSequence(respawnDelay, 0f));
        else if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        routine = null;
        yield break;
    }

    private void ResolveReferences()
    {
        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();

        if (backgroundAudioSource == null)
            backgroundAudioSource = GetComponent<AudioSource>();
    }

    private void ResetState()
    {
        if (cachedCollider != null)
            cachedCollider.enabled = true;
    }

    private void DisablePointLights()
    {
        if (pointLightA != null)
            pointLightA.enabled = false;

        if (pointLightB != null)
            pointLightB.enabled = false;
    }

    private void EnablePointLights()
    {
        if (pointLightA != null)
            pointLightA.enabled = true;

        if (pointLightB != null)
            pointLightB.enabled = true;
    }

    private void PlayBackgroundLoop()
    {
        if (backgroundAudioSource == null || backgroundLoopClip == null)
            return;

        backgroundAudioSource.clip = backgroundLoopClip;
        backgroundAudioSource.loop = true;

        if (!backgroundAudioSource.isPlaying)
            backgroundAudioSource.Play();
    }

    private void PlayDeathSound()
    {
        if (deathAudioSource == null || deathSoundClip == null)
            return;

        deathAudioSource.Stop();
        deathAudioSource.clip = deathSoundClip;
        deathAudioSource.loop = false;
        deathAudioSource.Play();
    }

    private void StopAllAudio()
    {
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            backgroundAudioSource.Stop();

        if (deathAudioSource != null && deathAudioSource.isPlaying)
            deathAudioSource.Stop();
    }

    private void SpawnVfx()
    {
        if (deathVfxPrefab == null)
            return;

        Transform spawnTransform = vfxSpawnPoint != null ? vfxSpawnPoint : transform;
        GameObject spawnedVfx = Instantiate(deathVfxPrefab, spawnTransform.position, spawnTransform.rotation);
        Destroy(spawnedVfx, 1.5f);
    }

}
