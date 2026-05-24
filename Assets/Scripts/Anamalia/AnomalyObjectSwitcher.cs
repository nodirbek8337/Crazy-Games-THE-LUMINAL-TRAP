using UnityEngine;

public class AnomalyObjectSwitcher : MonoBehaviour
{
    [Header("Switch Objects")]
    public GameObject objectA;
    public GameObject objectB;

    [Header("Behavior")]
    [SerializeField] private bool swapOnEnable = true;

    private bool objectAInitialState;
    private bool objectBInitialState;
    private bool cachedInitialState;

    private void Awake()
    {
        CacheInitialState();
    }

    private void OnEnable()
    {
        CacheInitialState();

        if (!swapOnEnable)
            return;

        if (objectA != null)
            objectA.SetActive(false);

        if (objectB != null)
            objectB.SetActive(true);
    }

    private void OnDisable()
    {
        RestoreInitialState();
    }

    private void CacheInitialState()
    {
        if (cachedInitialState)
            return;

        if (objectA != null)
            objectAInitialState = objectA.activeSelf;

        if (objectB != null)
            objectBInitialState = objectB.activeSelf;

        cachedInitialState = true;
    }

    private void RestoreInitialState()
    {
        if (!cachedInitialState)
            return;

        if (objectA != null)
            objectA.SetActive(objectAInitialState);

        if (objectB != null)
            objectB.SetActive(objectBInitialState);
    }
}
