using UnityEngine;

public class MainCameraAnimationController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;
    public string dieBoolName = "Die";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        SetDie(false);
    }

    public void SetDie(bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(dieBoolName))
            return;

        animator.SetBool(dieBoolName, value);
    }

    public void SetBool(string boolName, bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(boolName))
            return;

        animator.SetBool(boolName, value);
    }

    public void SetTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            return;

        animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
            return;

        animator.ResetTrigger(triggerName);
    }
}
