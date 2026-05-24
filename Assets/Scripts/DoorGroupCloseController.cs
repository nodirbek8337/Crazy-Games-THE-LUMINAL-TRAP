using System.Collections;
using UnityEngine;

public class DoorGroupCloseController : MonoBehaviour, IDoorBatchCloseReceiver
{
    [Header("Player Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Doors")]
    [SerializeField] private DoorAnimatorClosingResponder[] doorResponders;
    [Min(1)] public int doorsPerBatch = 2;
    [Min(0f)] public float batchDelay = 3f;

    [Header("Audio")]
    public AudioSource[] closingAudioSources;

    [Header("Pointer Lights")]
    public Light[] pointerLightsToDisable;
    [Min(0f)] public float pointerLightDisableInterval = 2f;

    private Coroutine sequenceRoutine;
    private Coroutine pointerLightRoutine;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        if (pointerLightRoutine != null)
            StopCoroutine(pointerLightRoutine);

        sequenceRoutine = null;
        pointerLightRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        StartCloseSequence();
    }

    public void StartCloseSequence()
    {
        if (sequenceRoutine != null)
            return;

        sequenceRoutine = StartCoroutine(CloseDoorsInBatches());
        StartPointerLightSequence();
    }

    private IEnumerator CloseDoorsInBatches()
    {
        if (doorResponders == null || doorResponders.Length == 0)
        {
            sequenceRoutine = null;
            yield break;
        }

        int batchSize = Mathf.Max(1, doorsPerBatch);

        for (int i = 0; i < doorResponders.Length; i += batchSize)
        {
            for (int j = 0; j < batchSize; j++)
            {
                int doorIndex = i + j;
                if (doorIndex >= doorResponders.Length)
                    break;

                DoorAnimatorClosingResponder responder = doorResponders[doorIndex];
                if (responder != null)
                {
                    responder.CloseOnTrigger();
                }

                PlayBatchAudio(j);
            }

            if (i + batchSize < doorResponders.Length && batchDelay > 0f)
                yield return new WaitForSeconds(batchDelay);
        }

        sequenceRoutine = null;
    }

    private void StartPointerLightSequence()
    {
        if (pointerLightRoutine != null)
            StopCoroutine(pointerLightRoutine);

        pointerLightRoutine = StartCoroutine(DisablePointerLightsSequentially());
    }

    private IEnumerator DisablePointerLightsSequentially()
    {
        if (pointerLightsToDisable == null || pointerLightsToDisable.Length == 0)
        {
            pointerLightRoutine = null;
            yield break;
        }

        for (int i = 0; i < pointerLightsToDisable.Length; i++)
        {
            if (i > 0 && pointerLightDisableInterval > 0f)
                yield return new WaitForSeconds(pointerLightDisableInterval);

            Light pointerLight = pointerLightsToDisable[i];
            if (pointerLight != null)
                pointerLight.enabled = false;
        }

        pointerLightRoutine = null;
    }

    private void PlayBatchAudio(int indexInBatch)
    {
        AudioSource audioSource = GetRandomClosingAudioSource();
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.Play();
    }

    private AudioSource GetRandomClosingAudioSource()
    {
        if (closingAudioSources == null || closingAudioSources.Length == 0)
            return null;

        int validCount = 0;
        for (int i = 0; i < closingAudioSources.Length; i++)
        {
            if (closingAudioSources[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int targetIndex = Random.Range(0, validCount);
        int currentIndex = 0;

        for (int i = 0; i < closingAudioSources.Length; i++)
        {
            AudioSource audioSource = closingAudioSources[i];
            if (audioSource == null)
                continue;

            if (currentIndex == targetIndex)
                return audioSource;

            currentIndex++;
        }

        return null;
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        return !string.IsNullOrWhiteSpace(playerTag) && other.CompareTag(playerTag);
    }
}
