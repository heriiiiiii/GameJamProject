using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JQG_PARALAX : MonoBehaviour
{
    Transform cam;
    Vector3 camStartPos;
    Vector3 distance;

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 1f)]
    public float parallaxSpeed = 0.5f;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        // Distancia de la cámara respecto al inicio (solo usamos X para el efecto)
        distance = cam.position - camStartPos;

        // El objeto contenedor sigue la cámara en Y (para mantener el encuadre)
        transform.position = new Vector3(cam.position.x, cam.position.y, cam.position.z + 10f);

        // Aplicar parallax SOLO en X
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;

            // Movimiento horizontal del material
            Vector2 offset = new Vector2(distance.x * speed, 0);

            // Aplica al material
            mat[i].SetTextureOffset("_MainTex", offset);
        }
    }
}
