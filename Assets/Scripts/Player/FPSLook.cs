using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class FPSLook : MonoBehaviour
{
    public Transform playerBody;
    public Transform cameraRig;

    [Header("Sensitivity")]
    public float lookSpeed = 1.0f;
    private float minSensitivity = 0.5f;
    private float maxSensitivity = 2.5f;
    private const float sensitivityBoost = 4f;

    [Header("Mouse Stability")]
    [SerializeField] private bool useRawMouseInput = true;
    [SerializeField] private bool clampMouseDelta = true;
    [SerializeField] private float maxMouseDeltaPerFrame = 3f;

    [Header("Movement Lean")]
    [SerializeField] private bool enableMovementLean = true;
    [SerializeField] private float strafeLeanAngle = 1.5f;
    [SerializeField] private float walkSwayAngle = 1.25f;
    [SerializeField] private float walkSwaySpeed = 7f;
    [SerializeField] private float runSwayMultiplier = 1.35f;
    [SerializeField] private float leanSmoothSpeed = 8f;

    private float xRotation = 0f;
    private float currentLeanZ = 0f;
    private float walkSwayTimer = 0f;
    private Quaternion targetRotation;

    public Movement movementScript;
    public static bool isStopping = false;

    private GameSettingsService settingsService;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        settingsService = GameSettingsService.Get();
        ApplySensitivity(settingsService.MouseSensitivityNormalized);
        settingsService.MouseSensitivityChanged += ApplySensitivity;

        xRotation = NormalizeAngle(cameraRig.localEulerAngles.x);
        targetRotation = Quaternion.Euler(xRotation, 0f, 0f);
        cameraRig.localRotation = targetRotation;
    }

    private void OnDisable()
    {
        if (settingsService != null)
            settingsService.MouseSensitivityChanged -= ApplySensitivity;
    }

    void Update()
    {
        if (PauseGame.isPaused || PauseGame.IsGameplayLocked || isStopping || (movementScript != null && movementScript.IsDead()))
            return;

        float rawMouseX = GetLookAxis("Mouse X");
        float rawMouseY = GetLookAxis("Mouse Y");

        if (clampMouseDelta)
        {
            rawMouseX = Mathf.Clamp(rawMouseX, -maxMouseDeltaPerFrame, maxMouseDeltaPerFrame);
            rawMouseY = Mathf.Clamp(rawMouseY, -maxMouseDeltaPerFrame, maxMouseDeltaPerFrame);
        }

        float mouseX = rawMouseX * lookSpeed * Time.deltaTime * 100f;
        float mouseY = rawMouseY * lookSpeed * Time.deltaTime * 100f;
        float moveX = GetMoveAxis("Horizontal");
        float moveZ = GetMoveAxis("Vertical");
        float moveMagnitude = Mathf.Clamp01(new Vector2(moveX, moveZ).magnitude);
        bool isMoving = moveMagnitude > 0.1f;
        bool isRunning = GetRunInput() && isMoving;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 70f);

        float targetLeanZ = 0f;
        if (enableMovementLean)
        {
            float strafeLean = -moveX * moveMagnitude * strafeLeanAngle;
            float swayMultiplier = isRunning ? runSwayMultiplier : 1f;

            if (isMoving)
                walkSwayTimer += Time.deltaTime * walkSwaySpeed * swayMultiplier * moveMagnitude;

            float walkSway = Mathf.Sin(walkSwayTimer) * walkSwayAngle * moveMagnitude;
            targetLeanZ = strafeLean + walkSway;
        }
        else
        {
            walkSwayTimer = 0f;
        }

        currentLeanZ = Mathf.Lerp(currentLeanZ, targetLeanZ, Time.deltaTime * leanSmoothSpeed);

        targetRotation = Quaternion.Euler(xRotation, 0f, currentLeanZ);
        cameraRig.localRotation = targetRotation;
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private float GetLookAxis(string axisName)
    {
        return useRawMouseInput
            ? CrossPlatformInputManager.GetAxisRaw(axisName)
            : CrossPlatformInputManager.GetAxis(axisName);
    }

    private float GetMoveAxis(string axisName)
    {
        return CrossPlatformInputManager.GetAxis(axisName);
    }

    private bool GetRunInput()
    {
        if (Application.isMobilePlatform)
            return CrossPlatformInputManager.GetButton("Run");

        return Input.GetKey(KeyCode.LeftShift);
    }

    public void SetMouseSensitivity(float sliderValue)
    {
        GameSettingsService.Get().SetMouseSensitivity(sliderValue);
    }

    public void ApplySensitivity(float sliderValue)
    {
        lookSpeed = CalculateSensitivity(sliderValue);
    }

    private float CalculateSensitivity(float sliderValue)
    {
        float sensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, Mathf.Pow(sliderValue, 3));
        return sensitivity * sensitivityBoost;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
