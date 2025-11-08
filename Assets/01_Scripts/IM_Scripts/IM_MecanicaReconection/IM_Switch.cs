using UnityEngine;

public class IM_Switch : MonoBehaviour
{
    [Header("Puerta que se liberará")]
    public GameObject puerta;

    public void ActivateSwitch()
    {
        if (puerta != null)
        {
            puerta.SetActive(false); // Desaparece o desactiva la puerta
        }

        // Opcional: destruir el switch
        Destroy(gameObject);
    }
}
