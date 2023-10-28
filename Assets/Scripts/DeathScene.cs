using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        scoreTxt.text = "Score: " + GameManager.currentScore.ToString("F1");
        highscoreTxt.text = "High Score: " + GameManager.savedata.highScore.ToString("F1");

        if (GameManager.endInfo.infinite)
        {
            waveTxt.text = "Wave: " + GameManager.endInfo.wave.ToString();
        }
        else
        {
            waveTxt.text = "Story: " + GameManager.savedata.storyChapter.ToString();
        }
    }
}
