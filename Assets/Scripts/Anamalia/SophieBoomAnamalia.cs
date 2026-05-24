using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SophieBoomAnamalia : MonoBehaviour
{
    public GameObject[] sophieObjects;
    public NavMeshAgent agent;
    public Transform startingPosition;
    public Transform lastPosition;
    public GameObject WFX_Nuke;
    public Transform WFX_NukePosition;
    private GameObject currentWFX_Nuke;
    public Movement playerMovement;
    public ElevatorController elevatorController;

    public float delayTime = 1.5f;
    private Coroutine routine;
    private bool isOnly;
    private bool playerTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnly)
        {
            playerTriggered = true;
            routine = StartCoroutine(ActivateAfterDelay(delayTime));
        }
    }

    void OnEnable()
    {
        DeactivateSophie();
        agent.enabled = true;
        agent.isStopped = true;
        agent.Warp(startingPosition.position);
        isOnly = true;
        playerTriggered = false;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        DeactivateSophie();

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        isOnly = true;
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        isOnly = true;
        currentWFX_Nuke = Instantiate(WFX_Nuke, WFX_NukePosition.position, WFX_NukePosition.rotation);

        DeactivateSophie();
        playerMovement.EnableRagdoll();

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnAndResetAfterDelaySequence(delay));
        else if (delay > 0f)
            yield return new WaitForSeconds(delay);

        playerMovement.DisableRagdoll();
        agent.isStopped = true;

        agent.Warp(startingPosition.position);
    }

    void ActivateSophie()
    {
        if (sophieObjects == null || sophieObjects.Length == 0) return;

        foreach (GameObject obj in sophieObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    void DeactivateSophie()
    {
        if (sophieObjects == null || sophieObjects.Length == 0) return;

        foreach (GameObject obj in sophieObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    public void WakeUpSophie()
    {
        isOnly = false;
        agent.isStopped = false;
        ActivateSophie();
        agent.SetDestination(lastPosition.position);

        StartCoroutine(CheckArrival());
    }

    private IEnumerator CheckArrival()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        if (!playerTriggered)
        {
            OnDestinationReachedWithoutPlayer();
        }

        agent.isStopped = true;
    }

    private void OnDestinationReachedWithoutPlayer()
    {
        isOnly = true;
        currentWFX_Nuke = Instantiate(WFX_Nuke, WFX_NukePosition.position, WFX_NukePosition.rotation);

        DeactivateSophie();

        agent.isStopped = true;

        agent.Warp(startingPosition.position);
    }
}
