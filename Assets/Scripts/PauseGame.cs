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
    public Canvas controlsCanvas;
    public Canvas creditsCanvas;

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
        if (controlsCanvas != null) controlsCanvas.enabled = false;
        if (creditsCanvas != null) creditsCanvas.enabled = false;
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
                if (IsAnySubmenuOpen())
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
        ShowCanvas(optionsMenu);
    }

    public void ControlsButton()
    {
        ShowCanvas(controlsCanvas);
    }

    public void CreditsButton()
    {
        ShowCanvas(creditsCanvas);
    }

    public void ReturnToPauseMenu()
    {
        ShowCanvas(mainMenuCanvas);
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
        ShowCanvas(showMenu ? mainMenuCanvas : null);
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

        if (!pause)
            HideSubmenus();

        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pause;

        if (pause)
        {
            CrazyGamesIntegration.GameplayStop();
            cachedTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            PauseSceneAudio();
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = Mathf.Approximately(cachedTimeScale, 0f) ? 1f : cachedTimeScale;
            ResumeSceneAudio();
            CrazyGamesIntegration.GameplayStart();
        }
    }

    public void FullFreeze(bool freeze)
    {
        noEscape = freeze;
        SetGameplayLock(freeze);
        isPaused = freeze;

        if (freeze)
        {
            CrazyGamesIntegration.GameplayStop();
            cachedTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
            PauseSceneAudio();
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = Mathf.Approximately(cachedTimeScale, 0f) ? 1f : cachedTimeScale;
            ResumeSceneAudio();
            CrazyGamesIntegration.GameplayStart();
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

    private bool IsAnySubmenuOpen()
    {
        return IsCanvasEnabled(optionsMenu)
            || IsCanvasEnabled(controlsCanvas)
            || IsCanvasEnabled(creditsCanvas);
    }

    private void ShowCanvas(Canvas canvasToShow)
    {
        SetCanvasVisible(mainMenuCanvas, false);
        SetCanvasVisible(optionsMenu, false);
        SetCanvasVisible(controlsCanvas, false);
        SetCanvasVisible(creditsCanvas, false);

        if (canvasToShow != null)
            SetCanvasVisible(canvasToShow, true);
    }

    private void HideSubmenus()
    {
        SetCanvasVisible(optionsMenu, false);
        SetCanvasVisible(controlsCanvas, false);
        SetCanvasVisible(creditsCanvas, false);
    }

    private static bool IsCanvasEnabled(Canvas canvas)
    {
        return canvas != null && canvas.enabled;
    }

    private static void SetCanvasVisible(Canvas canvas, bool isVisible)
    {
        if (canvas == null)
            return;

        if (canvas.gameObject.activeSelf != isVisible)
            canvas.gameObject.SetActive(isVisible);

        canvas.enabled = isVisible;
    }
}
