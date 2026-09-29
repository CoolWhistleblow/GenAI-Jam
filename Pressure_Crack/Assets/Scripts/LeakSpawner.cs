using UnityEngine;

public class LeakSpawner : MonoBehaviour
{
    [Header("Leak Locations")]
    [SerializeField] private HullLeak[] leakPoints;

    [Header("Spawn Intervals (Seconds)")]
    [SerializeField] private float minSpawnInterval = 8f;
    [SerializeField] private float maxSpawnInterval = 18f;

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
        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
