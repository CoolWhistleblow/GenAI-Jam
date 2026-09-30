using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class FuseBoxMiniGameUI : MonoBehaviour
{
    public static FuseBoxMiniGameUI Instance { get; private set; }

    [Header("UI Canvas References")]
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Timing Bar Setup")]
    [SerializeField] private RectTransform barContainer;
    [SerializeField] private RectTransform greenZone;
    [SerializeField] private RectTransform needle;

    [Header("Minigame Settings")]
    [SerializeField] private float needleSpeed = 500.0f;
    [SerializeField] private float timeLimit = 6.0f;
    [SerializeField] private int requiredWins = 2; // Must hit green zone twice in a row

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip stagePassSound;
    [SerializeField] private AudioClip failSound;

    public bool IsActive { get; private set; }

    private float currentTimer;
    private bool isGameFinished = false;
    private float barWidth;
    private float minX;
    private float maxX;

    private int currentWins = 0;
    private FuseBox targetFuseBox; // Tracks which fuse box opened this UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    private void Start()
    {
        CalculateBarBounds();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (stagePassSound == null) stagePassSound = Resources.Load<AudioClip>("SFX/fuse_stage_hit");
        if (successSound == null) successSound = Resources.Load<AudioClip>("SFX/fuse_repair_success");
        if (failSound == null) failSound = Resources.Load<AudioClip>("SFX/fuse_fail_buzz");
    }

    private void CalculateBarBounds()
    {
        if (barContainer != null)
        {
            barWidth = barContainer.rect.width;
            minX = -barWidth / 2f;
            maxX = barWidth / 2f;
        }
    }

    private void Update()
    {
        if (!IsActive || isGameFinished) return;

        // 1. Countdown Timer
        currentTimer -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = $"TIME: {Mathf.Max(0f, currentTimer):0.0}s";
        }

        if (currentTimer <= 0f)
        {
            FailMiniGame("TIME EXPIRED!");
            return;
        }

        // 2. Oscillate Yellow Needle
        if (needle != null)
        {
            float pingPong = Mathf.PingPong(Time.time * needleSpeed, barWidth);
            needle.anchoredPosition = new Vector2(minX + pingPong, needle.anchoredPosition.y);
        }

        // 3. Player Input
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool lmbPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (spacePressed || lmbPressed)
        {
            CheckTimingHit();
        }

        // 4. Close with ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseMiniGame();
        }
    }

    public void OpenMiniGame(FuseBox fuseBox = null)
    {
        IsActive = true;
        isGameFinished = false;
        currentTimer = timeLimit;
        currentWins = 0;
        targetFuseBox = fuseBox;

        CalculateBarBounds();

        if (miniGamePanel != null) miniGamePanel.SetActive(true);
        if (statusText != null) statusText.text = $"STAGE 1/{requiredWins}: PRESS [SPACE]";

        RandomizeGreenZone();
    }

    public void CloseMiniGame()
    {
        IsActive = false;
        targetFuseBox = null;
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
    }

    private void RandomizeGreenZone()
    {
        if (greenZone == null || barContainer == null) return;

        float zoneWidth = greenZone.rect.width;
        float safePadding = zoneWidth / 2f;

        float randomX = Random.Range(minX + safePadding, maxX - safePadding);
        greenZone.anchoredPosition = new Vector2(randomX, greenZone.anchoredPosition.y);
    }

    private void CheckTimingHit()
    {
        if (needle == null || greenZone == null) return;

        float needleX = needle.anchoredPosition.x;
        float greenX = greenZone.anchoredPosition.x;
        float halfGreenWidth = greenZone.rect.width / 2f;

        if (needleX >= (greenX - halfGreenWidth) && needleX <= (greenX + halfGreenWidth))
        {
            currentWins++;

            if (currentWins >= requiredWins)
            {
                CompleteMiniGame();
            }
            else
            {
                // Stage Passed! Set up next stage
                PlaySound(stagePassSound);
                if (statusText != null) statusText.text = $"STAGE {currentWins + 1}/{requiredWins}: GO AGAIN!";
                RandomizeGreenZone();
            }
        }
        else
        {
            FailMiniGame("MISSED! STREAK RESET!");
        }
    }

    private void CompleteMiniGame()
    {
        isGameFinished = true;
        if (statusText != null) statusText.text = "POWER RESTORED!";
        PlaySound(successSound);

        // Fix the target fuse box!
        if (targetFuseBox != null)
        {
            targetFuseBox.FixBox();
        }

        StartCoroutine(SuccessRoutine());
    }

    private void FailMiniGame(string reason)
    {
        isGameFinished = true;
        currentWins = 0; // Reset consecutive wins on fail
        if (statusText != null) statusText.text = reason;
        PlaySound(failSound);

        StartCoroutine(ResetFailureRoutine());
    }

    private IEnumerator SuccessRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        CloseMiniGame();
    }

    private IEnumerator ResetFailureRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        OpenMiniGame(targetFuseBox); // Restart puzzle with same box reference
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
