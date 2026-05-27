using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuLogic : MonoBehaviour
{
    private const float LoadingDotsInterval = 0.35f;

    public Canvas loadingCanvas;
    public Canvas mainMenuCanvas;
    public Canvas optionsMenuCanvas;
    public Canvas controlsCanvas;
    public Canvas creditsCanvas;

    [Header("Loading UI")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private RectTransform loadingSpinner;
    [SerializeField] private float spinnerRotationSpeed = -180f;
    [SerializeField] private Vector2 spinnerSize = new Vector2(72f, 72f);
    [SerializeField] private Vector2 spinnerPosition = new Vector2(0f, 92f);
    [SerializeField] private Color spinnerColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);

    [Header("Scene Settings")]
    public string sceneName = "Prologue";

    private bool isLoadingScene;
    private bool hasCompletedMainMenuInitialization;
    private Coroutine loadingVisualRoutine;
    private string loadingBaseText = "Loading";

    private enum LoadingState
    {
        General,
        Ad
    }

    void Start()
    {
        StartCoroutine(CrazyGamesIntegration.EnsureInitialized());
        CrazyGamesIntegration.GameplayStop();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnsureLoadingReferences();
        CompleteMainMenuInitialization();
    }

    public void StartGameButton()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;
        EnsureLoadingReferences();
        ShowCanvas(loadingCanvas);
        BeginLoadingVisuals(LoadingState.General);
        StartCoroutine(LoadSceneWithLightingFix(sceneName));
    }

    public void OptionsButton()
    {
        ShowCanvas(optionsMenuCanvas);
    }

    public void ControlsButton()
    {
        ShowCanvas(controlsCanvas);
    }

    public void CreditsButton()
    {
        ShowCanvas(creditsCanvas);
    }

    public void ReturnToMainMenuButton()
    {
        ShowCanvas(mainMenuCanvas);
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }

    private void ShowCanvas(Canvas canvasToShow)
    {
        SetCanvasVisible(loadingCanvas, false);
        SetCanvasVisible(mainMenuCanvas, false);
        SetCanvasVisible(optionsMenuCanvas, false);
        SetCanvasVisible(controlsCanvas, false);
        SetCanvasVisible(creditsCanvas, false);

        SetCanvasVisible(canvasToShow, true);
    }

    IEnumerator LoadSceneWithLightingFix(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;
    }

    private void CompleteMainMenuInitialization()
    {
        if (hasCompletedMainMenuInitialization)
            return;

        hasCompletedMainMenuInitialization = true;
        EndLoadingVisuals();
        ShowCanvas(mainMenuCanvas);
    }

    private static void SetCanvasVisible(Canvas canvas, bool isVisible)
    {
        if (canvas == null)
            return;

        if (canvas.gameObject.activeSelf != isVisible)
            canvas.gameObject.SetActive(isVisible);

        canvas.enabled = isVisible;
    }

    private void EnsureLoadingReferences()
    {
        if (loadingCanvas == null)
            return;

        if (loadingText == null)
            loadingText = loadingCanvas.GetComponentInChildren<TMP_Text>(true);

        if (loadingSpinner == null)
            loadingSpinner = FindOrCreateLoadingSpinner();
    }

    private RectTransform FindOrCreateLoadingSpinner()
    {
        if (loadingCanvas == null)
            return null;

        Transform existingSpinner = loadingCanvas.transform.Find("LoadingSpinner");
        if (existingSpinner != null)
            return existingSpinner as RectTransform;

        GameObject spinnerObject = new GameObject("LoadingSpinner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        spinnerObject.layer = loadingCanvas.gameObject.layer;

        RectTransform spinnerRect = spinnerObject.GetComponent<RectTransform>();
        spinnerRect.SetParent(loadingCanvas.transform, false);
        spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.pivot = new Vector2(0.5f, 0.5f);
        spinnerRect.anchoredPosition = spinnerPosition;
        spinnerRect.sizeDelta = spinnerSize;
        spinnerRect.localScale = Vector3.one;
        spinnerRect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image spinnerImage = spinnerObject.GetComponent<Image>();
        spinnerImage.color = new Color(spinnerColor.r, spinnerColor.g, spinnerColor.b, 0.18f);
        spinnerImage.preserveAspect = true;
        spinnerImage.raycastTarget = false;

        Outline spinnerOutline = spinnerObject.GetComponent<Outline>();
        spinnerOutline.effectColor = spinnerColor;
        spinnerOutline.effectDistance = new Vector2(6f, 6f);
        spinnerOutline.useGraphicAlpha = true;

        CreateSpinnerAccent(spinnerRect);

        return spinnerRect;
    }

    private void CreateSpinnerAccent(RectTransform spinnerRect)
    {
        if (spinnerRect == null)
            return;

        GameObject accentObject = new GameObject("SpinnerAccent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObject.layer = spinnerRect.gameObject.layer;

        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.SetParent(spinnerRect, false);
        accentRect.anchorMin = new Vector2(1f, 0f);
        accentRect.anchorMax = new Vector2(1f, 0f);
        accentRect.pivot = new Vector2(0.5f, 0.5f);
        accentRect.anchoredPosition = new Vector2(10f, -10f);
        accentRect.sizeDelta = spinnerSize * 0.28f;
        accentRect.localScale = Vector3.one;

        Image accentImage = accentObject.GetComponent<Image>();
        accentImage.color = spinnerColor;
        accentImage.raycastTarget = false;
    }

    private void BeginLoadingVisuals(LoadingState loadingState)
    {
        EnsureLoadingReferences();
        SetLoadingBaseText(loadingState);

        if (loadingVisualRoutine != null)
            StopCoroutine(loadingVisualRoutine);

        loadingVisualRoutine = StartCoroutine(AnimateLoadingVisuals());
    }

    private void EndLoadingVisuals()
    {
        if (loadingVisualRoutine != null)
        {
            StopCoroutine(loadingVisualRoutine);
            loadingVisualRoutine = null;
        }

        if (loadingSpinner != null)
            loadingSpinner.localRotation = Quaternion.identity;

        if (loadingText != null)
            loadingText.text = loadingBaseText;
    }

    private void SetLoadingBaseText(LoadingState loadingState)
    {
        switch (loadingState)
        {
            default:
                loadingBaseText = ExtractBaseLoadingLabel(loadingText != null ? loadingText.text : null, "Loading");
                break;
        }

        if (loadingText != null)
            loadingText.text = loadingBaseText;
    }

    private IEnumerator AnimateLoadingVisuals()
    {
        float dotsTimer = 0f;
        int dotsCount = 0;

        while (true)
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.localRotation *= Quaternion.Euler(0f, 0f, spinnerRotationSpeed * Time.unscaledDeltaTime);
            }

            dotsTimer += Time.unscaledDeltaTime;
            if (dotsTimer >= LoadingDotsInterval)
            {
                dotsTimer = 0f;
                dotsCount = (dotsCount + 1) % 4;

                if (loadingText != null)
                    loadingText.text = loadingBaseText + new string('.', dotsCount);
            }

            yield return null;
        }
    }

    private static string ExtractBaseLoadingLabel(string text, string fallback)
    {
        string value = string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
        return value.TrimEnd('.', ' ');
    }

}
