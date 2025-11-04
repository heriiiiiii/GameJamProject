using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CA_ZonaLoaderAutoTrigger : MonoBehaviour
{
    [Header("Configuración de Zona")]
    public Transform[] puntos; // 4 hijos opcionales para ver el área
    public string tagJugador = "Player";
    public float tiempoCarga = 0.1f;

    private List<GameObject> objetosDentro = new List<GameObject>();
    private Vector2 centro;
    private Vector2 tamano;
    private bool zonaCargada = false;
    private Collider2D colZona;

    void Awake()
    {
        colZona = GetComponent<Collider2D>();
        colZona.isTrigger = true;

        CalcularZona();
        EncontrarObjetosEnZona();
        DesactivarObjetos();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!zonaCargada && other.CompareTag(tagJugador))
        {
            StartCoroutine(ActivarZona());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (zonaCargada && other.CompareTag(tagJugador))
        {
            DesactivarObjetos();
            zonaCargada = false;
            Debug.Log($"🔻 Zona descargada: {name}");
        }
    }

    System.Collections.IEnumerator ActivarZona()
    {
        yield return new WaitForSeconds(tiempoCarga);
        ActivarObjetos();
        zonaCargada = true;
        Debug.Log($"✅ Zona cargada: {name}");
    }

    void CalcularZona()
    {
        if (puntos == null || puntos.Length < 4) return;

        float minX = Mathf.Min(puntos[0].position.x, puntos[1].position.x, puntos[2].position.x, puntos[3].position.x);
        float maxX = Mathf.Max(puntos[0].position.x, puntos[1].position.x, puntos[2].position.x, puntos[3].position.x);
        float minY = Mathf.Min(puntos[0].position.y, puntos[1].position.y, puntos[2].position.y, puntos[3].position.y);
        float maxY = Mathf.Max(puntos[0].position.y, puntos[1].position.y, puntos[2].position.y, puntos[3].position.y);

        centro = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        tamano = new Vector2(maxX - minX, maxY - minY);
    }

    void EncontrarObjetosEnZona()
    {
        objetosDentro.Clear();
        Collider2D[] colliders = Physics2D.OverlapBoxAll(centro, tamano, 0f);

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject != gameObject && col.transform.parent != transform)
                objetosDentro.Add(col.gameObject);
        }

        Debug.Log($"🧩 Detectados {objetosDentro.Count} objetos en {name}");
    }

    void ActivarObjetos()
    {
        foreach (GameObject obj in objetosDentro)
            if (obj != null) obj.SetActive(true);
    }

    void DesactivarObjetos()
    {
        foreach (GameObject obj in objetosDentro)
            if (obj != null) obj.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (puntos != null && puntos.Length >= 4)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < puntos.Length; i++)
            {
                Transform actual = puntos[i];
                Transform siguiente = puntos[(i + 1) % puntos.Length];
                if (actual != null && siguiente != null)
                    Gizmos.DrawLine(actual.position, siguiente.position);
            }

            CalcularZona();
            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Gizmos.DrawCube(centro, tamano);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centro, tamano);
        }
    }
}
