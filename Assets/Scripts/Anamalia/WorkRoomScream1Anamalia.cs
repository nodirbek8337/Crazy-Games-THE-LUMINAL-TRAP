using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkRoomScream1Anamalia : MonoBehaviour
{
    public GameObject[] activeObjects;
    public GameObject normalLamps;
    public GameObject redLamps;
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
        DeactivateObjects();
        normalLamps.SetActive(true);
        redLamps.SetActive(false);
        isOnly = false;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (ambientColorChanger != null) ambientColorChanger.SetNormal();
        DeactivateObjects();
        normalLamps.SetActive(true);
        redLamps.SetActive(false);
        isOnly = false;
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        isOnly = true;
        if (ambientColorChanger != null) ambientColorChanger.SetDark();

        yield return new WaitForSeconds(0.25f);

        yield return StartCoroutine(ActivateObjects());

        normalLamps.SetActive(false);
        redLamps.SetActive(true);
    }

    private IEnumerator ActivateObjects()
    {
        if (activeObjects == null || activeObjects.Length == 0) yield break;

        foreach (GameObject obj in activeObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
    
    void DeactivateObjects()
    {
        if (activeObjects == null || activeObjects.Length == 0) return;

        foreach (GameObject obj in activeObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
