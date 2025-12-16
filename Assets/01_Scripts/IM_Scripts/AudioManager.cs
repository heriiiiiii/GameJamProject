using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton

    [Header("Mixer Principal")]
    public AudioMixer mainMixer;

    [Header("Fuentes")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Garantiza que solo exista uno
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Escucha cambios de escena
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // Si salimos del menú principal, detiene la música
        if (newScene.name != "MainMenu")
        {
            StopMusic();
        }
    }

    // --- MÉTODOS DE USO GENERAL ---
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // --- CONTROL DE VOLUMEN ---
    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
}
