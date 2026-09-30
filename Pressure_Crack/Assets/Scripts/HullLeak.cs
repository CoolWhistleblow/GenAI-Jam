using UnityEngine;

public class HullLeak : MonoBehaviour
{
    [Header("Visual & Repair Config")]
    [SerializeField] private GameObject sprayParticleFX;
    [SerializeField] private float requiredSealingTime = 2.0f; // Hold LMB for 2 seconds to seal

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private AudioClip sealSound;

    public bool IsActive { get; private set; } = false;
    private float currentSealTimer = 0f;

    public float Progress => Mathf.Clamp01(currentSealTimer / requiredSealingTime);

    private void Start()
    {
        if (sprayParticleFX != null) sprayParticleFX.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1.0f;
        audioSource.maxDistance = 15.0f;

        if (spraySound == null) spraySound = Resources.Load<AudioClip>("SFX/hull_breach_spray");
        if (sealSound == null) sealSound = Resources.Load<AudioClip>("SFX/leak_seal_weld");
    }

    public void TriggerLeak()
    {
        if (IsActive) return;

        IsActive = true;
        currentSealTimer = 0f;
        if (sprayParticleFX != null) sprayParticleFX.SetActive(true);

        if (audioSource != null && spraySound != null)
        {
            audioSource.clip = spraySound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (WaterManager.Instance != null)
        {
            WaterManager.Instance.activeLeakCount++;
        }
    }

    public void RepairHold(float deltaTime)
    {
        if (!IsActive) return;

        currentSealTimer += deltaTime;

        if (currentSealTimer >= requiredSealingTime)
        {
            SealLeak();
        }
    }

    public void ResetRepair()
    {
        if (IsActive)
        {
            currentSealTimer = Mathf.Max(0f, currentSealTimer - Time.deltaTime * 2.5f);
        }
    }

    public void SealLeak()
    {
        if (!IsActive) return;

        IsActive = false;
        currentSealTimer = 0f;
        if (sprayParticleFX != null) sprayParticleFX.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Stop();
            if (sealSound != null)
            {
                audioSource.PlayOneShot(sealSound);
            }
        }

        if (WaterManager.Instance != null)
        {
            WaterManager.Instance.activeLeakCount = Mathf.Max(0, WaterManager.Instance.activeLeakCount - 1);
        }

        Debug.Log("[Hull Leak] Breach successfully sealed!");
    }
}
