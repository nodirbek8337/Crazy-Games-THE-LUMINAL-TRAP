using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

public static class AdSafetySettings
{
    public const float InterstitialOpenTimeout = 2f;
    public const float InterstitialCloseTimeout = 30f;
    public const float RewardedOpenTimeout = 2f;
    public const float RewardedCloseTimeout = 40f;
}

public enum AnomalyHintKey
{
    None = 0,
    ANOMALY_NONE = 1,
    ANOMALY_CORRIDOR = 2,
    ANOMALY_PICTURE = 3,
    ANOMALY_DOORS = 4,
    ANOMALY_GARBAGES = 5,
    ANOMALY_DOOR_WALL = 6,
    ANOMALY_DEMON_MONSTER = 7,
    ANOMALY_DOOR_MAN_SCREAM = 8,
    ANOMALY_STORM_DIE = 9,
    ANOMALY_LIGHT_COLOR_SWITCH = 10,
    ANOMALY_BOOM_DOOR = 11,
    ANOMALY_TRASH_BAGS = 12,
    ANOMALY_VENT = 13,
    ANOMALY_DOOR = 14,
    ANOMALY_TRASH_BAGS_2 = 15,
    ANOMALY_ATMOSPHERE = 16,
    ANOMALY_LIGHTS = 17,
    ANOMALY_VENTS = 18,
    ANOMALY_DANCE = 19,
    ANOMALY_SMOKE_DOOR = 20,
}

public static class AnomalyHintLocalizationKeys
{
    public const string TableName = "AnomalyHints";
}

[System.Serializable]
public class AnomalyEntry
{
    public GameObject anomalyObject;
    public AnomalyHintKey anomalyDescriptionKey = AnomalyHintKey.None;
    [Tooltip("If enabled, this anomaly is preferred over regular ones.")]
    public bool priorityFirst;
    [HideInInspector]
    public int cooldownCounter = 8;
}

[System.Serializable]
public class FloorNoteEntry
{
    [Min(1)]
    public int floorNumber;
    public GameObject noteObject;
}

public class ElevatorController : MonoBehaviour
{
    private const float OpenDingDongDelay = 2f;
    private const float CloseSoundDelay = 1f;
    private const float RideStartDelay = 3f;
    private const float RideDuration = 9f;
    private const float RespawnScreenHideDelay = 1f;
    private const float InterstitialSdkWaitTimeout = 5f;
    private const int DefaultInterstitialEveryNFloors = 3;

    [Header("Anomaly")]
    public AnomalyEntry[] anomalies;
    public GameObject activeAnomaly;
    [Range(0, 100)] public int anomalySpawnChance = 80;
    [Min(1)]
    public int priorityCooldown = 2;
    [Range(0, 30)] public int safeRideAnomalyBonus = 15;
    [Range(0, 30)] public int repeatedAnomalyPenalty = 20;
    [Min(1)] public int maxConsecutiveAnomalies = 3;
    [Min(1)] public int maxConsecutiveSafeRides = 2;

    [Header("Floor Notes")]
    public FloorNoteEntry[] floorNotes;

    [Header("Anomaly Hint")]
    public AnomalyHintKey noAnomalyDescriptionKey = AnomalyHintKey.ANOMALY_NONE;

    [Header("Score UI")]
    public TextMeshPro scoreText;
    public TextMeshProUGUI anomalyHintText;
    private int score = 0;
    private bool adsHintUsedThisFloor;
    private bool recoveringFromRespawn = false;
    private int rideSequenceVersion = 0;
    private Coroutine screenFadeRoutine;
    private int consecutiveAnomalyCount = 0;
    private int consecutiveSafeRideCount = 0;
    private int floorsSinceLastInterstitial = 0;

    [Header("Last Floor Number")]
    public int lastFloorNumber = 8;

    [Header("Elevator State")]
    public static bool isBlockingEvents = false;
    private bool isDoorOpen = true;
    private bool isExternallyLocked = false;

    public Animator doorAnimator;
    public GameObject elevatorDoorCollider;
    public AudioSource dingDongSound;
    public AudioSource openDoorSound;
    public AudioSource closeDoorSound;
    public AudioSource elevatorRidingSound;

    public ObjectTracking objectTracking;
    public ElevatorOutsideDeath elevatorOutsideDeath;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public GameObject player;
    public Movement playerMovement;
    public CharacterController controller;
    public MainCameraAnimationController mainCameraAnimationController;

    public float nextSceneOpenTime = 10f;
    public string FinalSceneName = "Final";
    [Min(1)]
    public int interstitialEveryNFloors = DefaultInterstitialEveryNFloors;
    
