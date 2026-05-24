using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorBoomAnamaliaHelperZombies : MonoBehaviour
{
    public GameObject WFX_Nuke;
    public Transform WFX_NukePosition;
    private GameObject currentWFX_Nuke;
    public Movement playerMovement;
    public ElevatorController elevatorController;
    public CorridorBoomAnamalia corridorBoomAnamalia;

    public float delayTime = 1.5f;
    private Coroutine routine;
    public static bool isOnlyCorridorBoom;

    void OnEnable()
    {
        isOnlyCorridorBoom = false;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        isOnlyCorridorBoom = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnlyCorridorBoom)
        {
            routine = StartCoroutine(ActivateAfterDelay(delayTime));
        }
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        isOnlyCorridorBoom = true;
        currentWFX_Nuke = Instantiate(WFX_Nuke, WFX_NukePosition.position, WFX_NukePosition.rotation);
        playerMovement.EnableRagdoll();

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnAndResetAfterDelaySequence(delay));
        else if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if(corridorBoomAnamalia != null) corridorBoomAnamalia.DeactivateZombies();

        playerMovement.DisableRagdoll();
    }
}
