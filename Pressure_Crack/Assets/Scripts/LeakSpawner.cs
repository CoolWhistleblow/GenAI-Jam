using UnityEngine;

public class LeakSpawner : MonoBehaviour
{
    [Header("Leak Locations")]
    [SerializeField] private HullLeak[] leakPoints;

    [Header("Surface Spawn Intervals (0m Depth)")]
    [SerializeField] private float surfaceMinInterval = 12f;
    [SerializeField] private float surfaceMaxInterval = 20f;

    [Header("Abyss Spawn Intervals (11,000m Depth)")]
    [SerializeField] private float abyssMinInterval = 3f;
    [SerializeField] private float abyssMaxInterval = 7f;

    private float spawnTimer;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (WaterManager.Instance != null && WaterManager.Instance.isDrowned) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnRandomLeak();
            ResetTimer();
        }
    }

    private void SpawnRandomLeak()
    {
        if (leakPoints == null || leakPoints.Length == 0) return;

        // 1. Count how many leaks are currently inactive
        int inactiveCount = 0;
        for (int i = 0; i < leakPoints.Length; i++)
        {
            if (leakPoints[i] != null && !leakPoints[i].IsActive)
            {
                inactiveCount++;
            }
        }

        if (inactiveCount == 0) return; // All leaks are already active

        // 2. Pick a random target index among the inactive ones
        int targetInactiveIndex = Random.Range(0, inactiveCount);
        int currentInactiveIndex = 0;

        // 3. Find and trigger that specific leak
        for (int i = 0; i < leakPoints.Length; i++)
        {
            if (leakPoints[i] != null && !leakPoints[i].IsActive)
            {
                if (currentInactiveIndex == targetInactiveIndex)
                {
                    leakPoints[i].TriggerLeak();
                    Debug.LogWarning("[HAZARD] Hull breached! Water spraying into the cockpit!");
                    return;
                }
                currentInactiveIndex++;
            }
        }
    }

    private void ResetTimer()
    {
        // Calculate depth ratio (0.0 at surface -> 1.0 at max depth)
        float depthRatio = 0f;
        if (DescentManager.Instance != null)
        {
            depthRatio = Mathf.Clamp01(DescentManager.Instance.currentDepth / DescentManager.Instance.maxDepth);
        }

        // Dynamically shrink spawn intervals as depth increases
        float currentMin = Mathf.Lerp(surfaceMinInterval, abyssMinInterval, depthRatio);
        float currentMax = Mathf.Lerp(surfaceMaxInterval, abyssMaxInterval, depthRatio);

        spawnTimer = Random.Range(currentMin, currentMax);
    }
}