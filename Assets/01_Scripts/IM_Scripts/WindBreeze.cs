using UnityEngine;

public class WindBreeze : MonoBehaviour
{
    [Header("Movimiento de viento")]
    public float amplitude = 0.05f;  // cuánto se inclina (pequeño)
    public float frequency = 2f;     // velocidad del viento
    public float offset;             // para desfasar varias plantas

    private Vector3 startRotation;

    void Start()
    {
        startRotation = transform.eulerAngles;
        offset = Random.Range(0f, Mathf.PI * 2f); // cada uno empieza distinto
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * frequency + offset) * amplitude * 30f;
        transform.eulerAngles = new Vector3(startRotation.x, startRotation.y, startRotation.z + angle);
    }
}
