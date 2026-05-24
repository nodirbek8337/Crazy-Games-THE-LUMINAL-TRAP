using UnityEngine;

public class DoorAnimatorClosingResponder : MonoBehaviour, IDoorTriggerCloseReceiver, ITriggerAudioReceiver
{
    [Header("Animator")]
    public Animator doorAnimator;
    [SerializeField] private string closingTriggerName = "Closing";

    [Header("Audio")]
    public AudioSource audioSource;

    private void Awake()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void CloseOnTrigger()
    {
        if (doorAnimator == null)
            return;

        doorAnimator.SetTrigger(closingTriggerName);
    }

    public void PlayOnTrigger()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        audioSource.Play();
    }
}
