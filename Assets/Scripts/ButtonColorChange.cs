using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Color Settings")]
    [SerializeField] private Color hoverColor = new Color(0.545f, 0.071f, 0.071f);

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private Color originalColor;

    void Start()
    {
        EnsureReferences();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeColor(buttonText, hoverColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeColor(buttonText, originalColor));
    }

    private void OnEnable()
    {
        EnsureReferences();
        ResetToOriginalColor();
    }

    private void OnDisable()
    {
        ResetToOriginalColor();
    }

    private IEnumerator ChangeColor(TextMeshProUGUI text, Color targetColor)
    {
        Color startColor = text.color;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            text.color = Color.Lerp(startColor, targetColor, elapsedTime / transitionDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        text.color = targetColor;
    }

    private void EnsureReferences()
    {
        if (buttonText == null)
            buttonText = GetComponent<TextMeshProUGUI>();

        if (buttonText != null)
            originalColor = buttonText.color;
    }

    private void ResetToOriginalColor()
    {
        StopAllCoroutines();

        if (buttonText != null)
            buttonText.color = originalColor;
    }
}
