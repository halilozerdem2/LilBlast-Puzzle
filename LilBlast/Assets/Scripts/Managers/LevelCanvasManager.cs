using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject lostPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private GameOverHandler handler;

    private void Awake()
    {
        handler = FindAnyObjectByType<GameOverHandler>();
    }

    public void ActivateWinPanel()
    {
        DeactivateAllPanels();
        OpenPanel(winPanel);
        Time.timeScale = 0f;
    }

    public void ActivateLostPanel()
    {
        DeactivateAllPanels();
        OpenPanel(lostPanel);
        Time.timeScale = 0f;
    }

    public void Pause()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        DeactivateAllPanels();
        OpenPanel(pausePanel);
    }

    public void Resume()
    {
        DeactivateAllPanels();
        OpenPanel(gamePanel);
        GameManager.Instance.ResumeGame();
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartGame();
    }

    public void ReplayLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        int nextIndex = current.buildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            nextIndex = 0;
        SceneManager.LoadScene(nextIndex);
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }

    public void ContinueByAds()
    {
        handler.moves += 5;
        Resume();
    }

    public void ContinueByUsingExtraLife()
    {
        handler.IncreaseMoves();
        Resume();
    }

    public void ShowSettings()
    {
        OpenPanel(settingsPanel);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        ClosePanel(settingsPanel);
        Time.timeScale = 1f;
    }

    private void DeactivateAllPanels()
    {
        ClosePanel(gamePanel);
        ClosePanel(lostPanel);
        ClosePanel(winPanel);
        ClosePanel(pausePanel);
        ClosePanel(settingsPanel);
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(true);
    }

    private void ClosePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
        GameManager.Instance.ChangeState(GameManager.GameState.Menu);
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
