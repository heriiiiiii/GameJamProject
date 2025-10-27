using UnityEngine;

public class CA_NubeEsporas : MonoBehaviour
{
    [Header("Configuración")]
    public float radioNube = 3f;
    public ParticleSystem particulasNube;
    public ParticleSystem particulasHumo;
    public Material materialNube;

    private float duracionTotal;
    private float reduccionVision;
    private float tiempoDisipacionNormal;
    private float tiempoDisipacionQuieto;
    private Transform jugador;
    private CA_EfectoVisionPlayer efectoVision;
    private float tiempoVida;
    private bool jugadorEnNube = false;
    private bool efectoAplicado = false;

    void Start()
    {
        CrearEfectoVisual();
    }

    public void Inicializar(float duracion, float reduccion, float disipacionNormal, float disipacionQuieto, Transform player, CA_EfectoVisionPlayer visionEffect)
    {
        duracionTotal = duracion;
        reduccionVision = reduccion;
        tiempoDisipacionNormal = disipacionNormal;
        tiempoDisipacionQuieto = disipacionQuieto;
        jugador = player;
        efectoVision = visionEffect;

        if (particulasNube != null)
        {
            var shape = particulasNube.shape;
            shape.radius = radioNube;

            var main = particulasNube.main;
            main.startLifetime = duracionTotal;
        }

        if (particulasHumo != null)
        {
            var shape = particulasHumo.shape;
            shape.radius = radioNube;
        }
    }

    void Update()
    {
        if (jugador == null) return;

        tiempoVida += Time.deltaTime;
        transform.position = jugador.position;

        jugadorEnNube = Vector3.Distance(transform.position, jugador.position) < radioNube;

        if (jugadorEnNube && !efectoAplicado)
        {
            AplicarEfectoVision();
        }
        else if (!jugadorEnNube && efectoAplicado)
        {
            RemoverEfectoVision();
        }

        VerificarDisipacion();

        // Actualizar opacidad basada en disipación
        ActualizarOpacidad();
    }

    void CrearEfectoVisual()
    {
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = radioNube;
        collider.isTrigger = true;

        // Crear sistema de partículas de humo si no existe
        if (particulasHumo == null)
        {
            GameObject humoObj = new GameObject("CA_HumoEsporas");
            humoObj.transform.SetParent(transform);
            humoObj.transform.localPosition = Vector3.zero;

            particulasHumo = humoObj.AddComponent<ParticleSystem>();
            ConfigurarParticulasHumo();
        }
    }

    void ConfigurarParticulasHumo()
    {
        if (particulasHumo != null)
        {
            var main = particulasHumo.main;
            main.startSpeed = 0.5f;
            main.startLifetime = 3f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.2f, 0.3f, 0.1f, 0.4f);
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particulasHumo.emission;
            emission.rateOverTime = 80f;

            var shape = particulasHumo.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radioNube;

            var velocity = particulasHumo.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
        }
    }

    void AplicarEfectoVision()
    {
        efectoAplicado = true;
        if (efectoVision != null)
        {
            efectoVision.AplicarReduccionVision(reduccionVision);
        }
        Debug.Log("Visión reducida por esporas!");
    }

    void RemoverEfectoVision()
    {
        efectoAplicado = false;
        if (efectoVision != null)
        {
            efectoVision.RemoverReduccionVision();
        }
        Debug.Log("Visión normalizada");
    }

    void ActualizarOpacidad()
    {
        bool jugadorQuieto = EstaJugadorQuieto();
        float tiempoDisipacion = jugadorQuieto ? tiempoDisipacionQuieto : tiempoDisipacionNormal;
        float progresoDisipacion = tiempoVida / tiempoDisipacion;
        float opacidad = Mathf.Clamp01(1f - progresoDisipacion);

        if (particulasHumo != null)
        {
            var main = particulasHumo.main;
            Color color = main.startColor.color;
            color.a = opacidad * 0.4f;
            main.startColor = color;
        }

        if (particulasNube != null)
        {
            var main = particulasNube.main;
            Color color = main.startColor.color;
            color.a = opacidad * 0.6f;
            main.startColor = color;
        }
    }

    void VerificarDisipacion()
    {
        bool jugadorQuieto = EstaJugadorQuieto();
        float tiempoDisipacion = jugadorQuieto ? tiempoDisipacionQuieto : tiempoDisipacionNormal;

        if (tiempoVida >= tiempoDisipacion)
        {
            Disipar();
        }
    }

    bool EstaJugadorQuieto()
    {
        if (jugador == null) return false;

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            return rb.velocity.magnitude < 0.1f;
        }

        return false;
    }

    void Disipar()
    {
        Debug.Log("Nube de esporas se disipa");

        RemoverEfectoVision();

        // Efecto de disipación
        StartCoroutine(DisipacionSuave());
    }

    System.Collections.IEnumerator DisipacionSuave()
    {
        float duracion = 1f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float progreso = tiempo / duracion;
            float opacidad = 1f - progreso;

            if (particulasHumo != null)
            {
                var main = particulasHumo.main;
                Color color = main.startColor.color;
                color.a = opacidad * 0.4f;
                main.startColor = color;
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnNube = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnNube = false;
        }
    }

    void OnDestroy()
    {
        RemoverEfectoVision();
    }
}