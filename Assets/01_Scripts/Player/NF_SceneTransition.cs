using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NF_SceneTransition : MonoBehaviour
{
    [Header("Configuración de transición")]
    [Tooltip("Nombre exacto de la escena a la que se viajará")]
    [SerializeField] private string targetSceneName;

    [Tooltip("Nombre del punto de spawn en la nueva escena")]
    [SerializeField] private string targetSpawnPoint;

    [Header("Efectos opcionales")]
    [SerializeField] private Animator transitionAnimator; // fade opcional
    [SerializeField] private float transitionTime = 1f;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return;
        if (!collision.CompareTag("Player")) return;

        isTransitioning = true;
        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        // 🔹 Guardar estado del jugador (habilidades, etc.)
        NF_GameData.SavePlayerState();

        // 🔹 Reproducir fade o animación de transición
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }

        // 🔹 Guardar el spawn al que debemos ir en la siguiente escena
        NF_GameData.nextSpawnName = targetSpawnPoint;

        // 🔹 Cargar la escena destino
        SceneManager.LoadScene(targetSceneName);
    }

    // 🔸 Llamar esto desde Start() en cada escena para colocar al jugador
    public static void PlacePlayerAtSpawn()
    {
        string spawnName = NF_GameData.nextSpawnName;
        if (string.IsNullOrEmpty(spawnName))
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find(spawnName);

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
        }

        // Restaurar habilidades persistentes
        if (player.TryGetComponent(out CA_PlayerController controller))
        {
            NF_GameData.LoadPlayerState(controller);
        }
    }
}