    public GameObject lightObj;
    public AmbientColorChanger ambientColorChanger;
    [Header("Animator Parameters")]
    public string openBoolParameter = "Open";
    public string ridingBoolParameter = "Riding";

    private void Start()
    {
        isBlockingEvents = true;
        score = 0;
        adsHintUsedThisFloor = false;
        UpdateScoreUI();
        ScreenCanvasFader.SetHidden();

        ResolveCoreReferences();

        DeactivateAllAnomalies();
        DeactivateAllFloorNotes();
        ResetAnomalyPatternState();
        InitializeAnomalyCooldowns();
        SetupClosedStateSilently();
        StartCoroutine(OpenDoor());
    }

    public void ElevatorButtonPressed(string buttonName)
    {
        if (isBlockingEvents || isExternallyLocked || PauseGame.isPaused || PauseGame.IsGameplayLocked || !isDoorOpen) return;

        if (buttonName == "Top" || buttonName == "Bottom")
            ClearAnomalyHintText();

        StopAllCoroutines();
        rideSequenceVersion++;

        StartCoroutine(HandleElevatorSequence(buttonName, rideSequenceVersion));
    }

    public void AllowElevatorOperate()
    {
        isBlockingEvents = false;
    }

    public bool IsPlayerInsideElevator()
    {
        return objectTracking != null && objectTracking.IsPlayerInside();
    }

    public bool IsDoorOpen()
    {
        return isDoorOpen;
    }

    public bool IsExternallyLocked()
    {
        return isExternallyLocked;
    }

    public bool CanUseButtons()
    {
        return !isBlockingEvents && !isExternallyLocked && isDoorOpen;
    }

    public bool CanUseAdsHint()
    {
        return CanUseButtons() && score > 0;
    }

    public string GetCurrentAnomalyDescription()
    {
        if (activeAnomaly == null)
            return GetLocalizedAnomalyHint(noAnomalyDescriptionKey, "No anomaly");

        if (anomalies != null)
        {
            foreach (AnomalyEntry entry in anomalies)
            {
                if (entry == null || entry.anomalyObject != activeAnomaly)
                    continue;

                if (entry.anomalyDescriptionKey != AnomalyHintKey.None)
                    return GetLocalizedAnomalyHint(entry.anomalyDescriptionKey, activeAnomaly.name);

                break;
            }
        }

        return activeAnomaly.name;
    }

    public bool TryUseAdsHintThisFloor()
    {
        if (adsHintUsedThisFloor)
            return false;

        adsHintUsedThisFloor = true;
        return true;
    }

    public bool HasUsedAdsHintThisFloor()
    {
        return adsHintUsedThisFloor;
    }

    public void ResetAdsHintThisFloor()
    {
        adsHintUsedThisFloor = false;
    }

    public void SetAnomalyHintText(string message)
    {
        if (anomalyHintText != null)
            anomalyHintText.text = message ?? string.Empty;
    }

    public void ClearAnomalyHintText()
    {
        if (anomalyHintText != null)
            anomalyHintText.text = string.Empty;
    }

    private static string GetLocalizedAnomalyHint(AnomalyHintKey key, string fallback)
    {
        if (key == AnomalyHintKey.None)
            return fallback;

        if (LocalizationSettings.StringDatabase == null)
            return fallback;

        string localized = LocalizationSettings.StringDatabase.GetLocalizedString(
            AnomalyHintLocalizationKeys.TableName,
            key.ToString());

        if (string.IsNullOrEmpty(localized))
            return fallback;

        if (localized.StartsWith("No translation found for '", System.StringComparison.Ordinal))
            return fallback;

        return localized;
    }

    public void NotAllowElevatorOperate()
    {
        isBlockingEvents = true;
    }

    public void LockButtonsExternally()
    {
        isExternallyLocked = true;
    }

    public void UnlockButtonsExternally()
    {
        isExternallyLocked = false;
    }

    private IEnumerator HandleElevatorSequence(string buttonName, int sequenceVersion)
    {
        isBlockingEvents = true;

        if (objectTracking != null && !objectTracking.IsPlayerInside())
        {
            PunishPlayer();
            yield return null;
            isBlockingEvents = false;
            yield break;
        }

        yield return StartCoroutine(CloseDoorSequence());

        if (!IsRideSequenceCurrent(sequenceVersion))
            yield break;

        yield return new WaitForSeconds(RideStartDelay);

        if (!IsRideSequenceCurrent(sequenceVersion))
            yield break;

        StartRidingState();

        yield return new WaitForSeconds(RideDuration);

        if (!IsRideSequenceCurrent(sequenceVersion))
            yield break;

        CheckPlayerChoice(buttonName);
        RefreshFloorNotes();

        bool shouldOpenDoor = PrepareNextAnomaly();
        if (shouldOpenDoor)
        {
            if (RegisterFloorTransitionAndShouldShowInterstitial())
                yield return StartCoroutine(ShowInterstitialAdAndWait());

            yield return StartCoroutine(OpenDoorWithDingDongSequence());
        }

        isBlockingEvents = false;
    }

