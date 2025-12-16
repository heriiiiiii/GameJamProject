using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Animator de transición (Canvas)")]
    public Animator transitionAnimator;

    private string nextScene;
    private string portalDirection; // "Right" o "Left"
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToScene(string sceneName, string direction)
    {
        if (isLoading) return;

        nextScene = sceneName;
        portalDirection = direction; // "Right" o "Left"
        isLoading = true;

        // Asegurar referencia al Animator actual
        if (transitionAnimator == null)
            transitionAnimator = FindObjectOfType<Animator>();

        if (transitionAnimator == null)
        {
            Debug.LogWarning("No hay Animator de transición en la escena. Cargando directo.");
            SceneManager.LoadScene(nextScene);
            isLoading = false;
            return;
        }

        // 1) SALIDA con la dirección pedida
        if (portalDirection == "Right")
            transitionAnimator.SetTrigger("ExitRight"); // izq→der (cubre)
        else
            transitionAnimator.SetTrigger("ExitLeft");  // der→izq (cubre)
    }

    // === Llamado por el Animation Event al final del clip de SALIDA ===
    public void OnTransitionComplete()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(nextScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Re-vincular Animator en la nueva escena
        transitionAnimator = FindObjectOfType<Animator>();
        if (transitionAnimator == null)
        {
            Debug.LogWarning("No hay Animator de transición en la nueva escena.");
            isLoading = false;
            return;
        }

        // Reposicionar jugador según la última dirección
        TryPlacePlayerAtSpawn();

        // 2) ENTRADA con la MISMA dirección visual
        if (portalDirection == "Right")
            transitionAnimator.SetTrigger("EnterRight"); // izq→der (descubre)
        else
            transitionAnimator.SetTrigger("EnterLeft");  // der→izq (descubre)
    }

    // === Llamado por el Animation Event al final del clip de ENTRADA (opcional) ===
    public void OnEntryComplete()
    {
        isLoading = false;
        // Si querés, acá puedes forzar volver a "Idle" con:
        // transitionAnimator.Play("Idle", 0, 0f);
    }

    private void TryPlacePlayerAtSpawn()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { Debug.LogWarning("Player no encontrado en la nueva escena."); return; }

        string lastDir = PlayerPrefs.GetString("LastPortalDirection", "Right");
        var spawns = GameObject.FindObjectsOfType<SceneSpawnPoint>();
        foreach (var s in spawns)
        {
            if (s.spawnID == (lastDir == "Right" ? "Left" : "Right"))
            {
                // Si venías desde la derecha, apareces en la IZQUIERDA de la nueva escena, y viceversa
                player.transform.position = s.transform.position;
                return;
            }
        }
        Debug.LogWarning("No se encontró SceneSpawnPoint que matchee la dirección.");
    }
}
