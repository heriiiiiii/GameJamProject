using UnityEngine;

public class TransitionEventBridge : MonoBehaviour
{
    // Este método lo llamará el Animation Event
    public void OnTransitionComplete()
    {
        // Llama al manager real
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.OnTransitionComplete();
        else
            Debug.LogWarning("⚠️ SceneTransitionManager no encontrado en la escena.");
    }
}
