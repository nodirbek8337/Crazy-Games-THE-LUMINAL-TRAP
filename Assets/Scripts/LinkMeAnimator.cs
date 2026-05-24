using UnityEngine;

public class LinkMeAnimator : MonoBehaviour
{
    public Animator animator; 

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        else
        {
            Debug.LogWarning("Animator component is missing on this object or not assigned.");
        }
    }
}
