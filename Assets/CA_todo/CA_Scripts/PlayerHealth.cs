using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int vida = 5;
    private bool muerto = false;

    public void RecibirDanio(int cantidad)
    {
        if (muerto) return;

        vida -= cantidad;
        Debug.Log("Daño recibido: " + cantidad + " | Vida restante: " + vida);

        if (vida <= 0)
        {
            muerto = true;
            Debug.Log("Jugador muerto");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
