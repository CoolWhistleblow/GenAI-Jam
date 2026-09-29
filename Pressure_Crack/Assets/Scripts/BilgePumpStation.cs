using UnityEngine;

public class BilgePumpStation : MonoBehaviour
{
    [Header("Pump Hold Config")]
    [SerializeField] private float requiredHoldTime = 6.0f; // 5-7 seconds target
    [SerializeField] private float waterDrainAmount = 25.0f; // Percentage W drained on completion

    private float currentHoldTimer = 0f;
    private bool isBeingOperated = false;

    // Returns normalized progress from 0.0 to 1.0 for the UI
    public float Progress => Mathf.Clamp01(currentHoldTimer / requiredHoldTime);

    public void RegisterHold(float deltaTime)
    {
        isBeingOperated = true;
        currentHoldTimer += deltaTime;

        // Trigger drain once timer reaches 6 seconds
        if (currentHoldTimer >= requiredHoldTime)
        {
            OnPumpComplete();
            currentHoldTimer = 0f; // Reset progress
        }
    }

    public void ResetHold()
    {
        // Decay timer smoothly back to 0 if player stops holding E early
        if (!isBeingOperated)
        {
            currentHoldTimer = Mathf.Max(0f, currentHoldTimer - Time.deltaTime * 2.5f);
        }
        isBeingOperated = false;
    }

    private void OnPumpComplete()
    {
        if (WaterManager.Instance != null)
        {
            WaterManager.Instance.DrainWater(waterDrainAmount);
        }
        Debug.Log("[BilgePump] Successfully pumped bilge water!");
    }
}
