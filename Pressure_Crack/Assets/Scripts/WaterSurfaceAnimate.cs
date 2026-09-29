using UnityEngine;

public class WaterSurfaceAnimate : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.04f, 0.02f);
    private Renderer waterRenderer;

    private void Start()
    {
        // Using typeof() completely avoids the CS0411 generic bracket error
        waterRenderer = (Renderer)GetComponent(typeof(Renderer));
    }

    private void Update()
    {
        if (waterRenderer != null && waterRenderer.material != null)
        {
            Vector2 offset = Time.time * scrollSpeed;
            waterRenderer.material.SetTextureOffset("_BaseMap", offset);
            waterRenderer.material.SetTextureOffset("_BumpMap", offset);
        }
    }
}
