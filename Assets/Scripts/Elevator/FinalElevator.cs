using System.Collections;
using UnityEngine;

public class FinalElevator : MonoBehaviour, IGoActionReceiver
{
    [Header("Timing")]
    [Min(0f)] public float rideDuration = 4f;
    [Min(0f)] public float dingDongToOpenDelay = 2f;

    [Header("State")]
    public bool isDoorOpen;

    [Header("Animator")]
    public Animator doorAnimator;
    public string openBoolParameter = "Open";
    public string ridingBoolParameter = "Riding";

    [Header("Door Collider")]
    public GameObject elevatorDoorCollider;

    [Header("Left/Right Doors")]
    public GameObject leftDoorObject;
    public GameObject rightDoorObject;

    [Header("Close Light")]
    public Light lightToDisableOnClose;

    [Header("Go Effects")]
    public GameObject monsterObjectToActivateOnGo;
    [Min(0f)] public float goFollowUpDelay = 1f;
    public AudioSource goScreamSource;
    public AudioClip goScreamClip;

    [Header("Audio")]
    public AudioSource dingDongSound;
    public AudioSource openDoorSound;
    public AudioSource closeDoorSound;
    public AudioSource elevatorRidingSound;

    private bool isClosingOrOpening;
    private bool isGoSequenceRunning;

    private void Start()
    {
        StartCoroutine(CrazyGamesIntegration.EnsureInitialized());
        SetupClosedRidingState();
        StartCoroutine(PlayFinalElevatorSequence());
    }

    private IEnumerator PlayFinalElevatorSequence()
    {
        // reklama
        yield return StartCoroutine(CrazyGamesIntegration.ShowMidgameAdAndWait());

        if (rideDuration > 0f)
            yield return new WaitForSeconds(rideDuration);

        yield return StartCoroutine(OpenDoorWithDingDongSequence());
    }

    private void SetupClosedRidingState()
    {
        isDoorOpen = false;
        SetOpenState(false);
        SetRidingState(true);
        SetDoorObjectsActive(true);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(true);

        PlayAudio(elevatorRidingSound);
    }

    private IEnumerator OpenDoorWithDingDongSequence()
    {
        if (isClosingOrOpening)
            yield break;

        isClosingOrOpening = true;
        SetRidingState(false);
        StopAudio(elevatorRidingSound);
        PlayAudio(dingDongSound);

        if (dingDongToOpenDelay > 0f)
            yield return new WaitForSeconds(dingDongToOpenDelay);

        yield return StartCoroutine(OpenDoor());
        isClosingOrOpening = false;
    }

    private IEnumerator OpenDoor()
    {
        isDoorOpen = true;
        SetOpenState(true);
        SetDoorObjectsActive(true);
        PlayAudio(openDoorSound);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(false);

        CrazyGamesIntegration.GameplayStart();
        yield break;
    }

    public void CloseDoorFromTrigger()
    {
        if (isClosingOrOpening || !isDoorOpen)
            return;

        StartCoroutine(CloseDoorSequence());
    }

    public void Go()
    {
        if (isGoSequenceRunning)
            return;

        StartCoroutine(GoSequence());
    }

    private IEnumerator GoSequence()
    {
        isGoSequenceRunning = true;
        isClosingOrOpening = true;
        SetRidingState(false);
        StopAudio(elevatorRidingSound);

        if (lightToDisableOnClose != null)
        {
            lightToDisableOnClose.enabled = true;
            lightToDisableOnClose.color = new Color(1f, 0f, 0f, 1f);
        }

        if (monsterObjectToActivateOnGo != null)
            monsterObjectToActivateOnGo.SetActive(true);

        PlayGoScream();

        yield return StartCoroutine(OpenDoorByDeactivatingDoors());

        if (goFollowUpDelay > 0f)
            yield return new WaitForSeconds(goFollowUpDelay);

        GoFollowUp();

        isGoSequenceRunning = false;
    }

    private IEnumerator CloseDoorSequence()
    {
        CrazyGamesIntegration.GameplayStop();
        isClosingOrOpening = true;
        isDoorOpen = false;
        SetOpenState(false);
        PlayAudio(closeDoorSound);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(true);

        if (lightToDisableOnClose != null)
            lightToDisableOnClose.enabled = false;

        SetRidingState(true);
        SetDoorObjectsActive(true);
        PlayAudio(elevatorRidingSound);

        yield return null;
        isClosingOrOpening = false;
    }

    private void GoFollowUp()
    {
        // Keyingi bosqich uchun joy qoldirilgan.
        // Hozircha bu yerda qo'shimcha harakat yo'q.
    }

    private void PlayGoScream()
    {
        if (goScreamSource == null || goScreamClip == null)
            return;

        goScreamSource.Stop();
        goScreamSource.clip = goScreamClip;
        goScreamSource.loop = false;
        goScreamSource.Play();
    }

    private IEnumerator OpenDoorByDeactivatingDoors()
    {
        isDoorOpen = true;
        SetOpenState(false);
        SetDoorObjectsActive(false);
        SetRidingState(false);
        PlayAudio(openDoorSound);

        if (elevatorDoorCollider != null)
            elevatorDoorCollider.SetActive(false);

        CrazyGamesIntegration.GameplayStart();
        yield return null;
        isClosingOrOpening = false;
    }

    private void SetDoorObjectsActive(bool isActive)
    {
        if (leftDoorObject != null)
            leftDoorObject.SetActive(isActive);

        if (rightDoorObject != null)
            rightDoorObject.SetActive(isActive);
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
        if (doorAnimator == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in doorAnimator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }

    private static void PlayAudio(AudioSource sourceToPlay)
    {
        if (sourceToPlay != null)
            sourceToPlay.Play();
    }

    private static void StopAudio(AudioSource sourceToStop)
    {
        if (sourceToStop != null && sourceToStop.isPlaying)
            sourceToStop.Stop();
    }

}
