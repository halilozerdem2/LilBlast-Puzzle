using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public int currentScore = 0;

    private void OnEnable()
    {
        BlockManager.OnBlockBlasted += UpdateScore;
    }
    private void OnDisable()
    {
        BlockManager.OnBlockBlasted -= UpdateScore;

    }
    private void UpdateScoreText()
    {
        scoreText.text = currentScore == 0 ? "000" : currentScore.ToString("D3");
        scoreText.text = currentScore.ToString();
    }

    public void UpdateScore(Block ablock)
    {
        currentScore += ablock.scoreEffect;
        UpdateScoreText();
    }
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
    }

}
