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
    public TextMeshProUGUI scoreTxt, waveTxt;

    // Start is called before the first frame update
    void Start()
    {
        scoreTxt.text = "Score: " + GameManager.currentScore.ToString("F1");
        if (GameManager.endInfo.infinite)
        {
            waveTxt.text = "Wave: " + GameManager.endInfo.wave.ToString();
        }
        else
        {
            waveTxt.text = "Story: " + GameManager.storyWave.ToString();
        }
    }
}
