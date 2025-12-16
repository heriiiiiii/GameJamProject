using UnityEngine;

public class CA_ataque : StateMachineBehaviour
{
    private BrotePanico enemigo;
    private bool disparoEjecutado = false;

    // Se llama una vez al entrar al estado de ataque
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemigo == null)
            enemigo = animator.GetComponent<BrotePanico>();

        disparoEjecutado = false;
    }

    // Se llama en cada frame mientras la animación está activa
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemigo == null) return;

        // Dispara justo al final de la animación
        if (!disparoEjecutado && stateInfo.normalizedTime >= 0.95f)
        {
            enemigo.Disparar();
            disparoEjecutado = true;
        }

        // Si la animación terminó (1 ciclo completo)
        if (stateInfo.normalizedTime >= 1f)
        {
            // Si el jugador sigue en rango  reinicia ataque
            if (enemigo.jugadorDetectado)
            {
                animator.Play(stateInfo.shortNameHash, layerIndex, 0f);
                disparoEjecutado = false;
            }
            else
            {
                // Si el jugador ya no está  volver a idle
                animator.SetBool("IsAttacking", false);
                animator.SetBool("IsMoving", false);
                animator.SetBool("PlayerDetected", false);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        disparoEjecutado = false;
    }
}
