using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuFocus : MonoBehaviour
{
    public GameObject firstButton;

    void Start()
    {
        // Selecciona el botón inicial
        ForceSelect(firstButton);

        // Ocultar y bloquear cursor en modo "solo teclado"
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Deshabilitar interacción del mouse sobre UI
        DisableMouseUIInteraction();
    }

    void Update()
    {
        // Si se hizo click este frame → ignoramos
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            return;

        // Si el foco se perdió → reponer suavemente sin reiniciar animación ni sonido
        if (EventSystem.current.currentSelectedGameObject == null)
            ForceSelect(firstButton);
    }

    private void ForceSelect(GameObject target)
    {
        // Previene que se llame OnSelect de nuevo (esto evitaba el "sonido de reset")
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void DisableMouseUIInteraction()
    {
        // 1) Desactivar raycasts del mouse para evitar clic real
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
        foreach (var r in raycasters)
            r.enabled = false;

        // 2) Reemplazar input por uno que ignora completamente el mouse
        var inputModules = FindObjectsOfType<StandaloneInputModule>();
        foreach (var module in inputModules)
        {
            module.forceModuleActive = true;
            module.inputOverride = new KeyboardOnlyInput();
        }
    }
}

// Este input elimina el mouse del sistema completamente
public class KeyboardOnlyInput : BaseInput
{
    public override bool mousePresent => false;
    public override Vector2 mousePosition => Vector2.zero;
    public override bool GetMouseButtonDown(int button) => false;
    public override bool GetMouseButtonUp(int button) => false;
    public override bool GetMouseButton(int button) => false;
}
