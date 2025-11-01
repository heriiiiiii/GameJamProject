using System.Collections;
using TMPro;
using UnityEngine;

public class NF_DialogueNPC : MonoBehaviour
{
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;

    private bool isPlayerInRange;
    private bool didDialogueStart;
    private int lineIndex;

    private CA_PlayerController playerMovement; // referencia al script de movimiento del jugador

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (!didDialogueStart)
            {
                StartDialogue();
            }
            else if (dialogueText.text == dialogueLines[lineIndex])
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[lineIndex];
            }
        }
    }

    private void StartDialogue()
    {
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(false);
        lineIndex = 0;

        // 🔹 Desactivar movimiento del jugador
        if (playerMovement != null)
            playerMovement.enabled = false;

        StartCoroutine(ShowLine());
    }

    private void NextDialogueLine()
    {
        lineIndex++;
        if (lineIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            didDialogueStart = false;
            dialoguePanel.SetActive(false);
            dialogueMark.SetActive(true);

            // 🔹 Reactivar movimiento del jugador
            if (playerMovement != null)
                playerMovement.enabled = true;
        }
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        foreach (var line in dialogueLines[lineIndex])
        {
            dialogueText.text += line;
            yield return new WaitForSeconds(typingTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            dialogueMark.SetActive(true);

            // 🔹 Buscar el script de movimiento del jugador
            playerMovement = collision.GetComponent<CA_PlayerController>();
            if (playerMovement == null)
            {
                // Si usas un script con nombre distinto (por ejemplo "RigibodyMovement" o "PlayerMovement")
                playerMovement = collision.GetComponent<CA_PlayerController>(); // cambia este nombre si tu script se llama diferente
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            dialogueMark.SetActive(false);
        }
    }
}
