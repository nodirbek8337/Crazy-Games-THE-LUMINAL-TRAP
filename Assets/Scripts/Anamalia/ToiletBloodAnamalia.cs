using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletBloodAnamalia : MonoBehaviour
{
    public GameObject lampA;
    public GameObject lampB;
    public float delayTime = 0.1f;
    private Coroutine routine;
    void OnEnable()
    {
        if (lampA != null) lampA.SetActive(true);
        if (lampB != null) lampB.SetActive(false);

        routine = StartCoroutine(ActivateAfterDelay(delayTime));
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (lampA != null) lampA.SetActive(true);
        if (lampB != null) lampB.SetActive(false);
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (lampA != null) lampA.SetActive(false);
        if (lampB != null) lampB.SetActive(true);
    }
}
