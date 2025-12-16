using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class JQ_CINEMATICAPrimera : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName;
    void Start()
    {
        videoPlayer.loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
