using System.Collections;
using UnityEngine;

public class EstadoEstocada : StateMachineBehaviour
{
    private CA_HongoCaballero enemigo;
    private Transform transform;
    private Vector3 posicionInicial;
    private Vector3 posicionObjetivo;
    private bool haAplicadoDaño;
    private bool haGeneradoPuas;

    // Altura fija para la estocada
    private float alturaSalto = 3f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemigo = animator.GetComponent<CA_HongoCaballero>();
        transform = animator.transform;
        posicionInicial = transform.position;
        haAplicadoDaño = false;
        haGeneradoPuas = false;
        enemigo.SetPuedeAtacar(false);

        // INICIAR EFECTO DASH TRAIL INMEDIATAMENTE
        if (enemigo.dashTrailEffect != null)
        {
            enemigo.dashTrailEffect.StartTrail();
            Debug.Log("🌈 Efecto Dash Trail ACTIVADO");
        }
        else
        {
            Debug.LogWarning("❌ DashTrailEffect no encontrado");
        }

        // CALCULAR POSICIÓN OBJETIVO
        if (enemigo.jugador != null)
        {
            posicionObjetivo = new Vector3(enemigo.jugador.position.x, posicionInicial.y, enemigo.jugador.position.z);

            bool deberiaMirarDerecha = posicionObjetivo.x > transform.position.x;
            if (deberiaMirarDerecha != enemigo.mirandoDerecha)
            {
                enemigo.Voltear();
            }
        }
        else
        {
            posicionObjetivo = transform.position + (enemigo.mirandoDerecha ? Vector3.right : Vector3.left) * 6f;
        }

        // Crear efecto de estocada al inicio
        enemigo.CrearEfectoEstocada();

        enemigo.StartCoroutine(EjecutarEstocadaBrutal());
    }

    private IEnumerator EjecutarEstocadaBrutal()
    {
        // FASE 1: SALTO VERTICAL BRUTAL (más rápido)
        float alturaMaxima = alturaSalto * 1.5f;
        Vector3 posicionAlta = posicionInicial + Vector3.up * alturaMaxima;
        float tiempoSalto = 0f;
        float duracionSalto = 0.2f; // Más rápido

        while (tiempoSalto < duracionSalto)
        {
            float progreso = tiempoSalto / duracionSalto;
            float curva = Mathf.Pow(progreso, 0.5f);
            transform.position = Vector3.Lerp(posicionInicial, posicionAlta, curva);

            tiempoSalto += Time.deltaTime;
            yield return null;
        }

        // FASE 2: CAÍDA EN PICADA HACIA EL OBJETIVO (más rápido y agresivo)
        float tiempoCaida = 0f;
        float duracionCaida = 0.3f; // Más rápido
        Vector3 posicionInicioCaida = transform.position;
        Vector3 posicionAltaCaida = posicionInicioCaida + Vector3.up * (alturaMaxima * 0.3f);

        while (tiempoCaida < duracionCaida)
        {
            float progreso = tiempoCaida / duracionCaida;
            float curvaCaida = Mathf.Pow(progreso, 2f); // Más agresivo

            Vector3 posicionHorizontal = Vector3.Lerp(posicionInicioCaida, posicionObjetivo, progreso);
            float altura = Mathf.Lerp(posicionAltaCaida.y, posicionObjetivo.y, curvaCaida);

            transform.position = new Vector3(posicionHorizontal.x, altura, posicionHorizontal.z);

            // DETECTAR IMPACTO CON EL SUELO PARA GENERAR PÚAS
            if (!haGeneradoPuas && altura <= posicionObjetivo.y + 0.1f)
            {
                GenerarPuasNegras();
                haGeneradoPuas = true;
            }

            // Aplicar daño cuando está cerca del player (más agresivo)
            if (!haAplicadoDaño && enemigo.jugador != null)
            {
                float distanciaAlJugador = Vector2.Distance(transform.position, enemigo.jugador.position);
                if (distanciaAlJugador < 3f) // Rango mayor de daño
                {
                    enemigo.AplicarDañoEstocada();
                    enemigo.CrearEfectoSlash();
                    haAplicadoDaño = true;
                    Debug.Log("💥 Daño de estocada aplicado");
                }
            }

            tiempoCaida += Time.deltaTime;
            yield return null;
        }

        // FASE 3: GARANTIZAR QUE LAS PÚAS SE GENEREN
        if (!haGeneradoPuas)
        {
            GenerarPuasNegras();
            haGeneradoPuas = true;
        }

        // Asegurar posición final
        transform.position = posicionObjetivo;

        // SLASH FINAL GARANTIZADO
        if (!haAplicadoDaño)
        {
            enemigo.AplicarDañoEstocada();
            enemigo.CrearEfectoSlash();
            Debug.Log("💥 Daño de estocada aplicado (final)");
        }

        // FASE 4: RECUPERACIÓN BREVE
        yield return new WaitForSeconds(0.2f); // Más rápido

        // DETENER EFECTO DASH TRAIL
        if (enemigo.dashTrailEffect != null)
        {
            enemigo.dashTrailEffect.StopTrail();
            Debug.Log("🌈 Efecto Dash Trail DETENIDO");
        }

        // FINALIZAR
        enemigo.ReiniciarContadorCortes();
        enemigo.SetPuedeAtacar(true);
    }

    private void GenerarPuasNegras()
    {
        // Crear púas negras directamente sin corrutina
        GameObject efectoPuas = new GameObject("EfectoPuasNegras");
        efectoPuas.transform.position = transform.position;

        LineRenderer lineRenderer = efectoPuas.AddComponent<LineRenderer>();

        // Configurar el LineRenderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        lineRenderer.startWidth = 0.4f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 16; // 8 púas

        // Generar las púas
        Vector3[] puntosPuas = GenerarPuas(8, 2.5f, 1.2f);
        lineRenderer.SetPositions(puntosPuas);

        // Destruir después de 1 segundo
        Destroy(efectoPuas, 1f);

        Debug.Log("⚫ Púas negras generadas");
    }

    private Vector3[] GenerarPuas(int cantidadPuas, float radio, float alturaMaxima)
    {
        Vector3[] puntos = new Vector3[cantidadPuas * 2];
        float anguloPorPua = 360f / cantidadPuas;

        for (int i = 0; i < cantidadPuas; i++)
        {
            float angulo = i * anguloPorPua * Mathf.Deg2Rad;
            Vector3 direccion = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));

            // Base
            int indiceBase = i * 2;
            puntos[indiceBase] = transform.position + direccion * (radio * 0.7f);
            puntos[indiceBase].y = transform.position.y;

            // Punta
            int indicePunta = i * 2 + 1;
            float alturaVariada = alturaMaxima * Random.Range(0.6f, 1.4f);
            float radioVariado = radio * Random.Range(0.7f, 1.3f);
            float inclinacion = Random.Range(-0.2f, 0.2f);

            puntos[indicePunta] = transform.position + direccion * radioVariado;
            puntos[indicePunta].y = transform.position.y + alturaVariada;

            Vector3 direccionLateral = Vector3.Cross(direccion, Vector3.up);
            puntos[indicePunta] += direccionLateral * inclinacion;
        }

        return puntos;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemigo != null)
        {
            // GARANTIZAR QUE EL DASH TRAIL SE DETENGA
            if (enemigo.dashTrailEffect != null)
            {
                enemigo.dashTrailEffect.StopTrail();
            }

            enemigo.StopAllCoroutines();
            enemigo.ReiniciarContadorCortes();
            enemigo.SetPuedeAtacar(true);
        }
    }
}