using UnityEngine;

public class PlatformUIVisibility : MonoBehaviour
{
    public enum VisibilityMode
    {
        Always,
        MobileOnly,
        DesktopOnly
    }

    [SerializeField] private VisibilityMode visibilityMode = VisibilityMode.Always;

    private void Awake()
    {
        ApplyVisibility();
    }

    private void OnEnable()
    {
        ApplyVisibility();
    }

    public void ApplyVisibility()
    {
        bool shouldBeActive;
        switch (visibilityMode)
        {
            case VisibilityMode.MobileOnly:
                shouldBeActive = Application.isMobilePlatform;
                break;
            case VisibilityMode.DesktopOnly:
                shouldBeActive = !Application.isMobilePlatform;
                break;
            default:
                shouldBeActive = true;
                break;
        }

        if (gameObject.activeSelf != shouldBeActive)
            gameObject.SetActive(shouldBeActive);
    }
}
