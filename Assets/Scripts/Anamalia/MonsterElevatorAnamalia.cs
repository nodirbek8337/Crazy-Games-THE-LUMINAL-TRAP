using System.Collections;
using UnityEngine;

public class MonsterElevatorAnamalia : MonoBehaviour
{
    [Header("References")]
    public ElevatorController elevatorController;
    public Movement playerMovement;
    public GameObject playerCamera;
    public GameObject monsterCamera;

    [Header("Audio")]
    public AudioSource backgroundSource;
    public AudioSource screamSource;
    public AudioClip scaryBg;
    public AudioClip scream;

    [Header("Timing")]
    public float scareDelay = 1f;
    public float respawnDelay = 3f;

    private bool isTriggered;
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
        isTriggered = false;
        if (cachedCollider != null)
            cachedCollider.enabled = true;

        SetMonsterView(false);
        PlayBackgroundMusic();
    }

    private void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
        isTriggered = false;

        StopAllAudio();
        SetMonsterView(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || other == null)
            return;

        if (!IsPlayerLikeObject(other))
            return;

        isTriggered = true;

        if (cachedCollider != null)
            cachedCollider.enabled = false;

        routine = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        SetMonsterView(true);
        StopBackgroundMusic();
        PlayScream();

        yield return new WaitForSeconds(scareDelay);
        SetMonsterView(false);

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnDieSequence(respawnDelay, 0f));
        else if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        PlayBackgroundMusic();

        ResetAnomalyState();
    }

    private void ResolveReferences()
    {
    }

    private void SetMonsterView(bool active)
    {
        if (playerCamera != null)
            playerCamera.SetActive(!active);

        if (monsterCamera != null)
            monsterCamera.SetActive(active);
    }

    private void PlayBackgroundMusic()
    {
        if (backgroundSource == null || scaryBg == null)
            return;

        if (backgroundSource.clip != scaryBg)
            backgroundSource.clip = scaryBg;

        backgroundSource.loop = true;

        if (!backgroundSource.isPlaying)
            backgroundSource.Play();
    }

    private void StopBackgroundMusic()
    {
        if (backgroundSource != null && backgroundSource.isPlaying)
            backgroundSource.Stop();
    }

    private void PlayScream()
    {
        if (screamSource == null || scream == null)
            return;

        screamSource.Stop();
        screamSource.clip = scream;
        screamSource.loop = false;
        screamSource.Play();
    }

    private void StopAllAudio()
    {
        if (backgroundSource != null && backgroundSource.isPlaying)
            backgroundSource.Stop();

        if (screamSource != null && screamSource.isPlaying)
            screamSource.Stop();
    }

    private bool IsPlayerLikeObject(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private void ResetAnomalyState()
    {
        if (cachedCollider != null)
            cachedCollider.enabled = true;

        SetMonsterView(false);
        isTriggered = false;
        routine = null;
    }
}
