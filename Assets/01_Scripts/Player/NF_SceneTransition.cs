using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NF_SceneTransition : MonoBehaviour
{
    [Header("Configuración de transición")]
    [Tooltip("Nombre de la escena destino")]
    public string targetSceneName;

    [Tooltip("ID del spawn point en la escena destino")]
    public string targetSpawnID;

    [Header("Efectos opcionales")]
    public Animator transitionAnimator;
    public float transitionTime = 1f;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        // Guardar habilidades del jugador
        NF_GameData.SavePlayerState();

        // Fade opcional
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }

        // Guardar el ID del spawn
        NF_GameData.nextSpawnName = targetSpawnID;

        // Cargar la escena destino
        SceneManager.LoadScene(targetSceneName);
    }

    // Colocar al jugador en el spawn correcto
    public static void PlacePlayerAtSpawn()
    {
        string spawnID = NF_GameData.nextSpawnName;
        if (string.IsNullOrEmpty(spawnID)) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Buscar spawns en la escena
        NF_SpawnPoint[] spawns = GameObject.FindObjectsOfType<NF_SpawnPoint>();

        foreach (NF_SpawnPoint sp in spawns)
        {
            if (sp.spawnID == spawnID)
            {
                player.transform.position = sp.transform.position;

                if (player.TryGetComponent(out CA_PlayerController controller))
                {
                    NF_GameData.LoadPlayerState(controller);
                }

                return; // ← se encontró
            }
        }

        Debug.LogWarning("⚠ No se encontró spawnID: " + spawnID);
    }
}
