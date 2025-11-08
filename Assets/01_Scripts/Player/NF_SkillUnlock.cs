using UnityEngine;

public class NF_SkillUnlock : MonoBehaviour
{
    public enum SkillType { Dash, WallJump, DoubleJump }
    public SkillType skillToUnlock;

    private bool playerInRange = false;
    private CA_PlayerController player;

    [Header("Opcional: mensaje o efecto")]
    public GameObject interactHint; // texto o icono de "Presiona G"

    void Update()
    {
        // Si el jugador está dentro y presiona G
        if (playerInRange && Input.GetKeyDown(KeyCode.G))
        {
            UnlockSkill();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.GetComponent<CA_PlayerController>();

            if (interactHint != null)
                interactHint.SetActive(true); // Mostrar texto de "Presiona G"
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;

            if (interactHint != null)
                interactHint.SetActive(false); // Ocultar texto
        }
    }

    private void UnlockSkill()
    {
        if (player == null) return;

        switch (skillToUnlock)
        {
            case SkillType.Dash:
                player.canUseDash = true;
                break;
            case SkillType.WallJump:
                player.canUseWallJump = true;
                break;
            case SkillType.DoubleJump:
                player.canUseDoubleJump = true;
                break;
        }

        Debug.Log($"🧩 Habilidad desbloqueada: {skillToUnlock}");

        if (interactHint != null)
            interactHint.SetActive(false);

        Destroy(gameObject);
    }
}
