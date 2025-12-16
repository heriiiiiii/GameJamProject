using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance;

    [Header("📺 Post Processing Volume")]
    public Volume globalVolume;

    [Header("☀️ Slider de Brillo")]
    public Slider brightnessSlider;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            float savedBrightness = PlayerPrefs.GetFloat("Brightness", 0.5f);
            SetBrightness(savedBrightness);
            brightnessSlider.value = savedBrightness;
        }

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
        {
            // Brillo más suave y equilibrado
            colorAdjustments.postExposure.value = Mathf.Lerp(-0.5f, 0.5f, value);
            PlayerPrefs.SetFloat("Brightness", value);
        }
    }
}
 