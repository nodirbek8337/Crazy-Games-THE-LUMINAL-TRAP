using System.Collections;
using UnityEngine;

public sealed class CrazyGamesAdService : MonoBehaviour
{
    private const float DefaultSdkWaitTimeout = 5f;
    private const float SdkStatusPollInterval = 0.25f;

    private static CrazyGamesAdService instance;
    private bool sdkReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
        CrazyGamesBridge.Preload();
    }

    public static bool IsSdkReady => CrazyGamesBridge.IsSdkReady || (instance != null && instance.sdkReady);

    public static void RefreshSdkStatus()
    {
        EnsureInstance();
        instance.RefreshSdkStatusInternal();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.name = nameof(CrazyGamesAdService);
        DontDestroyOnLoad(gameObject);
        RefreshSdkStatusInternal();
    }

    public static IEnumerator WaitForSdkReady(float timeout = DefaultSdkWaitTimeout)
    {
        EnsureInstance();
        instance.RefreshSdkStatusInternal();

        if (IsSdkReady)
            yield break;

        float waitTimer = Mathf.Max(0f, timeout);
        float pollTimer = 0f;
        while (!IsSdkReady && waitTimer > 0f)
        {
            waitTimer -= Time.unscaledDeltaTime;
            pollTimer -= Time.unscaledDeltaTime;

            if (pollTimer <= 0f)
            {
                instance.RefreshSdkStatusInternal();
                pollTimer = SdkStatusPollInterval;
            }

            yield return null;
        }

        instance.RefreshSdkStatusInternal();
    }

    public static IEnumerator ShowInterstitialAndWait(bool forceResetTimer, float sdkWaitTimeout = DefaultSdkWaitTimeout)
    {
        _ = forceResetTimer;
        EnsureInstance();
        yield return WaitForSdkReady(sdkWaitTimeout);

        if (!CrazyGamesBridge.IsAvailable)
            yield break;

        yield return instance.ShowAdAndWait(CrazyGamesAdType.Midgame);
    }

    public static IEnumerator ShowRewardedAndWait(System.Action<bool> onCompleted, float sdkWaitTimeout = DefaultSdkWaitTimeout)
    {
        EnsureInstance();
        yield return WaitForSdkReady(sdkWaitTimeout);

        if (!CrazyGamesBridge.IsAvailable)
        {
            onCompleted?.Invoke(false);
            yield break;
        }

        bool rewardGranted = false;
        yield return instance.ShowAdAndWait(
            CrazyGamesAdType.Rewarded,
            () => rewardGranted = true,
            () => rewardGranted = false);

        onCompleted?.Invoke(rewardGranted);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject serviceObject = new GameObject(nameof(CrazyGamesAdService));
        instance = serviceObject.AddComponent<CrazyGamesAdService>();
    }

    private void RefreshSdkStatusInternal()
    {
        if (sdkReady)
            return;

        CrazyGamesBridge.InitializeSdk();
        sdkReady = CrazyGamesBridge.IsSdkReady;
    }

    private IEnumerator ShowAdAndWait(CrazyGamesAdType adType, System.Action onFinished = null, System.Action onFailed = null)
    {
        bool adStarted = false;
        bool adFinished = false;
        bool adFailed = false;

        System.Action<CrazyGamesAdType> startedHandler = type =>
        {
            if (type == adType)
                adStarted = true;
        };

        System.Action<CrazyGamesAdType> finishedHandler = type =>
        {
            if (type == adType)
                adFinished = true;
        };

        System.Action<CrazyGamesAdType, string> errorHandler = (type, _) =>
        {
            if (type == adType)
                adFailed = true;
        };

        try
        {
            CrazyGamesBridge.OnAdStarted += startedHandler;
            CrazyGamesBridge.OnAdFinished += finishedHandler;
            CrazyGamesBridge.OnAdError += errorHandler;

            CrazyGamesBridge.GameplayStop();
            SetAdPauseState(true);
            CrazyGamesBridge.RequestAd(adType);

            float waitForOpenTimer = adType == CrazyGamesAdType.Rewarded
                ? AdSafetySettings.RewardedOpenTimeout
                : AdSafetySettings.InterstitialOpenTimeout;

            while (!adStarted && !adFinished && !adFailed && waitForOpenTimer > 0f)
            {
                waitForOpenTimer -= Time.unscaledDeltaTime;
                yield return null;
            }

            float waitForFinishTimer = adType == CrazyGamesAdType.Rewarded
                ? AdSafetySettings.RewardedCloseTimeout
                : AdSafetySettings.InterstitialCloseTimeout;

            while (adStarted && !adFinished && !adFailed && waitForFinishTimer > 0f)
            {
                waitForFinishTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
        finally
        {
            CrazyGamesBridge.OnAdStarted -= startedHandler;
            CrazyGamesBridge.OnAdFinished -= finishedHandler;
            CrazyGamesBridge.OnAdError -= errorHandler;
            SetAdPauseState(false);

            if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked)
                CrazyGamesBridge.GameplayStart();
        }

        if (adFinished)
            onFinished?.Invoke();
        else
            onFailed?.Invoke();
    }

    private static void SetAdPauseState(bool paused)
    {
        AudioListener.pause = paused;

        if (paused)
            Time.timeScale = 0f;
        else if (!PauseGame.isPaused)
            Time.timeScale = 1f;
    }
}
