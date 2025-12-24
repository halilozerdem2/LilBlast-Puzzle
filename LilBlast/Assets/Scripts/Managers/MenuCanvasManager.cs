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

        var nextLevelIndex = ResolveNextLevelIndex();
        levelManager.LoadLevel(nextLevelIndex, nextLevelIndex);
        DeactivateAllPanels();
    }

    public void PlayAgain()
    {
        var currentLevel = Mathf.Max(LevelManager.GetLastCompletedLevel(), LevelManager.FirstGameplayLevelBuildIndex);
        levelManager.LoadLevel(currentLevel, currentLevel);
        DeactivateAllPanels();
    }

    public void NextLevel()
    {
        var nextLevelIndex = ResolveNextLevelIndex();
        levelManager.LoadLevel(nextLevelIndex, nextLevelIndex);
        DeactivateAllPanels();
    }

    public void LoadLevel(int index)
    {
        levelManager.LoadLevel(index, index);
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

    private int ResolveNextLevelIndex()
    {
        int lastCompleted = LevelManager.GetLastCompletedLevel();
        int nextIndex = lastCompleted + 1;
        if (nextIndex > LevelManager.LastGameplayLevelBuildIndex)
        {
            LevelManager.ResetProgress();
            nextIndex = LevelManager.FirstGameplayLevelBuildIndex;
        }

        return Mathf.Clamp(nextIndex, LevelManager.FirstGameplayLevelBuildIndex, LevelManager.LastGameplayLevelBuildIndex);
    }
}
