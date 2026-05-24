using UnityEngine;

public sealed class CrazyGamesRuntimeReceiver : MonoBehaviour
{
    public const string ReceiverObjectName = "CrazyGamesRuntimeReceiver";

    private static CrazyGamesRuntimeReceiver instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.name = ReceiverObjectName;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject receiverObject = new GameObject(ReceiverObjectName);
        instance = receiverObject.AddComponent<CrazyGamesRuntimeReceiver>();
    }

    public void SetFocusWindowGame(string visible)
    {
        bool focused = string.Equals(visible, "true", System.StringComparison.OrdinalIgnoreCase);
        if (focused)
        {
            if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked)
                CrazyGamesBridge.GameplayStart();
        }
        else
        {
            CrazyGamesBridge.GameplayStop();
        }
    }

    public void SetPauseGame(string pause)
    {
        bool paused = string.Equals(pause, "true", System.StringComparison.OrdinalIgnoreCase);
        AudioListener.pause = paused;

        if (paused)
        {
            CrazyGamesBridge.GameplayStop();
            return;
        }

        if (!PauseGame.isPaused && !PauseGame.IsGameplayLocked)
            CrazyGamesBridge.GameplayStart();
    }
}
