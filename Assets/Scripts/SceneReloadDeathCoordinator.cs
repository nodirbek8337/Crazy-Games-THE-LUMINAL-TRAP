using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneReloadDeathCoordinator
{
    private static bool sequenceActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        sequenceActive = false;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sequenceActive = false;
    }

    public static bool TryBegin()
    {
        if (sequenceActive)
            return false;

        sequenceActive = true;
        return true;
    }

    public static void Reset()
    {
        sequenceActive = false;
    }
}
