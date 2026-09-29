using UnityEngine;

public class FuseBox : MonoBehaviour
{
    public static FuseBox Instance { get; private set; }

    [Header("Fuse Box State")]
    public bool IsBroken = false;

    [Header("3D Model References")]
    [SerializeField] private GameObject normalModel;
    [SerializeField] private GameObject brokenModel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void BreakFuseBox()
    {
        if (IsBroken) return;
        IsBroken = true;
        UpdateVisuals();
        Debug.LogWarning("[HAZARD] Fuse box blew! Submarine descent halted!");
    }

    public void FixFuseBox()
    {
        if (!IsBroken) return;
        IsBroken = false;
        UpdateVisuals();
        Debug.Log("[REPAIRED] Fuse box restored! Resuming descent.");
    }

    private void UpdateVisuals()
    {
        if (normalModel != null) normalModel.SetActive(!IsBroken);
        if (brokenModel != null) brokenModel.SetActive(IsBroken);
    }
}
