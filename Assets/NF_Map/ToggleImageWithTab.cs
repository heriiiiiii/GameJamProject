using UnityEngine;

public class ToggleImageWithTab : MonoBehaviour
{
    [SerializeField] private GameObject imageUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            imageUI.SetActive(!imageUI.activeSelf);
        }
    }
}
