using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Collects per-level telemetry and interprets backend difficulty recommendations.
/// Keeps data locally until the backend contract is ready so other systems can query the
/// latest snapshot and suggested difficulty.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyTrackingSnapshot latestSnapshot = new DifficultyTrackingSnapshot();
    [SerializeField] private DifficultyServiceResponse latestResponse = new DifficultyServiceResponse();
    [SerializeField] private DifficultySettings currentSettings = DifficultySettings.CreateDefault();
    [SerializeField] private DifficultyTier currentTier = DifficultyTier.Easy;
    [SerializeField] private string cachedOfflinePlayerId;
    [Header("Difficulty Backend")]
    [SerializeField] private string predictionRoute = "/difficulty/predict";
    [SerializeField] private string fallbackPredictionUrl = "https://api.lilgamelabs.com/difficulty/predict";

    public DifficultyTrackingSnapshot LatestSnapshot => latestSnapshot;
    public DifficultyServiceResponse LatestResponse => latestResponse;
    public DifficultySettings CurrentSettings => currentSettings;
    public DifficultyTier CurrentTier => currentTier;

    public event Action<DifficultyTier> DifficultyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplySwitchFor(currentTier);
    }

    /// <summary>
    /// Called once the player wins a level. Collects the payload we will send to the backend.
    /// </summary>
    public void ReportLevelSuccess(LevelProgress progress)
    {
        if (progress == null)
            return;

        latestSnapshot.playerId = ResolvePlayerId();
        latestSnapshot.attempts = Mathf.Max(0f, progress.Attempts);
        latestSnapshot.stars = Mathf.Max(0f, progress.Stars);
        latestSnapshot.powerups = Mathf.Max(0f, progress.PowerUpsUsed);

        var handler = GameOverHandler.Instance;
        var movesUsed = progress.MovesUsed;
        if (movesUsed <= 0)
        {
            if (handler != null)
                movesUsed = handler.MovesUsed;
            else
                movesUsed = Mathf.Max(0, progress.MovesLeft);
        }

        var totalMovesBudget = handler != null ? handler.TotalMovesGranted : movesUsed + Mathf.Max(0, progress.MovesLeft);
        if (totalMovesBudget <= 0)
            totalMovesBudget = movesUsed;
        var usedMoveRatio = totalMovesBudget <= 0 ? 1f : (float)movesUsed / totalMovesBudget;
        latestSnapshot.usedMovePercentage = Mathf.Max(0f, usedMoveRatio * 100f);
        latestSnapshot.timeSpent = Mathf.Max(0f, progress.CompletionTime);

        PostSnapshot(latestSnapshot);
    }

    /// <summary>
    /// Receives the heuristic / model output so we can select the upcoming difficulty tier.
    /// </summary>
    public void ApplyDifficultyResponse(DifficultyServiceResponse response)
    {
        if (response == null)
            return;

        latestResponse = response;
        currentTier = ResolveTier(response.nextRecommendedDifficulty);

        ApplySwitchFor(currentTier);
        DifficultyChanged?.Invoke(currentTier);
    }

    private DifficultyTier ResolveTier(string label)
    {
        if (string.IsNullOrEmpty(label))
            return DifficultyTier.Easy;

        switch (label)
        {
            case DifficultyLabels.VeryEasy:
                return DifficultyTier.VeryEasy;
            case DifficultyLabels.Easy:
                return DifficultyTier.Easy;
            case DifficultyLabels.Normal:
                return DifficultyTier.Normal;
            case DifficultyLabels.Hard:
                return DifficultyTier.Hard;
            case DifficultyLabels.VeryHard:
                return DifficultyTier.VeryHard;
            default:
                return DifficultyTier.Easy;
        }
    }

    private void ApplySwitchFor(DifficultyTier tier)
    {
        currentSettings = DifficultySettings.CreateDefault();

        switch (tier)
        {
            case DifficultyTier.VeryEasy:
                ConfigureVeryEasyDifficulty();
                break;
            case DifficultyTier.Easy:
                ConfigureEasyDifficulty();
                break;
            case DifficultyTier.Normal:
                ConfigureNormalDifficulty();
                break;
            case DifficultyTier.Hard:
                ConfigureHardDifficulty();
                break;
            case DifficultyTier.VeryHard:
                ConfigureVeryHardDifficulty();
                break;
            default:
                ConfigureNormalDifficulty();
                break;
        }

    }

    private void ConfigureVeryEasyDifficulty()
    {
        // TODO: Replace temporary random ranges with tuned Very Easy values.
        currentSettings.moves = UnityEngine.Random.Range(50, 60);
        currentSettings.targetBlockCount = UnityEngine.Random.Range(35, 50);
        currentSettings.boardSize = new Vector2Int(
            UnityEngine.Random.Range(7, 9),
            UnityEngine.Random.Range(9, 11));
        currentSettings.lilInvolvingPercentage = UnityEngine.Random.Range(0.6f, 0.9f);
        currentSettings.blockSpawnAccuracyPercentage = UnityEngine.Random.Range(0.25f, 0.4f);
    }

    private void ConfigureEasyDifficulty()
    {
        // TODO: Replace temporary random ranges with tuned Easy values.
        currentSettings.moves = UnityEngine.Random.Range(40, 55);
        currentSettings.targetBlockCount = UnityEngine.Random.Range(45, 60);
        currentSettings.boardSize = new Vector2Int(
            UnityEngine.Random.Range(6, 8),
            UnityEngine.Random.Range(8, 10));
        currentSettings.lilInvolvingPercentage = UnityEngine.Random.Range(0.5f, 0.8f);
        currentSettings.blockSpawnAccuracyPercentage = UnityEngine.Random.Range(0.2f, 0.35f);
    }

    private void ConfigureNormalDifficulty()
    {
        // TODO: Replace temporary random ranges with tuned Normal values.
        currentSettings.moves = UnityEngine.Random.Range(32, 45);
        currentSettings.targetBlockCount = UnityEngine.Random.Range(55, 75);
        currentSettings.boardSize = new Vector2Int(
            UnityEngine.Random.Range(5, 7),
            UnityEngine.Random.Range(7, 9));
        currentSettings.lilInvolvingPercentage = UnityEngine.Random.Range(0.4f, 0.7f);
        currentSettings.blockSpawnAccuracyPercentage = UnityEngine.Random.Range(0.15f, 0.3f);
    }

    private void ConfigureHardDifficulty()
    {
        // TODO: Replace temporary random ranges with tuned Hard values.
        currentSettings.moves = UnityEngine.Random.Range(25, 35);
        currentSettings.targetBlockCount = UnityEngine.Random.Range(65, 85);
        currentSettings.boardSize = new Vector2Int(
            UnityEngine.Random.Range(5, 6),
            UnityEngine.Random.Range(6, 8));
        currentSettings.lilInvolvingPercentage = UnityEngine.Random.Range(0.3f, 0.6f);
        currentSettings.blockSpawnAccuracyPercentage = UnityEngine.Random.Range(0.1f, 0.25f);
    }

    private void ConfigureVeryHardDifficulty()
    {
        // TODO: Replace temporary random ranges with tuned Very Hard values.
        currentSettings.moves = UnityEngine.Random.Range(18, 25);
        currentSettings.targetBlockCount = UnityEngine.Random.Range(75, 100);
        currentSettings.boardSize = new Vector2Int(
            UnityEngine.Random.Range(4, 5),
            UnityEngine.Random.Range(6, 7));
        currentSettings.lilInvolvingPercentage = UnityEngine.Random.Range(0.2f, 0.5f);
        currentSettings.blockSpawnAccuracyPercentage = UnityEngine.Random.Range(0.05f, 0.2f);
    }

    private Coroutine snapshotRoutine;

    private void PostSnapshot(DifficultyTrackingSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        // Difficulty prediction API temporarily disabled. Apply fallback immediately.
        ApplyFallbackDifficulty();
    }

    private string ResolvePredictionUrl()
    {
        var login = LoginManager.Instance;
        var baseUrl = login != null ? login.BaseApiUrl : string.Empty;
        if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(predictionRoute))
        {
            return $"{baseUrl.TrimEnd('/')}/{predictionRoute.TrimStart('/')}";
        }

        return string.IsNullOrEmpty(fallbackPredictionUrl) ? string.Empty : fallbackPredictionUrl;
    }

    private IEnumerator SendSnapshotCoroutine(string url, string json)
    {
        var authToken = LoginManager.Instance?.CurrentSession?.AuthToken;
        UnityWebRequest request = null;
        try
        {
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var payload = Encoding.UTF8.GetBytes(json ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(payload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(authToken))
                request.SetRequestHeader("Authorization", $"Bearer {authToken}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DifficultyManager] Failed to prepare snapshot request: {ex.Message}\n{ex.StackTrace}");
            ApplyFallbackDifficulty();
            snapshotRoutine = null;
            yield break;
        }

        using (request)
        {
            yield return request.SendWebRequest();

            var responseBodyText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[DifficultyManager] Difficulty snapshot upload failed. Status: {request.responseCode}, Error: {request.error}, Payload: {json}, Response: {responseBodyText}");
                ApplyFallbackDifficulty();
            }
            else
            {
                Debug.Log($"[DifficultyManager] Difficulty snapshot uploaded. Payload: {json}, Response: {responseBodyText}");
                TryApplyPrediction(responseBodyText);
            }
        }

        snapshotRoutine = null;
    }

    private void TryApplyPrediction(string responseBodyText)
    {
        if (string.IsNullOrEmpty(responseBodyText))
        {
            ApplyFallbackDifficulty();
            return;
        }

        try
        {
            var response = JsonUtility.FromJson<DifficultyServiceResponse>(responseBodyText);
            if (response != null)
                ApplyDifficultyResponse(response);
            else
                ApplyFallbackDifficulty();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DifficultyManager] Failed to parse difficulty prediction response: {ex.Message}\n{responseBodyText}");
            ApplyFallbackDifficulty();
        }
    }

    private string ResolvePlayerId()
    {
        var session = LoginManager.Instance?.CurrentSession;
        if (session != null && !string.IsNullOrEmpty(session.UserId))
            return session.UserId;

        if (string.IsNullOrEmpty(cachedOfflinePlayerId))
        {
            cachedOfflinePlayerId = PlayerPrefs.GetString(DifficultyLabels.LocalPlayerPrefsKey);
            if (string.IsNullOrEmpty(cachedOfflinePlayerId))
            {
                cachedOfflinePlayerId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(DifficultyLabels.LocalPlayerPrefsKey, cachedOfflinePlayerId);
                PlayerPrefs.Save();
            }
        }

        return cachedOfflinePlayerId;
    }

    private void ApplyFallbackDifficulty()
    {
        var tiers = Enum.GetValues(typeof(DifficultyTier));
        var nextIndex = ((int)currentTier + 1) % tiers.Length;
        currentTier = (DifficultyTier)tiers.GetValue(nextIndex);
        ApplySwitchFor(currentTier);
        DifficultyChanged?.Invoke(currentTier);
        Debug.LogWarning($"[DifficultyManager] Applying fallback difficulty tier: {currentTier}");
    }
}

