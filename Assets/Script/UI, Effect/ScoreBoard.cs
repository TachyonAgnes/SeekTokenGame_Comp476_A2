using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI seekerScoreText;
    public TextMeshProUGUI playerScoreText1;
    public TextMeshProUGUI seekerScoreText1;
    public TextMeshProUGUI tokenTotalText;
    public TextMeshProUGUI tokenTotalText1;
    public TextMeshProUGUI blizzardBomb;
    public TextMeshProUGUI report;
    public TextMeshProUGUI titleUI;

    public GameAgent GameAgent;
    public TokenSpawner tokenSpawner;

    private void Awake() {
        GameAgent = GameObject.FindObjectOfType<GameAgent>();
        tokenSpawner = FindObjectOfType<TokenSpawner>();
    }

    private void Update() {
        playerScoreText.text = " Player | " + GameAgent.playerScore.ToString();
        playerScoreText1.text = GameAgent.playerScore.ToString();
        seekerScoreText.text = " Seeker | " + GameAgent.seekerScore.ToString();
        seekerScoreText1.text = GameAgent.seekerScore.ToString();
        tokenTotalText.text = tokenSpawner.tokenTotal.ToString();
        tokenTotalText1.text = tokenSpawner.tokenTotal.ToString();
    }
}
