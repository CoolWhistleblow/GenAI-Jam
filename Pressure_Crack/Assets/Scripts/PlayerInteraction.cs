using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Config")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3.5f;

    [Header("UI Prompt References")]
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image progressRingImage;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color completeColor = Color.green;

    private BilgePumpStation activePump;
    private HullLeak activeLeak;

    private void Update()
    {
        // Pause raycast interaction if mini-game UI is open
        if (FuseBoxMiniGameUI.Instance != null && FuseBoxMiniGameUI.Instance.IsActive)
        {
            ResetInteraction();
            return;
        }

        if (playerCamera == null) playerCamera = Camera.main;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool isHoldingE = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        bool isHoldingLMB = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool pressedLMB = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // 1. Fuse Box Repair
            if (hit.collider.TryGetComponent(out FuseBox fuseBox) && fuseBox.IsBroken)
            {
                ClearPumpFocus();
                ClearLeakFocus();
                SetUI(true, "Click [LMB] to Repair Fuse Box");

                if (pressedLMB && FuseBoxMiniGameUI.Instance != null)
                {
                    FuseBoxMiniGameUI.Instance.OpenMiniGame();
                }
                UpdateRing(0f);
                return;
            }

            // 2. Bilge Pump Station
            if (hit.collider.TryGetComponent(out BilgePumpStation pump))
            {
                ClearLeakFocus();
                activePump = pump;
                SetUI(true, "Hold [E] to Pump Bilge");

                if (isHoldingE) pump.RegisterHold(Time.deltaTime);
                else pump.ResetHold();

                UpdateRing(pump.Progress);
                return;
            }

            // 3. Hull Leak Repair
            if (hit.collider.TryGetComponent(out HullLeak leak) && leak.IsActive)
            {
                ClearPumpFocus();
                activeLeak = leak;
                SetUI(true, "Hold [LMB] to Seal Leak");

                if (isHoldingLMB) leak.RepairHold(Time.deltaTime);
                else leak.ResetRepair();

                UpdateRing(leak.Progress);
                return;
            }
        }

        ResetInteraction();
    }

    private void SetUI(bool active, string text)
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(active);
        if (promptText != null) promptText.text = text;
    }

    private void UpdateRing(float progress)
    {
        if (progressRingImage != null)
        {
            progressRingImage.fillAmount = progress;
            progressRingImage.color = Color.Lerp(startColor, completeColor, progress);
        }
    }

    private void ClearPumpFocus()
    {
        if (activePump != null) { activePump.ResetHold(); activePump = null; }
    }

    private void ClearLeakFocus()
    {
        if (activeLeak != null) { activeLeak.ResetRepair(); activeLeak = null; }
    }

    private void ResetInteraction()
    {
        ClearPumpFocus();
        ClearLeakFocus();

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (progressRingImage != null)
        {
            progressRingImage.fillAmount = 0f;
            progressRingImage.color = startColor;
        }
    }
}
