using System;
using System.Collections;
using CrazyGames;
using UnityEngine;

public static class CrazyGamesIntegration
{
    private const float AdRequestTimeoutSeconds = 15f;

    public static bool IsAvailable => CrazySDK.IsAvailable;
    public static bool IsInitialized => CrazySDK.IsInitialized;

    public static IEnumerator EnsureInitialized()
    {
        if (!IsAvailable || IsInitialized)
            yield break;

        bool completed = false;

        try
        {
            CrazySDK.Init(() => completed = true);
        }
        catch (Exception)
        {
            yield break;
        }

        while (!completed)
            yield return null;
    }

    public static void GameplayStart()
    {
        if (!IsAvailable || !IsInitialized)
            return;

        CrazySDK.Game.GameplayStart();
    }

    public static void GameplayStop()
    {
        if (!IsAvailable || !IsInitialized)
            return;

        CrazySDK.Game.GameplayStop();
    }

    public static void HappyTime()
    {
        if (!IsAvailable || !IsInitialized)
            return;

        CrazySDK.Game.HappyTime();
    }

    public static IEnumerator ShowMidgameAdAndWait()
    {
        yield return EnsureInitialized();

        if (!IsAvailable || !IsInitialized)
            yield break;

        bool completed = false;

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Midgame,
            null,
            _ =>
            {
                completed = true;
            },
            () => completed = true
        );

        float timer = AdRequestTimeoutSeconds;
        while (!completed && timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public static IEnumerator ShowRewardedAdAndWait(Action<bool> onCompleted)
    {
        yield return EnsureInitialized();

        if (!IsAvailable || !IsInitialized)
        {
            onCompleted?.Invoke(false);
            yield break;
        }

        bool completed = false;
        bool rewardGranted = false;

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Rewarded,
            null,
            _ =>
            {
                completed = true;
            },
            () =>
            {
                rewardGranted = true;
                completed = true;
            }
        );

        float timer = AdRequestTimeoutSeconds;
        while (!completed && timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        onCompleted?.Invoke(rewardGranted);
    }
}
