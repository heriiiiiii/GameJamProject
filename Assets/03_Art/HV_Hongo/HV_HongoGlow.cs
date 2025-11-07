using UnityEngine;

public class HongoGlow : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("Esporas")]
    [Range(0f, 2f)] public float density = 0.8f;
    [Range(0f, 2f)] public float riseSpeed = 0.4f;

    Material mat;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        mat = spriteRenderer.material;
    }

    void Update()
    {
        mat.SetFloat("_SporeDensity", density);
        mat.SetFloat("_SporeSpeed", riseSpeed);
    }
}
