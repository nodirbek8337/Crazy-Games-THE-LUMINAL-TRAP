using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;

public class PauseGame : MonoBehaviour
{
    public static bool isPaused = false;
    public static bool noEscape = false;
    public static bool IsGameplayLocked => gameplayLocked;

    public Canvas mainMenuCanvas;
    public Canvas contactMe;
    public Canvas optionsMenu;

    private static bool gameplayLocked = false;
    private static float cachedTimeScale = 1f;
    private static readonly HashSet<AudioSource> pausedAudioSources = new HashSet<AudioSource>();

    void Awake()
    {
        ResetPauseStatics();
    }

    IEnumerator Start()
    {
        ResetPauseStatics();

        yield return null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainMenuCanvas != null) mainMenuCanvas.enabled = false;
        if (contactMe != null) contactMe.enabled = false;
        if (optionsMenu != null) optionsMenu.enabled = false;
    }

    void Update()
    {
        if (gameplayLocked)
            return;

        bool pauseRequested = Application.isMobilePlatform
            ? CrossPlatformInputManager.GetButtonDown("Pause")
            : Input.GetKeyDown(KeyCode.Tab);

        if (pauseRequested && !noEscape)
        {
            if (isPaused)
            {
                if (optionsMenu != null && optionsMenu.enabled)
                {
                    ReturnToPauseMenu();
                }
                else
                {
                    ResumeValues();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        ResumeValues();
    }

    public void OptionsButton()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = false;
        if (optionsMenu != null) optionsMenu.enabled = true;
    }

    public void ReturnToPauseMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = true;
        if (optionsMenu != null) optionsMenu.enabled = false;
    }

    private void ResumeValues()
    {
        SetPauseState(false);
    }

    public void Pause(bool showMenu = true)
    {
        if (gameplayLocked)
            return;

        SetPauseState(true);
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = showMenu;
    }

    public void ReturnMainMenu()
    {
        ResetPauseStatics();
        SceneManager.LoadScene("MainMenu");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void SetPauseState(bool pause)
    {
        isPaused = pause;

        if (mainMenuCanvas != null) mainMenuCanvas.enabled = pause;
        if (contactMe != null) contactMe.enabled = pause;

        if (optionsMenu != null && !pause)
            optionsMenu.enabled = false;

        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pause;

        if (pause)
        {
            CrazyGamesBridge.GameplayStop();
            cachedTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            PauseSceneAudio();
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = Mathf.Approximately(cachedTimeScale, 0f) ? 1f : cachedTimeScale;
            ResumeSceneAudio();
            CrazyGamesBridge.GameplayStart();
        }
    }

    public void FullFreeze(bool freeze)
    {
        noEscape = freeze;
        SetGameplayLock(freeze);
        isPaused = freeze;

        if (freeze)
        {
            CrazyGamesBridge.GameplayStop();
            cachedTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            PauseSceneAudio();
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = Mathf.Approximately(cachedTimeScale, 0f) ? 1f : cachedTimeScale;
            ResumeSceneAudio();
            CrazyGamesBridge.GameplayStart();
        }

        Cursor.lockState = freeze ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeze;
    }

    private void ResetPauseStatics()
    {
        isPaused = false;
        noEscape = false;
        gameplayLocked = false;
        cachedTimeScale = 1f;
        FPSLook.isStopping = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        pausedAudioSources.Clear();
    }

    public static void SetGameplayLock(bool locked)
    {
        gameplayLocked = locked;
        noEscape = locked;
        FPSLook.isStopping = locked;

        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private static void PauseSceneAudio()
    {
        pausedAudioSources.Clear();

        AudioSource[] audioSources = Object.FindObjectsOfType<AudioSource>(true);
        foreach (AudioSource source in audioSources)
        {
            if (source == null || !source.isActiveAndEnabled)
                continue;

            if (source.ignoreListenerPause)
                continue;

            if (!source.isPlaying)
                continue;

            source.Pause();
            pausedAudioSources.Add(source);
        }

        AudioListener.pause = true;
    }

    private static void ResumeSceneAudio()
    {
        AudioListener.pause = false;

        foreach (AudioSource source in pausedAudioSources)
        {
            if (source != null)
                source.UnPause();
        }

        pausedAudioSources.Clear();
    }
}
