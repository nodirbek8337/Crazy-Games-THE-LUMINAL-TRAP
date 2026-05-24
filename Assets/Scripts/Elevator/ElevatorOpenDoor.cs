using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorOpenDoor : MonoBehaviour
{
    public Animator doorAnimator;
    public GameObject elevatorDoorCollider;
    public AudioSource audioSource;
    public AudioClip openDoorSound;
    public Movement playerMovement;
    public GameObject canvasObj;
    public GameObject audioObj;
    public float delayTime = 40f;

    void Start()
    {
        if (elevatorDoorCollider != null) elevatorDoorCollider.SetActive(true);

        StartCoroutine(OpenDoor());
        canvasObj.SetActive(false);
    }

    private IEnumerator OpenDoor()
    {
        if (audioSource != null && openDoorSound != null) audioSource.PlayOneShot(openDoorSound);
        doorAnimator.SetBool("Open", true);
        yield return new WaitForSeconds(1f);
        if (elevatorDoorCollider != null) elevatorDoorCollider.SetActive(false);

        yield return new WaitForSeconds(delayTime);
        playerMovement.Freeze();
        canvasObj.SetActive(true);
        audioObj.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
