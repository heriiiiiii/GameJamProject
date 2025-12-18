using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public GameObject mapPanel;

    void Start()
    {
        mapPanel.SetActive(false); // Oculto al iniciar
    }

    public void ToggleMap()
    {
        mapPanel.SetActive(!mapPanel.activeSelf);
    }
}