[Serializable]
public class DifficultyTrackingSnapshot
{
    public string playerId;
    public float attempts;
    public float stars;
    public float powerups;
    public float usedMovePercentage;
    public float timeSpent;
}

[Serializable]
public class DifficultyServiceResponse
{
    public string heuristicDifficulty;
    public float heuristicScore;
    public string modelDifficulty;
    public string nextRecommendedDifficulty;
}

[Serializable]
public struct DifficultySettings
{
    public int moves;
    public int targetBlockCount;
    public Vector2Int boardSize;
    public float lilInvolvingPercentage;
    public float blockSpawnAccuracyPercentage;

    public static DifficultySettings CreateDefault()
    {
        return new DifficultySettings
        {
            moves = 40,
            targetBlockCount = 50,
            boardSize = new Vector2Int(6, 8),
            lilInvolvingPercentage = 0.5f,
            blockSpawnAccuracyPercentage = 1.0f
        };
    }
}

public static class DifficultyLabels
{
    public const string VeryEasy = "Very_Easy";
    public const string Easy = "Easy";
    public const string Normal = "Normal";
    public const string Hard = "Hard";
    public const string VeryHard = "Very_Hard";
    public const string LocalPlayerPrefsKey = "LilGames.LocalPlayerId";
}

public enum DifficultyTier
{
    VeryEasy,
    Easy,
    Normal,
    Hard,
    VeryHard
}
