using UnityEngine;

public class SpareFuse : MonoBehaviour
{
    [SerializeField] private bool destroyOnPickup = false;

    public void PickUp()
    {
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}