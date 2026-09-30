using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    [Header("Defeat UI Canvas")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Victory UI Canvas")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryTitleText;
    [SerializeField] private TextMeshProUGUI victorySubtitleText;

    [Header("Victory Target")]
    [SerializeField] private float targetDepth = 11000f;

    private bool isGameEnded = false;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameEnded) return;

        // 1. LOSS CONDITION: Water reaches 100%
        if (WaterManager.Instance != null && WaterManager.Instance.currentWaterLevel >= 100f)
        {
            TriggerDefeat();
        }

        // 2. WIN CONDITION: Depth reaches 11,000 meters
        if (DescentManager.Instance != null && DescentManager.Instance.currentDepth >= targetDepth)
        {
            TriggerVictory();
        }
    }

    private void TriggerDefeat()
    {
        isGameEnded = true;
        FreezeGame();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (titleText != null) titleText.text = "HULL CRUSH FAILURE";
        if (subtitleText != null) subtitleText.text = "The submarine flooded and succumbed to extreme water pressure.";
    }

    private void TriggerVictory()
    {
        isGameEnded = true;
        FreezeGame();

        if (victoryPanel != null) victoryPanel.SetActive(true);

        if (victoryTitleText != null)
        {
            victoryTitleText.text = "COSPT\n\n[ TOUCHDOWN: CHALLENGER DEEP - 10,994 M ]";
        }

        if (victorySubtitleText != null)
        {
            victorySubtitleText.text = "\"Exterior pressure: 1,086 bar. Hull strain stabilizing.\nSonar ping confirms solid sea floor touchdown.\n\nYou survived the descent through catastrophic power failure\nand pressure breaches. Scientific payload deployed.\"";
        }
    }

    private void FreezeGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Connect this method to BOTH of your UI Button OnClick() events!
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
