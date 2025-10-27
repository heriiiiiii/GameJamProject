using UnityEngine;
using UnityEngine.EventSystems;

public class MenuFocus : MonoBehaviour
{
    public GameObject firstButton;

    void Start()
    {
        // Selecciona el primer botón al iniciar
        EventSystem.current.SetSelectedGameObject(firstButton);

        // 🔒 Bloquea completamente el mouse
        Cursor.visible = false;                       // Oculta el cursor
        Cursor.lockState = CursorLockMode.Locked;     // Lo bloquea en el centro de la pantalla

        // 🔧 Desactiva raycasts de mouse sobre los botones UI
        DisableMouseInput();
    }

    private void DisableMouseInput()
    {
        // Desactiva todos los módulos de input de ratón del EventSystem
        var inputModules = FindObjectsOfType<BaseInputModule>();
        foreach (var module in inputModules)
        {
            if (module is StandaloneInputModule sim)
            {
                sim.inputOverride = new KeyboardOnlyInput();
            }
        }
    }
}

// 🧠 Clase auxiliar que ignora el mouse completamente
public class KeyboardOnlyInput : BaseInput
{
    public override bool mousePresent => false;
    public override Vector2 mousePosition => Vector2.zero;
    public override bool GetMouseButtonDown(int button) => false;
    public override bool GetMouseButtonUp(int button) => false;
    public override bool GetMouseButton(int button) => false;
}

