using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    private TMP_Text TextComponent;
    public static int score;
    public int previousScore;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = GameObject.FindGameObjectsWithTag("Score")[0];
        TextComponent = g.GetComponent<TMP_Text>();
        //score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (previousScore != score)
        {
            TextComponent.text = ("Score\n" + score);
            previousScore = score;
        }
    }
}
