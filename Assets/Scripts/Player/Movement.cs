using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Movement : MonoBehaviour
{
    public CharacterController controller;
    public GameObject mainCamera;
    public ModelSwitcher modelSwitcher;
    public GameObject PauseSystem;
    public GameObject HUD;

    [Header("Ragdoll Settings")]
    public GameObject ragdollRoot;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runForwardSpeed = 5f;
    public float runSideAndBackSpeed = 4f;
    public float gravity = -9.81f;

    private float currentSpeed;
    private Vector3 velocity;

    [Header("Ground Check")]
    private bool isGrounded;

    [Header("Standing Values")]
    public float standHeight = 2f;
    public float standCenterY = 1f;
    public float standCamY = 1.655f;
    public float standCamZ = 0.206f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] indoorClips;
    public AudioClip[] grassClips;
    public AudioClip[] metalClips;
    public AudioClip[] waterClips;
    public AudioClip[] dirtGravelClips;

    private CapsuleCollider capsuleCollider;
    private Collider[] interactionColliders;
    private Rigidbody[] interactionRigidbodies;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.5f;
    private bool isDead = false;

    private bool isFrozen = false;
    private float originalAnimatorSpeed = 1f;
    private bool hasWarnedMissingController = false;

    void Awake()
    {
        ResolveController();
    }

    void OnEnable()
    {
        ResolveController();
    }

    void Start()
    {
        ResolveController();

        capsuleCollider = GetComponent<CapsuleCollider>();
        CacheInteractionComponents();
        currentSpeed = walkSpeed;

        DisableRagdoll();
    }

    void Update()
    {
        if (isDead || isFrozen || PauseGame.isPaused || PauseGame.IsGameplayLocked) return;

        ResolveController();
        if (controller == null)
        {
            if (!hasWarnedMissingController)
            {
                Debug.LogWarning($"{name}: CharacterController not found. Movement is disabled until the controller is assigned.");
                hasWarnedMissingController = true;
            }
            return;
        }

        if (!controller.gameObject.activeInHierarchy)
        {
            if (!hasWarnedMissingController)
            {
                Debug.LogWarning($"{name}: CharacterController game object is inactive. Check the prefab/scene hierarchy.");
                hasWarnedMissingController = true;
            }
            return;
        }

        if (!controller.enabled)
        {
            if (!isDead && !isFrozen)
            {
                controller.enabled = true;
                Debug.LogWarning($"{name}: CharacterController was disabled unexpectedly and has been re-enabled.");
            }
            else
            {
                return;
            }
        }

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        SetAnimatorSpeed(1);

        float x = GetMoveAxis("Horizontal");
        float z = GetMoveAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        bool isMoving = move.magnitude > 0.1f;

        bool wantsToRun = GetRunInput() && isMoving;
        bool isRunning = wantsToRun;
        bool isForwardRunning = isRunning && z > 0.1f;

        currentSpeed = isRunning
            ? (isForwardRunning ? runForwardSpeed : runSideAndBackSpeed)
            : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateAnimator(x, z, isRunning);

        if (isGrounded && isMoving)
        {
            footstepTimer += Time.deltaTime;

            float speedBasedInterval = isRunning ? 0.25f : 0.5f;
            footstepInterval = speedBasedInterval;

            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
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

    void UpdateAnimator(float x, float z, bool isRunning)
    {
        if (!HasPlayableAnimator()) return;

        float inputMagnitude = new Vector2(x, z).magnitude;

        modelSwitcher.CurrentAnimator.SetFloat("Speed", inputMagnitude);
        modelSwitcher.CurrentAnimator.speed = isRunning ? 1.5f : 1f;
    }

    void SetAnimatorSpeed(float speed)
    {
        if (HasPlayableAnimator())
            modelSwitcher.CurrentAnimator.speed = speed;
    }

    string DetectSurfaceTag()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 2f))
            return hit.collider.tag;
        return "Default";
    }

    void PlayFootstepSound()
    {
        if (audioSource == null) return;

        string tag = DetectSurfaceTag();
        AudioClip[] clips = indoorClips;

        switch (tag)
        {
            case "GrassGround":
                clips = grassClips;
                break;
            case "MetalGround":
                clips = metalClips;
                break;
            case "WaterGround":
                clips = waterClips;
                break;
            case "DirtGravelGround":
                clips = dirtGravelClips;
                break;
            default:
                clips = indoorClips;
                break;
        }

        PlayRandomClip(clips);
    }

    void PlayRandomClip(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0) return;

        int clipIndex = Random.Range(0, clips.Length);
        if (clips[clipIndex] != null)
            audioSource.PlayOneShot(clips[clipIndex]);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void EnableRagdoll()
    {
        isDead = true;

        if (capsuleCollider != null)
            capsuleCollider.enabled = false;

        if (modelSwitcher != null && modelSwitcher.CurrentAnimator != null)
            modelSwitcher.CurrentAnimator.enabled = false;

        if (controller != null)
            controller.enabled = false;

        if (ragdollRoot != null)
        {
            foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            {
                if (IsProtectedInteractionRigidbody(rb)) continue;
                rb.isKinematic = false;
            }

            foreach (Collider col in ragdollRoot.GetComponentsInChildren<Collider>())
            {
                // Keep the main player collision components and interaction colliders under Movement's control.
                if (IsProtectedCollider(col)) continue;
                col.enabled = true;
            }
        }
    }

    public void DisableRagdoll()
    {
        isDead = false;

        if (capsuleCollider != null)
            capsuleCollider.enabled = true;

        if (modelSwitcher != null && modelSwitcher.CurrentAnimator != null)
            modelSwitcher.CurrentAnimator.enabled = true;

        if (controller != null)
            controller.enabled = true;

        RestoreInteractionComponents();

        if (ragdollRoot != null)
        {
            foreach (Rigidbody rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            {
                if (IsProtectedInteractionRigidbody(rb)) continue;
                rb.isKinematic = true;
            }

            foreach (Collider col in ragdollRoot.GetComponentsInChildren<Collider>())
            {
                // Keep the main player collision components and interaction colliders under Movement's control.
                if (IsProtectedCollider(col)) continue;
                col.enabled = false;
            }
        }
    }

    public void Freeze()
    {
        isFrozen = true;

        velocity = Vector3.zero;

        if (modelSwitcher != null && modelSwitcher.CurrentAnimator != null)
        {
            originalAnimatorSpeed = modelSwitcher.CurrentAnimator.speed;
            modelSwitcher.CurrentAnimator.speed = 0f;
        }
    }

    public void Unfreeze()
    {
        isFrozen = false;

        if (modelSwitcher != null && modelSwitcher.CurrentAnimator != null)
        {
            modelSwitcher.CurrentAnimator.speed = originalAnimatorSpeed;
        }
    }

    private void ResolveController()
    {
        if (controller != null) return;

        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = GetComponentInParent<CharacterController>();
    }

    private void CacheInteractionComponents()
    {
        if (mainCamera == null)
            return;

        interactionColliders = mainCamera.GetComponents<Collider>();
        interactionRigidbodies = mainCamera.GetComponents<Rigidbody>();
    }

    private void RestoreInteractionComponents()
    {
        CacheInteractionComponents();

        if (interactionColliders != null)
        {
            foreach (Collider col in interactionColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }

        if (interactionRigidbodies != null)
        {
            foreach (Rigidbody rb in interactionRigidbodies)
            {
                if (rb != null)
                    rb.isKinematic = false;
            }
        }
    }

    private bool IsProtectedCollider(Collider col)
    {
        if (col == null)
            return false;

        if (col == controller || col == capsuleCollider)
            return true;

        if (interactionColliders == null || interactionColliders.Length == 0)
            CacheInteractionComponents();

        if (interactionColliders == null)
            return false;

        foreach (Collider interactionCollider in interactionColliders)
        {
            if (col == interactionCollider)
                return true;
        }

        return false;
    }

    private bool IsProtectedInteractionRigidbody(Rigidbody rb)
    {
        if (rb == null)
            return false;

        if (interactionRigidbodies == null || interactionRigidbodies.Length == 0)
            CacheInteractionComponents();

        if (interactionRigidbodies == null)
            return false;

        foreach (Rigidbody interactionRigidbody in interactionRigidbodies)
        {
            if (rb == interactionRigidbody)
                return true;
        }

        return false;
    }

    private bool HasPlayableAnimator()
    {
        if (modelSwitcher == null || modelSwitcher.CurrentAnimator == null)
            return false;

        RuntimeAnimatorController animatorController = modelSwitcher.CurrentAnimator.runtimeAnimatorController;
        return animatorController != null;
    }
}
