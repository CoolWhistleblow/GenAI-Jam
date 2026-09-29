using UnityEngine;
using StarterAssets;

public class UnderwaterEffects : MonoBehaviour
{
    [Header("Underwater Visuals")]
    [SerializeField] private Color underwaterColor = new Color(0.1f, 0.4f, 0.5f);
    [SerializeField] private float underwaterFogDensity = 0.15f;

    [Header("Player Reference")]
    [Tooltip("Drag PlayerCapsule here if auto-detect fails")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private float underwaterSpeedMultiplier = 0.4f;

    private float defaultMoveSpeed;
    private float defaultSprintSpeed;

    private bool defaultFogState;
    private Color defaultFogColor;
    private float defaultFogDensity;
    private bool isUnderwater;

    private void Start()
    {
        // Save surface fog defaults
        defaultFogState = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;

        // Non-generic fetches completely prevent CS0411 bracket stripping errors
        if (playerController == null)
        {
            playerController = (FirstPersonController)GetComponentInParent(typeof(FirstPersonController));
        }
        if (playerController == null)
        {
            playerController = (FirstPersonController)FindFirstObjectByType(typeof(FirstPersonController));
        }

        if (playerController != null)
        {
            defaultMoveSpeed = playerController.MoveSpeed;
            defaultSprintSpeed = playerController.SprintSpeed;
        }
    }

    private void Update()
    {
        if (WaterManager.Instance == null || WaterManager.Instance.waterPlaneTransform == null) return;

        // Check if camera height is below water surface
        bool currentlySubmerged = transform.position.y < WaterManager.Instance.waterPlaneTransform.position.y;

        if (currentlySubmerged != isUnderwater)
        {
            isUnderwater = currentlySubmerged;

            if (isUnderwater)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = underwaterColor;
                RenderSettings.fogDensity = underwaterFogDensity;

                if (playerController != null)
                {
                    playerController.MoveSpeed = defaultMoveSpeed * underwaterSpeedMultiplier;
                    playerController.SprintSpeed = defaultSprintSpeed * underwaterSpeedMultiplier;
                }
            }
            else
            {
                RenderSettings.fog = defaultFogState;
                RenderSettings.fogColor = defaultFogColor;
                RenderSettings.fogDensity = defaultFogDensity;

                if (playerController != null)
                {
                    playerController.MoveSpeed = defaultMoveSpeed;
                    playerController.SprintSpeed = defaultSprintSpeed;
                }
            }
        }
    }
}
