using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Ranges")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float generalDistance = 4.0f;
    [SerializeField] private float fuseDistance = 2.5f;

    [Header("UI References")]
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image progressRingImage;
    [SerializeField] private GameObject crosshairUI;

    [Header("Fuse Setup")]
    [SerializeField] private bool hasSpareFuse = false;
    [SerializeField] private GameObject heldFuseVisual;
    [SerializeField] private GameObject droppedFusePrefab;
    [SerializeField] private float throwForce = 5.0f;

    [Header("Movement & Look Locking")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour cameraLookScript;

    [Header("Pumping Camera Shake")]
    [SerializeField] private float shakeIntensity = 0.025f;
    [SerializeField] private float shakeSpeed = 25.0f;

    [Header("Ring Color Styling")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color completeColor = Color.green;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fusePickupSound;

    private BilgePumpStation activePump;
    private HullLeak activeLeak;
    private Collider playerCollider;
    private Rigidbody playerRigidbody;
    private bool isPumping = false;

    // Camera Shake Variables
    private Vector3 originalCamLocalPos;
    private bool isCamPositionSaved = false;

    private void Start()
    {
        playerCollider = GetComponent<Collider>();
        playerRigidbody = GetComponent<Rigidbody>();

        if (playerInput == null)
        {
            playerInput = GetComponentInParent<PlayerInput>();
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetActive(true);
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D so player always hears interactions directly
        audioSource.volume = 1f;

        if (fusePickupSound == null) fusePickupSound = Resources.Load<AudioClip>("SFX/fuse_pickup");

        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
        }

        RefreshHeldFuseVisibility();
    }

    private void Update()
    {
        // 1. Minigame UI Check
        if (FuseBoxMiniGameUI.Instance != null && FuseBoxMiniGameUI.Instance.IsActive)
        {
            ResetAllInteractions();
            if (crosshairUI != null) crosshairUI.SetActive(false);
            return;
        }

        if (crosshairUI != null) crosshairUI.SetActive(true);
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        // 2. Read Inputs
        bool lmbPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool rmbPressed = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool lmbHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool eHeld = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        // 3. Drop/Throw Fuse (Right-Click)
        if (hasSpareFuse && rmbPressed)
        {
            DropFuse();
            ResetAllInteractions();
            return;
        }

        // 4. Center-Screen Raycast
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, generalDistance);
        System.Array.Sort(hits, (hitA, hitB) => hitA.distance.CompareTo(hitB.distance));

        // 5. Target Evaluation
        foreach (RaycastHit hit in hits)
        {
            // CASE A: HOLDING A FUSE
            if (hasSpareFuse)
            {
                if (hit.collider.TryGetComponent(out FuseBox box) && box.IsBroken)
                {
                    if (hit.distance <= fuseDistance)
                    {
                        ClearActivePump();
                        ClearActiveLeak();
                        DisplayPrompt("Click [LMB] Insert Fuse");

                        if (lmbPressed)
                        {
                            hasSpareFuse = false;
                            RefreshHeldFuseVisibility();

                            if (audioSource != null && fusePickupSound != null)
                            {
                                audioSource.PlayOneShot(fusePickupSound, 2.0f);
                            }

                            if (FuseBoxMiniGameUI.Instance != null)
                            {
                                FuseBoxMiniGameUI.Instance.OpenMiniGame(box);
                            }
                        }
                        UpdateProgressRing(0f);
                        return;
                    }
                }
                break;
            }

            // CASE B: HANDS ARE FREE

            // Pickup Fuse from Fuse Rack / Dispenser
            if (hit.collider.TryGetComponent(out SpareFuse spare))
            {
                if (hit.distance <= fuseDistance)
                {
                    ClearActivePump();
                    ClearActiveLeak();
                    DisplayPrompt("Click [LMB] Take Fuse");

                    if (lmbPressed)
                    {
                        hasSpareFuse = true;
                        spare.PickUp();
                        RefreshHeldFuseVisibility();

                        if (audioSource != null && fusePickupSound != null)
                        {
                            audioSource.PlayOneShot(fusePickupSound, 2.5f);
                        }
                    }
                    UpdateProgressRing(0f);
                    return;
                }
            }

            // Broken Fuse Box Notice
            if (hit.collider.TryGetComponent(out FuseBox targetBox) && targetBox.IsBroken)
            {
                if (hit.distance <= fuseDistance)
                {
                    ClearActivePump();
                    ClearActiveLeak();
                    DisplayPrompt("Requires Spare Fuse");
                    UpdateProgressRing(0f);
                    return;
                }
            }

            // Throttle Lever
            if (hit.collider.TryGetComponent(out ThrottleLever lever))
            {
                ClearActivePump();
                ClearActiveLeak();
                DisplayPrompt("Click [LMB]");

                if (lmbPressed)
                {
                    lever.Interact();
                }
                UpdateProgressRing(0f);
                return;
            }

            // Bilge Pump
            if (hit.collider.TryGetComponent(out BilgePumpStation pump))
            {
                ClearActiveLeak();
                activePump = pump;
                DisplayPrompt("Hold [E] to Pump water out");

                if (eHeld)
                {
                    if (!isPumping)
                    {
                        isPumping = true;
                        SetMovementLocked(true);
                        SaveCameraPosition();
                    }

                    ApplyCameraShake();
                    pump.RegisterHold(Time.deltaTime);
                }
                else
                {
                    ClearActivePump();
                }

                UpdateProgressRing(pump.Progress);
                return;
            }

            // Hull Leak
            if (hit.collider.TryGetComponent(out HullLeak leak) && leak.IsActive)
            {
                ClearActivePump();
                activeLeak = leak;
                DisplayPrompt("Hold [LMB] to Seal Leak");

                if (lmbHeld)
                {
                    leak.RepairHold(Time.deltaTime);
                }
                else
                {
                    leak.ResetRepair();
                }

                UpdateProgressRing(leak.Progress);
                return;
            }
        }

        // Default prompt
        if (hasSpareFuse)
        {
            DisplayPrompt("[RMB] Throw Fuse");
        }
        else
        {
            ResetAllInteractions();
        }
    }

    // --- Camera Shake Methods ---

    private void SaveCameraPosition()
    {
        if (playerCamera != null && !isCamPositionSaved)
        {
            originalCamLocalPos = playerCamera.transform.localPosition;
            isCamPositionSaved = true;
        }
    }

    private void ApplyCameraShake()
    {
        if (playerCamera == null || !isCamPositionSaved) return;

        float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * 2f * shakeIntensity;
        float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * 2f * shakeIntensity;

        playerCamera.transform.localPosition = originalCamLocalPos + new Vector3(offsetX, offsetY, 0f);
    }

    private void RestoreCameraPosition()
    {
        if (playerCamera != null && isCamPositionSaved)
        {
            playerCamera.transform.localPosition = originalCamLocalPos;
            isCamPositionSaved = false;
        }
    }

    // --- Control & Interaction Handlers ---

    private void SetMovementLocked(bool lockState)
    {
        if (playerInput != null)
        {
            playerInput.enabled = !lockState;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !lockState;
        }

        if (cameraLookScript != null)
        {
            cameraLookScript.enabled = !lockState;
        }

        if (lockState && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void DropFuse()
    {
        hasSpareFuse = false;
        RefreshHeldFuseVisibility();

        if (droppedFusePrefab == null) return;

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit wallHit, 0.5f))
        {
            spawnPosition = wallHit.point - playerCamera.transform.forward * 0.1f;
        }

        GameObject spawnedFuse = Instantiate(droppedFusePrefab, spawnPosition, playerCamera.transform.rotation);



        Collider fuseCollider = spawnedFuse.GetComponent<Collider>();
        if (playerCollider != null && fuseCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, fuseCollider);
        }

        Rigidbody fuseRigidbody = spawnedFuse.GetComponent<Rigidbody>();
        if (fuseRigidbody != null)
        {
            fuseRigidbody.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        }
    }

    private void RefreshHeldFuseVisibility()
    {
        if (heldFuseVisual != null)
        {
            heldFuseVisual.SetActive(hasSpareFuse);
        }
    }

    private void DisplayPrompt(string text)
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(true);
        if (promptText != null) promptText.text = text;
    }

    private void UpdateProgressRing(float value)
    {
        if (progressRingImage != null)
        {
            progressRingImage.fillAmount = value;
            progressRingImage.color = Color.Lerp(startColor, completeColor, value);
        }
    }

    private void ClearActivePump()
    {
        if (isPumping)
        {
            isPumping = false;
            RestoreCameraPosition();
            SetMovementLocked(false);
        }

        if (activePump != null)
        {
            activePump.ResetHold();
            activePump = null;
        }
    }

    private void ClearActiveLeak()
    {
        if (activeLeak != null)
        {
            activeLeak.ResetRepair();
            activeLeak = null;
        }
    }

    private void ResetAllInteractions()
    {
        ClearActivePump();
        ClearActiveLeak();

        if (!hasSpareFuse && interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }

        if (progressRingImage != null)
        {
            progressRingImage.fillAmount = 0f;
            progressRingImage.color = startColor;
        }
    }
}