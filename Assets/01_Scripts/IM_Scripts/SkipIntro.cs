using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SkipIntroTMP : MonoBehaviour
{
    public TMP_Text blinkText;
    public string nextSceneName = "JQ_Cinematica";
    public float blinkSpeed = 2f;

    void Update()
    {
        if (blinkText != null)
        {
            var c = blinkText.color;
            c.a = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            blinkText.color = c;
        }

        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
