using System.Collections;
using UnityEngine;

public class ElevatorOutsideDeath : MonoBehaviour
{
    [Header("References")]
    public ElevatorController elevatorController;

    [Header("VFX")]
    public GameObject deathVfxPrefab;
    public Transform vfxSpawnPoint;
    public float vfxLifetime = 1.5f;

    [Header("Audio")]
    public AudioSource explosionAudioSource;
    public AudioClip explosionClip;

    [Header("Death Timing")]
    public float outsideCheckDelay = 3f;
    public float deathDelay = 1f;
    public float respawnDelay = 0f;

    private bool previousDoorOpen;
    private Coroutine routine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        previousDoorOpen = elevatorController != null && elevatorController.IsDoorOpen();
        routine = null;
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
    }

    private void Update()
    {
        if (elevatorController == null)
            return;

        bool currentDoorOpen = elevatorController.IsDoorOpen();
        bool doorJustClosed = previousDoorOpen && !currentDoorOpen;
        previousDoorOpen = currentDoorOpen;

        if (!doorJustClosed || routine != null)
            return;

        if (elevatorController.IsPlayerInsideElevator())
            return;

        TriggerOutsideDeath();
    }

    public void TriggerOutsideDeath()
    {
        if (routine != null)
            return;

        routine = StartCoroutine(HandleOutsideDeath());
    }

    private IEnumerator HandleOutsideDeath()
    {
        if (outsideCheckDelay > 0f)
            yield return new WaitForSeconds(outsideCheckDelay);

        if (elevatorController == null || elevatorController.IsPlayerInsideElevator())
        {
            routine = null;
            yield break;
        }

        SpawnVfx();

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnDieSequence(deathDelay, respawnDelay));
        else if (deathDelay > 0f || respawnDelay > 0f)
            yield return new WaitForSeconds(deathDelay + respawnDelay);

        routine = null;
    }

    private void SpawnVfx()
    {
        if (deathVfxPrefab == null)
        {
            PlayExplosionSound();
            return;
        }

        Transform spawnPoint = vfxSpawnPoint != null ? vfxSpawnPoint : transform;
        GameObject spawnedVfx = Instantiate(deathVfxPrefab, spawnPoint.position, spawnPoint.rotation);
        PlayExplosionSound();

        if (vfxLifetime > 0f)
            Destroy(spawnedVfx, vfxLifetime);
    }

    private void PlayExplosionSound()
    {
        if (explosionAudioSource == null || explosionClip == null)
            return;

        explosionAudioSource.Stop();
        explosionAudioSource.clip = explosionClip;
        explosionAudioSource.loop = false;
        explosionAudioSource.Play();
    }

    private void ResolveReferences()
    {
        if (elevatorController == null)
            elevatorController = GetComponent<ElevatorController>();

        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();
    }
}
