using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject lostPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private int mainMenuBuildIndex = 0;
    [SerializeField] private int firstGameplayLevelBuildIndex = LevelManager.FirstGameplayLevelBuildIndex;
    [SerializeField] private int lastGameplayLevelBuildIndex = LevelManager.LastGameplayLevelBuildIndex;

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
        var manager = LevelManager.Instance;
        if (manager != null)
        {
            var levelNumber = manager.CurrentLevelProgress != null
                ? manager.CurrentLevelProgress.LevelNumber
                : current.buildIndex;
            manager.LoadLevel(current.buildIndex, levelNumber);
        }
        else
        {
            SceneManager.LoadScene(current.buildIndex);
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        int nextIndex = current.buildIndex + 1;
        int nextLevelNumber = LevelManager.Instance?.CurrentLevelProgress != null
            ? LevelManager.Instance.CurrentLevelProgress.LevelNumber + 1
            : nextIndex;

        if (current.buildIndex >= lastGameplayLevelBuildIndex)
        {
            LevelManager.ResetProgress();
            nextIndex = Mathf.Max(firstGameplayLevelBuildIndex, 1);
            nextLevelNumber = LevelManager.FirstGameplayLevelBuildIndex;
        }

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextIndex = Mathf.Max(firstGameplayLevelBuildIndex, 1);
            nextLevelNumber = Mathf.Max(LevelManager.FirstGameplayLevelBuildIndex, 1);
        }

        var manager = LevelManager.Instance;
        if (manager != null)
            manager.LoadLevel(nextIndex, nextLevelNumber);
        else
            SceneManager.LoadScene(nextIndex);
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }

    public void ContinueByAds()
    {
        handler.AddMoves(5);
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
