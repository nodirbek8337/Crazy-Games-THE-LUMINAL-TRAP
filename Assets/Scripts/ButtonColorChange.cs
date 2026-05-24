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
        if (buttonText == null)
            buttonText = GetComponent<TextMeshProUGUI>();

        originalColor = buttonText.color;
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
}
