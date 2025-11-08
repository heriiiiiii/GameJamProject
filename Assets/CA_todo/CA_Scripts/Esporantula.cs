using System.Collections;
using UnityEngine;

public class Esporantula : MonoBehaviour
{
    [Header("Puntos de salto")]
    public Transform[] puntos;
    public float velocidadSalto = 8f;
    public float tiempoEntreSaltos = 1f;
    public float tiempoPreparacionSalto = 1.5f; // Tiempo más largo para preparación
    public float tiempoPreparacionAtaque = 1f; // 1 segundo antes de atacar al jugador

    [Header("Jugador y detección")]
    public float rangoDeteccion = 5f;
    public string tagJugador = "Player";

    [Header("Mordisco")]
    public int danoMordisco = 1;
    public float knockbackForce = 5f;
    public int saltosParaAtacar = 2;

    [Header("Efecto Tela de Araña")]
    public LineRenderer lineRendererTela;
    public float anchoTela = 0.05f;
    public Color colorTela = new Color(1f, 1f, 1f, 0.7f);
    public float tiempoMostrarTela = 0.3f;

    // Componentes
    private Animator animator;
    private bool saltando = false;
    private Transform jugador;
    private bool jugadorDetectado = false;
    private int contadorSaltos = 0;
    private Vector3 destinoActual;
    private Vector3 escalaOriginal;

    // Parámetros Animator
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsLanding = Animator.StringToHash("IsLanding");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsJumpIdle = Animator.StringToHash("IsJumpIdle"); // Preparación de salto
    private static readonly int IsDetectingPlayer = Animator.StringToHash("IsDetectingPlayer"); // Nueva animación cuando detecta jugador

    // Control de movimiento
    private Vector3 posicionInicialSalto;
    private float duracionSaltoActual;
    private float tiempoTranscurridoSalto;
    private float alturaMaximaSalto;
    private bool movimientoActivo = false;
    private bool esAtaqueAlJugador = false;
    private bool preparandoAtaque = false;

    private bool estaMuerto = false;

    // Parámetros Animator - SIMPLIFICADOS
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsDead = Animator.StringToHash("IsDead"); // NUEVO
    private static readonly int IsAttackingPlayer = Animator.StringToHash("IsAttackingPlayer"); // HV_Sporantulaatac

    void Start()
    {
        animator = GetComponent<Animator>();
        escalaOriginal = transform.localScale;
        ConfigurarLineRenderer();

        //  Bloquear rotación para que no gire visualmente
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            rb.freezeRotation = true;

        ResetearAnimaciones();
        StartCoroutine(MovimientoAleatorio());
    }


