using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Configuración del Portal")]
    public string sceneToLoad;           // Nombre de la escena destino
    public string direction = "Right";   // "Right" o "Left"

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Guardamos la dirección para el spawn de la siguiente escena
        PlayerPrefs.SetString("LastPortalDirection", direction);
        PlayerPrefs.Save();

        // Disparamos la transición
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(sceneToLoad, direction);
        else
            Debug.LogWarning("SceneTransitionManager no encontrado.");
    }
}

