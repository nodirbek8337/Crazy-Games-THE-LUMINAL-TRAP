using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class PostProcessing : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool postProcessingEnabled = true;
    [SerializeField] private bool useCameraBackgroundAsTint = false;

    [Header("Camera Layers")]
    [SerializeField] private bool overrideCameraCullingMask = false;
    [SerializeField] private LayerMask cameraCullingMask = ~0;

    [Header("Blur")]
    [SerializeField] private bool blurEnabled = true;
    [SerializeField, Range(0f, 2f)] private float blurStrength = 0.18f;
    [SerializeField, Range(1, 4)] private int blurDownsample = 2;
    [SerializeField, Range(1, 3)] private int blurIterations = 1;
    [SerializeField] private Shader blurShader;

    [Header("Tint")]
    [SerializeField] private Color tintColor = new Color(0.95f, 0.92f, 0.88f, 1f);
    [SerializeField, Range(0f, 1f)] private float tintStrength = 0.08f;

    [Header("Vignette")]
    [SerializeField] private bool vignetteEnabled = true;
    [SerializeField, Range(0f, 1f)] private float vignetteStrength = 0.22f;
    [SerializeField, Range(0f, 1f)] private float vignetteSoftness = 0.7f;
    [SerializeField] private Color vignetteColor = Color.black;

    [Header("Scanlines")]
    [SerializeField] private bool scanlinesEnabled = false;
    [SerializeField, Range(0f, 1f)] private float scanlineOpacity = 0.08f;
    [SerializeField, Range(1f, 8f)] private float scanlineThickness = 2f;

    [Header("Grain")]
    [SerializeField] private bool grainEnabled = false;
    [SerializeField, Range(0f, 1f)] private float grainOpacity = 0.03f;
    [SerializeField, Range(8, 256)] private int grainTextureSize = 64;
    [SerializeField, Range(0.1f, 60f)] private float grainRefreshRate = 24f;

    [Header("Overlay Order")]
    [SerializeField] private int sortingOrder = 5000;

    private Camera targetCamera;
    private Canvas overlayCanvas;
    private RawImage tintImage;
    private RawImage vignetteImage;
    private RawImage scanlineImage;
    private RawImage grainImage;
    private Texture2D vignetteTexture;
    private Texture2D scanlineTexture;
    private Texture2D grainTexture;
    private Material blurMaterial;
    private float grainTimer;
    private Color originalBackgroundColor;
    private int originalCullingMask;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera != null)
        {
            originalBackgroundColor = targetCamera.backgroundColor;
            originalCullingMask = targetCamera.cullingMask;
        }

        if (blurShader == null)
            blurShader = Shader.Find("Hidden/SoftBlur");
    }

    private void OnEnable()
    {
        ApplySettings();
        if (Application.isPlaying)
            StartCoroutine(EnsureOverlayNextFrame());
    }

    private void Start()
    {
        BuildOverlay();
        ApplySettings();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ApplySettings();
    }

    private System.Collections.IEnumerator EnsureOverlayNextFrame()
    {
        yield return null;
        BuildOverlay();
        ApplySettings();
    }

    private void Update()
    {
        if (!postProcessingEnabled || !grainEnabled || grainImage == null)
            return;

        grainTimer += Time.unscaledDeltaTime;
        float interval = 1f / grainRefreshRate;
        if (grainTimer >= interval)
        {
            grainTimer = 0f;
            UpdateGrainTexture();
        }
    }

    private void BuildOverlay()
    {
        if (overlayCanvas != null)
            return;

        GameObject canvasObject = new GameObject("PostProcessingOverlay");
        canvasObject.hideFlags = HideFlags.HideAndDontSave;

        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        tintImage = CreateOverlayImage("Tint", overlayCanvas.transform);
        vignetteImage = CreateOverlayImage("Vignette", overlayCanvas.transform);
        scanlineImage = CreateOverlayImage("Scanlines", overlayCanvas.transform);
        grainImage = CreateOverlayImage("Grain", overlayCanvas.transform);

        if (vignetteTexture == null)
            vignetteTexture = CreateVignetteTexture(256, 256);

        if (scanlineTexture == null)
            scanlineTexture = CreateScanlineTexture(256, 256);

        if (grainTexture == null)
            grainTexture = CreateGrainTexture(grainTextureSize);

        vignetteImage.texture = vignetteTexture;
        scanlineImage.texture = scanlineTexture;
        grainImage.texture = grainTexture;
    }

    private RawImage CreateOverlayImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage image = go.GetComponent<RawImage>();
        image.raycastTarget = false;
        return image;
    }

    private void ApplySettings()
    {
        if (overlayCanvas != null)
            overlayCanvas.enabled = postProcessingEnabled;

        if (targetCamera != null && useCameraBackgroundAsTint)
        {
            targetCamera.backgroundColor = Color.Lerp(originalBackgroundColor, tintColor, tintStrength);
        }

        if (targetCamera != null)
        {
            targetCamera.cullingMask = overrideCameraCullingMask
                ? cameraCullingMask.value
                : originalCullingMask;
        }

        if (tintImage != null)
        {
            tintImage.color = new Color(tintColor.r, tintColor.g, tintColor.b, tintStrength);
            tintImage.enabled = postProcessingEnabled && tintStrength > 0f;
        }

        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, vignetteStrength);
            vignetteImage.enabled = postProcessingEnabled && vignetteEnabled && vignetteStrength > 0f;
        }

        if (scanlineImage != null)
        {
            scanlineImage.color = new Color(1f, 1f, 1f, scanlineOpacity);
            scanlineImage.enabled = postProcessingEnabled && scanlinesEnabled && scanlineOpacity > 0f;
            scanlineImage.uvRect = new Rect(0f, 0f, 1f, Mathf.Max(1f, scanlineThickness));
        }

        if (grainImage != null)
        {
            grainImage.color = new Color(1f, 1f, 1f, grainOpacity);
            grainImage.enabled = postProcessingEnabled && grainEnabled && grainOpacity > 0f;
        }
    }

    private Texture2D CreateVignetteTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color edge = new Color(1f, 1f, 1f, 1f);

        for (int y = 0; y < height; y++)
        {
            float ny = (y / (float)(height - 1)) * 2f - 1f;
            for (int x = 0; x < width; x++)
            {
                float nx = (x / (float)(width - 1)) * 2f - 1f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01(Mathf.InverseLerp(vignetteSoftness, 1f, dist));
                texture.SetPixel(x, y, Color.Lerp(clear, edge, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private Texture2D CreateScanlineTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            bool darkLine = (y % 2) == 0;
            Color c = darkLine ? new Color(0f, 0f, 0f, 1f) : new Color(1f, 1f, 1f, 0f);
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        return texture;
    }

    private Texture2D CreateGrainTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;
        FillGrainTexture(texture);
        texture.Apply();
        return texture;
    }

    private void UpdateGrainTexture()
    {
        if (grainTexture == null)
            return;

        FillGrainTexture(grainTexture);
        grainTexture.Apply();
    }

    private void FillGrainTexture(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = Random.value;
                texture.SetPixel(x, y, new Color(value, value, value, 1f));
            }
        }
    }

    private void OnDisable()
    {
        if (targetCamera != null)
            targetCamera.cullingMask = originalCullingMask;

        if (overlayCanvas != null)
            Destroy(overlayCanvas.gameObject);

        if (blurMaterial != null)
            Destroy(blurMaterial);

        overlayCanvas = null;
        blurMaterial = null;
        tintImage = null;
        vignetteImage = null;
        scanlineImage = null;
        grainImage = null;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!postProcessingEnabled || !blurEnabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (blurShader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (blurMaterial == null)
        {
            blurMaterial = new Material(blurShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        int scale = Mathf.Max(1, blurDownsample);
        int width = Mathf.Max(1, source.width / scale);
        int height = Mathf.Max(1, source.height / scale);

        RenderTexture rt1 = RenderTexture.GetTemporary(width, height, 0, source.format);
        RenderTexture rt2 = RenderTexture.GetTemporary(width, height, 0, source.format);

        Graphics.Blit(source, rt1);

        blurMaterial.SetFloat("_BlurSize", blurStrength);
        for (int i = 0; i < Mathf.Max(1, blurIterations); i++)
        {
            blurMaterial.SetVector("_BlurDirection", new Vector2(1f, 0f));
            Graphics.Blit(rt1, rt2, blurMaterial);

            blurMaterial.SetVector("_BlurDirection", new Vector2(0f, 1f));
            Graphics.Blit(rt2, rt1, blurMaterial);
        }

        Graphics.Blit(rt1, destination);

        RenderTexture.ReleaseTemporary(rt2);
        RenderTexture.ReleaseTemporary(rt1);
    }
}
