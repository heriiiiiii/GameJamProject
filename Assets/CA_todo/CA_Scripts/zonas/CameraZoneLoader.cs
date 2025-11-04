using System.Collections.Generic;
using UnityEngine;

public class CameraZoneLoader : MonoBehaviour
{
    public Camera cam;                       // Cámara principal
    public float buffer = 2f;                // Margen extra para activar objetos fuera de pantalla
    public string tagCargable = "Cargable";  // Tag de los objetos que queremos activar/desactivar

    private List<GameObject> objetos = new List<GameObject>();

    void Start()
    {
        if (cam == null) cam = Camera.main;

        // Encuentra todos los objetos cargables en la escena
        GameObject[] encontrados = GameObject.FindGameObjectsWithTag(tagCargable);
        objetos.AddRange(encontrados);

        // Inicialmente desactiva todos
        foreach (GameObject obj in objetos)
            obj.SetActive(false);
    }

    void Update()
    {
        Vector3 camPos = cam.transform.position;
        float altura = cam.orthographicSize;
        float ancho = altura * cam.aspect;

        // Rectángulo visible + buffer
        Rect cameraRect = new Rect(
            camPos.x - ancho - buffer,
            camPos.y - altura - buffer,
            2 * (ancho + buffer),
            2 * (altura + buffer)
        );

        foreach (GameObject obj in objetos)
        {
            if (obj == null) continue;

            Vector3 pos = obj.transform.position;
            bool dentro = cameraRect.Contains(new Vector2(pos.x, pos.y));

            if (dentro && !obj.activeSelf)
                obj.SetActive(true);
            else if (!dentro && obj.activeSelf)
                obj.SetActive(false);
        }
    }
}
