using System.Collections;
using UnityEngine;
using TMPro;

public class IM_NPCs : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;

    [Header("Decoración")]
    [SerializeField] private GameObject dialogueMask;

    [Header("Indicadores de distancia")]
    [SerializeField] private GameObject arrowIndicator;
    [SerializeField] private Transform player;
    [SerializeField] private float arrowRange = 4f;
    [SerializeField] private float talkRange = 2f;

    [Header("Tiempos y velocidad")]
    [SerializeField] private float typingTime = 0.05f;

    [Header("Posición del Panel")]
    [SerializeField] private Vector3 panelOffset = new Vector3(0, 2f, 0);

    private bool isPlayerInRange;
    public bool didDialogueStart { get; private set; }
    private int lineIndex;

    private CA_PlayerController playerMovement;

    // Seguimiento UI
    private RectTransform panelRect;
    private Canvas parentCanvas;
    private Camera mainCam;

    void Awake()
    {
        panelRect = dialoguePanel.GetComponent<RectTransform>();
        parentCanvas = dialoguePanel.GetComponentInParent<Canvas>();
        mainCam = Camera.main;

        dialoguePanel.SetActive(false); // Se asegura que no arranque visible
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Mostrar indicadores solo si NO estamos en diálogo
        if (!didDialogueStart)
        {
            if (distance > arrowRange)
                SetIndicatorState(false, false, false);
            else if (distance <= arrowRange && distance > talkRange)
                SetIndicatorState(true, false, false);
            else if (distance <= talkRange)
                SetIndicatorState(false, true, true);
        }

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.G))
        {
            if (!didDialogueStart)
                StartDialogue();
            else if (dialogueText.text == dialogueLines[lineIndex])
                NextDialogueLine();
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[lineIndex];
            }
        }

        // 🔹 Hace que el panel SIGA al NPC solo cuando el diálogo está activo
        if (didDialogueStart)
            FollowPanelPosition();
    }

    void FollowPanelPosition()
    {
        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + panelOffset);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPos,
            parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        panelRect.localPosition = localPoint;
    }


    private void StartDialogue()
    {
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
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
            StartCoroutine(ShowLine());
        else
        {
            didDialogueStart = false;
            dialoguePanel.SetActive(false);

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= talkRange)
                SetIndicatorState(false, true, true);
            else if (distance <= arrowRange)
                SetIndicatorState(true, false, false);
            else
                SetIndicatorState(false, false, false);

            if (playerMovement != null)
                playerMovement.enabled = true;
        }
    }


    private IEnumerator ShowLine()
    {
        dialogueText.text = "";
        foreach (var letter in dialogueLines[lineIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingTime);
        }
    }


    private void SetIndicatorState(bool arrow, bool mark, bool mask)
    {
        if (arrowIndicator != null) arrowIndicator.SetActive(arrow);
        if (dialogueMark != null) dialogueMark.SetActive(mark);
        if (dialogueMask != null) dialogueMask.SetActive(mask);
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
