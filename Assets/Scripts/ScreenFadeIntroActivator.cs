using System.Collections;
using UnityEngine;

public class ScreenFadeIntroActivator : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public GameObject targetObject;
    public AudioSource audioSource;
    public AudioClip introClip;

    [Header("Timing")]
    [Min(0f)] public float activationDelay = 0f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (targetObject != null)
            targetObject.SetActive(false);
    }

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(BeginIntroSequence());
    }

    private IEnumerator BeginIntroSequence()
    {
        // reklama: bu boshlanish qismi, shu sababli bu joyda ad qo'yilmaydi.
        yield return null;

        PlayIntroSound();
        StartCoroutine(ActivateTargetAfterDelay());
    }

    private void PlayIntroSound()
    {
        if (audioSource == null || introClip == null)
            return;

        audioSource.Stop();
        audioSource.clip = introClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private IEnumerator ActivateTargetAfterDelay()
    {
        if (targetObject == null)
            yield break;

        if (activationDelay > 0f)
            yield return new WaitForSecondsRealtime(activationDelay);

        targetObject.SetActive(true);
    }
}
