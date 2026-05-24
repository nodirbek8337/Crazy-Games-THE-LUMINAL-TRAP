using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    private Transform player;

    private void OnEnable()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player != null)
        {
            transform.LookAt(player);
        }
    }
}
