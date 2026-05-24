// CameraShake dastur kodi camerani tebranib turishi uchun
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float swayAmount = 0.03f; 
    public float swaySpeed = 0.05f;   

    private Vector3 originalPosition; 
    private Vector3 monsterOriginalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;          
    }

    void Update()
    {
        float swayOffsetX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float swayOffsetY = Mathf.Cos(Time.time * swaySpeed * 0.5f) * swayAmount; 

        transform.localPosition = new Vector3(originalPosition.x + swayOffsetX, originalPosition.y + swayOffsetY, originalPosition.z);
    }
}
