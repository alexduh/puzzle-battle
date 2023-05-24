using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Score : MonoBehaviour
{
    /**
     * Score = (10 * PC) * (CP + CB + GB)
     * PC = Number of puyo cleared in the chain.
     * CP = Chain Power (These values are listed in the Chain Power Table.)
     * CB = Color Bonus
     * GB = Group Bonus
     * The value of(CP + CB + GB) is limited to between 1 and 999 inclusive.
    */

    private TMP_Text scoreText;

    private int[] colorBonus = {0, 3, 6, 12, 24};
    private int[] chainPower = {0, 8, 16, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 480, 512, 544};
    private int[] groupBonus = {0, 2, 3, 4, 5, 6, 7, 10};
    private int score;

    public void UpdateScore(int newScore)
    {
        score = newScore;
        scoreText.text = score.ToString();
    }

    public void IncrementScore(int increase)
    {
        score += increase;
        UpdateScore(score);
    }

    public int CalculateScore(int puyoCleared, int chainCount, int numColors)
    {
        int multiplier = chainPower[chainCount-1] + colorBonus[numColors-1] + groupBonus[puyoCleared-4];
        multiplier = Mathf.Max(multiplier, 1);

        int connectScore = 10 * puyoCleared * multiplier;
        IncrementScore(connectScore);
        return connectScore;
    }

    // Start is called before the first frame update
    void Awake()
    {
        scoreText = this.GetComponent<TMP_Text>();
    }
}
