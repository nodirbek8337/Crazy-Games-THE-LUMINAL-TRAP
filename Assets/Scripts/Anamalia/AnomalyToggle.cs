using UnityEngine;

public class AnomalyToggle : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Switch")]
    [SerializeField] private bool enableOnEnable = true;

    private void OnEnable()
    {
        SetTargetActive(enableOnEnable);
    }

    private void OnDisable()
    {
        SetTargetActive(!enableOnEnable);
    }

    private void SetTargetActive(bool activeState)
    {
        if (targetObject == null)
            return;

        if (targetObject == gameObject)
            return;

        if (targetObject.activeSelf == activeState)
            return;

        targetObject.SetActive(activeState);
    }
}
