using System.Collections;
using UnityEngine;
using TMPro;

public class IM_NPCs : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject dialogueMark;      // Icono "Hablar"
    [SerializeField] private GameObject dialoguePanel;     // Panel del diálogo
    [SerializeField] private TMP_Text dialogueText;        // Texto del diálogo
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;

    [Header("Decoración")]
    [SerializeField] private GameObject dialogueMask;      // Ramas o efecto visual decorativo

    [Header("Indicadores de distancia")]
    [SerializeField] private GameObject arrowIndicator;    // Flecha verde sobre el NPC
    [SerializeField] private Transform player;             // Referencia al jugador
    [SerializeField] private float arrowRange = 4f;        // Distancia para mostrar la flecha
    [SerializeField] private float talkRange = 2f;         // Distancia para mostrar el "Hablar" y las ramas

    [Header("Tiempos y velocidad")]
    [SerializeField] private float typingTime = 0.05f;

    private bool isPlayerInRange;
    private bool didDialogueStart;
    private int lineIndex;

    private CA_PlayerController playerMovement; // Script de movimiento del jugador


    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 🔹 Solo se controla el rango si NO hay diálogo activo
        if (!didDialogueStart)
        {
            // Lejos → nada
            if (distance > arrowRange)
            {
                SetIndicatorState(false, false, false);
            }
            // Rango medio → solo flecha
            else if (distance <= arrowRange && distance > talkRange)
            {
                SetIndicatorState(true, false, false);
            }
            // Muy cerca → se quita flecha, aparecen hablar + ramas
            else if (distance <= talkRange)
            {
                SetIndicatorState(false, true, true);
            }
        }

        // 🔹 Control de diálogo
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
        // 🔹 Al iniciar el diálogo, ocultamos hablar, flecha y ramas
        SetIndicatorState(false, false, false);

        lineIndex = 0;

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

            // 🔹 Reactivar ramas y hablar si sigue cerca al terminar
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= talkRange)
            {
                SetIndicatorState(false, true, true);
            }
            else if (distance <= arrowRange)
            {
                SetIndicatorState(true, false, false);
            }
            else
            {
                SetIndicatorState(false, false, false);
            }

            if (playerMovement != null)
                playerMovement.enabled = true;
        }
    }


    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        foreach (var letter in dialogueLines[lineIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingTime);
        }
    }


    private void SetIndicatorState(bool arrow, bool mark, bool mask)
    {
        if (arrowIndicator != null)
            arrowIndicator.SetActive(arrow);

        if (dialogueMark != null)
            dialogueMark.SetActive(mark);

        if (dialogueMask != null)
            dialogueMask.SetActive(mask);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerMovement = collision.GetComponent<CA_PlayerController>();
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            SetIndicatorState(false, false, false);
        }
    }
}
