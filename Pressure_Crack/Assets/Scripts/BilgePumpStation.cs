using UnityEngine;

public class BilgePumpStation : MonoBehaviour
{
    [Header("Pump Hold Config")]
    [SerializeField] private float requiredHoldTime = 6.0f; // 5-7 seconds target
    [SerializeField] private float waterDrainAmount = 25.0f; // Percentage W drained on completion

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pumpCrankSound;
    [SerializeField] private AudioClip pumpFinishSound;

    private float currentHoldTimer = 0f;

    // Returns normalized progress from 0.0 to 1.0 for the UI
    public float Progress => Mathf.Clamp01(currentHoldTimer / requiredHoldTime);

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;

        if (pumpCrankSound == null) pumpCrankSound = Resources.Load<AudioClip>("SFX/bilge_pump_cycle");
        if (pumpFinishSound == null) pumpFinishSound = Resources.Load<AudioClip>("SFX/bilge_pump_finish");
    }

    public void RegisterHold(float deltaTime)
    {
        currentHoldTimer += deltaTime;

        // Loop cranking audio while pumping
        if (audioSource != null && pumpCrankSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = pumpCrankSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Trigger drain once timer reaches required hold time
        if (currentHoldTimer >= requiredHoldTime)
        {
            OnPumpComplete();
            currentHoldTimer = 0f; // Reset progress after completing full pump cycle
        }
    }

    public void ResetHold()
    {
        // Immediately drop timer back to zero when player releases [E] or looks away
        currentHoldTimer = 0f;

        if (audioSource != null && audioSource.isPlaying && audioSource.clip == pumpCrankSound)
        {
            audioSource.Stop();
        }
    }

    private void OnPumpComplete()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            if (pumpFinishSound != null)
            {
                audioSource.PlayOneShot(pumpFinishSound);
            }
        }

        if (WaterManager.Instance != null)
        {
            WaterManager.Instance.DrainWater(waterDrainAmount);
        }
        Debug.Log("[BilgePump] Successfully pumped bilge water!");
    }
}
