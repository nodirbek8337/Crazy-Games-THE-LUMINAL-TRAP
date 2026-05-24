using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OptionsMenuController : MonoBehaviour
{
    [System.Serializable]
    private class LanguageVisualState
    {
        public GameObject stateObject;
        public GameObject stateButton;

        public Button GetButtonComponent()
        {
            if (stateButton == null)
                return null;

            return stateButton.GetComponent<Button>();
        }
    }

    public enum LanguageOption
    {
        Uzbek = 0,
        Russian = 1,
        English = 2,
        Turkish = 3
    }

    public Slider volumeSlider;
    public Slider mouseSensitivitySlider;

    [Header("Language")]
    [SerializeField] private LanguageVisualState uzbekState = new LanguageVisualState();
    [SerializeField] private LanguageVisualState russianState = new LanguageVisualState();
    [SerializeField] private LanguageVisualState englishState = new LanguageVisualState();
    [SerializeField] private LanguageVisualState turkishState = new LanguageVisualState();

    public FPSLook fPSLook;
    private GameSettingsService settingsService;
    private Coroutine languageChangeRoutine;

    void Start()
    {
        settingsService = GameSettingsService.Get();

        // Mouse sensitivity
        mouseSensitivitySlider.minValue = 0f;
        mouseSensitivitySlider.maxValue = 1f;

        float savedSensitivity = settingsService.MouseSensitivityNormalized;
        mouseSensitivitySlider.value = savedSensitivity;
        ChangeMouseSensitivity(savedSensitivity);
        mouseSensitivitySlider.onValueChanged.AddListener(val => ChangeMouseSensitivity(val));

        // Volume
        float savedVolume = settingsService.Volume;
        volumeSlider.value = savedVolume;
        volumeSlider.onValueChanged.AddListener(val => ChangeVolume(val));

        StartCoroutine(AssignFPSControllerWhenReady());

        ApplyLatestQualitySettings();

        ConfigurePlatformSpecificVisibility();
        BindLanguageButtons();
        StartCoroutine(ApplySavedLanguageWhenReady());
    }

    IEnumerator AssignFPSControllerWhenReady()
    {
        while (fPSLook == null)
        {
            fPSLook = FindObjectOfType<FPSLook>();
            yield return null;
        }
    }

    private void ApplyLatestQualitySettings()
    {
        int latestQualityLevel = Mathf.Max(0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(latestQualityLevel, true);
        Application.targetFrameRate = -1;
    }

    public void ChangeVolume(float volume)
    {
        settingsService.SetVolume(volume);
    }

    public void ChangeMouseSensitivity(float sliderValue)
    {
        settingsService.SetMouseSensitivity(sliderValue);
    }

    private void ConfigurePlatformSpecificVisibility()
    {
        // Fullscreen controls removed; nothing to toggle here.
    }

    private IEnumerator ApplySavedLanguageWhenReady()
    {
        yield return LocalizationSettings.InitializationOperation;

        int savedLanguageIndex = settingsService != null
            ? settingsService.LanguageIndex
            : (int)LanguageOption.English;
        UpdateLanguageVisualState(savedLanguageIndex);
    }

    private void BindLanguageButtons()
    {
        BindLanguageButton(uzbekState, (int)LanguageOption.Uzbek);
        BindLanguageButton(russianState, (int)LanguageOption.Russian);
        BindLanguageButton(englishState, (int)LanguageOption.English);
        BindLanguageButton(turkishState, (int)LanguageOption.Turkish);
    }

    private void BindLanguageButton(LanguageVisualState visualState, int languageIndex)
    {
        if (visualState == null)
            return;

        Button button = visualState.GetButtonComponent();
        if (button == null)
            return;

        button.onClick.AddListener(() => ChangeLanguage(languageIndex));
    }

    public void ChangeLanguage(int languageIndex)
    {
        if (languageChangeRoutine != null)
            StopCoroutine(languageChangeRoutine);

        languageChangeRoutine = StartCoroutine(ChangeLanguageRoutine(languageIndex));
    }

    public void ChangeLanguage(LanguageOption language)
    {
        ChangeLanguage((int)language);
    }

    public void SetUzbekLanguage()
    {
        ChangeLanguage(LanguageOption.Uzbek);
    }

    public void SetRussianLanguage()
    {
        ChangeLanguage(LanguageOption.Russian);
    }

    public void SetEnglishLanguage()
    {
        ChangeLanguage(LanguageOption.English);
    }

    public void SetTurkishLanguage()
    {
        ChangeLanguage(LanguageOption.Turkish);
    }

    private IEnumerator ChangeLanguageRoutine(int languageIndex)
    {
        AsyncOperationHandle initOperation = LocalizationSettings.InitializationOperation;
        if (!initOperation.IsDone)
            yield return initOperation;

        ApplyLanguageInternal(languageIndex);
        languageChangeRoutine = null;
    }

    private void ApplyLanguageInternal(int languageIndex)
    {
        int clampedLanguageIndex = Mathf.Clamp(languageIndex, 0, 3);
        string localeCode = GetLocaleCodeByIndex(clampedLanguageIndex);
        Locale locale = FindLocaleByCode(localeCode);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        if (settingsService != null)
            settingsService.SetLanguageIndex(clampedLanguageIndex);

        UpdateLanguageVisualState(clampedLanguageIndex);
    }

    private void UpdateLanguageVisualState(int selectedLanguageIndex)
    {
        SetLanguageVisualState(uzbekState, selectedLanguageIndex == (int)LanguageOption.Uzbek);
        SetLanguageVisualState(russianState, selectedLanguageIndex == (int)LanguageOption.Russian);
        SetLanguageVisualState(englishState, selectedLanguageIndex == (int)LanguageOption.English);
        SetLanguageVisualState(turkishState, selectedLanguageIndex == (int)LanguageOption.Turkish);
    }

    private static void SetLanguageVisualState(LanguageVisualState visualState, bool isSelected)
    {
        if (visualState == null)
            return;

        if (visualState.stateObject != null)
            visualState.stateObject.SetActive(isSelected);

        if (visualState.stateButton != null)
            visualState.stateButton.SetActive(!isSelected);
    }

    private static string GetLocaleCodeByIndex(int languageIndex)
    {
        switch (languageIndex)
        {
            case (int)LanguageOption.Uzbek:
                return "uz";
            case (int)LanguageOption.Russian:
                return "ru";
            case (int)LanguageOption.Turkish:
                return "tr";
            case (int)LanguageOption.English:
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