    private void CheckPlayerChoice(string buttonName)
    {
        if (recoveringFromRespawn)
        {
            recoveringFromRespawn = false;
            activeAnomaly = null;
        }

        if (score == 0)
        {
            activeAnomaly = null;

            if (buttonName == "Top")
                AddScore(1);
            else
                UpdateScoreUI();

            return;
        }

        bool anomalyExists = (activeAnomaly != null);

        if (anomalyExists)
        {
            if (buttonName == "Bottom") AddScore(1);
            else
            {
                score = 0;
                UpdateScoreUI();
                activeAnomaly = null;
            }
        }
        else
        {
            if (buttonName == "Top") AddScore(1);
            else
            {
                score = 0;
                UpdateScoreUI();
                activeAnomaly = null;
            }
        }
    }

    private void CloseDoor()
    {
        CrazyGamesBridge.GameplayStop();
        isDoorOpen = false;
        SetRidingState(false);
        PlayAudio(closeDoorSound);
        SetOpenState(false);
        if (elevatorDoorCollider != null) elevatorDoorCollider.SetActive(true);
    }

    public void OpenDoorSecondDialogue()
    {
        StartCoroutine(OpenDoorWithDingDongSequence());
    }

    public void CloseDoorSecondDialogue()
    {
        StartCoroutine(CloseDoorSequence());
    }

    private IEnumerator OpenDoor()
    {
        isDoorOpen = true;
        ResetAdsHintThisFloor();
        SetCameraDie(false);
        SetRidingState(false);
        PlayAudio(openDoorSound);
        SetOpenState(true);
        yield return new WaitForSeconds(1f);
        if (elevatorDoorCollider != null) elevatorDoorCollider.SetActive(false);
        RefreshFloorNotes();
        isBlockingEvents = false;
        CrazyGamesBridge.GameplayStart();
    }

    private void AddScore(int amount)
    {
        if (score == -1) return;

        score += amount;
        UpdateScoreUI();
    }

