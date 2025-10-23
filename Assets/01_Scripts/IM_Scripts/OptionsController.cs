using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [Header("🎧 Sliders")]
    public Slider volumeSlider;
    public Slider brightnessSlider;

    private void Start()
    {
        // Inicializar slider de volumen
        if (SettingsManager.Instance != null)
            volumeSlider.value = SettingsManager.Instance.volume;

        // Inicializar slider de brillo
        if (BrightnessManager.Instance != null)
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 0.5f);

        // Asignar listeners
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChange);
    }

    private void OnVolumeChange(float value)
    {
        SettingsManager.Instance?.SetVolume(value);
    }

    private void OnBrightnessChange(float value)
    {
        BrightnessManager.Instance?.SetBrightness(value);
    }
}
