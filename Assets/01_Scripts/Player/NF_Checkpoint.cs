using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NF_Checkpoint : MonoBehaviour
{
    //NF_GameController gameController;
    //bool playerInRange = false;

    //private void Awake()
    //{
    //    gameController = GameObject.FindGameObjectWithTag("Player").GetComponent<NF_GameController>();
    //}

    //private void Update()
    //{
    //    // Si el jugador está cerca del checkpoint y presiona "E", se guarda el punto de respawn
    //    if (playerInRange && Input.GetKeyDown(KeyCode.E))
    //    {
    //        gameController.UpdateCheckpoint(transform.position); // Guardamos el checkpoint
    //        Debug.Log("Checkpoint guardado en: " + transform.position);
    //    }
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // Detectar si el jugador entra en la zona del checkpoint
    //    if (collision.CompareTag("Player"))
    //    {
    //        playerInRange = true; // El jugador está dentro del rango
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    // Si el jugador sale del rango del checkpoint
    //    if (collision.CompareTag("Player"))
    //    {
    //        playerInRange = false; // El jugador salió del rango
    //    }
    //}
}
