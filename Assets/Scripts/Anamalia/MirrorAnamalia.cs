using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorAnamalia : MonoBehaviour
{
    public GameObject mirrorObj;
    public float delayTime = 10f;
    private Coroutine routine;
    void OnEnable()
    {
        if (mirrorObj != null) mirrorObj.SetActive(true);

        routine = StartCoroutine(ActivateAfterDelay(delayTime));
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (mirrorObj != null) mirrorObj.SetActive(true);
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (mirrorObj != null) mirrorObj.SetActive(false);
    }
}
