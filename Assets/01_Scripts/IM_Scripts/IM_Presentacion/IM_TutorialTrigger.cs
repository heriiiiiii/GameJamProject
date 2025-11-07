using System.Collections;
using UnityEngine;
using TMPro;

public class IM_TutorialTrigger : MonoBehaviour
{
    [Header("Referencias del Panel")]
    public GameObject panel;
    public TMP_Text tutorialText;
    [TextArea(3, 6)]
    public string[] Lines;
    public float typingTime = 0.04f;

    [Header("Tipo de cierre")]
    [Tooltip("Usa 'Horizontal' para movimiento, o una tecla como 'z' o 'x'")]
    public string closeKey = "Horizontal";

    private bool isOpen;
    private CA_PlayerController playerMovement;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerMovement = collision.GetComponent<CA_PlayerController>();
            ShowPanel();
        }
    }

    void ShowPanel()
    {
        // Mostrar panel
        panel.SetActive(true);

        // Congelar movimiento como en tu NPC
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Tipado
        StartCoroutine(TypeLine());
        isOpen = true;
    }

    IEnumerator TypeLine()
    {
        tutorialText.text = "";
        foreach (string line in Lines)
        {
            foreach (char c in line)
            {
                tutorialText.text += c;
                yield return new WaitForSeconds(typingTime);
            }
            tutorialText.text += "\n";
        }
    }

    void Update()
    {
        if (!isOpen) return;

        // Panel de MOVIMIENTO → detectamos flechas
        if (closeKey == "Horizontal")
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
                ClosePanel();
        }
        else
        {
            // Panel de SALTO o ATAQUE (Z o X)
            if (Input.GetKeyDown(closeKey))
                ClosePanel();
        }
    }

    void ClosePanel()
    {
        panel.SetActive(false);

        // Descongelar jugador
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Destruir trigger para no repetirse
        Destroy(gameObject);
    }
}
