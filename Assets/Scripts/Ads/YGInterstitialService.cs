using System.Collections;
using System.Reflection;
using UnityEngine;

public sealed class YGInterstitialService : MonoBehaviour
{
    private const float DefaultSdkWaitTimeout = 5f;
    private const float SdkStatusPollInterval = 0.25f;

    private static YGInterstitialService instance;

    private bool sdkReady;
    private bool sdkSubscribed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static bool IsSdkReady
    {
        get
        {
            return GetStaticBool("YG.YG2", "isSDKEnabled") || (instance != null && instance.sdkReady);
        }
    }

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
        DontDestroyOnLoad(gameObject);
        SubscribeSdkState();
    }

    private void OnEnable()
    {
        SubscribeSdkState();
    }

    private void OnDisable()
    {
        if (instance == this)
            UnsubscribeSdkState();
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
        EnsureInstance();

        yield return WaitForSdkReady(sdkWaitTimeout);

        if (!IsSdkReady || GetStaticBool("YG.YG2", "nowAdsShow"))
            yield break;

        if (forceResetTimer)
            InvokeStaticMethod("YG.YGInsides", "ResetTimerInterAdv");

        if (!GetStaticBool("YG.YG2", "isTimerAdvCompleted"))
            yield break;

        bool adOpened = false;
        bool adFinished = false;

        System.Action openHandler = () => adOpened = true;
        System.Action<bool> closeHandler = _ => adFinished = true;
        System.Action errorHandler = () => adFinished = true;

        FieldInfo openInterAdvField = null;
        FieldInfo closeInterAdvWasShowField = null;
        FieldInfo errorInterAdvField = null;

        try
        {
            System.Type yg2Type = FindType("YG.YG2");
            if (yg2Type == null)
                yield break;

            openInterAdvField = yg2Type.GetField("onOpenInterAdv", BindingFlags.Public | BindingFlags.Static);
            closeInterAdvWasShowField = yg2Type.GetField("onCloseInterAdvWasShow", BindingFlags.Public | BindingFlags.Static);
            errorInterAdvField = yg2Type.GetField("onErrorInterAdv", BindingFlags.Public | BindingFlags.Static);

            AddDelegate(openInterAdvField, openHandler);
            AddDelegate(closeInterAdvWasShowField, closeHandler);
            AddDelegate(errorInterAdvField, errorHandler);

            if (!InvokeStaticMethod(yg2Type, "InterstitialAdvShow"))
                yield break;

            float waitForOpenTimer = AdSafetySettings.InterstitialOpenTimeout;
            while (!adOpened && !adFinished && waitForOpenTimer > 0f)
            {
                waitForOpenTimer -= Time.unscaledDeltaTime;
                yield return null;
            }

            float waitForFinishTimer = AdSafetySettings.InterstitialCloseTimeout;
            while (adOpened && !adFinished && waitForFinishTimer > 0f)
            {
                waitForFinishTimer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
        finally
        {
            RemoveDelegate(openInterAdvField, openHandler);
            RemoveDelegate(closeInterAdvWasShowField, closeHandler);
            RemoveDelegate(errorInterAdvField, errorHandler);
        }
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject serviceObject = new GameObject("YGInterstitialService");
        instance = serviceObject.AddComponent<YGInterstitialService>();
    }

    private void SubscribeSdkState()
    {
        RefreshSdkStatusInternal();

        if (sdkSubscribed)
            return;

        FieldInfo onGetSdkDataField = GetStaticField("YG.YG2", "onGetSDKData");
        AddDelegate(onGetSdkDataField, (System.Action)HandleSdkReady);
        sdkSubscribed = true;
    }

    private void UnsubscribeSdkState()
    {
        if (!sdkSubscribed)
            return;

        FieldInfo onGetSdkDataField = GetStaticField("YG.YG2", "onGetSDKData");
        RemoveDelegate(onGetSdkDataField, (System.Action)HandleSdkReady);
        sdkSubscribed = false;
    }

    private void HandleSdkReady()
    {
        sdkReady = true;
    }

    private void RefreshSdkStatusInternal()
    {
        if (sdkReady)
            return;

        sdkReady = GetStaticBool("YG.YG2", "isSDKEnabled");
    }

    private static FieldInfo GetStaticField(string fullTypeName, string fieldName)
    {
        System.Type type = FindType(fullTypeName);
        if (type == null)
            return null;

        return type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
    }

    private static bool GetStaticBool(string fullTypeName, string memberName)
    {
        System.Type type = FindType(fullTypeName);
        if (type == null)
            return false;

        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Static);
        if (property != null && property.PropertyType == typeof(bool))
            return (bool)property.GetValue(null, null);

        FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(null);

        return false;
    }

    private static bool InvokeStaticMethod(string fullTypeName, string methodName)
    {
        System.Type type = FindType(fullTypeName);
        return type != null && InvokeStaticMethod(type, methodName);
    }

    private static bool InvokeStaticMethod(System.Type type, string methodName)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null)
            return false;

        try
        {
            method.Invoke(null, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void AddDelegate(FieldInfo field, System.Delegate handler)
    {
        if (field == null || handler == null)
            return;

        System.Delegate current = field.GetValue(null) as System.Delegate;
        field.SetValue(null, System.Delegate.Combine(current, handler));
    }

    private static void RemoveDelegate(FieldInfo field, System.Delegate handler)
    {
        if (field == null || handler == null)
            return;

        System.Delegate current = field.GetValue(null) as System.Delegate;
        field.SetValue(null, System.Delegate.Remove(current, handler));
    }

    private static System.Type FindType(string fullName)
    {
        System.Type type = System.Type.GetType(fullName);
        if (type != null)
            return type;

        Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            type = assemblies[i].GetType(fullName);
            if (type != null)
                return type;
        }

        return null;
    }
}
