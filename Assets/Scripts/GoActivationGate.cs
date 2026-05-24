using UnityEngine;
using UnityEngine.SceneManagement;

public static class GoActivationGate
{
    private static bool isLocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        isLocked = false;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLocked = false;
    }

    public static bool TryLock()
    {
        if (isLocked)
            return false;

        isLocked = true;
        return true;
    }

    public static void Reset()
    {
        isLocked = false;
    }

    public static bool IsLocked()
    {
        return isLocked;
    }
}
