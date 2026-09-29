using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class FuseBoxMiniGameUI : MonoBehaviour
{
    public static FuseBoxMiniGameUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private RectTransform needleTransform;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Mini-Game Config")]
    [SerializeField] private float needleSpeed = 500f;  // Pixel oscillation speed
    [SerializeField] private float barWidth = 300f;     // Total range (-150 to +150)
    [SerializeField] private float greenZoneWidth = 60f;// Sweet spot centered at 0

    private float currentNeedlePos = 0f;
    private int moveDirection = 1;
    private int consecutiveHits = 0;
    public bool IsActive { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    public void OpenMiniGame()
    {
        if (IsActive) return;
        IsActive = true;
        consecutiveHits = 0;
        currentNeedlePos = -barWidth * 0.5f;
        moveDirection = 1;

        if (miniGamePanel != null) miniGamePanel.SetActive(true);
        UpdateStatusText();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMiniGame()
    {
        IsActive = false;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsActive) return;

        // Oscillate needle back and forth
        currentNeedlePos += moveDirection * needleSpeed * Time.deltaTime;
        float halfBar = barWidth * 0.5f;

        if (currentNeedlePos >= halfBar)
        {
            currentNeedlePos = halfBar;
            moveDirection = -1;
        }
        else if (currentNeedlePos <= -halfBar)
        {
            currentNeedlePos = -halfBar;
            moveDirection = 1;
        }

        if (needleTransform != null)
        {
            needleTransform.anchoredPosition = new Vector2(currentNeedlePos, needleTransform.anchoredPosition.y);
        }

        // Left Click to attempt timing
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AttemptSkillCheck();
        }
    }

    private void AttemptSkillCheck()
    {
        float halfGreen = greenZoneWidth * 0.5f;

        // Check if needle is inside central green zone [-30, +30]
        if (currentNeedlePos >= -halfGreen && currentNeedlePos <= halfGreen)
        {
            consecutiveHits++;

            if (consecutiveHits >= 2)
            {
                if (FuseBox.Instance != null) FuseBox.Instance.FixFuseBox();
                CloseMiniGame();
            }
            else
            {
                UpdateStatusText();
            }
        }
        else
        {
            // Miss resets consecutive hit count back to 0
            consecutiveHits = 0;
            UpdateStatusText();
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = $"Click when aligned!\nProgress: {consecutiveHits}/2";
        }
    }
}
