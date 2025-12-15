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
        PlayerDataController.Instance?.RecordLevelFailure();
    }
     public void CompleteLevel(int score, int movesLeft, int powerUpsUsed, PowerUpUsageSnapshot usageSnapshot)
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
        PlayerDataController.Instance?.RecordLevelCompletion(currentLevelProgress, score, usageSnapshot);

    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.buildIndex > 0;
        HandleSceneLoaded(scene, isGameplayScene);
    }

    private void HandleSceneLoaded(Scene scene, bool isGameplayScene)
    {
        int buildIndex = scene.buildIndex;
        Debug.Log($"Scene loaded: {scene.name} (Index: {buildIndex})");

        if (isGameplayScene)
        {
            gridManager = FindObjectOfType<GridManager>();
            blockManager = FindObjectOfType<BlockManager>();
            GameManager.Instance.ChangeState(GameManager.GameState.Play);
            gridManager?.InitializeGrid();
        }
        else
        {
            gridManager = null;
            blockManager = null;
        }

        StartLevel(buildIndex);
        OnLevelSceneLoaded?.Invoke();
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

[Serializable]
public class LevelProgress
{
    public int LevelNumber;
    public int Stars;
    public int CompletionTime;
    public DateTime CompletedAt;
    public int Attempts;
    public int Failures;
    public int MovesLeft;
    public int PowerUpsUsed;
}
