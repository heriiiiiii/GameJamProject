using UnityEngine;

public class EstadoPersecucion : StateMachineBehaviour
{
    private CA_HongoCaballero enemigo;
    private Transform transform;
    private float tiempoPersecucion;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemigo = animator.GetComponent<CA_HongoCaballero>();
        transform = animator.transform;
        tiempoPersecucion = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemigo == null || enemigo.jugador == null) return;

        tiempoPersecucion += Time.deltaTime;

        float distanciaAlJugador = Vector2.Distance(transform.position, enemigo.jugador.position);

        // MANTENER DISTANCIA ÓPTIMA PARA EL CORTE (1.5f - 2f unidades de distancia)
        float distanciaOptima = 1.8f;

        if (distanciaAlJugador > distanciaOptima)
        {
            // Acercarse al jugador
            Vector3 direccion = (enemigo.jugador.position - transform.position).normalized;
            transform.position += direccion * enemigo.velocidadPatrulla * 1.5f * Time.deltaTime;
        }
        else if (distanciaAlJugador < distanciaOptima - 0.5f)
        {
            // Alejarse un poco si está demasiado cerca
            Vector3 direccion = (transform.position - enemigo.jugador.position).normalized;
            transform.position += direccion * enemigo.velocidadPatrulla * 0.8f * Time.deltaTime;
        }

        // Si pasa mucho tiempo persiguiendo sin poder atacar, volver a patrullar
        if (tiempoPersecucion > 5f)
        {
            animator.SetBool("JugadorEnRango", false);
        }
    }
}