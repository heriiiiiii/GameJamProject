using UnityEngine;
using System.Collections;

public class HelpTrigger : MonoBehaviour
{
    public GameObject panelToShow;

    // 🔥 Ahora con las nuevas opciones
    public enum CloseKeyOption { LeftOrRight, X, Z, F, C, AnyKey }
    public CloseKeyOption closeKeyOption;

    [Header("Tiempo extra antes de poder cerrar (además de la animación)")]
    public float extraWaitTime = 1.5f;

    private CA_PlayerController playerMovement;
    private Rigidbody2D rb;
    private Animator anim;
    private UITutorialSequence seq;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            panelToShow.SetActive(true);

            playerMovement = other.GetComponent<CA_PlayerController>();
            rb = other.GetComponent<Rigidbody2D>();
            anim = other.GetComponentInChildren<Animator>();

            // Congelar Player
            if (playerMovement != null) playerMovement.enabled = false;
            if (rb != null) rb.velocity = Vector2.zero;
            if (anim != null) anim.SetFloat("Speed", 0);

            seq = panelToShow.GetComponent<UITutorialSequence>();

            float totalTime =
                seq.panelFadeTime +
                seq.backgroundFadeTime +
                (seq.hongosFadeTime * 2f) +
                seq.keysFadeTime +
                extraWaitTime;

            StartCoroutine(WaitAndClose(totalTime));
        }
    }

    IEnumerator WaitAndClose(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // 🔹 Aquí evaluamos qué tecla debe cerrar el panel:
        switch (closeKeyOption)
        {
            case CloseKeyOption.LeftOrRight:
                while (!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow))
                    yield return null;
                break;

            case CloseKeyOption.X:
                while (!Input.GetKeyDown(KeyCode.X))
                    yield return null;
                break;

            case CloseKeyOption.Z:
                while (!Input.GetKeyDown(KeyCode.Z))
                    yield return null;
                break;

            case CloseKeyOption.F:
                while (!Input.GetKeyDown(KeyCode.F))
                    yield return null;
                break;

            case CloseKeyOption.C:
                while (!Input.GetKeyDown(KeyCode.C))
                    yield return null;
                break;

            case CloseKeyOption.AnyKey:
                while (!Input.anyKeyDown) // cualquier tecla
                    yield return null;
                break;
        }

        // Descongelar jugador
        if (playerMovement != null) playerMovement.enabled = true;

        // Cerrar panel
        panelToShow.SetActive(false);

        // Eliminar trigger
        Destroy(gameObject);
    }
}
