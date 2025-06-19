using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
public class CanvasManager : MonoBehaviour
{
    [SerializeField] LevelManager _levelmanager;
    [SerializeField] GameObject explanationPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject lostPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject levelsPanel;

    ShuffleManager shuffle;
    private void Awake()
    {
        shuffle = FindAnyObjectByType<ShuffleManager>();
        DontDestroyOnLoad(this);
    }

    public void ActivateMainMenu()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        Time.timeScale = 0f;
        DeactivateAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ActivateWinPanel()
    {
        DeactivateAllPanels();
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void ActivateSettingsPanel()
    {
        Time.timeScale = 0f;
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        settingsPanel.SetActive(true);
    }
    public void ActivateLevelsPanel()
    {
        DeactivateAllPanels();
        levelsPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Inform()
    {
        //DeactivateAllPanels();
        explanationPanel.SetActive(true);
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
        levelsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void DeactivateAllPanels()
    {
        explanationPanel.SetActive(false);
        levelsPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(false);
        lostPanel.SetActive(false);
        winPanel.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DeactivateAllPanels();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        GameManager.Instance.RestartGame();
        gamePanel.SetActive(true);

    }
    public void Play()
    {
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel());
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
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

    public void Order()
    {
        shuffle.HandleShuffle(true);
    }
    public void PlayAgain()
    {
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel()-1);
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }
    public void NextLevel()
    {
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel());
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        _levelmanager.LoadLevel(levelIndex);
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }
    
}
