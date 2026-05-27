using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrologueElevator : MonoBehaviour, IGoActionReceiver
{
    private const float OpenDingDongDelay = 2f;
    private const float CloseSoundDelay = 1f;
    private const float RideStartDelay = 3f;
    private const float PrologueRideDuration = 14f;
    private const float PrologueLogicDelay = 2f;
    [Header("Elevator State")]
    public bool isBlockingEvents = true;
    public bool isDoorOpen = true;

    [Header("Animator")]
    public Animator doorAnimator;
    public string openBoolParameter = "Open";
    public string ridingBoolParameter = "Riding";

    [Header("Audio")]
    public AudioSource dingDongSound;
    public AudioSource openDoorSound;
    public AudioSource closeDoorSound;
    public AudioSource elevatorRidingSound;

    [Header("Objects")]
    public GameObject elevatorDoorCollider;
    public GameObject objectToActivateOnGo;
    public GameObject objectToDeactivateOnGo;
    public ObjectTracking objectTracking;

    [Header("Scene Loading")]
    public string nextSceneName = "AnomalyLoop";
    public string persistentScreenCanvasName = "ScreenCanvas";
    public PrologueElevatorWordSequence wordSequence;

    private AsyncOperation preloadOperation;
    private bool isPreloadStarted;
    private bool isScreenCanvasPreserved;

    private void Start()
    {
        StartCoroutine(CrazyGamesIntegration.EnsureInitialized());
        ResolveReferences();
        SetupClosedStateSilently();
        StartCoroutine(PlaySceneStartSequence());
    }

    private IEnumerator PlaySceneStartSequence()
    {
        yield return StartCoroutine(OpenDoor());
    }

    public void GoPressed()
    {
        if (isBlockingEvents || PauseGame.isPaused || PauseGame.IsGameplayLocked || !isDoorOpen)
            return;

        if (!IsPlayerInsideElevator())
            return;

        StopAllCoroutines();
        StartCoroutine(HandleGoSequence());
    }

    public void Go()
    {
        GoPressed();
    }

    public bool IsPlayerInsideElevator()
    {
        return objectTracking != null && objectTracking.IsPlayerInside();
    }

    private IEnumerator HandleGoSequence()
    {
        isBlockingEvents = true;

        if (objectToActivateOnGo != null)
            objectToActivateOnGo.SetActive(true);

        if (objectToDeactivateOnGo != null)
            objectToDeactivateOnGo.SetActive(false);

        yield return StartCoroutine(CloseDoorSequence());
        yield return new WaitForSeconds(RideStartDelay);

        StartRidingState();
        yield return new WaitForSeconds(PrologueRideDuration);
        yield return new WaitForSeconds(PrologueLogicDelay);
        yield return StartCoroutine(HandlePrologueGoLogic());
    }

    private IEnumerator HandlePrologueGoLogic()
    {
        BeginNextScenePreload();
        PreserveScreenCanvas();

        if (wordSequence != null)
            wordSequence.StartSequence();

        yield break;
    }

    public void ActivatePreloadedScene()
    {
        if (preloadOperation == null)
            return;

        preloadOperation.allowSceneActivation = true;
    }

    private void BeginNextScenePreload()
    {
        if (isPreloadStarted)
            return;

        isPreloadStarted = true;
        preloadOperation = SceneManager.LoadSceneAsync(nextSceneName);

        if (preloadOperation != null)
            preloadOperation.allowSceneActivation = false;
    }

    private void PreserveScreenCanvas()
    {
        if (isScreenCanvasPreserved)
            return;

        if (string.IsNullOrWhiteSpace(persistentScreenCanvasName))
            return;

        GameObject screenCanvas = GameObject.Find(persistentScreenCanvasName);
        if (screenCanvas == null)
            return;

        DontDestroyOnLoad(screenCanvas.transform.root.gameObject);
        isScreenCanvasPreserved = true;
    }

    private IEnumerator OpenDoor()
    {
        isDoorOpen = true;
        SetRidingState(false);
        PlayAudio(openDoorSound);
        SetOpenState(true);
        yield return new WaitForSeconds(1f);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(false);

        isBlockingEvents = false;
        CrazyGamesIntegration.GameplayStart();
    }

    public void OpenDoorSecondDialogue()
    {
        StartCoroutine(OpenDoorWithDingDongSequence());
    }

    public void CloseDoorSecondDialogue()
    {
        StartCoroutine(CloseDoorSequence());
    }

    private IEnumerator OpenDoorWithDingDongSequence()
    {
        PlayAudio(dingDongSound);
        yield return new WaitForSeconds(OpenDingDongDelay);
        yield return StartCoroutine(OpenDoor());
    }

    private IEnumerator CloseDoorSequence()
    {
        yield return new WaitForSeconds(CloseSoundDelay);
        CloseDoor();
    }

    private void CloseDoor()
    {
        CrazyGamesIntegration.GameplayStop();
        isDoorOpen = false;
        SetRidingState(false);
        PlayAudio(closeDoorSound);
        SetOpenState(false);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(true);
    }

    private void StartRidingState()
    {
        PlayAudio(elevatorRidingSound);
        SetOpenState(true);
        SetRidingState(true);
    }

    private void SetupClosedStateSilently()
    {
        isDoorOpen = false;
        SetRidingState(false);
        SetOpenState(false);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(true);
    }

    private void SetOpenState(bool isOpen)
    {
        if (doorAnimator == null || !HasAnimatorParameter(openBoolParameter))
            return;

        doorAnimator.SetBool(openBoolParameter, isOpen);
    }

    private void SetRidingState(bool isRiding)
    {
        if (doorAnimator == null || !HasAnimatorParameter(ridingBoolParameter))
            return;

        doorAnimator.SetBool(ridingBoolParameter, isRiding);
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        if (doorAnimator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in doorAnimator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }

    private void PlayAudio(AudioSource sourceToPlay)
    {
        if (sourceToPlay == null)
            return;

        sourceToPlay.Play();
    }

    private void ResolveReferences()
    {
        if (objectTracking == null)
            objectTracking = GetComponentInChildren<ObjectTracking>();
    }

}
