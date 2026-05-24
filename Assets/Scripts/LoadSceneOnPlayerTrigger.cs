using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadSceneOnPlayerTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Final State")]
    [SerializeField] private Movement playerMovement;
    [SerializeField] private GameObject firstObjectToEnable;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private bool applySpawnRotation = true;
    [Min(0f)] [SerializeField] private float secondObjectEnableDelay = 1f;
    [SerializeField] private GameObject secondObjectToEnable;
    [Min(0f)] [SerializeField] private float thirdObjectEnableDelay = 24f;
    [SerializeField] private GameObject thirdObjectToEnable;
    [Min(0f)] [SerializeField] private float returnToMainMenuDelay = 600f;
    [SerializeField] private string returnSceneName = "MainMenu";
    [SerializeField] private AudioSource audioSourceToStopOnThirdObject;
    [SerializeField] private GameObject monsterObjectToDisable;

    private bool hasTriggered;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<Movement>();

        if (firstObjectToEnable != null)
            firstObjectToEnable.SetActive(false);

        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(false);

        if (thirdObjectToEnable != null)
            thirdObjectToEnable.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce)
            return;

        if (other == null || !other.CompareTag(targetTag))
            return;

        hasTriggered = true;
        BeginEndingSequence();
    }

    public void BeginEndingSequence()
    {
        if (playerMovement != null)
            playerMovement.Freeze();

        PauseGame.SetGameplayLock(true);

        if (firstObjectToEnable != null)
            firstObjectToEnable.SetActive(true);

        MovePlayerToSpawnPoint();

        if (secondObjectToEnable != null)
            StartCoroutine(EnableSecondObjectRoutine());

        if (thirdObjectToEnable != null)
            StartCoroutine(EnableThirdObjectRoutine());

        if (monsterObjectToDisable != null)
            monsterObjectToDisable.SetActive(false);
    }

    private IEnumerator EnableSecondObjectRoutine()
    {
        yield return new WaitForSeconds(secondObjectEnableDelay);

        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(true);
    }

    private IEnumerator EnableThirdObjectRoutine()
    {
        yield return new WaitForSeconds(thirdObjectEnableDelay);

        if (thirdObjectToEnable != null)
            thirdObjectToEnable.SetActive(true);

        if (audioSourceToStopOnThirdObject != null)
            audioSourceToStopOnThirdObject.Stop();

        if (!string.IsNullOrEmpty(returnSceneName))
            StartCoroutine(ReturnToSceneAfterDelayRoutine());
    }

    private IEnumerator ReturnToSceneAfterDelayRoutine()
    {
        if (returnToMainMenuDelay > 0f)
            yield return new WaitForSeconds(returnToMainMenuDelay);

        SceneManager.LoadScene(returnSceneName);
    }

    private void MovePlayerToSpawnPoint()
    {
        if (playerSpawnPoint == null || playerMovement == null)
            return;

        Transform playerTransform = playerMovement.transform;
        CharacterController characterController = playerMovement.controller;
        bool restoreController = characterController != null && characterController.enabled;

        if (restoreController)
            characterController.enabled = false;

        playerTransform.position = playerSpawnPoint.position;

        if (applySpawnRotation)
            playerTransform.rotation = playerSpawnPoint.rotation;

        if (restoreController)
            characterController.enabled = true;
    }
}
