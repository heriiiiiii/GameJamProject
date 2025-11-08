using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("🎧 Audio")]
    public AudioMixer mainMixer; // Arrastra tu MainMixer aquí
    [Range(0f, 1f)] public float volume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cargar volumen guardado
        volume = PlayerPrefs.GetFloat("Volume", 1f);
        SetVolume(volume);
    }

    // 🔊 Ajuste de volumen
    public void SetVolume(float value)
    {
        volume = value;
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat("Volume", value);
    }
}
