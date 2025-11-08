using System.Collections;
using UnityEngine;

public class NF_DamageFlash : MonoBehaviour
{
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float flashTime = 0.25f;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;
    private Coroutine _damageFlashCoroutine;

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        Init(); // ⚡ LLAMAMOS Init() aquí
    }

    private void Init()
    {
        _materials = new Material[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            // 🧠 Cada renderer tiene su propia copia del material
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    public void CallDamageFlash()
    {
        if (_damageFlashCoroutine != null)
            StopCoroutine(_damageFlashCoroutine);

        _damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        // 🔥 Activa el color de flash
        SetFlashColor(_flashColor);

        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            float currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / flashTime);
            SetFlashAmount(currentFlashAmount);
            yield return null;
        }

        // 💨 Asegura volver al estado normal
        SetFlashAmount(0f);
    }

    private void SetFlashColor(Color color)
    {
        if (_materials == null) return;

        for (int i = 0; i < _materials.Length; i++)
        {
            if (_materials[i].HasProperty("_FlashColor"))
                _materials[i].SetColor("_FlashColor", color);
        }
    }

    private void SetFlashAmount(float amount)
    {
        if (_materials == null) return;

        for (int i = 0; i < _materials.Length; i++)
        {
            if (_materials[i].HasProperty("_FlashAmount"))
                _materials[i].SetFloat("_FlashAmount", amount);
        }
    }
}
