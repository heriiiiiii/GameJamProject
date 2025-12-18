using UnityEngine;

public class PlayerMapTracker : MonoBehaviour
{
    public Transform player;         // El jugador real
    public RectTransform playerIcon; // Icono dentro del mapa

    public Vector2 worldMin; // Límite inferior del mundo
    public Vector2 worldMax; // Límite superior del mundo

    public RectTransform mapRect;

    void Update()
    {
        Vector2 normalizedPos = new Vector2(
            Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x),
            Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.y)
        );

        Vector2 mapSize = mapRect.sizeDelta;

        playerIcon.anchoredPosition = new Vector2(
            normalizedPos.x * mapSize.x,
            normalizedPos.y * mapSize.y
        );
    }
}
