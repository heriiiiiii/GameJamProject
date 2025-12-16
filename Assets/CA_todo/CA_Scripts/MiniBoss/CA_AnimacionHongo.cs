using UnityEngine;
using System.Collections;

public class CA_AnimacionHongo : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public float velocidadAnimacion = 1f;
    public float intensidadMovimiento = 0.5f;
    public bool animacionActiva = true;

    [Header("Estiramientos")]
    public float fuerzaEstiramiento = 2f;
    public float duracionEstiramiento = 0.5f;

    [Header("Aturdimientos")]
    public float fuerzaAturdimiento = 3f;
    public float duracionAturdimiento = 1f;

    private Vector3 escalaOriginal;
    private Vector3 posicionOriginal;
    private bool estaAturdido = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        escalaOriginal = transform.localScale;
        posicionOriginal = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Iniciar animación idle
        if (animacionActiva)
        {
            StartCoroutine(AnimacionIdle());
        }
    }

    // ANIMACIÓN IDLE (movimiento constante)
    IEnumerator AnimacionIdle()
    {
        while (animacionActiva)
        {
            // Flotación suave
            yield return StartCoroutine(FlotarSuave());

            // Inclinación leve
            yield return StartCoroutine(InclinarSuave());

            // Cambio de tamaño sutil
            yield return StartCoroutine(Respirar());
        }
    }

    IEnumerator FlotarSuave()
    {
        float tiempo = 0f;
        float duracion = 2f / velocidadAnimacion;
        Vector3 posInicial = transform.position;

        while (tiempo < duracion)
        {
            if (estaAturdido) yield break;

            float altura = Mathf.Sin(tiempo * Mathf.PI * 2f) * intensidadMovimiento * 0.1f;
            transform.position = posInicial + Vector3.up * altura;

            tiempo += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator InclinarSuave()
    {
        float tiempo = 0f;
        float duracion = 1.5f / velocidadAnimacion;
        Vector3 rotInicial = transform.eulerAngles;

        while (tiempo < duracion)
        {
            if (estaAturdido) yield break;

            float inclinacion = Mathf.Sin(tiempo * Mathf.PI * 4f) * 5f;
            transform.eulerAngles = rotInicial + new Vector3(0, 0, inclinacion);

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.eulerAngles = rotInicial;
    }

    IEnumerator Respirar()
    {
        float tiempo = 0f;
        float duracion = 1f / velocidadAnimacion;

        while (tiempo < duracion)
        {
            if (estaAturdido) yield break;

            float escala = 1f + Mathf.Sin(tiempo * Mathf.PI * 2f) * 0.1f;
            transform.localScale = escalaOriginal * escala;

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    // ANIMACIÓN DE ESTRIRAMIENTO
    public void IniciarEstiramiento(Vector3 direccion)
    {
        if (!estaAturdido)
        {
            StartCoroutine(Estirarse(direccion));
        }
    }

    IEnumerator Estirarse(Vector3 direccion)
    {
        Debug.Log("Hongo estirándose!");

        Vector3 escalaObjetivo = escalaOriginal + (Vector3)direccion.normalized * fuerzaEstiramiento;
        Vector3 escalaInicial = transform.localScale;

        float tiempo = 0f;

        // Estirar
        while (tiempo < duracionEstiramiento / 2f)
        {
            float progreso = tiempo / (duracionEstiramiento / 2f);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaObjetivo, progreso);

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Volver a normal
        tiempo = 0f;
        while (tiempo < duracionEstiramiento / 2f)
        {
            float progreso = tiempo / (duracionEstiramiento / 2f);
            transform.localScale = Vector3.Lerp(escalaObjetivo, escalaOriginal, progreso);

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    // ANIMACIÓN DE ATURDIMIENTO
    public void IniciarAturdimiento()
    {
        if (!estaAturdido)
        {
            StartCoroutine(Aturdirse());
        }
    }

    IEnumerator Aturdirse()
    {
        Debug.Log(" Hongo aturdido!");

        estaAturdido = true;
        Vector3 escalaInicial = transform.localScale;

        // Cambiar color (opcional)
        if (spriteRenderer != null)
        {
            Color colorOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.yellow;
        }

        float tiempo = 0f;

        // Movimiento caótico
        while (tiempo < duracionAturdimiento)
        {
            // Rotación loca
            float rotacion = Mathf.Sin(tiempo * 20f) * 30f;
            transform.eulerAngles = new Vector3(0, 0, rotacion);

            // Escala aleatoria
            float escalaAleatoria = 1f + Mathf.Sin(tiempo * 15f) * 0.3f;
            transform.localScale = escalaOriginal * escalaAleatoria;

            // Movimiento tembloroso
            Vector3 posicionTemblor = posicionOriginal + Random.insideUnitSphere * 0.2f;
            posicionTemblor.z = posicionOriginal.z;
            transform.position = posicionTemblor;

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Volver a normal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        transform.eulerAngles = Vector3.zero;
        transform.localScale = escalaOriginal;
        transform.position = posicionOriginal;
        estaAturdido = false;
    }

    // ANIMACIÓN DE SALTO EXTRAVAGANTE
    public void IniciarSaltoLocura()
    {
        StartCoroutine(SaltoLocura());
    }

    IEnumerator SaltoLocura()
    {
        Debug.Log(" Hongo en salto de locura!");

        Vector3 posicionInicial = transform.position;
        float alturaSalto = 2f;
        float duracionSalto = 1f;

        float tiempo = 0f;

        while (tiempo < duracionSalto)
        {
            // Trayectoria de salto con rotación
            float progreso = tiempo / duracionSalto;
            float altura = Mathf.Sin(progreso * Mathf.PI) * alturaSalto;

            // Rotación extravagante
            float rotacion = progreso * 720f; // 2 rotaciones completas

            transform.position = posicionInicial + Vector3.up * altura;
            transform.eulerAngles = new Vector3(0, 0, rotacion);

            // Cambio de escala durante el salto
            float escala = 1f + Mathf.Sin(progreso * Mathf.PI * 2f) * 0.5f;
            transform.localScale = escalaOriginal * escala;

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionInicial;
        transform.eulerAngles = Vector3.zero;
        transform.localScale = escalaOriginal;
    }

    // ANIMACIÓN DE BAILE
    public void IniciarBaile()
    {
        StartCoroutine(BaileExtravagante());
    }

    IEnumerator BaileExtravagante()
    {
        Debug.Log(" Hongo bailando!");

        float duracionBaile = 3f;
        float tiempo = 0f;

        while (tiempo < duracionBaile)
        {
            // Movimiento de baile complejo
            float rotacion = Mathf.Sin(tiempo * 8f) * 20f;
            float inclinacion = Mathf.Cos(tiempo * 6f) * 15f;

            transform.eulerAngles = new Vector3(0, 0, rotacion + inclinacion);

            // Movimiento lateral
            float movimientoX = Mathf.Sin(tiempo * 4f) * 0.5f;
            float movimientoY = Mathf.Cos(tiempo * 3f) * 0.3f;

            transform.position = posicionOriginal + new Vector3(movimientoX, movimientoY, 0);

            // Cambios de escala rítmicos
            float escala = 1f + Mathf.Sin(tiempo * 10f) * 0.2f;
            transform.localScale = escalaOriginal * escala;

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Volver a normal
        transform.eulerAngles = Vector3.zero;
        transform.position = posicionOriginal;
        transform.localScale = escalaOriginal;
    }
}