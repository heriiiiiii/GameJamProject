using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EspinasLetales : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();

            if (salud != null)
            {
                salud.RecibirDanio(100);
            }
        }
    }
}
