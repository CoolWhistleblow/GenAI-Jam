using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance { get; private set; }

    [Header("Water System")]
    [Range(0, 100)] public float currentWaterLevel = 0f;
    public float maxWaterLevel = 100f;
    public Transform waterPlaneTransform;

    [Header("Leak System")]
    [Tooltip("Water rise percentage per second FOR EACH active leak")]
    [SerializeField] private float riseRatePerLeak = 1.5f;
    public int activeLeakCount = 0;

    [Header("State")]
    public bool isDrowned = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isDrowned) return;

        // Water only rises if active leaks exist
        if (activeLeakCount > 0)
        {
            float totalRiseRate = activeLeakCount * riseRatePerLeak;
            currentWaterLevel += totalRiseRate * Time.deltaTime;
            currentWaterLevel = Mathf.Clamp(currentWaterLevel, 0f, maxWaterLevel);
        }

        // Drowning / Game Over Condition (100% water level = no air)
        if (currentWaterLevel >= maxWaterLevel && !isDrowned)
        {
            TriggerDrowning();
        }
    }

    public void DrainWater(float amount)
    {
        currentWaterLevel -= amount;
        currentWaterLevel = Mathf.Clamp(currentWaterLevel, 0f, maxWaterLevel);
    }

    private void TriggerDrowning()
    {
        isDrowned = true;
        Debug.LogError("[GAME OVER] Submarine completely flooded! Oxygen supply depleted.");
    }
}
