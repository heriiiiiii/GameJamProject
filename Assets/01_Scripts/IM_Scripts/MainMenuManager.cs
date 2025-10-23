using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("🎮 Escenas y Paneles")]
    public string presentationScene = "PresentacionScene";
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("🧭 Navegación por teclado")]
    public GameObject firstMainButton;     // 🔹 Ej: Btn_Start
    public GameObject firstOptionsButton;  // 🔹 Ej: Return o SliderAudio

    [Header("🎧 Sonidos")]
    public AudioClip startSound;
    public AudioClip optionsSound;
    public AudioClip exitSound;

    [Header("🔊 Fuente de audio (AudioSource global o local)")]
    public AudioSource audioSource;

    private void Start()
    {
        // Buscar fuente de audio si no está asignada
        if (audioSource == null && AudioManager.Instance != null)
            audioSource = AudioManager.Instance.GetComponent<AudioSource>();

        // Mostrar solo el menú principal
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);

        // 🔹 Establecer foco inicial
        if (firstMainButton != null)
            EventSystem.current.SetSelectedGameObject(firstMainButton);
    }

    // ======================
    // 🔹 ACCIONES DEL MENÚ
    // ======================

    public void StartGame()
    {
        Debug.Log("🎮 Iniciando juego...");
        StartCoroutine(PlaySoundAndThen(startSound, () =>
        {
            SceneManager.LoadScene(presentationScene);
        }));
    }

    public void OpenOptions()
    {
        Debug.Log("⚙️ Abriendo panel de opciones...");
        StartCoroutine(PlaySoundAndThen(optionsSound, () =>
        {
            // Ocultar menú principal y mostrar panel de opciones
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (optionsPanel) optionsPanel.SetActive(true);

            // 🔹 Establecer foco inicial en Options
            if (firstOptionsButton != null)
                EventSystem.current.SetSelectedGameObject(firstOptionsButton);

            Debug.Log("✅ Panel de opciones activado y foco configurado.");
        }));
    }

    public void ExitGame()
    {
        Debug.Log("🚪 Saliendo del juego...");
        StartCoroutine(PlaySoundAndThen(exitSound, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
    }

    // ======================
    // 🔹 FUNCIÓN CENTRALIZADA
    // ======================

    private IEnumerator PlaySoundAndThen(AudioClip clip, System.Action action)
    {
        float waitTime = 0f;

        if (clip && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            waitTime = clip.length;
        }

        yield return new WaitForSeconds(waitTime);
        action?.Invoke();
    }

    // ======================
    // 🔹 CERRAR OPCIONES
    // ======================

    public void CloseOptions()
    {
        Debug.Log("↩️ Cerrando panel de opciones...");
        StartCoroutine(PlaySoundAndThen(exitSound, () =>
        {
            if (optionsPanel) optionsPanel.SetActive(false);
            if (mainMenuPanel) mainMenuPanel.SetActive(true);

            // 🔹 Volver a enfocar el botón principal
            if (firstMainButton != null)
                EventSystem.current.SetSelectedGameObject(firstMainButton);

            Debug.Log("✅ Volvió al menú principal con foco restaurado.");
        }));
    }
}
