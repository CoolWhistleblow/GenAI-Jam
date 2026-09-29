using UnityEngine;

public class WaterLevelController : MonoBehaviour
{
    [Header("Height Bounds (Local Y Axis)")]
    [Tooltip("Y Position when Water Level W is 0% (Floor)")]
    [SerializeField] private float minY = -0.8f;

    [Tooltip("Y Position when Water Level W is 100% (Flooded)")]
    [SerializeField] private float maxY = 0.9f;

    [SerializeField] private float moveSpeed = 2.5f;

    private void Update()
    {
        if (WaterManager.Instance == null) return;

        // Convert 0..100% to a 0.0..1.0 float value
        float normalizedWater = WaterManager.Instance.currentWaterLevel / WaterManager.Instance.maxWaterLevel;

        // Calculate target Y coordinate
        float targetY = Mathf.Lerp(minY, maxY, normalizedWater);

        // Smoothly move water height toward target
        Vector3 currentPos = transform.localPosition;
        currentPos.y = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * moveSpeed);
        transform.localPosition = currentPos;
    }
}