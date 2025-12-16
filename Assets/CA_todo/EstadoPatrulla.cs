using UnityEngine;

public class EstadoPatrulla : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // El movimiento se maneja completamente en CA_HongoCaballero
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // No se necesita lógica adicional aquí
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Limpieza si es necesaria
    }
}