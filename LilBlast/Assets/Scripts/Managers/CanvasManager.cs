using System.Runtime.InteropServices;
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

    [SerializeField] GameOverHandler handler;
    public GameObject bombanimation;


    [SerializeField] private LowerPanelButtonHandler lowerPanelButtonHandler;

    ShuffleManager shuffle;
    private void Awake()
    {
        shuffle = FindAnyObjectByType<ShuffleManager>();
        DontDestroyOnLoad(this);
    }

    public void ActivateMainMenu()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        DeactivateAllPanels();
        OpenPanel(mainMenuPanel);
    }

    public void ActivateWinPanel()
    {
        DeactivateAllPanels();
        OpenPanel(winPanel);
        Time.timeScale = 0f;
    }
    public void ActivateSettingsPanel()
    {
        Time.timeScale = 0f;
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        OpenPanel(settingsPanel);
    }
    public void Inform()
    {
        //DeactivateAllPanels();
        OpenPanel(explanationPanel);
        Time.timeScale = 0f;
    }
    public void Pause()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Pause);
        DeactivateAllPanels();
        OpenPanel(pausePanel);
    }

    public void ActivateLostPanel()
    {
        OpenPanel(lostPanel);
        Time.timeScale = 0f;
    }

    public void DeactivateAllPanels()
    {
        ClosePanel(explanationPanel);
        ClosePanel(mainMenuPanel);
        ClosePanel(gamePanel);
        ClosePanel(lostPanel);
        ClosePanel(winPanel);
        ClosePanel(pausePanel);
    }

    
    public void Resume()
    {
        DeactivateAllPanels();
        OpenPanel(gamePanel);
        GameManager.Instance.ResumeGame();

    }

    public void TryAgain()
    {
        GameManager.Instance.RestartGame();
        GameManager.Instance.Reset();
        DeactivateAllPanels();
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        OpenPanel(gamePanel);
    }
    public void Play()
    {
        if (lowerPanelButtonHandler.activeButton.name != "Play Button") return; 
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel()+1);
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        DeactivateAllPanels();
        OpenPanel(gamePanel);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void PlayAgain()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel());
        GameManager.Instance.ResumeGame();
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }
    public void NextLevel()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        SceneManager.LoadScene(LevelManager.GetLastCompletedLevel()+1);
        GameManager.Instance.ResumeGame();
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Play);
        _levelmanager.LoadLevel(levelIndex);
        DeactivateAllPanels();
        gamePanel.SetActive(true);
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }
//-------------------------New coding Logic---------------------------
    private void OpenPanel(GameObject aPanel)
    {
        aPanel.SetActive(true);
    }

    private void ClosePanel(GameObject aPanel)
    {
        aPanel.SetActive(false);
    }
    
    public void ContinueByAds()
    {
        //Ads Logic
        GameOverHandler.Instance.moves += 5;
        Resume();
    }

    public void ContinueByUsingExtraLife()
    {
        //Use Extra Life Logic
        GameOverHandler.Instance.IncreaseMoves();
        Resume();
    }
    
}
