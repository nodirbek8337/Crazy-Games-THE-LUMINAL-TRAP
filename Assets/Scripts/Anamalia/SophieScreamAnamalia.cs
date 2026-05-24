using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SophieScreamAnamalia : MonoBehaviour
{
    public GameObject[] sophie;
    public GameObject zombieObject;
    public GameObject playerCamera;
    public ElevatorController elevatorController;
    public AmbientColorChanger ambientColorChanger;
    public float delayTime = 1.5f;
    private Coroutine routine;
    private bool isOnly;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnly)
        {
            routine = StartCoroutine(ActivateAfterDelay(delayTime));
        }
    }

    void OnEnable()
    {
        ActivateSophie();
        if (playerCamera != null) playerCamera.SetActive(true);
        isOnly = false;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        DeactivateSophie();
        if (zombieObject != null) zombieObject.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);
        isOnly = false;
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        isOnly = true;
        ambientColorChanger.SetDark();

        yield return new WaitForSeconds(0.25f);

        DeactivateSophie();
        if (zombieObject != null) zombieObject.SetActive(true);
        if (playerCamera != null) playerCamera.SetActive(false);

        if (elevatorController != null)
            yield return StartCoroutine(elevatorController.RespawnAndResetAfterDelaySequence(delay));
        else if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (zombieObject != null) zombieObject.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);
        ActivateSophie();
        ambientColorChanger.SetNormal();
        isOnly = false;
    }

    void ActivateSophie()
    {
        if (sophie == null || sophie.Length == 0) return;

        foreach (GameObject obj in sophie)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
    
    void DeactivateSophie()
    {
        if (sophie == null || sophie.Length == 0) return;

        foreach (GameObject obj in sophie)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
