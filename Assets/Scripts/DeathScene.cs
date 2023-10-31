using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct EndInfo
{
    public bool infinite;
    public int wave;

    public EndInfo(bool infinite, int wave)
    {
        this.infinite = infinite;
        this.wave = wave;
    }
}

public class DeathScene : MonoBehaviour
{
    public TextMeshProUGUI scoreTxt, waveTxt, highscoreTxt;

    // Start is called before the first frame update
    void Start()
    {
        scoreTxt.text = "Score: " + GameManager.currentScore.ToString("F0");
        highscoreTxt.text = "High Score: " + GameManager.savedata.highScore.ToString("F0");

        if (GameManager.endInfo.infinite)
        {
            waveTxt.text = "Wave: " + GameManager.endInfo.wave.ToString();
        }
        else
        {
            waveTxt.text = "Story: " + GameManager.savedata.storyChapter.ToString();
        }
    }

    public void ClickRetry()
    {
        SceneManager.LoadScene("Game");
    }

    public void ClickBack()
    {
        SceneManager.LoadScene("Start");
    }
}
