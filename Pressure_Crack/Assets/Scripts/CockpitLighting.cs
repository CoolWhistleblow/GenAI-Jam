using UnityEngine;

public class CockpitLighting : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private FuseBox fuseBox;

    [Header("Main Lights")]
    [SerializeField] private Light mainCabinLight;
    [SerializeField] private Light redAlarmLight;

    [Header("Station Accent Lights")]
    [SerializeField] private Light fuseBoxIndicator;
    [SerializeField] private Light pumpIndicator;
    [SerializeField] private Light controlBoxLight;

    [Header("Panic Strobe Settings")]
    [SerializeField] private float flashSpeed = 6.0f;
    [SerializeField] private float peakIntensity = 4.5f;
    [SerializeField] private float minRedIntensity = 0.12f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource alarmAudioSource;
    [SerializeField] private AudioClip alarmKlaxonSound;

    private void Start()
    {
        if (fuseBox == null)
        {
            fuseBox = FindFirstObjectByType<FuseBox>();
        }

        if (alarmAudioSource == null) alarmAudioSource = GetComponent<AudioSource>();
        if (alarmAudioSource == null) alarmAudioSource = gameObject.AddComponent<AudioSource>();
        alarmAudioSource.loop = true;
        alarmAudioSource.spatialBlend = 0.5f;

        if (alarmKlaxonSound == null) alarmKlaxonSound = Resources.Load<AudioClip>("SFX/emergency_alarm");
    }

    private void Update()
    {
        bool isPowerOff = (fuseBox != null && fuseBox.IsBroken);

        // 1. POWER NORMAL
        if (!isPowerOff)
        {
            if (mainCabinLight != null && !mainCabinLight.gameObject.activeSelf) mainCabinLight.gameObject.SetActive(true);
            if (fuseBoxIndicator != null && !fuseBoxIndicator.gameObject.activeSelf) fuseBoxIndicator.gameObject.SetActive(true);
            if (pumpIndicator != null && !pumpIndicator.gameObject.activeSelf) pumpIndicator.gameObject.SetActive(true);
            if (controlBoxLight != null && !controlBoxLight.gameObject.activeSelf) controlBoxLight.gameObject.SetActive(true);

            if (redAlarmLight != null && redAlarmLight.gameObject.activeSelf)
                redAlarmLight.gameObject.SetActive(false);

            if (alarmAudioSource != null && alarmAudioSource.isPlaying)
            {
                alarmAudioSource.Stop();
            }

            return;
        }

        // 2. POWER FAILURE / BLACKOUT
        if (mainCabinLight != null && mainCabinLight.gameObject.activeSelf) mainCabinLight.gameObject.SetActive(false);
        if (fuseBoxIndicator != null && fuseBoxIndicator.gameObject.activeSelf) fuseBoxIndicator.gameObject.SetActive(false);
        if (pumpIndicator != null && pumpIndicator.gameObject.activeSelf) pumpIndicator.gameObject.SetActive(false);
        if (controlBoxLight != null && controlBoxLight.gameObject.activeSelf) controlBoxLight.gameObject.SetActive(false);

        if (alarmAudioSource != null && alarmKlaxonSound != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.clip = alarmKlaxonSound;
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }

        // 3. STROBE RED LIGHT
        if (redAlarmLight != null)
        {
            if (!redAlarmLight.gameObject.activeSelf) redAlarmLight.gameObject.SetActive(true);
            if (!redAlarmLight.enabled) redAlarmLight.enabled = true;

            bool isFlash = (Mathf.Sin(Time.time * flashSpeed) > 0.2f);
            redAlarmLight.intensity = isFlash ? peakIntensity : minRedIntensity;
        }
    }
}
