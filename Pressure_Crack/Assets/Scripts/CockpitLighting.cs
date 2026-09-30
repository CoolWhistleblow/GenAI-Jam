using UnityEngine;

public class CockpitLighting : MonoBehaviour
{
    [Header("Lighting References")]
    [SerializeField] private Light[] mainCabinLights;
    [SerializeField] private Light[] redAlarmLights;

    [Header("Alarm Visual Settings")]
    [SerializeField] private float pulseSpeed = 4.0f;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 2.5f;

    [Header("Alarm Audio Settings")]
    [SerializeField] private AudioSource alarmAudioSource;
    [SerializeField] private AudioClip alarmSound;

    private bool isPowerOn = true;

    private void Start()
    {
        if (alarmAudioSource == null)
        {
            alarmAudioSource = GetComponent<AudioSource>();
        }
        if (alarmAudioSource == null)
        {
            alarmAudioSource = gameObject.AddComponent<AudioSource>();
        }
        alarmAudioSource.spatialBlend = 0.0f;

        if (alarmSound == null)
        {
            alarmSound = Resources.Load<AudioClip>("SFX/emergency_alarm");
        }

        SetPowerState(isPowerOn);
    }

    private void Update()
    {
        if (!isPowerOn)
        {
            // Pulse red lights during blackout/alarm
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            foreach (Light redLight in redAlarmLights)
            {
                if (redLight != null) redLight.intensity = intensity;
            }
        }
    }

    public void SetPowerState(bool powerState)
    {
        isPowerOn = powerState;

        // Toggle main white lights
        foreach (Light whiteLight in mainCabinLights)
        {
            if (whiteLight != null) whiteLight.gameObject.SetActive(powerState);
        }

        // Toggle red alarm lights
        foreach (Light redLight in redAlarmLights)
        {
            if (redLight != null) redLight.gameObject.SetActive(!powerState);
        }

        // Alarm audio playback matching BilgePumpStation pattern
        if (alarmAudioSource != null && alarmSound != null)
        {
            if (!powerState)
            {
                if (!alarmAudioSource.isPlaying)
                {
                    alarmAudioSource.clip = alarmSound;
                    alarmAudioSource.loop = true;
                    alarmAudioSource.Play();
                }
            }
            else
            {
                if (alarmAudioSource.isPlaying)
                {
                    alarmAudioSource.Stop();
                }
            }
        }
    }
}
