using UnityEngine;

public class FinalElevatorOutsideTrigger : MonoBehaviour
{
    [Header("References")]
    public FinalElevator finalElevator;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Options")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || (triggerOnlyOnce && hasTriggered))
            return;

        if (finalElevator == null)
            finalElevator = GetComponentInParent<FinalElevator>();

        if (finalElevator == null)
            return;

        hasTriggered = true;
        finalElevator.CloseDoorFromTrigger();
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
}
