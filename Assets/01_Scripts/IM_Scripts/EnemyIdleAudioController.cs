using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyIdleAudioController : MonoBehaviour
{
    [Header("Idle Audio")]
    public AudioSource idleSource;
    public AudioClip idleClip;

    [Header("Rango de audio")]
    public float audioDistance = 6f;

    private Transform player;
    private bool isDead = false;

    void Awake()
    {
        if (!idleSource)
            idleSource = GetComponent<AudioSource>();

        idleSource.loop = true;
        idleSource.playOnAwake = false;
        idleSource.Stop();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void Update()
    {
        if (isDead || !player)
        {
            StopIdle();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= audioDistance)
        {
            if (!idleSource.isPlaying)
            {
                idleSource.clip = idleClip;
                idleSource.Play();
            }
        }
        else
        {
            StopIdle();
        }
    }

    public void StopIdle()
    {
        if (idleSource && idleSource.isPlaying)
            idleSource.Stop();
    }

    // ?? llamado desde el enemigo
    public void OnEnemyDeath()
    {
        isDead = true;
        StopIdle();
    }

    void OnDisable()
    {
        StopIdle();
    }

    void OnDestroy()
    {
        StopIdle();
    }
}
