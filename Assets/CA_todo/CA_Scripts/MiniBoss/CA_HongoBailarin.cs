using UnityEngine;

public class CA_HongoBailarin : MonoBehaviour
{
    private CA_AnimacionHongo animacion;

    void Start()
    {
        animacion = GetComponent<CA_AnimacionHongo>();

        // Iniciar baile automático cada 5 segundos
        InvokeRepeating("BaileAleatorio", 2f, 5f);
    }

    void BaileAleatorio()
    {
        if (Random.Range(0, 100) > 70) // 30% de probabilidad
        {
            animacion.IniciarBaile();
        }
    }

    // Llamar desde otros scripts para reacciones
    public void ReaccionarAtaque()
    {
        animacion.IniciarAturdimiento();
        animacion.IniciarSaltoLocura();
    }
}