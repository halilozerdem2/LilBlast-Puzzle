using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private const string LastCompletedLevelKey = "LastCompletedLevel";
    [SerializeField] GridManager gridManager;
    [SerializeField] BlockManager blockManager;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int buildIndex = scene.buildIndex;

        Debug.Log($"Scene loaded: {scene.name} (Index: {buildIndex})");

        switch (buildIndex)
        {
            case 0: // Level 1
                //gridManager.InitializeGrid();
                break;

            case 1: // Level 2
                gridManager.InitializeGrid();
                break;

            case 2: // Level 3
                gridManager.InitializeGrid();
                break;
            case 3: // Level 4
                gridManager.InitializeGrid();
                break;
            case 4: // Level 5
                gridManager.InitializeGrid();
                break;
            case 5: // Level 6
                gridManager.InitializeGrid();
                break;

            default:
                HandleGenericLevel(buildIndex);
                break;
        }
    }

    void HandleMainMenu()
    {
        Debug.Log("Main menu scene loaded.");
    }
    
   
    void HandleGenericLevel(int index)
    {
        Debug.Log($"Generic level scene loaded: {index}");
        // Diğer sahnelerde ortak yapılacak işler (örneğin grid setup, UI açma vs.)
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    
    // En son tamamlanan seviyeyi kaydeder
    public static void SaveLevelProgress(int levelIndex)
    {
        int savedLevel = GetLastCompletedLevel();
        if (levelIndex > savedLevel)
        {
            PlayerPrefs.SetInt(LastCompletedLevelKey, levelIndex);
            PlayerPrefs.Save();
        }
    }

    // Kaydedilen son seviyeyi getirir (başlangıçta 0 döner)
    public static int GetLastCompletedLevel()
    {
        return PlayerPrefs.GetInt(LastCompletedLevelKey, 1);
    }

    // Progress'i sıfırlamak istersen
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LastCompletedLevelKey);
    }

}