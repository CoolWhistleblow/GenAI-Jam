using UnityEngine;

public class SpareFuse : MonoBehaviour
{
    [SerializeField] private bool destroyOnPickup = false;
    [SerializeField] private AudioClip dropSound;
    private AudioSource audioSource;
    private float lastSoundTime = 0f;

    private void Awake()
    {
        InitAudio();
    }

    private void Start()
    {
        InitAudio();
    }

    private void InitAudio()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.3f; // Semi-2D so player clearly hears it from any camera angle
        audioSource.minDistance = 1.0f;
        audioSource.maxDistance = 25.0f;
        audioSource.volume = 1.0f;

        if (dropSound == null) dropSound = Resources.Load<AudioClip>("SFX/fuse_drop");
    }

    public void PickUp()
    {
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        InitAudio();

        if (Time.time - lastSoundTime < 0.1f) return;

        // Trigger on any reasonable impact with ground, walls, or props
        if (collision.relativeVelocity.magnitude > 0.2f && dropSound != null && audioSource != null)
        {
            lastSoundTime = Time.time;
            float vol = Mathf.Clamp(collision.relativeVelocity.magnitude / 2.0f, 0.6f, 1.0f);
            audioSource.PlayOneShot(dropSound, vol);
        }
    }
}