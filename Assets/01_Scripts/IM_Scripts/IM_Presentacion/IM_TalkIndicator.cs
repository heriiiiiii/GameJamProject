using UnityEngine;

public class IM_TalkIndicator : MonoBehaviour
{
    [Header("Referencia al Indicador (HABLAR)")]
    public GameObject talkIndicator;

    [Header("Rango para mostrar el indicador")]
    public float activationRange = 2f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        talkIndicator.SetActive(false); // inicia oculto
    }

    private void Update()
    {
        float distance = Vector2.Distance(player.position, transform.position);

        // Si el jugador está cerca → mostrar
        if (distance <= activationRange)
        {
            if (!talkIndicator.activeSelf)
                talkIndicator.SetActive(true);
        }
        else
        {
            if (talkIndicator.activeSelf)
                talkIndicator.SetActive(false);
        }
    }
}
