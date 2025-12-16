using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_LeverSystem : MonoBehaviour
{
    [Header("Configuración de la Palanca")]
    [SerializeField] private Transform leverHandle;
    [SerializeField] private float activationAngle = 30f;
    [SerializeField] private float activationTime = 0.5f;
    [SerializeField] private AudioClip activationSound;

    [Header("Sistema de Plataforma")]
    [SerializeField] private Transform platform;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float platformMoveTime = 2f;

    [Header("Detección de Ataque")]
    [SerializeField] private LayerMask playerAttackLayer = 1; // Default layer por defecto
    [SerializeField] private float detectionRadius = 1.5f;

    private bool isActivated = false;
    private bool canBeActivated = true;
    private AudioSource audioSource;

    // Referencia al player
    private CA_PlayerController playerController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Buscar el player controller
        playerController = FindObjectOfType<CA_PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("No se encontró CA_PlayerController en la escena");
        }

        Debug.Log("Lever System inicializado. Esperando ataques del jugador...");
    }

    void Update()
    {
        // TEST: Presiona T para activar la palanca manualmente
        if (Input.GetKeyDown(KeyCode.T) && !isActivated)
        {
            Debug.Log("Activando palanca con tecla T (TEST)");
            ActivateLever();
        }

        // Detectar ataques del jugador en Update
        if (canBeActivated && !isActivated && IsPlayerAttackingNearby())
        {
            Debug.Log("¡Ataque del jugador detectado! Activando palanca...");
            ActivateLever();
        }
    }

    private bool IsPlayerAttackingNearby()
    {
        if (playerController == null) return false;

        // Verificar distancia al jugador primero
        float distanceToPlayer = Vector2.Distance(transform.position, playerController.transform.position);
        if (distanceToPlayer > detectionRadius) return false;

        // Método 1: Verificar si el jugador está en estado de ataque
        // Necesitamos acceder a las variables internas del player controller
        // Como no podemos modificar el player, usaremos reflection o métodos públicos alternativos

        // Método 2: Detectar colliders de ataque alrededor de la palanca
        Collider2D[] attackColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerAttackLayer);
        foreach (Collider2D collider in attackColliders)
        {
            if (IsPlayerAttackCollider(collider))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPlayerAttackCollider(Collider2D collider)
    {
        // Verificar si este collider pertenece al sistema de ataque del jugador
        if (collider.CompareTag("Player")) return true;

        // Verificar si está en los transforms de ataque del player
        if (playerController != null)
        {
            // Usar reflection para acceder a los transforms de ataque
            System.Reflection.FieldInfo sideAttackField = typeof(CA_PlayerController).GetField("SideAttackTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo upAttackField = typeof(CA_PlayerController).GetField("UpAttackTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Reflection.FieldInfo downAttackField = typeof(CA_PlayerController).GetField("DownAttackTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (sideAttackField != null)
            {
                Transform sideAttack = (Transform)sideAttackField.GetValue(playerController);
                Transform upAttack = (Transform)upAttackField.GetValue(playerController);
                Transform downAttack = (Transform)downAttackField.GetValue(playerController);

                if (sideAttack != null && collider.transform == sideAttack) return true;
                if (upAttack != null && collider.transform == upAttack) return true;
                if (downAttack != null && collider.transform == downAttack) return true;
            }
        }

        return false;
    }

    // Método alternativo: Hacer la palanca detectable por el sistema de ataque del player
    public void GetHitByAttack()
    {
        if (canBeActivated && !isActivated)
        {
            Debug.Log("Palanca golpeada por ataque del jugador!");
            ActivateLever();
        }
    }

    // Método que puede ser llamado desde el player controller si decides modificarlo luego
    public void RegisterAttackHit()
    {
        if (canBeActivated && !isActivated)
        {
            Debug.Log("Ataque registrado en palanca!");
            ActivateLever();
        }
    }

    public void ActivateLever()
    {
        if (!canBeActivated || isActivated) return;

        isActivated = true;
        canBeActivated = false;

        StartCoroutine(AnimateLeverHandle());
        StartCoroutine(MovePlatform());

        if (activationSound != null)
            audioSource.PlayOneShot(activationSound);

        Debug.Log("Palanca activada completamente!");
    }

    private IEnumerator AnimateLeverHandle()
    {
        if (leverHandle == null) yield break;

        float elapsedTime = 0f;
        Quaternion startRotation = leverHandle.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, activationAngle);

        while (elapsedTime < activationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / activationTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            leverHandle.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        leverHandle.localRotation = targetRotation;
    }

    private IEnumerator MovePlatform()
    {
        if (platform == null || pointA == null || pointB == null) yield break;

        float elapsedTime = 0f;
        Vector3 startPosition = platform.position;
        Vector3 targetPosition = pointB.position;

        yield return new WaitForSeconds(0.2f);

        while (elapsedTime < platformMoveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / platformMoveTime;
            t = t * t * (3f - 2f * t);

            platform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        platform.position = targetPosition;
    }

    // Método público para forzar activación (para testing)
    public void ForceActivate()
    {
        if (!isActivated)
        {
            Debug.Log("Activación forzada de la palanca");
            ActivateLever();
        }
    }

    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.3f);
            Gizmos.DrawSphere(pointB.position, 0.3f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        // Dibujar radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}