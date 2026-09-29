using UnityEngine;

public class DescentManager : MonoBehaviour
{
    public static DescentManager Instance { get; private set; }

    [Header("Depth Settings")]
    public float currentDepth = 0f;
    public float maxDepth = 11000f;
    [SerializeField] private float metersPerSecond = 50f; // ~3.6 minutes to reach 11,000m

    [Header("Fuse Failure Risk")]
    [SerializeField] private float checkInterval = 6f; // Roll for break every 6 seconds
    [SerializeField] private float maxBreakChanceAtBottom = 0.35f; // 35% break chance per check at max depth

    private float checkTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Stop descending if drowned or if the Fuse Box is broken
        bool canDescend = true;

        if (WaterManager.Instance != null && WaterManager.Instance.isDrowned) canDescend = false;
        if (FuseBox.Instance != null && FuseBox.Instance.IsBroken) canDescend = false;

        if (canDescend)
        {
            if (currentDepth < maxDepth)
            {
                currentDepth += metersPerSecond * Time.deltaTime;
                currentDepth = Mathf.Min(currentDepth, maxDepth);
            }

            // Periodic fuse box failure check
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                RollFuseBreak();
            }
        }
    }

    private void RollFuseBreak()
    {
        if (FuseBox.Instance == null || FuseBox.Instance.IsBroken) return;

        // Chance increases linearly with depth (0% at surface -> 35% at 11,000m)
        float depthRatio = currentDepth / maxDepth;
        float currentBreakChance = depthRatio * maxBreakChanceAtBottom;

        if (Random.value < currentBreakChance)
        {
            FuseBox.Instance.BreakFuseBox();
        }
    }
}
