using UnityEngine;
public class CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject lostPanel;
    [SerializeField] GameObject winPanel;

    public void ActivateMenuPanel()
    {
        Time.timeScale = 0f;
        DeactivateAllPanels();
        menuPanel.SetActive(true);
    }

    public void ActivateWinPanel()
    {
        DeactivateAllPanels();
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ActivateLostPanel()
    {
        DeactivateAllPanels();
        lostPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void DeactivateAllPanels()
    {
        menuPanel.SetActive(false);
        gamePanel.SetActive(false);
        lostPanel.SetActive(false);
        winPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Play()
    {
        DeactivateAllPanels();
        GameManager.Instance.RestartGame();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        gamePanel.SetActive(true);
    }
    public void Resume()
    {
        DeactivateAllPanels();
        gamePanel.SetActive(true);
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }

    public void TryAgain()
    {
        DeactivateAllPanels();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        gamePanel.SetActive(true);

    }

    public void Quit()
    {
        Application.Quit();
    }
}
