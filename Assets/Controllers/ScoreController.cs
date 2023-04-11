using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public TMP_Text TextComponent;
    public static int score;
    public int previousScore;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (previousScore != score)
        {
            TextComponent.text = ("Score : " + score);
            previousScore = score;
        }
    }
}
