using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSawMovement : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    public Transform pointA;
    public Transform pointB;

    [Header("Configuración")]
    public float speed = 2f;
    public bool startAtA = true;

    private Vector2 target;

    void Start()
    {
        // Asegurarse de que la sierra y los puntos estén en el plano Z = 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        if (pointA != null) pointA.position = new Vector3(pointA.position.x, pointA.position.y, 0f);
        if (pointB != null) pointB.position = new Vector3(pointB.position.x, pointB.position.y, 0f);

        // Definir punto inicial
        target = startAtA ? (Vector2)pointB.position : (Vector2)pointA.position;
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        // Movimiento solo en 2D (X y Y)
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Mantener el eje Z fijo en 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // Cambiar dirección al llegar al destino
        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            target = (target == (Vector2)pointA.position) ? (Vector2)pointB.position : (Vector2)pointA.position;
        }
    }
}
