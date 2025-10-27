using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // ✅ Necesario para usar corrutinas

public class MainMenuManager : MonoBehaviour
{
    [Header("Nombre de la escena de presentación")]
    public string presentationScene = "PresentacionScene";

    [Header("Tiempo de espera antes de ejecutar la acción")]
    public float delay = 1.5f; // segundos

    // 🔹 Método para cargar la escena de presentación
    public void StartGame()
    {
        Debug.Log("🎮 Cargando escena de presentación...");
        StartCoroutine(LoadSceneWithDelay());
    }

    // 🔹 Método para salir completamente del juego
    public void ExitGame()
    {
        Debug.Log("🚪 Saliendo del juego...");
        StartCoroutine(ExitWithDelay());
    }

    // === Corrutinas ===
    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(delay); // ⏱ espera antes de ejecutar
        SceneManager.LoadScene(presentationScene);
    }

    private IEnumerator ExitWithDelay()
    {
        yield return new WaitForSeconds(delay); // ⏱ espera antes de ejecutar

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
