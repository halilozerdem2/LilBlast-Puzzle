using UnityEngine;

public class LosePanelController : MonoBehaviour
{
    [SerializeField] private LevelCanvasManager levelCanvas;

    private void Awake()
    {
        if (levelCanvas == null)
            levelCanvas = GetComponent<LevelCanvasManager>();

        if (levelCanvas == null)
            levelCanvas = FindAnyObjectByType<LevelCanvasManager>();
    }

    public void OpenAdsPanel()
    {
        if (levelCanvas != null)
        {
            levelCanvas.ContinueByAds();
            return;
        }

        ContinueGameFallback(5);
    }

    public void UseExtraLifeAndContinue()
    {
        if (levelCanvas != null)
        {
            levelCanvas.ContinueByUsingExtraLife();
            return;
        }

        GameOverHandler.Instance?.IncreaseMoves();
        GameManager.Instance?.ResumeGame();
    }

    public void GoToMainMenu()
    {
        if (levelCanvas != null)
        {
            levelCanvas.ReturnToMainMenu();
            return;
        }

        GameManager.Instance?.ChangeState(GameManager.GameState.Menu);
    }

    public void TryAgain()
    {
        if (levelCanvas != null)
        {
            levelCanvas.TryAgain();
            return;
        }

        Time.timeScale = 1f;
        GameManager.Instance?.RestartGame();
    }

    private void ContinueGameFallback(int extraMoves)
    {
        if (GameManager.Instance != null && GameManager.Instance._state == GameManager.GameState.Lose)
        {
            if (GameOverHandler.Instance != null)
                GameOverHandler.Instance.moves += extraMoves;
        }

        GameManager.Instance?.ResumeGame();
    }
}
