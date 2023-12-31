using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    public TextMeshProUGUI storyTxt;
    // Start is called before the first frame update
    void Start()
    {
        SaveManager.LoadData();
        storyTxt.text = "Story Mode: Ch." + GameManager.savedata.storyChapter;
    }

    public void ClickStory()
    {
        GameManager.infiniteRandomMode = false;
        SceneManager.LoadScene("Game");
    }

    public void ClickInfinite()
    {
        GameManager.infiniteRandomMode = true;
        SceneManager.LoadScene("Game");
    }
}
