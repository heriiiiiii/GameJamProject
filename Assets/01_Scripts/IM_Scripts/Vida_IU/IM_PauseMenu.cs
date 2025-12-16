using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class IM_PauseMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Botón inicial al pausar (CONTINUE)")]
    public GameObject firstButton;

    [Header("Botón RETURN dentro de OPTIONS")]
    public GameObject returnButton;

    [Header("Fade")]
    public float fadeTime = 0.35f;

    private CanvasGroup pauseCG;
    private CanvasGroup optionsCG;

    private bool isPaused = false;


    void Start()
    {
        pauseCG = GetOrAddCanvasGroup(pausePanel);
        optionsCG = GetOrAddCanvasGroup(optionsPanel);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        pauseCG.alpha = 0;
        optionsCG.alpha = 0;

        Time.timeScale = 1;
    }


    void Update()
    {
        // Si está en Options y presiona ESC → vuelve
        if (isPaused && optionsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseOptions();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }


    // ================= PAUSE =================
    public void PauseGame()
    {
        isPaused = true;

        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        StartCoroutine(FadeCanvas(pauseCG, 0, 1));
        Time.timeScale = 0;

        SelectUI(firstButton);
    }


    // ================= RESUME =================
    public void ResumeGame()
    {
        isPaused = false;
        StartCoroutine(ClosePauseRoutine());
    }

    IEnumerator ClosePauseRoutine()
    {
        yield return FadeCanvas(pauseCG, 1, 0);

        pausePanel.SetActive(false);
        Time.timeScale = 1;
        ClearUI();
    }


    // ================= OPEN OPTIONS =================
    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);

        StartCoroutine(FadeCanvas(optionsCG, 0, 1));

        // ENCRIPTADO: Aquí ganamos contra el slider 🍄
        SelectUI(returnButton);
    }


    // ================= CLOSE OPTIONS =================
    public void CloseOptions()
    {
        StartCoroutine(CloseOptionsRoutine());
    }

    IEnumerator CloseOptionsRoutine()
    {
        yield return FadeCanvas(optionsCG, 1, 0);

        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        StartCoroutine(FadeCanvas(pauseCG, 0, 1));

        SelectUI(firstButton);
    }


    // ================= GO TO MAIN MENU =================
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }


    // ================= UTILIDAD =================
    CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to)
    {
        cg.alpha = from;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }

        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void ClearUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    void SelectUI(GameObject obj)
    {
        StartCoroutine(SelectAfterFrame(obj));
    }

    IEnumerator SelectAfterFrame(GameObject obj)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(obj);
    }
}