    private void ResetScore()
    {
        if (score == -1) return;

        score = 0;
        recoveringFromRespawn = true;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    private bool ShouldSpawnAnomalyThisRide()
    {
        if (consecutiveAnomalyCount >= Mathf.Max(1, maxConsecutiveAnomalies))
            return false;

        if (consecutiveSafeRideCount >= Mathf.Max(1, maxConsecutiveSafeRides))
            return true;

        int adjustedChance = Mathf.Clamp(anomalySpawnChance + 10, 0, 100);
        adjustedChance += consecutiveSafeRideCount * Mathf.Max(0, safeRideAnomalyBonus);
        adjustedChance -= consecutiveAnomalyCount * Mathf.Max(0, repeatedAnomalyPenalty);
        adjustedChance = Mathf.Clamp(adjustedChance, 20, 90);

        return Random.Range(0, 100) < adjustedChance;
    }

    private void RegisterAnomalyResult(bool anomalySpawned)
    {
        if (anomalySpawned)
        {
            consecutiveAnomalyCount++;
            consecutiveSafeRideCount = 0;
            return;
        }

        consecutiveSafeRideCount++;
        consecutiveAnomalyCount = 0;
    }

    private void ResetAnomalyPatternState()
    {
        consecutiveAnomalyCount = 0;
        consecutiveSafeRideCount = 0;
    }

    private bool PrepareNextAnomaly()
    {
        if (score == 0)
        {
            activeAnomaly = null;
            DeactivateAllAnomalies();
            ResetAnomalyPatternState();
            return true;
        }

        foreach (var entry in anomalies)
        {
            if (entry.anomalyObject != null)
                entry.anomalyObject.SetActive(false);
 
            if (entry.cooldownCounter > 0)
                entry.cooldownCounter--;
        }

        if (score > lastFloorNumber)
        {
            score = 0;
            UpdateScoreUI();
            activeAnomaly = null;
            ResetAnomalyPatternState();
            DeactivateAllFloorNotes();
            isBlockingEvents = true;
            StartCoroutine(TriggerSpecialEvent());
            return false;
        }

        if (!ShouldSpawnAnomalyThisRide())
        {
            activeAnomaly = null;
            RegisterAnomalyResult(false);
            return true;
        }

        List<AnomalyEntry> candidates = new List<AnomalyEntry>();
        foreach (var entry in anomalies)
        {
            if (entry.cooldownCounter == 0 && entry.anomalyObject != null)
                candidates.Add(entry);
        }

        if (candidates.Count == 0)
        {
            activeAnomaly = null;
            RegisterAnomalyResult(false);
            return true;
        }

        List<AnomalyEntry> priorityCandidates = new List<AnomalyEntry>();
        foreach (var entry in candidates)
        {
            if (entry.priorityFirst)
                priorityCandidates.Add(entry);
        }

        if (priorityCandidates.Count > 0)
            candidates = priorityCandidates;

        int selectedIndex = Random.Range(0, candidates.Count);
        var selected = candidates[selectedIndex];

        activeAnomaly = selected.anomalyObject;
        selected.cooldownCounter = selected.priorityFirst ? priorityCooldown : 16;

        if (activeAnomaly != null)
        {
            activeAnomaly.SetActive(true);
            RegisterAnomalyResult(true);
            return true;
        }

        RegisterAnomalyResult(false);
        return true;
    }

    private void PunishPlayer()
    {
        if (elevatorOutsideDeath == null)
            elevatorOutsideDeath = GetComponent<ElevatorOutsideDeath>();

        if (elevatorOutsideDeath != null)
        {
            elevatorOutsideDeath.TriggerOutsideDeath();
            return;
        }

        StartCoroutine(RespawnDieSequence(1f, 0f));
    }

    public void RespawnAndResetAfterDelay(float delay)
    {
        StartCoroutine(RespawnAndResetAfterDelaySequence(delay));
    }

    public IEnumerator RespawnAndResetAfterDelaySequence(float delay)
    {
        InvalidateRideSequence();
        NotAllowElevatorOperate();
        StopScreenFadeRoutine();
        ClearAnomalyHintText();

        if (playerMovement != null)
            playerMovement.Freeze();

        SetCameraDie(true);
        yield return StartCoroutine(ScreenCanvasFader.FadeIn());
        yield return StartCoroutine(ShowInterstitialAdAndWait());
        yield return StartCoroutine(RespawnRoutine(delay, true));
    }

    public Coroutine RespawnDie(float deathDelay, float respawnDelay = 0f)
    {
        return StartCoroutine(RespawnDieSequence(deathDelay, respawnDelay));
    }

    public IEnumerator RespawnDieSequence(float deathDelay, float respawnDelay = 0f)
    {
        InvalidateRideSequence();
        NotAllowElevatorOperate();
        StopScreenFadeRoutine();
        ClearAnomalyHintText();

        if (playerMovement != null)
            playerMovement.Freeze();

        SetCameraDie(true);
        yield return StartCoroutine(ScreenCanvasFader.FadeIn());
        yield return StartCoroutine(ShowInterstitialAdAndWait());

        if (deathDelay > 0f)
            yield return new WaitForSeconds(deathDelay);

        yield return StartCoroutine(RespawnRoutine(respawnDelay, false));
    }

    private IEnumerator RespawnRoutine(float delay, bool freezePlayerBeforeTeleport)
    {
        isBlockingEvents = true;
        CloseDoor();
        ClearAnomalyHintText();

        if (player != null && respawnPoint != null)
        {
            if (freezePlayerBeforeTeleport && playerMovement != null)
                playerMovement.Freeze();

            if (controller != null)
            {
                controller.enabled = false;
                player.transform.position = respawnPoint.position;
                player.transform.rotation = respawnPoint.rotation;
                controller.enabled = true;
            }
        }

        yield return new WaitForSeconds(delay);

        if (playerMovement != null) playerMovement.Unfreeze();
        ResetScore();
        SetCameraDie(false);
        StartScreenFadeOutAfterDelay(RespawnScreenHideDelay);

        PlayAudio(elevatorRidingSound);

        yield return new WaitForSeconds(5.5f);

        bool shouldOpenDoor = PrepareNextAnomaly();
        if (shouldOpenDoor)
        {
            RefreshFloorNotes();

            if (RegisterFloorTransitionAndShouldShowInterstitial())
                yield return StartCoroutine(ShowInterstitialAdAndWait());

            yield return StartCoroutine(OpenDoorWithDingDongSequence());
        }

        isBlockingEvents = false;
    }

    private IEnumerator TriggerSpecialEvent()
    {
        ambientColorChanger.SetDark();
        if (lightObj != null) lightObj.SetActive(true);
        yield return new WaitForSeconds(0.25f);

        yield return new WaitForSeconds(nextSceneOpenTime);
        
        PlayAudio(elevatorRidingSound);

        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(FinalSceneName);
    }

    private void SetupClosedStateSilently()
    {
        isDoorOpen = false;
        SetRidingState(false);
        SetOpenState(false);
        if (elevatorDoorCollider != null) elevatorDoorCollider.SetActive(true);
    }

    private void DeactivateAllAnomalies()
    {
        activeAnomaly = null;

        if (anomalies == null)
            return;

        foreach (var entry in anomalies)
        {
            if (entry != null && entry.anomalyObject != null)
                entry.anomalyObject.SetActive(false);
        }
    }

    private void DeactivateAllFloorNotes()
    {
        if (floorNotes == null)
            return;

        foreach (var entry in floorNotes)
        {
            if (entry != null && entry.noteObject != null)
                entry.noteObject.SetActive(false);
        }
    }

    private void RefreshFloorNotes()
    {
        if (floorNotes == null || floorNotes.Length == 0)
            return;

        for (int i = 0; i < floorNotes.Length; i++)
        {
            FloorNoteEntry entry = floorNotes[i];
            if (entry == null || entry.noteObject == null)
                continue;

            bool shouldShow = entry.floorNumber == score;
            entry.noteObject.SetActive(shouldShow);
        }
    }

    private void InitializeAnomalyCooldowns()
    {
        if (anomalies == null)
            return;

        bool hasPriorityAnomaly = false;

        foreach (var entry in anomalies)
        {
            if (entry != null && entry.priorityFirst)
            {
                hasPriorityAnomaly = true;
                break;
            }
        }

        foreach (var entry in anomalies)
        {
            if (entry == null)
                continue;

            if (hasPriorityAnomaly)
                entry.cooldownCounter = 0;
        }
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

    private void StartRidingState()
    {
        PlayAudio(elevatorRidingSound);

        SetOpenState(true);
        SetRidingState(true);
    }

    private void SetOpenState(bool isOpen)
    {
        if (doorAnimator == null)
            return;

        if (HasAnimatorParameter(openBoolParameter))
            doorAnimator.SetBool(openBoolParameter, isOpen);
    }

    private void SetRidingState(bool isRiding)
    {
        if (doorAnimator == null)
            return;

        if (HasAnimatorParameter(ridingBoolParameter))
            doorAnimator.SetBool(ridingBoolParameter, isRiding);
    }

    private bool HasAnimatorParameter(string parameterName)
    {
        if (doorAnimator == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (var parameter in doorAnimator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }

    private void PlayAudio(AudioSource sourceToPlay)
    {
        if (sourceToPlay != null)
        {
            sourceToPlay.Play();
            return;
        }
    }

    private void SetCameraDie(bool value)
    {
        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();

        if (mainCameraAnimationController != null)
            mainCameraAnimationController.SetDie(value);
    }

    private void ResolveCoreReferences()
    {
        if (objectTracking == null)
            objectTracking = GetComponentInChildren<ObjectTracking>();

        if (elevatorOutsideDeath == null)
            elevatorOutsideDeath = GetComponent<ElevatorOutsideDeath>();

        if (mainCameraAnimationController == null)
            mainCameraAnimationController = FindObjectOfType<MainCameraAnimationController>();
    }

    private void InvalidateRideSequence()
    {
        rideSequenceVersion++;
    }

    private bool IsRideSequenceCurrent(int sequenceVersion)
    {
        return sequenceVersion == rideSequenceVersion;
    }

    private void StartScreenFadeOutAfterDelay(float delay)
    {
        StopScreenFadeRoutine();
        screenFadeRoutine = StartCoroutine(FadeOutScreenAfterDelay(delay));
    }

    private void StopScreenFadeRoutine()
    {
        if (screenFadeRoutine == null)
            return;

        StopCoroutine(screenFadeRoutine);
        screenFadeRoutine = null;
    }

    private IEnumerator FadeOutScreenAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return StartCoroutine(ScreenCanvasFader.FadeOut());
        screenFadeRoutine = null;
    }

    private IEnumerator ShowInterstitialAdAndWait()
    {
        yield return StartCoroutine(CrazyGamesAdService.ShowInterstitialAndWait(true, InterstitialSdkWaitTimeout));
    }

    private bool RegisterFloorTransitionAndShouldShowInterstitial()
    {
        if (interstitialEveryNFloors <= 0)
            return false;

        floorsSinceLastInterstitial++;

        if (floorsSinceLastInterstitial < interstitialEveryNFloors)
            return false;

        floorsSinceLastInterstitial = 0;
        return true;
    }

}
