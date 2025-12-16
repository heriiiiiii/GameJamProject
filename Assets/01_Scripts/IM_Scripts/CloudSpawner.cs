using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [Header("🌥️ Prefabs de Nubes")]
    public GameObject[] cloudPrefabs; // Arrastra tus 2 prefabs aquí

    [Header("📍 Configuración")]
    public float startY = 0f;           // Altura donde aparecen
    public float startX = -20f;         // Punto inicial fuera de cámara
    public float endX = 20f;            // Punto donde desaparecen
    public float moveSpeed = 0.5f;      // Velocidad del movimiento
    public float spacing = 15f;         // Distancia entre las dos nubes

    private GameObject[] activeClouds;

    private void Start()
    {
        // Instancia los 2 grupos de nubes
        activeClouds = new GameObject[cloudPrefabs.Length];
        for (int i = 0; i < cloudPrefabs.Length; i++)
        {
            Vector3 spawnPos = new Vector3(startX - (i * spacing), startY, 0);
            activeClouds[i] = Instantiate(cloudPrefabs[i], spawnPos, Quaternion.identity, transform);
        }
    }

    private void Update()
    {
        foreach (GameObject cloud in activeClouds)
        {
            if (cloud == null) continue;

            // Mover nubes hacia la derecha
            cloud.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

            // Si salen del límite derecho, reaparecen a la izquierda
            if (cloud.transform.position.x > endX)
            {
                float offset = Random.Range(-2f, 2f); // variación vertical leve
                cloud.transform.position = new Vector3(startX, startY + offset, 0);
            }
        }
    }
}


