using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public enum CrazyGamesAdType
{
    Midgame,
    Rewarded
}

public sealed class CrazyGamesBridge : MonoBehaviour
{
    public const string BridgeObjectName = "CrazyGamesBridge";

    private static CrazyGamesBridge instance;
    private static bool sdkReady;
    private static bool sdkInitAttempted;
    private static bool gameplayActive;

    public static event System.Action<bool> OnSdkReadyChanged;
    public static event System.Action<CrazyGamesAdType> OnAdStarted;
    public static event System.Action<CrazyGamesAdType> OnAdFinished;
    public static event System.Action<CrazyGamesAdType, string> OnAdError;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void CG_InitSdk(string gameObjectName);

    [DllImport("__Internal")]
    private static extern void CG_RequestAd(string adType, string gameObjectName);

    [DllImport("__Internal")]
    private static extern void CG_GameplayStart();

    [DllImport("__Internal")]
    private static extern void CG_GameplayStop();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        sdkReady = false;
        sdkInitAttempted = false;
        gameplayActive = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void EarlyBootstrap()
    {
        Preload();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Preload();
    }

    public static bool IsSdkReady => sdkReady;

    public static bool IsAvailable
    {
        get
        {
#if UNITY_WEBGL
            return true;
#else
            return Application.isEditor;
#endif
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.name = BridgeObjectName;
        DontDestroyOnLoad(gameObject);
        InitializeSdk();
    }

    public static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject bridgeObject = new GameObject(BridgeObjectName);
        instance = bridgeObject.AddComponent<CrazyGamesBridge>();
    }

    public static void Preload()
    {
        EnsureInstance();
        InitializeSdk();
    }

    public static void InitializeSdk()
    {
        EnsureInstance();

        if (sdkInitAttempted)
            return;

        sdkInitAttempted = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        CG_InitSdk(BridgeObjectName);
#else
        sdkReady = true;
        OnSdkReadyChanged?.Invoke(true);
#endif
    }

    public static void RequestAd(CrazyGamesAdType adType)
    {
        EnsureInstance();

#if UNITY_WEBGL && !UNITY_EDITOR
        CG_RequestAd(ToJavascriptAdType(adType), BridgeObjectName);
#else
        instance.StartCoroutine(instance.SimulateAd(adType));
#endif
    }

    public static void GameplayStart()
    {
        EnsureInstance();

        if (gameplayActive)
            return;

        gameplayActive = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (sdkReady)
            CG_GameplayStart();
#endif
    }

    public static void GameplayStop()
    {
        EnsureInstance();

        if (!gameplayActive)
            return;

        gameplayActive = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (sdkReady)
            CG_GameplayStop();
#endif
    }

    public void HandleSdkInitialized(string readyValue)
    {
        bool isReady = string.Equals(readyValue, "true", System.StringComparison.OrdinalIgnoreCase);
        sdkReady = isReady;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (isReady && gameplayActive)
            CG_GameplayStart();
#endif

        OnSdkReadyChanged?.Invoke(isReady);
    }

    public void HandleAdStarted(string adTypeValue)
    {
        if (TryParseAdType(adTypeValue, out CrazyGamesAdType adType))
            OnAdStarted?.Invoke(adType);
    }

    public void HandleAdFinished(string adTypeValue)
    {
        if (TryParseAdType(adTypeValue, out CrazyGamesAdType adType))
            OnAdFinished?.Invoke(adType);
    }

    public void HandleAdError(string payload)
    {
        string[] parts = (payload ?? string.Empty).Split('|');
        string adTypeValue = parts.Length > 0 ? parts[0] : string.Empty;
        string errorCode = parts.Length > 1 ? parts[1] : "other";

        if (TryParseAdType(adTypeValue, out CrazyGamesAdType adType))
            OnAdError?.Invoke(adType, errorCode);
    }

    private IEnumerator SimulateAd(CrazyGamesAdType adType)
    {
        OnAdStarted?.Invoke(adType);
        yield return new WaitForSecondsRealtime(0.75f);
        OnAdFinished?.Invoke(adType);
    }

    private static string ToJavascriptAdType(CrazyGamesAdType adType)
    {
        return adType == CrazyGamesAdType.Rewarded ? "rewarded" : "midgame";
    }

    private static bool TryParseAdType(string value, out CrazyGamesAdType adType)
    {
        if (string.Equals(value, "rewarded", System.StringComparison.OrdinalIgnoreCase))
        {
            adType = CrazyGamesAdType.Rewarded;
            return true;
        }

        adType = CrazyGamesAdType.Midgame;
        return string.Equals(value, "midgame", System.StringComparison.OrdinalIgnoreCase);
    }
}
