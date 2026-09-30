using UnityEngine;
using TMPro;

public class ControlBoxDisplay : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private FuseBox fuseBox;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI waterText;

    private void Start()
    {
        if (fuseBox == null)
        {
            fuseBox = FindFirstObjectByType<FuseBox>();
        }
    }

    private void Update()
    {
        // 1. Determine if power is active
        bool hasPower = (fuseBox != null && !fuseBox.IsBroken);

        // 2. Directly toggle text visibility based on power state
        if (depthText != null && depthText.gameObject.activeSelf != hasPower)
        {
            depthText.gameObject.SetActive(hasPower);
        }

        if (waterText != null && waterText.gameObject.activeSelf != hasPower)
        {
            waterText.gameObject.SetActive(hasPower);
        }

        // 3. Stop rendering values if power is out
        if (!hasPower) return;

        // 4. Render Depth Value
        if (depthText != null && DescentManager.Instance != null)
        {
            int depth = Mathf.FloorToInt(DescentManager.Instance.currentDepth);
            depthText.text = $"D E P T H :\n{depth:D4}  m";
        }

        // 5. Render Water Level Value
        if (waterText != null && WaterManager.Instance != null)
        {
            int water = Mathf.FloorToInt(WaterManager.Instance.currentWaterLevel);
            waterText.text = $"W A T E R :\n{water} %";
        }
    }
}