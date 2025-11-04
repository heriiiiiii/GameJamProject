using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_MiniBossVigiasEsporales : MonoBehaviour
{
    [System.Serializable]
    public class HongoData
    {
        public GameObject hongo;
        public GameObject hongoDormido; // AÑADIR: Prefab del hongo dormido
        public float vida;
        public float vidaMaxima = 100f;
        public bool estaVivo = true;
        public int tipoAtaque;
        public Animator animator;
        public bool estaActivo = false; // AÑADIR: Estado de activación
    }

    [Header("Configuración de Animaciones")]
    public float duracionAnimacionAtaque = 1.5f;
    public float duracionAnimacionRetorno = 1f;

    [Header("Rotación Visual")]
    public bool usarRotacionVisual = false; // CAMBIADO: Desactivado por defecto

    [Header("Separación entre Hongos")]
    public float distanciaMinimaSeparacion = 2.5f;
    public float fuerzaSeparacion = 5f;

    private Vector3[] direccionesMovimiento = new Vector3[3];

    [Header("Configuración Hongos")]
    public HongoData[] hongos = new HongoData[3];
    public float velocidadAtaqueNormal = 2f;
    public float velocidadAtaqueFusion = 1f;

    [Header("Estados")]
    public bool estaDespierto = false;
    public bool enMovimientoEvasivo = false;
    public int hongosVivos = 3;
    public float tiempoActivacion = 1f; // AÑADIR: Tiempo para activarse

    [Header("Movimiento Evasivo Zig-Zag")]
    public float rangoMovimientoX = 3f;
    public float rangoMovimientoY = 2f;
    public float velocidadMovimiento = 3f;
    public float frecuenciaCambioDireccion = 2f;
    public float duracionMovimientoEvasivo = 6f;

    [Header("Ataques")]
    public GameObject proyectilEspora;
    public Material materialHilo;
    public float tiempoEntreAtaques = 2f;
    private int ataqueActual = 0;
    private bool ataqueEnCurso = false;

    [Header("Ataque de Balas Giratorias")]
    public float radioHilos = 3f;
    public int numeroHilos = 6;
    public float velocidadRotacionHilos = 90f;
    public float velocidadHilosRectos = 8f;
    public float anchoHilo = 0.1f;
    public Color colorHilo = new Color(1f, 1f, 1f, 0.8f);
    public LayerMask layerSuelo;

    [Header("Estocada")]
    public float velocidadEstocada = 15f;
    public float duracionEstocada = 1f;
    public int danioEstocada = 1;
    public Transform[] puntosRetirada;

    [Header("Disparos Lentos")]
    public float velocidadProyectilLento = 3f;
    public float tiempoEntreDisparosLentos = 1.5f;

    [Header("Daño por Contacto")]
    public int danioContacto = 1;
    public float tiempoEntreDanioContacto = 1f;



    // VARIABLES PARA DAÑO Y VELOCIDAD ESCALABLES
    private float multiplicadorDanio = 2f;
    private float multiplicadorVelocidad = 2f;
    private int danioBaseEstocada = 1;
    private int danioBaseProyectil = 1;
    private int danioBaseHilos = 1;
    private int danioBaseContacto = 1;
    private float velocidadBaseMovimiento = 3f;
    private float velocidadBaseEstocada = 15f;
    private float velocidadBaseHilos = 8f;

    private float tiempoUltimoAtaque = 0f;
    private Vector3[] posicionesOriginales = new Vector3[3];
    private List<GameObject> hilosActivos = new List<GameObject>();
    private Transform jugador;
    private bool movimientoEvasivoActivado = false;
    private Vector3[] direccionesActuales = new Vector3[3];
    private float[] tiemposCambioDireccion = new float[3];
    private Coroutine movimientoEvasivoCoroutine;
    private Coroutine disparosLentosCoroutine;
    private float[] ultimoDanioContacto = new float[3];

    // NUEVO: Estados de animación
    private enum EstadoAnimacion
    {
        Inactivo, // AÑADIR: Estado para hongos dormidos
        Idle,
        Attacking,
        Returning,
        Dead,
        Stacked
    }

    void Start()
    {
        danioBaseEstocada = danioEstocada;
        danioBaseContacto = danioContacto;
        velocidadBaseMovimiento = velocidadMovimiento;
        velocidadBaseEstocada = velocidadEstocada;
        velocidadBaseHilos = velocidadHilosRectos;

        InicializarHongos();
        GuardarPosicionesOriginales();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;

        for (int i = 0; i < direccionesActuales.Length; i++)
        {
            direccionesActuales[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            tiemposCambioDireccion[i] = 0f;
            ultimoDanioContacto[i] = 0f;
        }
    }

    void InicializarHongos()
    {
        hongos[0].tipoAtaque = 0;
        hongos[1].tipoAtaque = 1;
        hongos[2].tipoAtaque = 1;

        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].hongo == null)
            {
                Debug.LogError($"Hongo activo {i} no asignado en inspector!");
                continue;
            }

            if (hongos[i].hongoDormido == null)
            {
                Debug.LogError($"Hongo dormido {i} no asignado en inspector!");
                continue;
            }

            hongos[i].vida = hongos[i].vidaMaxima;
            hongos[i].estaVivo = true;
            hongos[i].estaActivo = false; // Inician inactivos

            // Configurar hongo activo (inicialmente desactivado)
            hongos[i].animator = hongos[i].hongo.GetComponent<Animator>();
            if (hongos[i].animator == null)
            {
                Debug.LogWarning($"Hongo activo {i} no tiene componente Animator");
            }

            CircleCollider2D collider = hongos[i].hongo.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = hongos[i].hongo.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.8f;
            }

            SpriteRenderer sr = hongos[i].hongo.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = i == 0 ? Color.blue : (i == 1 ? Color.green : Color.red);
            }

            if (hongos[i].hongo != null)
            {
                hongos[i].hongo.transform.rotation = Quaternion.identity;
                hongos[i].hongo.SetActive(false); // Desactivar hongo activo inicialmente
            }

            // Configurar hongo dormido (inicialmente activado)
            if (hongos[i].hongoDormido != null)
            {
                hongos[i].hongoDormido.transform.position = hongos[i].hongo.transform.position;
                hongos[i].hongoDormido.transform.rotation = Quaternion.identity;
                hongos[i].hongoDormido.SetActive(true); // Activar hongo dormido inicialmente
            }

            SetAnimacionHongo(i, EstadoAnimacion.Inactivo);
        }
    }

    // Método para activar el boss completo
    public void ActivarBoss()
    {
        if (!estaDespierto)
        {
            estaDespierto = true;
            Debug.Log("¡BOSS ACTIVADO! Cambiando hongos dormidos a activos...");
            StartCoroutine(TransicionActivacion());
        }
    }

    // Corrutina para transición suave de dormido a activo
    IEnumerator TransicionActivacion()
    {
        // Primero: Efectos visuales en los hongos dormidos
        foreach (HongoData hongo in hongos)
        {
            if (hongo.hongoDormido != null)
            {
                StartCoroutine(EfectoDespertar(hongo.hongoDormido.transform));
            }
        }

        yield return new WaitForSeconds(tiempoActivacion * 0.5f);

        // Segundo: Cambiar de prefab dormido a activo
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].hongoDormido != null)
            {
                hongos[i].hongoDormido.SetActive(false);
            }

            if (hongos[i].hongo != null)
            {
                hongos[i].hongo.SetActive(true);
                hongos[i].estaActivo = true;

                // Efecto de aparición del hongo activo
                StartCoroutine(EfectoAparicion(hongos[i].hongo.transform));

                // Cambiar a estado Idle
                SetAnimacionHongo(i, EstadoAnimacion.Idle);
            }
        }

        yield return new WaitForSeconds(tiempoActivacion * 0.5f);

        // Tercero: Iniciar comportamiento normal del boss
        tiempoUltimoAtaque = Time.time;
        Debug.Log("¡BOSS COMPLETAMENTE ACTIVO! Iniciando patrones de ataque...");
    }

    // Efecto visual para despertar hongos dormidos
    IEnumerator EfectoDespertar(Transform hongoDormido)
    {
        if (hongoDormido == null) yield break;

        Vector3 escalaOriginal = hongoDormido.localScale;
        float tiempo = 0f;

        while (tiempo < tiempoActivacion * 0.5f)
        {
            float escala = escalaOriginal.x * (1f + Mathf.Sin(tiempo * 10f) * 0.2f);
            hongoDormido.localScale = new Vector3(escala, escala, escalaOriginal.z);
            tiempo += Time.deltaTime;
            yield return null;
        }

        hongoDormido.localScale = escalaOriginal;
    }

    // Efecto visual para aparición de hongos activos
    IEnumerator EfectoAparicion(Transform hongoActivo)
    {
        if (hongoActivo == null) yield break;

        Vector3 escalaFinal = hongoActivo.localScale;
        hongoActivo.localScale = Vector3.zero;
        float tiempo = 0f;

        while (tiempo < tiempoActivacion * 0.5f)
        {
            hongoActivo.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, tiempo / (tiempoActivacion * 0.5f));
            tiempo += Time.deltaTime;
            yield return null;
        }

        hongoActivo.localScale = escalaFinal;
    }

    void GuardarPosicionesOriginales()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            posicionesOriginales[i] = hongos[i].hongo.transform.position;
        }
    }

    void Update()
    {
        if (!estaDespierto) return;

        if (!movimientoEvasivoActivado && hongosVivos == 3)
        {
            VerificarMovimientoEvasivoPorVida();
        }

        if (enMovimientoEvasivo)
        {
            ActualizarMovimientoEvasivo();
        }

        ActualizarDanioContacto();

        // Aplicar separación entre hongos
        if (!ataqueEnCurso)
        {
            AplicarSeparacionEntreHongos();
        }

        // ELIMINADO: Llamada a ActualizarRotacionesVisuales()

        if (ataqueEnCurso || enMovimientoEvasivo) return;

        if (Time.time - tiempoUltimoAtaque > tiempoEntreAtaques)
        {
            SeleccionarYRealizarAtaque();
            tiempoUltimoAtaque = Time.time;
        }
    }

    // Sistema de separación entre hongos
    void AplicarSeparacionEntreHongos()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (!hongos[i].estaVivo || hongos[i].hongo == null) continue;

            for (int j = i + 1; j < hongos.Length; j++)
            {
                if (!hongos[j].estaVivo || hongos[j].hongo == null) continue;

                Vector3 posicionI = hongos[i].hongo.transform.position;
                Vector3 posicionJ = hongos[j].hongo.transform.position;
                float distancia = Vector3.Distance(posicionI, posicionJ);

                if (distancia < distanciaMinimaSeparacion)
                {
                    // Calcular dirección de repulsión
                    Vector3 direccionRepulsion = (posicionI - posicionJ).normalized;
                    float fuerza = (1f - (distancia / distanciaMinimaSeparacion)) * fuerzaSeparacion;

                    // Aplicar movimiento de separación
                    Vector3 nuevaPosicionI = posicionI + direccionRepulsion * fuerza * Time.deltaTime;
                    Vector3 nuevaPosicionJ = posicionJ - direccionRepulsion * fuerza * Time.deltaTime;

                    // Mantener dentro de los rangos permitidos
                    nuevaPosicionI = LimitarPosicionARango(nuevaPosicionI, posicionesOriginales[i]);
                    nuevaPosicionJ = LimitarPosicionARango(nuevaPosicionJ, posicionesOriginales[j]);

                    hongos[i].hongo.transform.position = nuevaPosicionI;
                    hongos[j].hongo.transform.position = nuevaPosicionJ;
                }
            }
        }
    }

    // Limitar posición dentro del rango permitido
    Vector3 LimitarPosicionARango(Vector3 posicion, Vector3 posicionOriginal)
    {
        posicion.x = Mathf.Clamp(posicion.x,
            posicionOriginal.x - rangoMovimientoX,
            posicionOriginal.x + rangoMovimientoX);
        posicion.y = Mathf.Clamp(posicion.y,
            posicionOriginal.y - rangoMovimientoY,
            posicionOriginal.y + rangoMovimientoY);

        return posicion;
    }

    // Sistema de control de animaciones (sin cambios)
    private void SetAnimacionHongo(int indiceHongo, EstadoAnimacion estado)
    {
        if (indiceHongo < 0 || indiceHongo >= hongos.Length) return;
        if (!hongos[indiceHongo].estaVivo || hongos[indiceHongo].animator == null) return;

        ResetAnimacionHongo(indiceHongo);

        switch (estado)
        {
            case EstadoAnimacion.Inactivo: // AÑADIR este caso
                                           // Los hongos inactivos usan su propio prefab con animación
                break;
            case EstadoAnimacion.Idle:
                hongos[indiceHongo].animator.SetBool("IsIdle", true);
                break;
            case EstadoAnimacion.Attacking:
                hongos[indiceHongo].animator.SetBool("IsAttacking", true);
                break;
            case EstadoAnimacion.Returning:
                hongos[indiceHongo].animator.SetBool("IsReturning", true);
                break;
            case EstadoAnimacion.Dead:
                hongos[indiceHongo].animator.SetBool("IsDead", true);
                break;
            case EstadoAnimacion.Stacked:
                hongos[indiceHongo].animator.SetBool("IsStacked", true);
                break;
        }
    }

    private void ResetAnimacionHongo(int indiceHongo)
    {
        if (hongos[indiceHongo].animator == null) return;

        hongos[indiceHongo].animator.SetBool("IsIdle", false);
        hongos[indiceHongo].animator.SetBool("IsAttacking", false);
        hongos[indiceHongo].animator.SetBool("IsReturning", false);
        hongos[indiceHongo].animator.SetBool("IsDead", false);
        hongos[indiceHongo].animator.SetBool("IsStacked", false);
    }

    private void ActivarAnimacionAtaque()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo)
            {
                SetAnimacionHongo(i, EstadoAnimacion.Attacking);
            }
        }
    }

    private void ActivarAnimacionRetorno()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo)
            {
                SetAnimacionHongo(i, EstadoAnimacion.Returning);
            }
        }
    }

    private void RestaurarAnimacionIdle()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo)
            {
                SetAnimacionHongo(i, EstadoAnimacion.Idle);
            }
        }
    }

    private void ActivarAnimacionMuerte(int indiceHongo)
    {
        SetAnimacionHongo(indiceHongo, EstadoAnimacion.Dead);
    }

    void ActualizarDanioContacto()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null && jugador != null)
            {
                float distancia = Vector3.Distance(hongos[i].hongo.transform.position, jugador.position);
                if (distancia < 1.2f && Time.time - ultimoDanioContacto[i] > tiempoEntreDanioContacto)
                {
                    AplicarDanioContacto(i);
                }
            }
        }
    }

    void AplicarDanioContacto(int indiceHongo)
    {
        if (jugador == null) return;

        PlayerHealth playerHealth = jugador.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int danioActual = Mathf.RoundToInt(danioBaseContacto * multiplicadorDanio);
            playerHealth.RecibirDanio(danioActual);
            ultimoDanioContacto[indiceHongo] = Time.time;
        }
    }

    void VerificarMovimientoEvasivoPorVida()
    {
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.vida <= hongo.vidaMaxima / 2f)
            {
                movimientoEvasivoActivado = true;
                StartCoroutine(ActivarMovimientoEvasivoPorVida());
                break;
            }
        }
    }

    IEnumerator ActivarMovimientoEvasivoPorVida()
    {
        yield return StartCoroutine(IniciarMovimientoEvasivo());
        movimientoEvasivoActivado = false;
    }

    // --- SISTEMA DE MOVIMIENTO EVASIVO ZIG-ZAG ---
    IEnumerator IniciarMovimientoEvasivo()
    {
        enMovimientoEvasivo = true;

        movimientoEvasivoCoroutine = StartCoroutine(MovimientoEvasivoCoroutine());
        disparosLentosCoroutine = StartCoroutine(DisparosLentosDuranteMovimiento());

        yield return new WaitForSeconds(duracionMovimientoEvasivo);

        if (movimientoEvasivoCoroutine != null)
            StopCoroutine(movimientoEvasivoCoroutine);
        if (disparosLentosCoroutine != null)
            StopCoroutine(disparosLentosCoroutine);

        yield return StartCoroutine(RegresarAPosicionesOriginales());

        enMovimientoEvasivo = false;
    }

    IEnumerator MovimientoEvasivoCoroutine()
    {
        while (enMovimientoEvasivo)
        {
            yield return null;
        }
    }

    void ActualizarMovimientoEvasivo()
    {
        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null)
            {
                tiemposCambioDireccion[i] += Time.deltaTime;

                if (tiemposCambioDireccion[i] >= frecuenciaCambioDireccion)
                {
                    direccionesActuales[i] = GenerarNuevaDireccion(i);
                    tiemposCambioDireccion[i] = 0f;
                }

                MoverHongoEvasivo(i);
            }
        }
    }

    Vector3 GenerarNuevaDireccion(int indiceHongo)
    {
        Vector3 nuevaDireccion = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;

        Vector3 posicionActual = hongos[indiceHongo].hongo.transform.position;
        Vector3 posicionOriginal = posicionesOriginales[indiceHongo];

        // Evitar direcciones que acerquen demasiado a otros hongos
        for (int i = 0; i < hongos.Length; i++)
        {
            if (i != indiceHongo && hongos[i].estaVivo && hongos[i].hongo != null)
            {
                Vector3 direccionAOtro = (hongos[i].hongo.transform.position - posicionActual).normalized;
                float distancia = Vector3.Distance(posicionActual, hongos[i].hongo.transform.position);

                if (distancia < distanciaMinimaSeparacion * 1.5f)
                {
                    // Reducir componente en dirección al otro hongo
                    float dotProduct = Vector3.Dot(nuevaDireccion, direccionAOtro);
                    if (dotProduct > 0.3f) // Si se está moviendo hacia el otro hongo
                    {
                        nuevaDireccion -= direccionAOtro * dotProduct * 0.5f;
                        nuevaDireccion.Normalize();
                    }
                }
            }
        }

        if (Mathf.Abs(posicionActual.x - posicionOriginal.x) > rangoMovimientoX * 0.8f)
        {
            nuevaDireccion.x = Mathf.Sign(posicionOriginal.x - posicionActual.x);
        }

        if (Mathf.Abs(posicionActual.y - posicionOriginal.y) > rangoMovimientoY * 0.8f)
        {
            nuevaDireccion.y = Mathf.Sign(posicionOriginal.y - posicionActual.y);
        }

        return nuevaDireccion.normalized;
    }

    void MoverHongoEvasivo(int indiceHongo)
    {
        if (hongos[indiceHongo].hongo == null) return;

        Transform hongoTransform = hongos[indiceHongo].hongo.transform;
        Vector3 posicionOriginal = posicionesOriginales[indiceHongo];

        float velocidadActual = velocidadBaseMovimiento * multiplicadorVelocidad;
        Vector3 movimiento = direccionesActuales[indiceHongo] * velocidadActual * Time.deltaTime;
        Vector3 nuevaPosicion = hongoTransform.position + movimiento;

        nuevaPosicion = LimitarPosicionARango(nuevaPosicion, posicionOriginal);

        // NUEVO: Mantener la rotación original
        Quaternion rotacionOriginal = hongoTransform.rotation;
        hongoTransform.position = nuevaPosicion;
        hongoTransform.rotation = rotacionOriginal; // Preservar rotación
    }

    IEnumerator DisparosLentosDuranteMovimiento()
    {
        while (enMovimientoEvasivo)
        {
            foreach (HongoData hongo in hongos)
            {
                if (hongo.estaVivo && hongo.hongo != null && jugador != null)
                {
                    DisparoProyectilLento(hongo.hongo.transform);
                }
            }

            yield return new WaitForSeconds(tiempoEntreDisparosLentos);
        }
    }

    void DisparoProyectilLento(Transform origen)
    {
        if (origen == null || jugador == null) return;

        if (proyectilEspora != null)
        {
            GameObject proyectil = Instantiate(proyectilEspora, origen.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - origen.position).normalized;

            CA_ProyectilHilo proyectilScript = proyectil.GetComponent<CA_ProyectilHilo>();
            if (proyectilScript != null)
            {
                int danioActual = Mathf.RoundToInt(danioBaseProyectil * multiplicadorDanio);
                proyectilScript.danio = danioActual;
                proyectilScript.layerSuelo = layerSuelo;
            }

            Rigidbody2D rb = proyectil.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direccion * velocidadProyectilLento;
            }

            Destroy(proyectil, 5f);
        }
    }

    IEnumerator RegresarAPosicionesOriginales()
    {
        List<Coroutine> movimientos = new List<Coroutine>();

        for (int i = 0; i < hongos.Length; i++)
        {
            if (hongos[i].estaVivo && hongos[i].hongo != null)
            {
                movimientos.Add(StartCoroutine(MoverHongoSuave(hongos[i].hongo.transform, posicionesOriginales[i], 1f)));
            }
        }

        foreach (Coroutine mov in movimientos)
        {
            yield return mov;
        }
    }

    // --- SISTEMA DE SELECCIÓN DE ATAQUES ---
    void SeleccionarYRealizarAtaque()
    {
        List<int> ataquesDisponibles = new List<int>();

        ataquesDisponibles.Add(1); // Balas Giratorias con Rayo

        if (hongosVivos == 3)
        {
            ataquesDisponibles.Add(0); // Movimiento Evasivo
        }

        if (hongosVivos < 3)
        {
            for (int i = 0; i < 3; i++)
            {
                ataquesDisponibles.Add(2); // Estocada
            }
        }
        else
        {
            ataquesDisponibles.Add(2); // Estocada
        }

        ataqueActual = ataquesDisponibles[Random.Range(0, ataquesDisponibles.Count)];

        switch (ataqueActual)
        {
            case 0:
                StartCoroutine(AtaqueMovimientoEvasivo());
                break;
            case 1:
                StartCoroutine(AtaqueBalasGiratorias());
                break;
            case 2:
                StartCoroutine(AtaqueEstocada());
                break;
        }
    }

    IEnumerator AtaqueMovimientoEvasivo()
    {
        ataqueEnCurso = true;
        yield return StartCoroutine(IniciarMovimientoEvasivo());
        ataqueEnCurso = false;
    }

    // --- ATAQUE BALAS GIRATORIAS ---
    IEnumerator AtaqueBalasGiratorias()
    {
        ataqueEnCurso = true;
        ActivarAnimacionAtaque();

        List<Coroutine> coroutinesBalas = new List<Coroutine>();

        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                coroutinesBalas.Add(StartCoroutine(CrearBalasGiratorias(hongo.hongo.transform)));
            }
        }

        yield return new WaitForSeconds(3f);

        foreach (GameObject bala in hilosActivos)
        {
            if (bala != null) Destroy(bala);
        }
        hilosActivos.Clear();

        RestaurarAnimacionIdle();
        ataqueEnCurso = false;
    }

    IEnumerator CrearBalasGiratorias(Transform origen)
    {
        if (origen == null) yield break;

        GameObject contenedorBalas = new GameObject("BalasGiratoriasConRayo");
        contenedorBalas.transform.position = origen.position;
        hilosActivos.Add(contenedorBalas);

        List<GameObject> balasConEfecto = new List<GameObject>();

        for (int i = 0; i < numeroHilos; i++)
        {
            GameObject contenedorBala = new GameObject("BalaConRayo_" + i);
            contenedorBala.transform.SetParent(contenedorBalas.transform);

            GameObject balaObj = Instantiate(proyectilEspora, origen.position, Quaternion.identity);
            balaObj.name = "BalaVisual_" + i;
            balaObj.transform.SetParent(contenedorBala.transform);

            CA_ProyectilHilo proyectilHilo = balaObj.GetComponent<CA_ProyectilHilo>();
            if (proyectilHilo != null)
            {
                int danioActual = Mathf.RoundToInt(danioBaseHilos * 0.7f * multiplicadorDanio);
                proyectilHilo.danio = danioActual;
                proyectilHilo.layerSuelo = layerSuelo;

                Rigidbody2D rb = balaObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.gravityScale = 0f;
                }

                balaObj.transform.localScale = Vector3.one * 0.5f;
            }

            GameObject efectoRayo = new GameObject("EfectoRayo_" + i);
            efectoRayo.transform.SetParent(contenedorBala.transform);
            efectoRayo.transform.position = origen.position;

            LineRenderer lineRenderer = efectoRayo.AddComponent<LineRenderer>();
            ConfigurarLineRenderer(lineRenderer);

            lineRenderer.startWidth = anchoHilo * 0.5f;
            lineRenderer.endWidth = anchoHilo * 0.3f;
            lineRenderer.positionCount = 8;

            balasConEfecto.Add(contenedorBala);

            float angulo = i * (360f / numeroHilos);
            PosicionarBalaConRayoEnCirculo(contenedorBala.transform, balaObj.transform, efectoRayo, origen.position, angulo, radioHilos);
        }

        float tiempoInicio = Time.time;
        while (Time.time - tiempoInicio < 3f && contenedorBalas != null && origen != null)
        {
            contenedorBalas.transform.RotateAround(origen.position, Vector3.forward, velocidadRotacionHilos * Time.deltaTime);

            for (int i = 0; i < balasConEfecto.Count; i++)
            {
                if (balasConEfecto[i] != null)
                {
                    float angulo = i * (360f / numeroHilos) + contenedorBalas.transform.eulerAngles.z;
                    ActualizarBalaConRayo(balasConEfecto[i].transform, origen.position, angulo, radioHilos, Time.time - tiempoInicio);
                }
            }
            yield return null;
        }

        if (contenedorBalas != null)
        {
            foreach (GameObject bala in balasConEfecto)
            {
                if (bala != null) Destroy(bala);
            }
            Destroy(contenedorBalas);
        }
    }

    void PosicionarBalaConRayoEnCirculo(Transform contenedorBala, Transform balaVisual, GameObject efectoRayo, Vector3 centro, float angulo, float radio)
    {
        float anguloRad = angulo * Mathf.Deg2Rad;
        Vector3 posicionBala = centro + new Vector3(Mathf.Cos(anguloRad), Mathf.Sin(anguloRad), 0) * radio;

        contenedorBala.position = posicionBala;
        balaVisual.position = posicionBala;

        ActualizarRayo(efectoRayo, centro, posicionBala, 0f);
    }

    void ActualizarBalaConRayo(Transform contenedorBala, Vector3 centro, float angulo, float radio, float tiempo)
    {
        if (contenedorBala == null) return;

        float anguloRad = angulo * Mathf.Deg2Rad;
        Vector3 posicionBala = centro + new Vector3(Mathf.Cos(anguloRad), Mathf.Sin(anguloRad), 0) * radio;

        contenedorBala.position = posicionBala;

        Transform efectoRayo = null;
        Transform balaVisual = null;

        foreach (Transform child in contenedorBala)
        {
            if (child.name.StartsWith("EfectoRayo_"))
                efectoRayo = child;
            else if (child.name.StartsWith("BalaVisual_"))
                balaVisual = child;
        }

        if (efectoRayo != null)
        {
            ActualizarRayo(efectoRayo.gameObject, centro, posicionBala, tiempo);
        }

        if (balaVisual != null)
        {
            balaVisual.position = posicionBala;
        }
    }

    void ActualizarRayo(GameObject efectoRayo, Vector3 inicio, Vector3 fin, float tiempo)
    {
        LineRenderer lineRenderer = efectoRayo.GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        Vector3 direccion = (fin - inicio).normalized;
        float distancia = Vector3.Distance(inicio, fin);
        Vector3 normal = new Vector3(-direccion.y, direccion.x, 0);

        lineRenderer.positionCount = 8;

        for (int i = 0; i < 8; i++)
        {
            float t = i / 7f;
            Vector3 puntoBase = Vector3.Lerp(inicio, fin, t);

            float frecuencia = 12f;
            float amplitud = 0.15f * (1f - t * 0.5f);
            float onda = Mathf.Sin(t * frecuencia * Mathf.PI + tiempo * 20f) * amplitud;

            Vector3 puntoFinal = puntoBase + normal * onda;
            lineRenderer.SetPosition(i, puntoFinal);
        }
    }

    void ConfigurarLineRenderer(LineRenderer lr)
    {
        lr.startWidth = anchoHilo;
        lr.endWidth = anchoHilo;
        lr.material = materialHilo != null ? materialHilo : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = colorHilo;
        lr.endColor = colorHilo;
        lr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.useWorldSpace = true;
    }

    // --- ATAQUE ESTOCADA ---
    IEnumerator AtaqueEstocada()
    {
        ataqueEnCurso = true;
        ActivarAnimacionAtaque();

        List<Coroutine> estocadas = new List<Coroutine>();

        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null && jugador != null)
            {
                float velocidadBase = hongosVivos < 3 ? velocidadBaseEstocada * 1.5f : velocidadBaseEstocada;
                float velocidadActual = velocidadBase * multiplicadorVelocidad;
                estocadas.Add(StartCoroutine(EstocadaIndividualMejorada(hongo, velocidadActual)));
            }
        }

        foreach (Coroutine estocada in estocadas)
        {
            yield return estocada;
        }

        ActivarAnimacionRetorno();
        yield return new WaitForSeconds(duracionAnimacionRetorno);
        RestaurarAnimacionIdle();
        ataqueEnCurso = false;
    }

    IEnumerator EstocadaIndividualMejorada(HongoData hongoData, float velocidad)
    {
        if (hongoData.hongo == null || jugador == null) yield break;

        Transform hongo = hongoData.hongo.transform;
        Vector3 posicionInicial = hongo.position;

        SpriteRenderer sr = hongo.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        if (sr != null)
        {
            sr.color = Color.yellow;
            Vector3 escalaOriginal = hongo.localScale;
            hongo.localScale = escalaOriginal * 1.2f;
        }

        yield return new WaitForSeconds(0.3f);

        if (sr != null)
        {
            sr.color = Color.red;
        }

        Vector3 direccion = (jugador.position - hongo.position).normalized;
        float distanciaInicial = Vector3.Distance(hongo.position, jugador.position);
        float distanciaRecorrida = 0f;
        bool golpeado = false;

        while (distanciaRecorrida < distanciaInicial && !golpeado && hongo != null)
        {
            float distanciaFrame = velocidad * Time.deltaTime;

            // NUEVO: Guardar rotación antes de mover
            Quaternion rotacionOriginal = hongo.rotation;
            hongo.position += direccion * distanciaFrame;
            hongo.rotation = rotacionOriginal; // Restaurar rotación

            distanciaRecorrida += distanciaFrame;

            Collider2D colision = Physics2D.OverlapCircle(hongo.position, 1f);
            if (colision != null && colision.CompareTag("Player"))
            {
                PlayerHealth playerHealth = colision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    int danioActual = Mathf.RoundToInt(danioBaseEstocada * multiplicadorDanio);
                    playerHealth.RecibirDanio(danioActual);
                    golpeado = true;
                }
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        Vector3 puntoRetirada = ObtenerPuntoRetiradaAleatorio();

        if (sr != null)
        {
            sr.color = originalColor;
            hongo.localScale = hongo.localScale / 1.2f;
        }

        Vector3 direccionRetirada = (puntoRetirada - hongo.position).normalized;
        float distanciaRetirada = Vector3.Distance(hongo.position, puntoRetirada);
        float distanciaRetiradaRecorrida = 0f;

        while (distanciaRetiradaRecorrida < distanciaRetirada && hongo != null)
        {
            float distanciaFrame = velocidad * 0.7f * Time.deltaTime;

            // NUEVO: Guardar rotación antes de mover
            Quaternion rotacionOriginal = hongo.rotation;
            hongo.position += direccionRetirada * distanciaFrame;
            hongo.rotation = rotacionOriginal; // Restaurar rotación

            distanciaRetiradaRecorrida += distanciaFrame;
            yield return null;
        }
    }

    Vector3 ObtenerPuntoRetiradaAleatorio()
    {
        if (puntosRetirada != null && puntosRetirada.Length > 0)
        {
            return puntosRetirada[Random.Range(0, puntosRetirada.Length)].position;
        }
        else
        {
            Vector3 centro = transform.position;
            float radio = 5f;
            float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return centro + new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio, 0);
        }
    }

    IEnumerator MoverHongoSuave(Transform hongo, Vector3 objetivo, float duracion)
    {
        if (hongo == null) yield break;

        Vector3 inicio = hongo.position;
        Quaternion rotacionOriginal = hongo.rotation; // NUEVO: Guardar rotación
        float tiempo = 0f;

        while (tiempo < duracion && hongo != null)
        {
            hongo.position = Vector3.Lerp(inicio, objetivo, tiempo / duracion);
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
            tiempo += Time.deltaTime;
            yield return null;
        }

        if (hongo != null)
        {
            hongo.position = objetivo;
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
        }
    }

    // --- SISTEMA DE DAÑO Y FUSIÓN CON MULTIPLICADORES ---
    public void RecibirDano(int indiceHongo, float dano)
    {
        if (!estaDespierto || ataqueEnCurso) return;
        if (indiceHongo < 0 || indiceHongo >= hongos.Length) return;
        if (!hongos[indiceHongo].estaVivo || hongos[indiceHongo].hongo == null) return;

        hongos[indiceHongo].vida -= dano;
        ActualizarColorHongo(indiceHongo);

        if (hongos[indiceHongo].vida <= 0)
        {
            HongoDerrotado(indiceHongo);
        }
    }

    void ActualizarColorHongo(int indice)
    {
        if (hongos[indice].hongo == null) return;
        SpriteRenderer sr = hongos[indice].hongo.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float porcentajeVida = hongos[indice].vida / hongos[indice].vidaMaxima;
            sr.color = Color.Lerp(Color.black, indice == 2 ? Color.red : Color.green, porcentajeVida);
        }
    }

    void HongoDerrotado(int indice)
    {
        if (hongos[indice].hongo != null)
        {
            SpriteRenderer sr = hongos[indice].hongo.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.black;

            ActivarAnimacionMuerte(indice);
            StartCoroutine(EfectoMuerte(hongos[indice].hongo));
        }

        hongos[indice].estaVivo = false;
        hongosVivos--;

        ActualizarMultiplicadores();

        if (hongosVivos == 2) StartCoroutine(FusionarHongos());
        else if (hongosVivos == 1) StartCoroutine(FusionFinal());
        else if (hongosVivos == 0) Debug.Log("¡Mini Boss Derrotado!");
    }

    void ActualizarMultiplicadores()
    {
        int hongosMuertos = 3 - hongosVivos;

        multiplicadorDanio = Mathf.Pow(2f, hongosMuertos);
        multiplicadorVelocidad = Mathf.Pow(2f, hongosMuertos);

        danioEstocada = Mathf.RoundToInt(danioBaseEstocada * multiplicadorDanio);
        velocidadMovimiento = velocidadBaseMovimiento * multiplicadorVelocidad;
        velocidadHilosRectos = velocidadBaseHilos * multiplicadorVelocidad;
        danioContacto = Mathf.RoundToInt(danioBaseContacto * multiplicadorDanio);
    }

    IEnumerator EfectoMuerte(GameObject hongo)
    {
        if (hongo == null) yield break;
        Quaternion rotacionOriginal = hongo.transform.rotation; // NUEVO: Guardar rotación
        for (int i = 0; i < 3; i++)
        {
            if (hongo != null)
            {
                hongo.transform.localScale *= 0.7f;
                hongo.transform.rotation = rotacionOriginal; // NUEVO: Mantener rotación
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (hongo != null) hongo.SetActive(false);
    }

    IEnumerator FusionarHongos()
    {
        tiempoEntreAtaques = velocidadAtaqueFusion;
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                SpriteRenderer sr = hongo.hongo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.yellow;
                    StartCoroutine(EfectoFusion(hongo.hongo.transform));
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator FusionFinal()
    {
        tiempoEntreAtaques = velocidadAtaqueFusion * 0.5f;
        foreach (HongoData hongo in hongos)
        {
            if (hongo.estaVivo && hongo.hongo != null)
            {
                SpriteRenderer sr = hongo.hongo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.red;
                    StartCoroutine(EfectoFusionFinal(hongo.hongo.transform));
                }
            }
        }
        yield return new WaitForSeconds(1f);
    }

    IEnumerator EfectoFusion(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        Quaternion rotacionOriginal = hongo.rotation; // NUEVO: Guardar rotación
        float tiempo = 0f;
        while (tiempo < 0.5f && hongo != null)
        {
            hongo.localScale = escalaOriginal * (1f + Mathf.Sin(tiempo * 10f) * 0.2f);
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
            tiempo += Time.deltaTime;
            yield return null;
        }
        if (hongo != null)
        {
            hongo.localScale = escalaOriginal;
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
        }
    }

    IEnumerator EfectoFusionFinal(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        Quaternion rotacionOriginal = hongo.rotation; // NUEVO: Guardar rotación
        hongo.localScale = escalaOriginal * 1.3f;
        float tiempo = 0f;
        while (tiempo < 1f && hongo != null)
        {
            hongo.localScale = escalaOriginal * 1.3f * (1f + Mathf.Sin(tiempo * 15f) * 0.1f);
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
            tiempo += Time.deltaTime;
            yield return null;
        }
    }

    //public void ActivarBoss()
    //{
    //    if (!estaDespierto)
    //    {
    //        estaDespierto = true;
    //        foreach (HongoData hongo in hongos)
    //        {
    //            if (hongo.hongo != null)
    //            {
    //                StartCoroutine(EfectoActivacion(hongo.hongo.transform));
    //            }
    //        }
    //        tiempoUltimoAtaque = Time.time;
    //    }
    //}

    IEnumerator EfectoActivacion(Transform hongo)
    {
        if (hongo == null) yield break;
        Vector3 escalaOriginal = hongo.localScale;
        Quaternion rotacionOriginal = hongo.rotation; // NUEVO: Guardar rotación
        float duracion = 0.5f;
        float tiempo = 0f;
        while (tiempo < duracion && hongo != null)
        {
            hongo.localScale = escalaOriginal * (1f + Mathf.PingPong(tiempo * 2f, 0.3f));
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
            tiempo += Time.deltaTime;
            yield return null;
        }
        if (hongo != null)
        {
            hongo.localScale = escalaOriginal;
            hongo.rotation = rotacionOriginal; // NUEVO: Mantener rotación
        }
    }
}