using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCanvasManager : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject explanationPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lowerPanel;
    [SerializeField] private LowerPanelButtonHandler lowerPanelButtonHandler;

    private void Awake()
    {
        if (levelManager == null)
            levelManager = FindAnyObjectByType<LevelManager>();
    }

    public void ActivateMainMenu()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Menu);
        DeactivateAllPanels();
        OpenPanel(mainMenuPanel);
        if (lowerPanel != null) lowerPanel.SetActive(true);
    }

    public void Play()
    {
        if (lowerPanelButtonHandler != null && lowerPanelButtonHandler.activeButton.name != "Play Button")
            return;

        var nextLevelIndex = LevelManager.GetLastCompletedLevel() + 1;
        SceneManager.LoadScene(nextLevelIndex);
        DeactivateAllPanels();
    }

    public void PlayAgain()
    {
        var currentLevel = LevelManager.GetLastCompletedLevel();
        SceneManager.LoadScene(currentLevel);
        DeactivateAllPanels();
    }

    public void NextLevel()
    {
        var nextLevelIndex = LevelManager.GetLastCompletedLevel() + 1;
        SceneManager.LoadScene(nextLevelIndex);
        DeactivateAllPanels();
    }

    public void LoadLevel(int index)
    {
        levelManager.LoadLevel(index);
        DeactivateAllPanels();
    }

    public void Inform()
    {
        OpenPanel(explanationPanel);
        Time.timeScale = 0f;
    }

    public void CloseExplanation()
    {
        ClosePanel(explanationPanel);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void DeactivateAllPanels()
    {
        ClosePanel(explanationPanel);
        ClosePanel(settingsPanel);
        ClosePanel(mainMenuPanel);
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
}
