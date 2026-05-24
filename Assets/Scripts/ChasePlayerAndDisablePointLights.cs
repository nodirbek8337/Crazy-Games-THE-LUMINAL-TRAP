using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChasePlayerAndDisablePointLights : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform playerTarget;
    public Movement playerMovement;
    public GameObject playerCamera;
    public GameObject monsterCamera;
    public MainCameraAnimationController mainCameraAnimationController;

    [Header("Chase")]
    [SerializeField] private string playerTag = "Player";
    [Min(0f)] public float movementStartDelay = 4f;
    [Min(0.05f)] public float repathInterval = 0.15f;
    [Min(0f)] public float reachDistance = 1.25f;
    public bool stopAfterReachingPlayer = true;

    [Header("Audio")]
    public AudioSource preChaseBackgroundSource;
    public AudioSource backgroundSource;
    public AudioSource screamSource;
    public AudioClip scaryBg;
    public AudioClip scream;

    [Header("Death")]
    [Min(0f)] public float scareDelay = 1f;
    [Min(0f)] public float reloadDelayAfterDeath = 1.5f;

    [Header("Events")]
    public UnityEvent onPlayerReached;

    private float repathTimer;
    private bool playerReached;
    private bool canChase;
    private Coroutine startDelayRoutine;
    private Coroutine sequenceRoutine;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();
    }

    private void OnEnable()
    {
        playerReached = false;
        canChase = false;
        repathTimer = 0f;
        SceneReloadDeathCoordinator.Reset();

        if (agent != null)
            agent.isStopped = true;

        PlayPreChaseBackgroundMusic();
        SetMonsterView(false);

        if (startDelayRoutine != null)
            StopCoroutine(startDelayRoutine);

        startDelayRoutine = StartCoroutine(BeginChaseAfterDelay());
    }

    private void OnDisable()
    {
        if (startDelayRoutine != null)
            StopCoroutine(startDelayRoutine);

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        startDelayRoutine = null;
        sequenceRoutine = null;
        StopAllAudio();
        SetMonsterView(false);
    }

    private void Update()
    {
        if (playerReached || !canChase || agent == null || !agent.isActiveAndEnabled)
            return;

        ResolvePlayerTarget();
        if (playerTarget == null)
            return;

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathInterval)
        {
            repathTimer = 0f;
            UpdateDestination(false);
        }

        float sqrDistance = (playerTarget.position - transform.position).sqrMagnitude;
        if (sqrDistance <= reachDistance * reachDistance)
            ReachPlayer();
    }

    private IEnumerator BeginChaseAfterDelay()
    {
        if (movementStartDelay > 0f)
            yield return new WaitForSeconds(movementStartDelay);

        if (!isActiveAndEnabled)
            yield break;

        canChase = true;

        if (agent != null)
            agent.isStopped = false;

        PlayBackgroundMusic();
        ResolvePlayerTarget();
        UpdateDestination(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (other.CompareTag(playerTag))
            ReachPlayer();
    }

    private void ResolvePlayerTarget()
    {
        if (playerTarget != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            playerTarget = player.transform;
    }

    private void UpdateDestination(bool force)
    {
        if (agent == null || playerTarget == null)
            return;

        if (force || !agent.hasPath)
            agent.SetDestination(playerTarget.position);
        else
            agent.SetDestination(playerTarget.position);
    }

    private void ReachPlayer()
    {
        if (playerReached)
            return;

        playerReached = true;

        if (agent != null && stopAfterReachingPlayer)
            agent.isStopped = true;

        onPlayerReached?.Invoke();
        sequenceRoutine = StartCoroutine(HandlePlayerReachedSequence());
    }

    private IEnumerator HandlePlayerReachedSequence()
    {
        if (!SceneReloadDeathCoordinator.TryBegin())
            yield break;

        StopPreChaseBackgroundMusic();
        SetMonsterView(true);
        StopBackgroundMusic();
        PlayScream();

        if (playerMovement != null)
            playerMovement.Freeze();

        if (scareDelay > 0f)
            yield return new WaitForSeconds(scareDelay);

        if (mainCameraAnimationController != null)
            mainCameraAnimationController.SetDie(true);

        if (reloadDelayAfterDeath > 0f)
            yield return new WaitForSeconds(reloadDelayAfterDeath);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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
        StopPreChaseBackgroundMusic();

        if (backgroundSource != null && backgroundSource.isPlaying)
            backgroundSource.Stop();

        if (screamSource != null && screamSource.isPlaying)
            screamSource.Stop();
    }

    private void PlayPreChaseBackgroundMusic()
    {
        if (preChaseBackgroundSource == null)
            return;

        preChaseBackgroundSource.loop = true;

        if (!preChaseBackgroundSource.isPlaying)
            preChaseBackgroundSource.Play();
    }

    private void StopPreChaseBackgroundMusic()
    {
        if (preChaseBackgroundSource != null && preChaseBackgroundSource.isPlaying)
            preChaseBackgroundSource.Stop();
    }
}
