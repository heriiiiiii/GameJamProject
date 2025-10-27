using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JQG_LevelChange : MonoBehaviour
{
    //[Header("Configuración de conexión")]
    //[SerializeField] private string _targetSceneName;      // Escena a cargar
    //[SerializeField] private string _targetEntryID;        // ID del portal destino en la otra escena
    //[SerializeField] private Transform _spawnPoint;        // Punto de spawn local
    //[SerializeField] private string _entryID;              // ID de este portal actual
    //public string EntryID => _entryID;

    //private bool isTransitioning = false;

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (isTransitioning) return;

    //    var player = collision.collider.GetComponent<CA_PlayerController>();
    //    if (player != null)
    //    {
    //        isTransitioning = true;
    //        StartCoroutine(Transition(player));
    //    }
    //}

    //private IEnumerator Transition(CA_PlayerController player)
    //{
    //    yield return StartCoroutine(JQG_FadeManager.Instance.FadeOut());

    //    Scene oldScene = SceneManager.GetActiveScene();
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
    //    while (!asyncLoad.isDone)
    //        yield return null;

    //    yield return null;

    //    Scene newScene = SceneManager.GetSceneByName(_targetSceneName);
    //    SceneManager.SetActiveScene(newScene);

    //    JQG_LevelChange targetPortal = null;
    //    var portals = FindObjectsOfType<JQG_LevelChange>(true);
    //    foreach (var p in portals)
    //    {
    //        if (p.EntryID == _targetEntryID)
    //        {
    //            targetPortal = p;
    //            break;
    //        }
    //    }

    //    if (player != null && targetPortal != null)
    //    {
    //        player.transform.position = targetPortal._spawnPoint.position;

    //        yield return new WaitUntil(() => Camera.main != null);

    //        var camFollow = Camera.main.GetComponent<CA_CameraFollow>();
    //        if (camFollow != null)
    //        {
    //            camFollow.transform.position = player.transform.position + camFollow.offtet;
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"No se encontró portal destino con ID '{_targetEntryID}' en la escena {_targetSceneName}");
    //    }

    //    yield return SceneManager.UnloadSceneAsync(oldScene);

    //    yield return StartCoroutine(JQG_FadeManager.Instance.FadeIn());

    //    isTransitioning = false;
    //}
}
