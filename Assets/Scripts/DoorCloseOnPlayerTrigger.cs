using UnityEngine;

public class DoorCloseOnPlayerTrigger : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Receivers")]
    [SerializeField] private MonoBehaviour[] combinedReceivers;

    [Header("Pointer Light")]
    public Light pointerLightToDisable;
    [Min(0f)] public float pointerLightDisableDelay = 1f;

    [Header("Options")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;
    private Coroutine pointerLightRoutine;

    private void OnEnable()
    {
        hasTriggered = false;
    }

    private void OnDisable()
    {
        if (pointerLightRoutine != null)
            StopCoroutine(pointerLightRoutine);

        pointerLightRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || (triggerOnlyOnce && hasTriggered))
            return;

        if (!TryInvokeReceivers())
            return;

        StartPointerLightDisable();
        hasTriggered = true;
    }

    private bool TryInvokeReceivers()
    {
        if (combinedReceivers == null || combinedReceivers.Length == 0)
            return false;

        bool invoked = false;

        foreach (MonoBehaviour receiverBehaviour in combinedReceivers)
        {
            if (receiverBehaviour == null)
                continue;

            if (receiverBehaviour is IDoorBatchCloseReceiver batchCloseReceiver)
            {
                batchCloseReceiver.StartCloseSequence();
                invoked = true;
                continue;
            }

            if (receiverBehaviour is IDoorTriggerCloseReceiver closeReceiver)
            {
                closeReceiver.CloseOnTrigger();
                invoked = true;
            }

            if (receiverBehaviour is ITriggerAudioReceiver triggerAudioReceiver)
            {
                triggerAudioReceiver.PlayOnTrigger();
                invoked = true;
            }
        }

        return invoked;
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrWhiteSpace(playerTag) && other.CompareTag(playerTag))
            return true;

        if (other.CompareTag("MainCamera"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private void StartPointerLightDisable()
    {
        if (pointerLightRoutine != null)
            StopCoroutine(pointerLightRoutine);

        pointerLightRoutine = StartCoroutine(DisablePointerLightAfterDelay());
    }

    private System.Collections.IEnumerator DisablePointerLightAfterDelay()
    {
        if (pointerLightDisableDelay > 0f)
            yield return new WaitForSeconds(pointerLightDisableDelay);

        if (pointerLightToDisable != null)
            pointerLightToDisable.enabled = false;

        pointerLightRoutine = null;
    }
}
