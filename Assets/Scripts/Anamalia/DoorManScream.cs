using UnityEngine;

public class DoorManScream : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip screamClip;

    [Header("State")]
    [SerializeField] private bool isOnly = false;

    private Collider cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        EnsureAudioSource();
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnDisable()
    {
        ResetState();
        StopScream();
    }

    private void OnDestroy()
    {
        ResetState();
        StopScream();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOnly || other == null)
            return;

        if (!IsPlayerLikeObject(other))
            return;

        isOnly = true;

        if (cachedCollider != null)
            cachedCollider.enabled = false;

        PlayScream();
    }

    private void PlayScream()
    {
        EnsureAudioSource();

        if (audioSource == null || screamClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = screamClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void StopScream()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void ResetState()
    {
        isOnly = false;

        if (cachedCollider != null)
            cachedCollider.enabled = true;
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

    private void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }
}
