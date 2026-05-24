using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class ElevatorSecondDialogue : MonoBehaviour
{
    public ElevatorController elevatorController;
    public AmbientColorChanger ambientColorChanger;
    private TextMeshProUGUI reachUIText;
    public float dialogueStartTime = 1f;
    public float dialogueEndTime = 1f;
    public GameObject dialogueObject;
    public GameObject lightObj;
    public GameObject text61;
    public GameObject text62;
    public GameObject elevatorNumber1;
    public GameObject elevatorNumber2;
    public GameObject elevatorButtons;
    public GameObject elevatorTextObj;
    public GameObject gameAtmospereSound;
    private bool inReach = false;
    private bool isOny = false;

    void OnEnable()
    {
        StartCoroutine(FindReachUIText());
        if (dialogueObject != null) dialogueObject.SetActive(false);
        if (lightObj != null) lightObj.SetActive(false);
        if (text61 != null) text61.SetActive(true);
        if (text62 != null) text62.SetActive(true);
        if (elevatorNumber1 != null) elevatorNumber1.SetActive(false);
        if (elevatorNumber2 != null) elevatorNumber2.SetActive(false);
        if (elevatorButtons != null) elevatorButtons.SetActive(false);
        if (elevatorTextObj != null) elevatorTextObj.SetActive(false);
        elevatorController.NotAllowElevatorOperate();
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
                if (reachUIText != null)
                    reachUIText.text = "";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayerInteractor(other) && !isOny)
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

        if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked && inReach && !isOny && interactPressed)
        {
            isOny = true;
            StartCoroutine(GoToDialogue());
        }
    }

    IEnumerator GoToDialogue()
    {
        elevatorController.CloseDoorSecondDialogue();

        yield return new WaitForSeconds(dialogueStartTime);

        ambientColorChanger.SetDark();
        if (gameAtmospereSound != null) gameAtmospereSound.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        if (dialogueObject != null) dialogueObject.SetActive(true);
        if (lightObj != null) lightObj.SetActive(true);
        if (text61 != null) text61.SetActive(false);
        if (text62 != null) text62.SetActive(false);
        if (elevatorNumber1 != null) elevatorNumber1.SetActive(true);
        if (elevatorNumber2 != null) elevatorNumber2.SetActive(true);

        yield return new WaitForSeconds(dialogueEndTime);

        ambientColorChanger.SetNormal();
        if (lightObj != null) lightObj.SetActive(false);
        yield return new WaitForSeconds(1f);
        if (gameAtmospereSound != null) gameAtmospereSound.SetActive(true);
        elevatorController.OpenDoorSecondDialogue();
        elevatorController.AllowElevatorOperate();
        if (elevatorTextObj != null) elevatorTextObj.SetActive(true);
        if (elevatorButtons != null) elevatorButtons.SetActive(true);

        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
