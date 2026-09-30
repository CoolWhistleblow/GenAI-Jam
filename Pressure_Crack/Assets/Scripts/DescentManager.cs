using UnityEngine;

public class DescentManager : MonoBehaviour
{
    public static DescentManager Instance { get; private set; }

    [Header("System References")]
    [SerializeField] private FuseBox primaryFuseBox;

    [Header("Depth Settings")]
    public float currentDepth = 0f;
    public float maxDepth = 11000f;
    [SerializeField] private float baseMetersPerSecond = 50f;

    [Header("Fuse Failure Risk")]
    [SerializeField] private float surfaceCheckInterval = 10f;
    [SerializeField] private float abyssCheckInterval = 4f;
    [SerializeField] private float maxBreakChanceAtBottom = 0.45f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hullGroanSound;
    [SerializeField] private float minGroanInterval = 12f;
    [SerializeField] private float maxGroanInterval = 30f;

    private float checkTimer;
    private float groanTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-find FuseBox in scene if not assigned in Inspector
        if (primaryFuseBox == null)
        {
            primaryFuseBox = FindFirstObjectByType<FuseBox>();
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.3f;

        if (hullGroanSound == null) hullGroanSound = Resources.Load<AudioClip>("SFX/hull_stress_groan");
        ResetGroanTimer();
    }

    private void Update()
    {
        // Depth-dependent hull groaning
        groanTimer -= Time.deltaTime;
        if (groanTimer <= 0f)
        {
            if (audioSource != null && hullGroanSound != null && currentDepth > 300f)
            {
                audioSource.PlayOneShot(hullGroanSound);
            }
            ResetGroanTimer();
        }

        bool canDescend = true;

        if (WaterManager.Instance != null && WaterManager.Instance.isDrowned) canDescend = false;
        if (primaryFuseBox != null && primaryFuseBox.IsBroken) canDescend = false;

        if (canDescend)
        {
            // 1. Get Speed Multiplier from Throttle Lever
            float throttleMult = ThrottleLever.Instance != null ? ThrottleLever.Instance.speedMultiplier : 1.0f;
            float actualSpeed = baseMetersPerSecond * throttleMult;

            if (currentDepth < maxDepth)
            {
                currentDepth += actualSpeed * Time.deltaTime;
                currentDepth = Mathf.Min(currentDepth, maxDepth);
            }

            // 2. Scale fuse risk timing by throttle hazard multiplier
            float hazardMult = ThrottleLever.Instance != null ? ThrottleLever.Instance.hazardMultiplier : 1.0f;
            float depthRatio = currentDepth / maxDepth;

            // Overdrive accelerates hazard checks
            float currentCheckInterval = Mathf.Lerp(surfaceCheckInterval, abyssCheckInterval, depthRatio) / hazardMult;

            checkTimer += Time.deltaTime;
            if (checkTimer >= currentCheckInterval)
            {
                checkTimer = 0f;
                RollFuseBreak(hazardMult);
            }
        }
    }

    private void RollFuseBreak(float hazardMult)
    {
        if (primaryFuseBox == null || primaryFuseBox.IsBroken) return;

        float depthRatio = currentDepth / maxDepth;
        float currentBreakChance = Mathf.Clamp01(depthRatio * maxBreakChanceAtBottom * hazardMult);

        if (Random.value < currentBreakChance)
        {
            primaryFuseBox.BreakFuseBox();
            Debug.LogWarning("[HAZARD] High engine load / depth pressure blew the main fuse!");
        }
    }

    private void ResetGroanTimer()
    {
        float depthRatio = currentDepth / maxDepth;
        float interval = Mathf.Lerp(maxGroanInterval, minGroanInterval, depthRatio);
        groanTimer = Random.Range(interval * 0.8f, interval * 1.2f);
    }
}
