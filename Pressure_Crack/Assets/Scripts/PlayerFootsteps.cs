using UnityEngine;
using StarterAssets;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip metalFootstepClip;
    [SerializeField] private AudioClip waterFootstepClip;

    [Header("Step Timing")]
    [SerializeField] private float walkStepInterval = 0.45f;
    [SerializeField] private float sprintStepInterval = 0.32f;

    [Header("Volume & Pitch")]
    [SerializeField] private float stepVolume = 0.9f;
    [SerializeField] private float minPitch = 0.92f;
    [SerializeField] private float maxPitch = 1.08f;

    private CharacterController characterController;
    private FirstPersonController fpController;
    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float stepCycleTimer;

    private void Awake()
    {
        InitComponents();
    }

    private void Start()
    {
        InitComponents();
        lastPosition = transform.root.position;
    }

    private void InitComponents()
    {
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>() ?? GetComponent<CharacterController>() ?? FindFirstObjectByType<CharacterController>();

        if (fpController == null)
            fpController = GetComponentInParent<FirstPersonController>() ?? GetComponent<FirstPersonController>() ?? FindFirstObjectByType<FirstPersonController>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D so footsteps are always clear to the player
        audioSource.volume = 1f;

        if (metalFootstepClip == null) metalFootstepClip = Resources.Load<AudioClip>("SFX/footstep_metal");
        if (waterFootstepClip == null) waterFootstepClip = Resources.Load<AudioClip>("SFX/footstep_water");
    }

    private void Update()
    {
        if (characterController == null && fpController == null)
        {
            InitComponents();
            if (characterController == null && fpController == null) return;
        }

        // Reliable grounded check: prioritize FirstPersonController sphere check
        bool isGrounded = true;
        if (fpController != null) isGrounded = fpController.Grounded;
        else if (characterController != null) isGrounded = characterController.isGrounded;

        // Calculate horizontal displacement per frame (works even if CC.velocity is zero/jittery)
        Vector3 currentPos = transform.root.position;
        Vector3 displacement = currentPos - lastPosition;
        displacement.y = 0f;
        float horizontalSpeed = displacement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = currentPos;

        // Fallback speed from CharacterController
        if (characterController != null)
        {
            Vector3 ccVel = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            if (ccVel.magnitude > horizontalSpeed) horizontalSpeed = ccVel.magnitude;
        }

        // Footstep trigger
        if (isGrounded && horizontalSpeed > 0.4f)
        {
            float targetInterval = (fpController != null && horizontalSpeed > fpController.MoveSpeed + 0.5f) 
                ? sprintStepInterval 
                : walkStepInterval;

            stepCycleTimer += Time.deltaTime;
            if (stepCycleTimer >= targetInterval)
            {
                stepCycleTimer = 0f;
                PlayFootstep(currentPos);
            }
        }
        else
        {
            stepCycleTimer = Mathf.Min(stepCycleTimer, 0.1f);
        }
    }

    private void PlayFootstep(Vector3 playerPos)
    {
        if (audioSource == null) return;

        // Check if player feet are in water
        bool feetInWater = false;
        if (WaterManager.Instance != null && WaterManager.Instance.waterPlaneTransform != null)
        {
            // If water plane height is at or above player feet level (within 0.5m)
            feetInWater = (playerPos.y - 0.2f) <= WaterManager.Instance.waterPlaneTransform.position.y;
        }

        AudioClip clipToPlay = feetInWater ? waterFootstepClip : metalFootstepClip;
        if (clipToPlay != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clipToPlay, stepVolume);
        }
    }
}
