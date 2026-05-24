using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorBoomAnamalia : MonoBehaviour
{
    public GameObject[] zombieObjects;
    public AmbientColorChanger ambientColorChanger;
    public GameObject normalPictures;
    public GameObject redPictures;
    public GameObject normalLamps;
    public GameObject redLamps;
    public GameObject atmosphereSound;
    public AudioSource screamSound;
    private Coroutine routine;
    private bool isOnly;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOnly)
        {
            isOnly = true;
            routine = StartCoroutine(ActivateZombies());
        }
    }

    void OnEnable()
    {
        DeactivateZombies();
        isOnly = false;
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        DeactivateZombies();
        isOnly = false;
    }

    IEnumerator ActivateZombies()
    {
        if (zombieObjects == null || zombieObjects.Length == 0) yield break;

        if (ambientColorChanger != null) ambientColorChanger.SetDark();
        if (atmosphereSound != null) atmosphereSound.SetActive(false);
        if (screamSound != null) screamSound.Play();

        yield return new WaitForSeconds(0.25f);

        if (normalPictures != null) normalPictures.SetActive(false);
        if (redPictures != null) redPictures.SetActive(true);
        if (normalLamps != null) normalLamps.SetActive(false);
        if (redLamps != null) redLamps.SetActive(true);

        foreach (GameObject obj in zombieObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void DeactivateZombies()
    {
        if (zombieObjects == null || zombieObjects.Length == 0) return;

        if (ambientColorChanger != null) ambientColorChanger.SetNormal();
        if (atmosphereSound != null) atmosphereSound.SetActive(true);
        if (normalPictures != null) normalPictures.SetActive(true);
        if (redPictures != null) redPictures.SetActive(false);
        if (normalLamps != null) normalLamps.SetActive(true);
        if (redLamps != null) redLamps.SetActive(false);

        foreach (GameObject obj in zombieObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
