using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private LevelProgress currentLevelProgress;
    private DateTime levelStartTime;
    private const string LastCompletedLevelKey = "LastCompletedLevel";
    private const string LevelDifficultyKeyPrefix = "LevelDifficulty";
    private const string LevelProgressUserPrefsKey = "LilGames.LevelProgress.CurrentUser";
    private const string LevelProgressKeyPrefix = "LilGames.LevelProgress.";
    private const string GuestProgressKey = "guest";
    private const string LastDifficultySuffix = "LastDifficulty";
    private static string cachedProgressUser;
    private static readonly Dictionary<int, BackendLevelProgressData> backendProgressCache = new Dictionary<int, BackendLevelProgressData>();
    private static readonly Regex LevelNumberRegex = new Regex("(\\d+)$", RegexOptions.Compiled);
    public const int FirstGameplayLevelBuildIndex = 1;
    public const int LastGameplayLevelBuildIndex = 10;
    [SerializeField] GridManager gridManager;
    [SerializeField] BlockManager blockManager;

    public static event Action OnLevelSceneLoaded;
    public static event Action LevelProgressUpdated;

    private int? pendingLevelNumberOverride;

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

    public void StartLevel(int buildIndex, int levelNumber)
    {
        // Yeni LevelProgress oluştur
        currentLevelProgress = new LevelProgress
        {
            LevelNumber = levelNumber,
            SceneBuildIndex = buildIndex,
            LevelId = ResolveLevelId(buildIndex),
            Attempts = 0,
            Failures = 0,
            Stars = 0,
            MovesLeft = 0,
            MovesUsed = 0,
            PowerUpsUsed = 0,
            CompletionTime = 0,
            CompletedAt = DateTime.MinValue,
            AttemptReported = false,
            DifficultyUsed = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentDifficulty : Mathf.Clamp(levelNumber, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty)
        };
        levelStartTime = DateTime.UtcNow;
        ReportLevelAttemptForCurrentRun();
    }
    // Level başarısız olduğunda
    public void FailLevel()
    {
        if (currentLevelProgress == null) return;

        currentLevelProgress.Attempts++;
        currentLevelProgress.Failures++;

        // Local olarak kaydetmek istersen PlayerPrefs veya Json
        string key = BuildLevelProgressInfoKey(currentLevelProgress.LevelNumber);
        PlayerPrefs.SetInt(key + "_Attempts", currentLevelProgress.Attempts);
        PlayerPrefs.SetInt(key + "_Failures", currentLevelProgress.Failures);
        PlayerPrefs.Save();
        PlayerDataController.Instance?.RecordLevelFailure();

        ReportLevelAttemptForCurrentRun();
    }
    public void CompleteLevel(int score, int movesLeft, int powerUpsUsed, PowerUpUsageSnapshot usageSnapshot)
    {
        if (currentLevelProgress == null) return;

        ReportLevelAttemptForCurrentRun();
        currentLevelProgress.Attempts++;
        currentLevelProgress.MovesLeft = movesLeft;
        var handler = GameOverHandler.Instance;
        var difficultyManager = DifficultyManager.Instance;
        int configuredMoves = difficultyManager != null ? difficultyManager.CurrentConfig.moveCount : movesLeft;
        int totalMovesBudget = handler != null
            ? handler.TotalMovesGranted
            : Mathf.Max(movesLeft, configuredMoves);
        int movesUsed = handler != null
            ? handler.MovesUsed
            : Mathf.Max(0, totalMovesBudget - movesLeft);
        currentLevelProgress.MovesUsed = movesUsed;
        currentLevelProgress.PowerUpsUsed = powerUpsUsed;
                
        TimeSpan duration = DateTime.UtcNow - levelStartTime;
        currentLevelProgress.CompletionTime = Mathf.RoundToInt((float)duration.TotalSeconds);
        currentLevelProgress.CompletedAt = DateTime.UtcNow;
        float moveUsagePercent = totalMovesBudget <= 0 ? 1f : (float)movesUsed / totalMovesBudget;
        float completionMinutes = currentLevelProgress.CompletionTime / 60f;
        currentLevelProgress.Stars = WinManager.Instance.CalculateStarCount(score, moveUsagePercent, completionMinutes);
        Debug.Log($"Level {currentLevelProgress.LevelNumber} stars calculated: {currentLevelProgress.Stars} (score={score}, moveUsage={moveUsagePercent:F2}, time={completionMinutes:F2})");

   

        SaveLevelProgressToLocal(currentLevelProgress.LevelNumber);
        SaveLevelStarCount(currentLevelProgress.LevelNumber, currentLevelProgress.Stars);
        SaveLevelDifficulty(currentLevelProgress.LevelNumber,
            difficultyManager != null
                ? difficultyManager.CurrentDifficulty
                : Mathf.Clamp(currentLevelProgress.LevelNumber, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty));
        PlayerDataController.Instance?.RecordLevelCompletion(currentLevelProgress, score, usageSnapshot);
        DifficultyManager.Instance?.ReportLevelSuccess(currentLevelProgress);

        if (currentLevelProgress.LevelNumber >= LastGameplayLevelBuildIndex)
            ResetProgress();

        if (currentLevelProgress.LevelNumber >= FirstGameplayLevelBuildIndex)
        {
            var scoreLong = (long)Mathf.Max(0, score);
            int difficulty = ResolveCurrentRunDifficulty();
            RegisterBackendCompletionLocal(currentLevelProgress.LevelNumber, difficulty, currentLevelProgress.Stars, currentLevelProgress.PowerUpsUsed);
            LoginManager.Instance?.ReportLevelCompletion(
                currentLevelProgress.LevelNumber,
                difficulty,
                currentLevelProgress.Stars,
                currentLevelProgress.PowerUpsUsed,
                scoreLong);
        }
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplayScene = scene.buildIndex > 0;
        HandleSceneLoaded(scene, isGameplayScene);
    }

    private void HandleSceneLoaded(Scene scene, bool isGameplayScene)
    {
        int buildIndex = scene.buildIndex;
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

        var levelNumber = pendingLevelNumberOverride ?? (isGameplayScene ? ResolveDisplayedLevelNumber(scene) : buildIndex);
        StartLevel(buildIndex, levelNumber);
        pendingLevelNumberOverride = null;
        OnLevelSceneLoaded?.Invoke();
    }

    private void ReportLevelAttemptForCurrentRun()
    {
        if (currentLevelProgress == null)
            return;

        if (currentLevelProgress.AttemptReported)
            return;

        if (currentLevelProgress.LevelNumber < FirstGameplayLevelBuildIndex)
            return;

        int difficulty = ResolveCurrentRunDifficulty();
        currentLevelProgress.DifficultyUsed = difficulty;
        RegisterBackendAttemptLocal(currentLevelProgress.LevelNumber, difficulty);
        LoginManager.Instance?.ReportLevelAttempt(currentLevelProgress.LevelNumber, difficulty);
        currentLevelProgress.AttemptReported = true;
    }

    private int ResolveCurrentRunDifficulty()
    {
        if (currentLevelProgress != null && currentLevelProgress.DifficultyUsed >= DifficultyConfig.MinDifficulty)
            return Mathf.Clamp(currentLevelProgress.DifficultyUsed, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);

        var difficultyManager = DifficultyManager.Instance;
        if (difficultyManager != null)
            return difficultyManager.CurrentDifficulty;

        return LoadLevelDifficulty(currentLevelProgress != null ? currentLevelProgress.LevelNumber : FirstGameplayLevelBuildIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        var resolvedLevelNumber = ResolveLevelNumberFromBuildIndex(levelIndex);
        pendingLevelNumberOverride = resolvedLevelNumber;
        ApplyDifficultyForLevel(resolvedLevelNumber);
        SceneManager.LoadScene(levelIndex);
    }

    public void LoadLevel(int levelIndex, int levelNumber)
    {
        pendingLevelNumberOverride = levelNumber;
        ApplyDifficultyForLevel(levelNumber);
        SceneManager.LoadScene(levelIndex);
    }

    public LevelProgress CurrentLevelProgress => currentLevelProgress;
    
    // En son tamamlanan seviyeyi kaydeder
    public static void SaveLevelProgressToLocal(int levelIndex)
    {
        int savedLevel = GetLastCompletedLevel();
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        if (levelIndex > savedLevel)
        {
            PlayerPrefs.SetInt(BuildLastCompletedKey(), levelIndex);
            PlayerPrefs.Save();
        }
    }

    public static void SaveLevelStarCount(int levelIndex, int stars)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        int clampedStars = Mathf.Clamp(stars, 0, 3);
        PlayerPrefs.SetInt(BuildLevelStarsKey(levelIndex), clampedStars);
        PlayerPrefs.Save();

        if (clampedStars > 0)
        {
            var entry = GetOrCreateBackendEntry(levelIndex);
            entry.HighestStarsAchieved = Mathf.Max(entry.HighestStarsAchieved, clampedStars);
            NotifyLevelProgressChanged();
        }
    }

    public static int GetLevelStarCount(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        if (backendProgressCache.TryGetValue(levelIndex, out var entry) && entry != null)
            return Mathf.Clamp(entry.HighestStarsAchieved, 0, 3);

        return ReadIntWithLegacy(BuildLevelStarsKey(levelIndex), BuildLegacyLevelStarsKey(levelIndex), 0);
    }

    public static void RecordUpcomingDifficulty(int difficulty)
    {
        int targetLevel = Mathf.Clamp(GetLastCompletedLevel() + 1, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        SaveLevelDifficulty(targetLevel, difficulty);
    }

    public static void SaveLevelDifficulty(int levelIndex, int difficulty)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        int clampedDifficulty = Mathf.Clamp(difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        PlayerPrefs.SetInt(BuildLevelDifficultyKey(levelIndex), clampedDifficulty);
        PlayerPrefs.Save();

        if (backendProgressCache.TryGetValue(levelIndex, out var entry) && entry != null)
            entry.Difficulty = clampedDifficulty;
    }

    public static int LoadLevelDifficulty(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        if (backendProgressCache.TryGetValue(levelIndex, out var backendEntry) && backendEntry != null && backendEntry.Difficulty >= DifficultyConfig.MinDifficulty)
            return Mathf.Clamp(backendEntry.Difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        int fallback = LoadLastAppliedDifficulty(Mathf.Clamp(levelIndex, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty));
        return ReadIntWithLegacy(BuildLevelDifficultyKey(levelIndex), BuildLegacyLevelDifficultyKey(levelIndex), fallback);
    }

    // Kaydedilen son seviyeyi getirir (başlangıçta 0 döner)
    public static int GetLastCompletedLevel()
    {
        int backendLast = FirstGameplayLevelBuildIndex - 1;
        foreach (var kvp in backendProgressCache)
        {
            var progress = kvp.Value;
            if (progress == null)
                continue;

            bool completed = progress.Wins > 0 || progress.HighestStarsAchieved > 0;
            if (completed)
                backendLast = Mathf.Max(backendLast, kvp.Key);
        }

        if (backendLast >= FirstGameplayLevelBuildIndex)
            return backendLast;

        int last = ReadIntWithLegacy(BuildLastCompletedKey(), LastCompletedLevelKey, FirstGameplayLevelBuildIndex - 1);
        return Mathf.Clamp(last, FirstGameplayLevelBuildIndex - 1, LastGameplayLevelBuildIndex);
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt(BuildLastCompletedKey(), FirstGameplayLevelBuildIndex - 1);
        for (int level = FirstGameplayLevelBuildIndex; level <= LastGameplayLevelBuildIndex; level++)
        {
            string key = BuildLevelProgressInfoKey(level);
            PlayerPrefs.DeleteKey(key + "_Attempts");
            PlayerPrefs.DeleteKey(key + "_Failures");
            PlayerPrefs.DeleteKey(BuildLevelStarsKey(level));
            PlayerPrefs.DeleteKey(BuildLevelDifficultyKey(level));
        }
        PlayerDataController.ResetLocalData();
    }

    public static bool IsLevelCompleted(int levelIndex)
    {
        return levelIndex <= GetLastCompletedLevel();
    }

    private static string BuildLevelStarsKey(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        return BuildProgressKey($"Level_{levelIndex}_Stars");
    }

    private static string BuildLegacyLevelStarsKey(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        return $"Level_{levelIndex}_Stars";
    }

    private static string BuildLevelDifficultyKey(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        return BuildProgressKey($"{LevelDifficultyKeyPrefix}{levelIndex}");
    }

    private static string BuildLegacyLevelDifficultyKey(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        return $"LevelDifficulty.{levelIndex}";
    }

    public static void SetActiveProgressUser(string userId)
    {
        cachedProgressUser = string.IsNullOrEmpty(userId) ? GuestProgressKey : userId;
        PlayerPrefs.SetString(LevelProgressUserPrefsKey, cachedProgressUser);
        PlayerPrefs.Save();
        ClearCachedBackendProgress();
    }

    private static string ResolveProgressUser()
    {
        if (!string.IsNullOrEmpty(cachedProgressUser))
            return cachedProgressUser;

        var sessionUser = LoginManager.Instance?.CurrentSession?.UserId;
        if (!string.IsNullOrEmpty(sessionUser))
        {
            cachedProgressUser = sessionUser;
            PlayerPrefs.SetString(LevelProgressUserPrefsKey, cachedProgressUser);
            PlayerPrefs.Save();
            return cachedProgressUser;
        }

        cachedProgressUser = PlayerPrefs.GetString(LevelProgressUserPrefsKey, GuestProgressKey);
        if (string.IsNullOrEmpty(cachedProgressUser))
            cachedProgressUser = GuestProgressKey;
        return cachedProgressUser;
    }

    private static string BuildProgressKey(string suffix)
    {
        return $"{LevelProgressKeyPrefix}{ResolveProgressUser()}.{suffix}";
    }

    private static string BuildLastCompletedKey()
    {
        return BuildProgressKey(LastCompletedLevelKey);
    }

    private static string BuildLevelProgressInfoKey(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        return BuildProgressKey($"Level_{levelIndex}_Progress");
    }

    public static void SetBackendProgress(IEnumerable<BackendLevelProgressData> entries)
    {
        ClearCachedBackendProgress();
        if (entries == null)
            return;

        int lastCompleted = FirstGameplayLevelBuildIndex - 1;
        foreach (var entry in entries)
        {
            if (entry == null)
                continue;

            var level = Mathf.Clamp(entry.LevelNumber, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
            var clone = new BackendLevelProgressData(entry);
            backendProgressCache[level] = clone;

            if (clone.HighestStarsAchieved > 0)
                PlayerPrefs.SetInt(BuildLevelStarsKey(level), Mathf.Clamp(clone.HighestStarsAchieved, 0, 3));

            if (clone.Difficulty >= DifficultyConfig.MinDifficulty)
                PlayerPrefs.SetInt(BuildLevelDifficultyKey(level), Mathf.Clamp(clone.Difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty));

            bool completed = clone.Wins > 0 || clone.HighestStarsAchieved > 0;
            if (completed)
                lastCompleted = Mathf.Max(lastCompleted, level);
        }

        PlayerPrefs.SetInt(BuildLastCompletedKey(), lastCompleted);
        PlayerPrefs.Save();
        NotifyLevelProgressChanged();
    }

    public static void ClearCachedBackendProgress()
    {
        backendProgressCache.Clear();
        PlayerPrefs.SetInt(BuildLastCompletedKey(), FirstGameplayLevelBuildIndex - 1);
        PlayerPrefs.Save();
        NotifyLevelProgressChanged();
    }

    private static int ReadIntWithLegacy(string primaryKey, string legacyKey, int defaultValue)
    {
        if (PlayerPrefs.HasKey(primaryKey))
            return PlayerPrefs.GetInt(primaryKey, defaultValue);

        if (!string.IsNullOrEmpty(legacyKey) && PlayerPrefs.HasKey(legacyKey))
        {
            int legacyValue = PlayerPrefs.GetInt(legacyKey, defaultValue);
            PlayerPrefs.SetInt(primaryKey, legacyValue);
            PlayerPrefs.Save();
            return legacyValue;
        }

        return defaultValue;
    }

    public static void SaveLastAppliedDifficulty(int difficulty)
    {
        int clamped = Mathf.Clamp(difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        PlayerPrefs.SetInt(BuildProgressKey(LastDifficultySuffix), clamped);
        PlayerPrefs.Save();
    }

    public static int LoadLastAppliedDifficulty(int fallback)
    {
        int clampedFallback = Mathf.Clamp(fallback, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        return PlayerPrefs.GetInt(BuildProgressKey(LastDifficultySuffix), clampedFallback);
    }

    private static void RegisterBackendAttemptLocal(int levelNumber, int difficulty)
    {
        if (levelNumber < FirstGameplayLevelBuildIndex)
            return;

        var entry = GetOrCreateBackendEntry(levelNumber);
        entry.Attempts = Mathf.Max(0, entry.Attempts) + 1;
        entry.LastPlayedAt = DateTime.UtcNow.ToString("o");
        entry.Difficulty = Mathf.Clamp(difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        NotifyLevelProgressChanged();
    }

    private static void RegisterBackendCompletionLocal(int levelNumber, int difficulty, int stars, int usedPowerups)
    {
        if (levelNumber < FirstGameplayLevelBuildIndex)
            return;

        var entry = GetOrCreateBackendEntry(levelNumber);
        entry.Wins = Mathf.Max(0, entry.Wins) + 1;
        entry.HighestStarsAchieved = Mathf.Max(entry.HighestStarsAchieved, Mathf.Clamp(stars, 0, 3));
        entry.TotalPowerupsUsedInThisLevel = Mathf.Max(0, entry.TotalPowerupsUsedInThisLevel) + Mathf.Max(0, usedPowerups);
        entry.LastPlayedAt = DateTime.UtcNow.ToString("o");
        entry.Difficulty = Mathf.Clamp(difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        NotifyLevelProgressChanged();
    }

    private static BackendLevelProgressData GetOrCreateBackendEntry(int levelNumber)
    {
        levelNumber = Mathf.Clamp(levelNumber, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        if (!backendProgressCache.TryGetValue(levelNumber, out var entry) || entry == null)
        {
            entry = new BackendLevelProgressData { LevelNumber = levelNumber };
            backendProgressCache[levelNumber] = entry;
        }

        return entry;
    }

    private static void NotifyLevelProgressChanged()
    {
        LevelProgressUpdated?.Invoke();
    }

    private void ApplyDifficultyForLevel(int levelNumber)
    {
        var difficultyManager = DifficultyManager.Instance;
        if (difficultyManager == null)
            return;

        int difficulty = LoadLevelDifficulty(levelNumber);
        difficultyManager.ApplyDifficulty(difficulty, false, false);
        SaveLevelDifficulty(levelNumber, difficulty);
    }

    private int ResolveDisplayedLevelNumber(Scene scene)
    {
        if (!scene.IsValid())
            return FirstGameplayLevelBuildIndex;

        var name = scene.name;
        if (!string.IsNullOrEmpty(name))
        {
            var match = LevelNumberRegex.Match(name);
            if (match.Success && int.TryParse(match.Value, out var parsed))
                return Mathf.Clamp(parsed, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        }

        return Mathf.Clamp(scene.buildIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
    }

    private int ResolveLevelNumberFromBuildIndex(int buildIndex)
    {
        var scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        if (!string.IsNullOrEmpty(scenePath))
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            var match = LevelNumberRegex.Match(fileName);
            if (match.Success && int.TryParse(match.Value, out var parsed))
                return Mathf.Clamp(parsed, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
        }

        return Mathf.Clamp(buildIndex, FirstGameplayLevelBuildIndex, LastGameplayLevelBuildIndex);
    }

    private string ResolveLevelId(int buildIndex)
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        if (string.IsNullOrEmpty(scenePath))
            scenePath = $"Level_{buildIndex}";

        return ComputeStableGuid(scenePath);
    }

    private static string ComputeStableGuid(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Guid.Empty.ToString();

        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash).ToString();
        }
    }

    [Serializable]
    public class BackendLevelProgressData
    {
        public string Id;
        public int LevelNumber;
        public int Attempts;
        public int Wins;
        public int HighestStarsAchieved;
        public string LastPlayedAt;
        public int TotalPowerupsUsedInThisLevel;
        public int Difficulty;

        public BackendLevelProgressData()
        {
        }

        public BackendLevelProgressData(BackendLevelProgressData other)
        {
            if (other == null)
                return;

            Id = other.Id;
            LevelNumber = other.LevelNumber;
            Attempts = other.Attempts;
            Wins = other.Wins;
            HighestStarsAchieved = other.HighestStarsAchieved;
            LastPlayedAt = other.LastPlayedAt;
            TotalPowerupsUsedInThisLevel = other.TotalPowerupsUsedInThisLevel;
            Difficulty = other.Difficulty;
        }
    }
}

[Serializable]
public class LevelProgress
{
    public int LevelNumber;
    public int SceneBuildIndex;
    public string LevelId;
    public int Stars;
    public int CompletionTime;
    public DateTime CompletedAt;
    public int Attempts;
    public int Failures;
    public int MovesLeft;
    public int MovesUsed;
    public int PowerUpsUsed;
    public bool AttemptReported;
    public int DifficultyUsed;
}
