using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class PickUpNeededObject : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip pickUpClip;
    public GameObject nextLevelBtn;
    private bool isOnly = false;
    private bool inReach = false;
    private TextMeshProUGUI reachUIText;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        StartCoroutine(FindReachUIText());
        if (nextLevelBtn != null) nextLevelBtn.SetActive(false);
    }

    IEnumerator FindReachUIText()
    {
        yield return new WaitForSeconds(1f);
        GameObject hud = GameObject.Find("HUD");
        if (hud != null)
        {
            Transform reachUI = hud.transform.Find("ReachUI");
            if (reachUI != null)
            {
                reachUIText = reachUI.GetComponentInChildren<TextMeshProUGUI>();
                if (reachUIText != null) reachUIText.text = "";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayerInteractor(other))
        {
            inReach = true;
            if (reachUIText != null) reachUIText.text = InteractionPromptLocalization.GetPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayerInteractor(other))
        {
            inReach = false;
            if (reachUIText != null) reachUIText.text = "";
        }
    }

    private bool IsPlayerInteractor(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
            return true;

        if (other.GetComponent<CharacterController>() != null)
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    void Update()
    {
        bool interactPressed = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Interact")
            : Input.GetKeyDown(KeyCode.E);

        if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked && !isOnly && inReach && interactPressed)
        {
            isOnly = true;
            StartCoroutine(PickUpObjectRoutine());
        }
    }

    IEnumerator PickUpObjectRoutine()
    {
        if (reachUIText != null) reachUIText.text = "";
        if (audioSource != null && pickUpClip != null) audioSource.PlayOneShot(pickUpClip);
        transform.position = new Vector3(1000f, 1000f, 1000f);
        if (nextLevelBtn != null) nextLevelBtn.SetActive(true);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
