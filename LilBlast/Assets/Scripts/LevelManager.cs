using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private LevelProgress currentLevelProgress;
    private DateTime levelStartTime;
    private const string LastCompletedLevelKey = "LastCompletedLevel";
    [SerializeField] GridManager gridManager;
    [SerializeField] BlockManager blockManager;

    public static event Action OnLevelSceneLoaded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartLevel(int levelNumber)
    {
        // Yeni LevelProgress oluştur
        currentLevelProgress = new LevelProgress
        {
            LevelNumber = levelNumber,
            Attempts = 0,
            Failures = 0,
            Stars = 0,
            MovesLeft = 0,
            PowerUpsUsed = 0,
            CompletionTime = 0,
            CompletedAt = DateTime.MinValue
        };
        levelStartTime = DateTime.UtcNow;
    }
    // Level başarısız olduğunda
    public void FailLevel()
    {
        if (currentLevelProgress == null) return;

        currentLevelProgress.Attempts++;
        currentLevelProgress.Failures++;

        Debug.Log($"Level {currentLevelProgress.LevelNumber} failed. Attempts: {currentLevelProgress.Attempts}, Failures: {currentLevelProgress.Failures}");

        // Local olarak kaydetmek istersen PlayerPrefs veya Json
        string key = $"Level_{currentLevelProgress.LevelNumber}_Progress";
        PlayerPrefs.SetInt(key + "_Attempts", currentLevelProgress.Attempts);
        PlayerPrefs.SetInt(key + "_Failures", currentLevelProgress.Failures);
        PlayerPrefs.Save();
    }
     public void CompleteLevel(int score, int movesLeft, int powerUpsUsed)
    {
        if (currentLevelProgress == null) return;

        currentLevelProgress.Attempts++;
        currentLevelProgress.Stars = WinManager.Instance.CalculateStarCount(score);
        currentLevelProgress.MovesLeft = movesLeft;
        currentLevelProgress.PowerUpsUsed = powerUpsUsed;
                
        TimeSpan duration = DateTime.UtcNow - levelStartTime;
        currentLevelProgress.CompletionTime = (int)duration.TotalMinutes;
        currentLevelProgress.CompletedAt = DateTime.UtcNow;

        Debug.Log($"Level {currentLevelProgress.LevelNumber} completed with {currentLevelProgress.Stars} stars.");
        Debug.Log($"Attempts {currentLevelProgress.Attempts}, Moves Left: {currentLevelProgress.MovesLeft}, Power-Ups Used: {currentLevelProgress.PowerUpsUsed}, Completion Time: {currentLevelProgress.CompletionTime}s");
   

        SaveLevelProgressToLocal(currentLevelProgress.LevelNumber);
        PlayerDataManager.Instance.UpdateLevelProgress(currentLevelProgress);

    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameManager.Instance.Reset();
        int buildIndex = scene.buildIndex;

        Debug.Log($"Scene loaded: {scene.name} (Index: {buildIndex})");
        if(buildIndex >0) GameManager.Instance.ChangeState(GameManager.GameState.Play);
        StartLevel(buildIndex);
        OnLevelSceneLoaded?.Invoke();

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
                break;
        }
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    
    // En son tamamlanan seviyeyi kaydeder
    public static void SaveLevelProgressToLocal(int levelIndex)
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

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(LastCompletedLevelKey);
    }

}