using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    private void Start()
    {
        // Pause game and show mouse immediately
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }

    // Connect this to your [ BEGIN DESCENT ] button's OnClick event!
    public void StartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
}
