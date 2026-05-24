using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnamalia : MonoBehaviour
{
    public ModelSwitcher modelSwitcher;
    public int anomalyModelIndex = 1;
    public int normalModelIndex = 0;
    public float delayTime = 10f;
    private Coroutine routine;
    void OnEnable()
    {
        if (modelSwitcher != null) modelSwitcher.SetActiveModel(anomalyModelIndex);

        routine = StartCoroutine(ActivateAfterDelay(delayTime));
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (modelSwitcher != null) modelSwitcher.SetActiveModel(normalModelIndex);
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (modelSwitcher != null) modelSwitcher.SetActiveModel(normalModelIndex);
    }
}
