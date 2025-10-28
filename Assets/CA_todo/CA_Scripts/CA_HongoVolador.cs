using System.Collections;
using UnityEngine;

public class CA_HongoVolador : MonoBehaviour
{
    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform[] puntosDisparo; // 4 puntos
    public float tiempoEntreRafagas = 3f;
    public float tiempoEntreBalas = 0.2f;

    [Header("Detección del Jugador")]
    public float rangoDeteccion = 10f;
    private Transform jugador;

    [Header("Movimiento")]
    public float velocidadMovimiento = 2f;
    private Vector3 posicionInicial;
    private bool siguiendo;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        posicionInicial = transform.position;
        StartCoroutine(Rafagas());
    }

    void Update()
    {
        Mover();
    }

    void Mover()
    {
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia < rangoDeteccion)
        {
            siguiendo = true;
            transform.position = Vector2.MoveTowards(transform.position, jugador.position, velocidadMovimiento * Time.deltaTime);
        }
        else
        {
            siguiendo = false;
            transform.position = Vector2.MoveTowards(transform.position, posicionInicial, velocidadMovimiento * Time.deltaTime);
        }
    }

    IEnumerator Rafagas()
    {
        while (true)
        {
            if (jugador != null && Vector2.Distance(transform.position, jugador.position) < rangoDeteccion)
            {
                // 3 disparos seguidos por ráfaga
                for (int i = 0; i < 3; i++)
                {
                    DispararDesdePuntos();
                    yield return new WaitForSeconds(tiempoEntreBalas);
                }
            }
            yield return new WaitForSeconds(tiempoEntreRafagas);
        }
    }

    void DispararDesdePuntos()
    {
        foreach (Transform punto in puntosDisparo)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, punto.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - punto.position).normalized;
            nuevaBala.GetComponent<CA_BalaVolador>().SetDireccion(direccion);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
