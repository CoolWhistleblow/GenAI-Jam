using UnityEngine;
using TMPro;

public class ThrottleLever : MonoBehaviour
{
    public static ThrottleLever Instance { get; private set; }

    public enum ThrottleState { Slow, Standard, Overdrive }

    [Header("Current State")]
    public ThrottleState currentState = ThrottleState.Standard;

    [Header("Lever Target Transform")]
    [SerializeField] private Transform leverPivotTransform;

    [Header("Rotation Angles (Local X-Axis)")]
    [SerializeField] private float slowAngle = -25f;
    [SerializeField] private float standardAngle = 0f;
    [SerializeField] private float overdriveAngle = 25f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Speed & Hazard Multipliers")]
    public float speedMultiplier = 1.0f;
    public float hazardMultiplier = 1.0f;

    [Header("Status Text Indicators")]
    [SerializeField] private TMP_Text slowText;
    [SerializeField] private TMP_Text standardText;
    [SerializeField] private TMP_Text overdriveText;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip leverShiftSound;

    private Quaternion targetRotation;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (leverPivotTransform == null) leverPivotTransform = transform;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        if (leverShiftSound == null) leverShiftSound = Resources.Load<AudioClip>("SFX/throttle_notch");

        ApplyState(currentState);
    }

    private void Update()
    {
        if (leverPivotTransform != null)
        {
            leverPivotTransform.localRotation = Quaternion.Slerp(
                leverPivotTransform.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void Interact()
    {
        if (audioSource != null && leverShiftSound != null)
        {
            audioSource.PlayOneShot(leverShiftSound);
        }

        switch (currentState)
        {
            case ThrottleState.Slow:
                ApplyState(ThrottleState.Standard);
                break;
            case ThrottleState.Standard:
                ApplyState(ThrottleState.Overdrive);
                break;
            case ThrottleState.Overdrive:
                ApplyState(ThrottleState.Slow);
                break;
        }
    }

    private void ApplyState(ThrottleState newState)
    {
        currentState = newState;
        float targetXAngle = standardAngle;

        switch (currentState)
        {
            case ThrottleState.Slow:
                targetXAngle = slowAngle;
                speedMultiplier = 0.5f;
                hazardMultiplier = 0.5f;
                break;

            case ThrottleState.Standard:
                targetXAngle = standardAngle;
                speedMultiplier = 1.0f;
                hazardMultiplier = 1.0f;
                break;

            case ThrottleState.Overdrive:
                targetXAngle = overdriveAngle;
                speedMultiplier = 2.0f;
                hazardMultiplier = 2.5f;
                break;
        }

        targetRotation = Quaternion.Euler(targetXAngle, 0f, 0f);

        // Update Text Indicators
        if (slowText != null) slowText.color = (currentState == ThrottleState.Slow) ? activeColor : inactiveColor;
        if (standardText != null) standardText.color = (currentState == ThrottleState.Standard) ? activeColor : inactiveColor;
        if (overdriveText != null) overdriveText.color = (currentState == ThrottleState.Overdrive) ? activeColor : inactiveColor;
    }
}
