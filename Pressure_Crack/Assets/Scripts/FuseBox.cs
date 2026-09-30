using UnityEngine;

public class FuseBox : MonoBehaviour
{
    [Header("Fuse Box State")]
    [SerializeField] private bool isBroken = false;

    [Header("3D Model References")]
    [SerializeField] private GameObject normalModel;
    [SerializeField] private GameObject brokenModel;

    [Header("Lighting Connection")]
    [SerializeField] private CockpitLighting cockpitLighting;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blowSound;

    public bool IsBroken => isBroken;

    private void Start()
    {
        UpdateVisuals();

        if (cockpitLighting == null)
        {
            cockpitLighting = FindFirstObjectByType<CockpitLighting>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1.0f;

        if (blowSound == null)
        {
            blowSound = Resources.Load<AudioClip>("SFX/fuse_blow");
        }
    }

    public void BreakFuseBox()
    {
        if (isBroken) return;
        isBroken = true;
        UpdateVisuals();

        if (cockpitLighting != null)
        {
            cockpitLighting.SetPowerState(false);
        }

        if (audioSource != null && blowSound != null)
        {
            audioSource.PlayOneShot(blowSound);
        }

        Debug.LogWarning("[HAZARD] Fuse box blew! Submarine power lost!");
    }

    public void FixFuseBox()
    {
        if (!isBroken) return;
        isBroken = false;
        UpdateVisuals();

        if (cockpitLighting != null)
        {
            cockpitLighting.SetPowerState(true);
        }

        Debug.Log("[REPAIRED] Fuse box restored! Power online.");
    }

    public void FixBox()
    {
        FixFuseBox();
    }

    private void UpdateVisuals()
    {
        if (normalModel != null) normalModel.SetActive(!isBroken);
        if (brokenModel != null) brokenModel.SetActive(isBroken);
    }
}
