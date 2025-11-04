using UnityEngine;

public class EstadoCorte : StateMachineBehaviour
{
    private CA_HongoCaballero enemigo;
    private bool haAplicadoDaño;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemigo = animator.GetComponent<CA_HongoCaballero>();
        haAplicadoDaño = false;
        enemigo.SetPuedeAtacar(false);
        enemigo.CrearEfectoSlash();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemigo == null) return;

        if (!haAplicadoDaño && stateInfo.normalizedTime >= 0.4f)
        {
            // APLICAR DAÑO CON MÁS ALCANCE
            AplicarCorteConAlcance();
            haAplicadoDaño = true;
        }

        if (stateInfo.normalizedTime >= 0.9f)
        {
            enemigo.IncrementarContadorCortes();
            enemigo.SetPuedeAtacar(true);
        }
    }

    private void AplicarCorteConAlcance()
    {
        // Punto de golpe MÁS ALEJADO del enemigo
        Vector2 puntoGolpe = (Vector2)enemigo.transform.position +
                            (enemigo.mirandoDerecha ? Vector2.right : Vector2.left) * 2.5f;

        // Radio MÁS GRANDE para el área de daño
        Collider2D[] objetivos = Physics2D.OverlapCircleAll(puntoGolpe, 2.2f);

        foreach (Collider2D objetivo in objetivos)
        {
            if (objetivo.CompareTag("Player"))
            {
                NF_PlayerHealth salud = objetivo.GetComponent<NF_PlayerHealth>();
                if (salud != null)
                {
                    salud.TakeDamage(enemigo.danoCorte);

                    Rigidbody2D rb = objetivo.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direccionEmpuje = (objetivo.transform.position - enemigo.transform.position).normalized;
                        direccionEmpuje.y = 0.2f;
                        rb.AddForce(direccionEmpuje * (enemigo.fuerzaEmpuje * 0.3f), ForceMode2D.Impulse);
                    }
                }
            }
        }

        // Debug visual del área de ataque
        Debug.DrawRay(enemigo.transform.position,
                     (enemigo.mirandoDerecha ? Vector2.right : Vector2.left) * 2.5f,
                     Color.red, 1f);
    }
}