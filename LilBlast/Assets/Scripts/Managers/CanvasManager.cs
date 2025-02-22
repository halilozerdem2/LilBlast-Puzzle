using UnityEngine;
public class CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject lostPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject pausePanel;
    ShuffleManager shuffle;
    private void Awake()
    {
        shuffle = FindAnyObjectByType<ShuffleManager>();
    }

    public void ActivateMainMenu()
    {
        Time.timeScale = 0f;
        DeactivateAllPanels();
        mainMenuPanel.SetActive(true);
        GameManager.Instance.ChangeState(GameManager.GameState.Menu);
    }

    public void ActivateWinPanel()
    {
        DeactivateAllPanels();
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Pause()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        DeactivateAllPanels();
        pausePanel.SetActive(true);
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
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(false);
        lostPanel.SetActive(false);
        winPanel.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PlayAgain()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
        GameManager.Instance.RestartGame();
        gamePanel.SetActive(true);
    }
    public void Resume()
    {
        DeactivateAllPanels();
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }

    public void TryAgain()
    {
        DeactivateAllPanels();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        GameManager.Instance.RestartGame();
        gamePanel.SetActive(true);

    }
    public void Play()
    {
        DeactivateAllPanels();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        gamePanel.SetActive(true);

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Shuffle()
    {
        shuffle.HandleShuffle();
    }
}
