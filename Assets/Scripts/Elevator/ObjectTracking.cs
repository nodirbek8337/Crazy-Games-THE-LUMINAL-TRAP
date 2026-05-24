using UnityEngine;

public class ObjectTracking : MonoBehaviour
{
    private bool isPlayerInside;
    private bool isGhostInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (IsPlayerLikeObject(other))
            isPlayerInside = true;

        if (other.CompareTag("Ghost"))
            isGhostInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null)
            return;

        if (IsPlayerLikeObject(other))
            isPlayerInside = false;

        if (other.CompareTag("Ghost"))
            isGhostInside = false;
    }

    private bool IsPlayerLikeObject(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    public bool IsPlayerInside() => isPlayerInside;
    public bool IsGhostInside() => isGhostInside;
}
