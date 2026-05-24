using UnityEngine;

public class FlashLightTransform : MonoBehaviour
{
    public Transform cameraTransform;
    public float offsetDistance = 0.2f;

    void Update()
    {
        if (cameraTransform == null) return;

        transform.position = cameraTransform.position + cameraTransform.forward * offsetDistance;

        transform.rotation = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);
    }
}
