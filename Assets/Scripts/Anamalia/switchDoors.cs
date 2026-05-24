using UnityEngine;

public class switchDoors : MonoBehaviour
{
    [Header("Door References")]
    public GameObject door;
    public GameObject boomDoor;

    private bool originalDoorState = true;
    private bool originalBoomDoorState = false;

    private void Awake()
    {
        CacheOriginalStates();
    }

    private void OnEnable()
    {
        CacheOriginalStates();
        ApplySwappedStates();
    }

    private void OnDisable()
    {
        RestoreOriginalStates();
    }

    private void CacheOriginalStates()
    {
        if (door != null)
            originalDoorState = door.activeSelf;

        if (boomDoor != null)
            originalBoomDoorState = boomDoor.activeSelf;
    }

    private void ApplySwappedStates()
    {
        if (door != null)
            door.SetActive(false);

        if (boomDoor != null)
            boomDoor.SetActive(true);
    }

    private void RestoreOriginalStates()
    {
        if (door != null)
            door.SetActive(originalDoorState);

        if (boomDoor != null)
            boomDoor.SetActive(originalBoomDoorState);
    }
}
