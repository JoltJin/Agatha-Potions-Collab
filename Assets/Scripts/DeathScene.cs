using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathScene : MonoBehaviour
{
    public TextMeshProUGUI scoreTxt;

    // Start is called before the first frame update
    void Start()
    {
        scoreTxt.text = "Score: " + GameManager.currentScore.ToString("F1");
    }
}
