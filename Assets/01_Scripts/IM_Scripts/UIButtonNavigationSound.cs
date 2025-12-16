using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonNavigationSound : MonoBehaviour
{
    [Header("Sonido de navegación")]
    public AudioClip navigationSound;

    private GameObject lastSelected;

    void Update()
    {
        // Detecta cuando cambias de selección con teclado o gamepad
        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != lastSelected)
        {
            if (current != null && navigationSound != null)
                AudioManager.Instance?.PlaySFX(navigationSound);

            lastSelected = current;
        }
    }
}
