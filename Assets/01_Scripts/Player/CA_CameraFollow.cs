using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] public Vector3 offtet;

    private CA_PlayerController player; // referencia segura

    void Start()
    {
        player = CA_PlayerController.Instance;
    }

    void Update()
    {
        if (player == null)
        {
            player = CA_PlayerController.Instance;
            if (player == null) return; // aún no existe ninguno
        }
        Vector3 targetPos = player.transform.position + offtet;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed);
    }
}
