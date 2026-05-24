using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class GameSettingsService : MonoBehaviour
{
    public const string MouseSensitivityKey = "TheCallOfDarknessMouseSensitivity";
    public const string VolumeKey = "TheCallOfDarknessVolume";
    public const string FullscreenKey = "TheCallOfDarknessFullscreen";
    public const string ResolutionWidthKey = "TheCallOfDarknessResolutionWidth";
    public const string ResolutionHeightKey = "TheCallOfDarknessResolutionHeight";
    public const string LanguageIndexKey = "TheCallOfDarknessLanguageIndex";

    public const float DefaultMouseSensitivity = 0.5f;
    public const float DefaultVolume = 0.5f;
    public const bool DefaultFullscreen = true;
    public const int DefaultResolutionWidth = 1280;
    public const int DefaultResolutionHeight = 720;
    public const int DefaultLanguageIndex = 2;

    public static GameSettingsService Instance { get; private set; }

    public float MouseSensitivityNormalized { get; private set; }
    public float Volume { get; private set; }
    public bool IsFullscreen { get; private set; }
    public int ResolutionWidth { get; private set; }
    public int ResolutionHeight { get; private set; }
    public int LanguageIndex { get; private set; }

    public event System.Action<float> MouseSensitivityChanged;
    public event System.Action<float> VolumeChanged;
    public event System.Action<bool> FullscreenChanged;
    public event System.Action<Vector2Int> ResolutionChanged;
    public event System.Action<int> LanguageChanged;

    private bool initialized;
    private Coroutine applyLanguageRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject(nameof(GameSettingsService));
        go.hideFlags = HideFlags.HideAndDontSave;
        Instance = go.AddComponent<GameSettingsService>();
        DontDestroyOnLoad(go);
    }

    public static GameSettingsService Get()
    {
        if (Instance == null)
            Bootstrap();

        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (initialized)
            return;

        MouseSensitivityNormalized = PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultMouseSensitivity);
        Volume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        IsFullscreen = PlayerPrefs.GetInt(FullscreenKey, DefaultFullscreen ? 1 : 0) == 1;
        ResolutionWidth = PlayerPrefs.GetInt(ResolutionWidthKey, DefaultResolutionWidth);
        ResolutionHeight = PlayerPrefs.GetInt(ResolutionHeightKey, DefaultResolutionHeight);
        LanguageIndex = Mathf.Clamp(PlayerPrefs.GetInt(LanguageIndexKey, DefaultLanguageIndex), 0, 3);
        AudioListener.volume = Volume;
        ApplyScreenSettings();
        ApplySavedLanguage();

        initialized = true;
    }

    public void ApplyScreenSettings()
    {
        FullScreenMode mode = IsFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, mode);
    }

    public void SetMouseSensitivity(float normalizedValue)
    {
        InitializeIfNeeded();

        MouseSensitivityNormalized = Mathf.Clamp01(normalizedValue);
        PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivityNormalized);
        PlayerPrefs.Save();
        MouseSensitivityChanged?.Invoke(MouseSensitivityNormalized);
    }

    public void SetVolume(float volume)
    {
        InitializeIfNeeded();

        Volume = Mathf.Clamp01(volume);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(VolumeKey, Volume);
        PlayerPrefs.Save();
        VolumeChanged?.Invoke(Volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        InitializeIfNeeded();

        IsFullscreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, IsFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        ApplyScreenSettings();
        FullscreenChanged?.Invoke(IsFullscreen);
    }

    public void SetResolution(int width, int height)
    {
        InitializeIfNeeded();

        ResolutionWidth = Mathf.Max(640, width);
        ResolutionHeight = Mathf.Max(360, height);
        PlayerPrefs.SetInt(ResolutionWidthKey, ResolutionWidth);
        PlayerPrefs.SetInt(ResolutionHeightKey, ResolutionHeight);
        PlayerPrefs.Save();
        ApplyScreenSettings();
        ResolutionChanged?.Invoke(new Vector2Int(ResolutionWidth, ResolutionHeight));
    }

    public void SetLanguageIndex(int languageIndex)
    {
        InitializeIfNeeded();

        LanguageIndex = Mathf.Clamp(languageIndex, 0, 3);
        PlayerPrefs.SetInt(LanguageIndexKey, LanguageIndex);
        PlayerPrefs.Save();
        ApplySavedLanguage();
        LanguageChanged?.Invoke(LanguageIndex);
    }

    private void ApplySavedLanguage()
    {
        if (applyLanguageRoutine != null)
            StopCoroutine(applyLanguageRoutine);

        applyLanguageRoutine = StartCoroutine(ApplySavedLanguageWhenReady());
    }

    private IEnumerator ApplySavedLanguageWhenReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        Locale locale = FindLocaleByCode(GetLocaleCodeByIndex(LanguageIndex));
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        applyLanguageRoutine = null;
    }

    private static string GetLocaleCodeByIndex(int languageIndex)
    {
        switch (languageIndex)
        {
            case 0:
                return "uz";
            case 1:
                return "ru";
            case 3:
                return "tr";
            case 2:
            default:
                return "en";
        }
    }

    private static Locale FindLocaleByCode(string localeCode)
    {
        if (LocalizationSettings.AvailableLocales == null)
            return null;

        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (locale != null && locale.Identifier.Code == localeCode)
                return locale;
        }

        return null;
    }
}
