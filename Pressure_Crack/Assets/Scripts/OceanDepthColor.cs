using UnityEngine;

public class OceanDepthColor : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Ocean Colors")]
    [SerializeField] private Color surfaceOceanColor = new Color(0.04f, 0.10f, 0.20f, 1f); // Deep Oceanic Blue
    [SerializeField] private Color abyssColor = new Color(0f, 0f, 0f, 1f);                  // Pitch Black

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private void Update()
    {
        if (mainCamera == null || DescentManager.Instance == null) return;

        // Calculate depth fraction from 0.0 to 1.0
        float depthRatio = Mathf.Clamp01(DescentManager.Instance.currentDepth / DescentManager.Instance.maxDepth);

        // Smoothly fade window background from deep blue to absolute black
        mainCamera.backgroundColor = Color.Lerp(surfaceOceanColor, abyssColor, depthRatio);
    }
}
