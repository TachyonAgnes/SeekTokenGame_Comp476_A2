using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshProUGUI chaserScoreText;
    public TextMeshProUGUI evaderScoreText;
    public TextMeshProUGUI chaserScoreText1;
    public TextMeshProUGUI evaderScoreText1;
    public TextMeshProUGUI tokenTotalText;
    public TextMeshProUGUI tokenTotalText1;
    public TextMeshProUGUI report;
    public TextMeshProUGUI titleUI;
    public int tokenTotal = 0;

    public void UpdateScore(string colEntTag, int score, int tokenTotal)
    {
        if (colEntTag == "Chaser") {
            chaserScoreText.text = score.ToString();
            chaserScoreText1.text = score.ToString();
        }
        else if (colEntTag == "Evader") {
            evaderScoreText.text = score.ToString();
            evaderScoreText1.text = score.ToString();
        }
        tokenTotalText.text = tokenTotal.ToString();
        tokenTotalText1.text = tokenTotal.ToString();
    }
}