    void ResetearAnimaciones()
    {
        animator.SetBool(IsIdle, true);      // HV_Sporantulaidle (INICIO EN IDLE)
        animator.SetBool(IsJumping, false);  // HV_Sporantulajump
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsDead, false);
        animator.SetBool(IsAttackingPlayer, false);  // HV_Sporantulaatac - NUEVO
    }



    void ConfigurarLineRenderer()
    {
        if (lineRendererTela == null)
        {
            GameObject telaObj = new GameObject("TelaArana");
            telaObj.transform.SetParent(transform);
            telaObj.transform.localPosition = Vector3.zero;
            lineRendererTela = telaObj.AddComponent<LineRenderer>();
        }

        lineRendererTela.startWidth = anchoTela;
        lineRendererTela.endWidth = anchoTela;
        lineRendererTela.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererTela.startColor = colorTela;
        lineRendererTela.endColor = colorTela;
        lineRendererTela.positionCount = 2;
        lineRendererTela.SetPosition(0, transform.position);
        lineRendererTela.SetPosition(1, transform.position);
        lineRendererTela.enabled = false;
    }

    void Update()
    {
        DetectarJugador();
        MirarJugador();

        // Ajusta rotación según superficie
        AjustarRotacionSuperficie();

        if (movimientoActivo)
            EjecutarMovimientoSalto();
    }

    void AjustarRotacionSuperficie()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            // Rotación según normal
            Vector2 normal = hit.normal;
            float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    void EjecutarMovimientoSalto()
    {
        tiempoTranscurridoSalto += Time.deltaTime;
        float progreso = tiempoTranscurridoSalto / duracionSaltoActual;

        if (progreso >= 1f)
        {
            // Salto completado
            transform.position = destinoActual;
            movimientoActivo = false;

            // DESACTIVAR HV_Sporantulajump SOLO AL FINAL
            animator.SetBool(IsJumping, false);  // <-- SE DESACTIVA AL ATERRIZAR
            animator.SetBool(IsIdle, true);
            animator.SetBool(IsAttacking, false);
            animator.SetBool(IsAttackingPlayer, false);

            StartCoroutine(CompletarAterrizaje());
            return;
        }

        // Calcular posición durante el salto
        Vector3 posicionHorizontal = Vector3.Lerp(posicionInicialSalto, destinoActual, progreso);
        float altura = Mathf.Sin(progreso * Mathf.PI) * alturaMaximaSalto;
        transform.position = new Vector3(posicionHorizontal.x, posicionHorizontal.y + altura, posicionHorizontal.z);

        // NO CAMBIAR ANIMACIONES DURANTE EL SALTO - HV_Sporantulajump SE MANTIENE ACTIVO
        // La animación de salto ya está activa desde IniciarSalto
    }

    // MÉTODO PARA ACTIVAR LA MUERTE
    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;

        // Detener todas las corrutinas
        StopAllCoroutines();

        // Activar animación de muerte y desactivar todas las demás
        animator.SetBool(IsIdle, false);
        animator.SetBool(IsJumping, false);
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsAttackingPlayer, false); // DESACTIVAR ATAQUE AL JUGADOR
        animator.SetBool(IsDead, true);

        // Desactivar colisiones y movimiento
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        movimientoActivo = false;
        saltando = false;

        Debug.Log(" Esporantula murió");
    }

    IEnumerator CompletarAterrizaje()
    {
        // Pequeña pausa para mostrar la transición
        yield return new WaitForSeconds(0.1f);

        // Asegurar que todas las animaciones estén desactivadas correctamente
        animator.SetBool(IsJumping, false);  // <-- CONFIRMAR QUE JUMP ESTÉ APAGADO
        animator.SetBool(IsAttackingPlayer, false);
        animator.SetBool(IsAttacking, false);
        animator.SetBool(IsIdle, true);      // <-- ACTIVAR IDLE

        saltando = false;
        esAtaqueAlJugador = false;
    }

    void IniciarSalto(Vector3 destino, bool esAtaque)
    {
        posicionInicialSalto = transform.position;
        float distanciaTotal = Vector3.Distance(posicionInicialSalto, destino);

        duracionSaltoActual = distanciaTotal / velocidadSalto;
        tiempoTranscurridoSalto = 0f;
        alturaMaximaSalto = Mathf.Min(distanciaTotal * 0.3f, 2f);

        movimientoActivo = true;

        // ANIMACIÓN: ACTIVAR HV_Sporantulajump INMEDIATAMENTE AL INICIAR EL SALTO
        animator.SetBool(IsIdle, false);
        animator.SetBool(IsJumping, true);  // <-- ESTA LÍNEA SE ACTIVA AL INICIO
        animator.SetBool(IsAttacking, esAtaque);

        // ACTIVAR ANIMACIÓN DE ATAQUE AL JUGADOR SI ES ATAQUE
        if (esAtaque)
        {
            animator.SetBool(IsAttackingPlayer, true);
        }
    }

    void DetectarJugador()
    {
        bool jugadorDetectadoAnterior = jugadorDetectado;
        jugadorDetectado = false;

        Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion);
        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag(tagJugador))
            {
                jugadorDetectado = true;
                jugador = col.transform;

                // Si acaba de detectar al jugador y no está preparando ataque
                if (!jugadorDetectadoAnterior && !preparandoAtaque && !saltando)
                {
                    StartCoroutine(PrepararAtaque());
                }
                break;
            }
        }

        // Si perdió de vista al jugador
        if (jugadorDetectadoAnterior && !jugadorDetectado)
        {
            animator.SetBool(IsDetectingPlayer, false);
            preparandoAtaque = false;
        }
    }

    IEnumerator PrepararAtaque()
    {
        if (preparandoAtaque || saltando) yield break;

        preparandoAtaque = true;

        // Activar animación HV_Sporantulalanding cuando detecta al jugador
        animator.SetBool(IsDetectingPlayer, true);
        Debug.Log("🎯 Jugador detectado - Preparando ataque...");

        // Esperar 1 segundo antes de atacar
        yield return new WaitForSeconds(tiempoPreparacionAtaque);

        // Forzar ataque inmediato
        if (jugadorDetectado && !saltando)
        {
            contadorSaltos = saltosParaAtacar; // Forzar ataque en el próximo movimiento
            Debug.Log("⚡ Ataque forzado al jugador!");
        }

        preparandoAtaque = false;
        animator.SetBool(IsDetectingPlayer, false);
    }

    void MirarJugador()
    {
        if (jugador != null && !saltando)
        {
            if (jugador.position.x < transform.position.x)
                transform.localScale = new Vector3(-escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
            else
                transform.localScale = new Vector3(escalaOriginal.x, escalaOriginal.y, escalaOriginal.z);
        }
        else if (!saltando)
        {
            transform.localScale = escalaOriginal;
        }
    }

    IEnumerator MovimientoAleatorio()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (!saltando && !movimientoActivo && !preparandoAtaque && puntos.Length > 0)
            {
                Transform destino;
                bool esAtaque = false;

                // Lógica de patrón: 2 saltos normales + 1 ataque
                if (jugadorDetectado && contadorSaltos >= saltosParaAtacar && jugador != null)
                {
                    destino = jugador;
                    contadorSaltos = 0;
                    esAtaque = true;
                    esAtaqueAlJugador = true;
                    Debug.Log(" 🎯 Atacando al jugador! - Contador: " + contadorSaltos);
                }
                else
                {
                    destino = ObtenerPuntoAleatorio();
                    contadorSaltos++;
                    esAtaqueAlJugador = false;
                    Debug.Log(" 🕷️ Saltando a punto aleatorio - Contador: " + contadorSaltos);
                }

                saltando = true;

                // FASE 1: PREPARACIÓN DEL SALTO (HV_Sporantulajumpidle)
                yield return StartCoroutine(PrepararSalto());

                // FASE 2: MOSTRAR TELA DE ARAÑA
                yield return StartCoroutine(MostrarTelaArana(destino.position));

                // FASE 3: INICIAR SALTO CON ANIMACIONES
                destinoActual = destino.position;
                IniciarSalto(destinoActual, esAtaque);

                // FASE 4: ESPERAR A QUE TERMINE EL MOVIMIENTO
                yield return new WaitUntil(() => !movimientoActivo);

                // FASE 5: DESCANSO ENTRE SALTOS
                yield return new WaitForSeconds(tiempoEntreSaltos);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator PrepararSalto()
    {
        // Activar animación HV_Sporantulajumpidle (preparación de salto)
        animator.SetBool(IsJumpIdle, true);
        Debug.Log(" 🕸️ Preparando salto...");

        // Esperar tiempo de preparación (más largo)
        yield return new WaitForSeconds(tiempoPreparacionSalto);

        animator.SetBool(IsJumpIdle, false);
    }

    IEnumerator MostrarTelaArana(Vector3 destino)
    {
        lineRendererTela.enabled = true;

        Vector3 puntoInicio = transform.position;
        Vector3 puntoFinal = destino;
        Vector3 puntoControl = (puntoInicio + puntoFinal) / 2 + Vector3.up * 1.5f;

        lineRendererTela.positionCount = 15;

        for (int i = 0; i < 15; i++)
        {
            float t = i / 14f;
            Vector3 puntoCurva = CalcularCurvaBezier(puntoInicio, puntoControl, puntoFinal, t);
            lineRendererTela.SetPosition(i, puntoCurva);
        }

        float tiempo = 0f;
        while (tiempo < tiempoMostrarTela)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        lineRendererTela.enabled = false;
    }

    Vector3 CalcularCurvaBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float tt = t * t;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    Transform ObtenerPuntoAleatorio()
    {
        if (puntos.Length == 0) return null;

        Transform puntoSeleccionado;
        int intentos = 0;

        do
        {
            puntoSeleccionado = puntos[Random.Range(0, puntos.Length)];
            intentos++;

            if (intentos >= 5) break;

        } while (Vector3.Distance(transform.position, puntoSeleccionado.position) < 0.5f);

        return puntoSeleccionado;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagJugador) && !animator.GetBool(IsLanding))
        {
            PlayerHealth salud = collision.gameObject.GetComponent<PlayerHealth>();
            if (salud != null) salud.RecibirDanio(danoMordisco);

            Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direccion = (collision.transform.position - transform.position).normalized;
                rbPlayer.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
    

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Dibujar línea hacia el jugador si está detectado
        if (jugador != null && jugadorDetectado)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, jugador.position);
        }
    }


}