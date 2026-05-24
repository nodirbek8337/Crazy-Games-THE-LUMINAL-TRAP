using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private string actionName = "Interact";
    [SerializeField] private bool resetOnDisable = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return;

        CrossPlatformInputManager.SetButtonDown(actionName);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseAction();
    }

    private void OnDisable()
    {
        if (resetOnDisable)
            ReleaseAction();
    }

    private void ReleaseAction()
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return;

        CrossPlatformInputManager.SetButtonUp(actionName);
    }
}
