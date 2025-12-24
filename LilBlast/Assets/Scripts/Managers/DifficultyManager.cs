using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Computes deterministic difficulty parameters from an integer scale and optionally syncs with the backend.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    private const string LocalPlayerPrefsKey = "LilGames.LocalPlayerId";

    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyTrackingSnapshot latestSnapshot = new DifficultyTrackingSnapshot();
    [SerializeField] private DifficultyServiceResponse latestResponse = new DifficultyServiceResponse();
    [SerializeField] private DifficultyConfig currentConfig = DifficultyConfig.CreateDefault();
    [SerializeField] [Range(DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty)]
    private int currentDifficulty = DifficultyConfig.MinDifficulty;
    [SerializeField] private string cachedOfflinePlayerId;
    [SerializeField] private string predictionRoute = "/difficulty/predict";
    [SerializeField] private string fallbackPredictionUrl = "https://api.lilgamelabs.com/difficulty/predict";

    private Coroutine snapshotRoutine;

    public DifficultyTrackingSnapshot LatestSnapshot => latestSnapshot;
    public DifficultyServiceResponse LatestResponse => latestResponse;
    public DifficultyConfig CurrentConfig => currentConfig;
    public int CurrentDifficulty => currentDifficulty;

    public event Action<DifficultyConfig> DifficultyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentDifficulty = LevelManager.LoadLastAppliedDifficulty(currentDifficulty);
        ApplyDifficulty(currentDifficulty, false);
    }

    /// <summary>
    /// Public API used by gameplay / backend systems to apply an explicit difficulty value.
    /// </summary>
    public void ApplyDifficulty(int difficulty, bool isOnline, bool notifyNextLevel = true)
    {
        int clamped = Mathf.Clamp(difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
        currentDifficulty = clamped;
        currentConfig = DifficultyConfig.FromDifficulty(clamped);
        Debug.Log($"[DifficultyManager] Applied difficulty {currentDifficulty} (online={isOnline}). Config: board={currentConfig.boardSize.x}x{currentConfig.boardSize.y}, moves={currentConfig.moveCount}, accuracy={currentConfig.spawnAccuracy:F2}");

        LevelManager.SaveLastAppliedDifficulty(currentDifficulty);
        if (notifyNextLevel)
            LevelManager.RecordUpcomingDifficulty(currentDifficulty);
        DifficultyChanged?.Invoke(currentConfig);
    }

    /// <summary>
    /// Called once the player wins a level. Collects the payload we will send to the backend.
    /// </summary>
    public void ReportLevelSuccess(LevelProgress progress)
    {
        if (progress == null)
            return;

        latestSnapshot.playerId = ResolvePlayerId();
        latestSnapshot.levelId = string.IsNullOrEmpty(progress.LevelId) ? Guid.Empty.ToString() : progress.LevelId;
        latestSnapshot.attempts = Mathf.Max(0f, progress.Attempts);
        latestSnapshot.stars = Mathf.Max(0f, progress.Stars);
        latestSnapshot.powerupsUsed = Mathf.Max(0f, progress.PowerUpsUsed);

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

        bool isOnline = IsOnline();
        if (isOnline)
            PostSnapshot(latestSnapshot);
        else
            AdvanceOfflineDifficulty();
    }

    private bool IsOnline()
    {
        var login = LoginManager.Instance;
        return login != null && !login.IsOfflineMode;
    }

    private void AdvanceOfflineDifficulty()
    {
        int nextDifficulty = Mathf.Min(currentDifficulty + 1, DifficultyConfig.MaxDifficulty);
        if (nextDifficulty == currentDifficulty)
        {
            Debug.Log("[DifficultyManager] Offline difficulty already at maximum. No change applied.");
            return;
        }

        Debug.Log($"[DifficultyManager] Offline progression applied. Next difficulty: {nextDifficulty}.");
        ApplyDifficulty(nextDifficulty, false);
    }

    private void PostSnapshot(DifficultyTrackingSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        var url = ResolvePredictionUrl();
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[DifficultyManager] Prediction URL missing. Falling back to offline progression.");
            AdvanceOfflineDifficulty();
            return;
        }

        var json = JsonUtility.ToJson(snapshot);
        Debug.Log($"[DifficultyManager] Sending difficulty request. url={url}, payload={json}");

        if (snapshotRoutine != null)
            StopCoroutine(snapshotRoutine);

        snapshotRoutine = StartCoroutine(SendSnapshotCoroutine(url, json));
    }

    private string ResolvePredictionUrl()
    {
        var login = LoginManager.Instance;
        var baseUrl = login != null ? login.BaseApiUrl : string.Empty;
        if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(predictionRoute))
            return $"{baseUrl.TrimEnd('/')}/{predictionRoute.TrimStart('/')}";

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
            Debug.LogError($"[DifficultyManager] Failed to prepare snapshot request: {ex.Message}\\n{ex.StackTrace}");
            HandleDifficultyRequestFailure();
            snapshotRoutine = null;
            yield break;
        }

        using (request)
        {
            yield return request.SendWebRequest();

            var responseBodyText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[DifficultyManager] Difficulty request failed. Status: {request.responseCode}, Error: {request.error}, Payload: {json}, Response: {responseBodyText}");
                HandleDifficultyRequestFailure();
            }
            else
            {
                Debug.Log($"[DifficultyManager] Difficulty response received. Payload: {responseBodyText}");
                TryApplyPrediction(responseBodyText);
            }
        }

        snapshotRoutine = null;
    }

    private void HandleDifficultyRequestFailure()
    {
        Debug.LogWarning("[DifficultyManager] Backend difficulty request failed. Reverting to offline progression.");
        AdvanceOfflineDifficulty();
    }

    private void TryApplyPrediction(string responseBodyText)
    {
        if (string.IsNullOrEmpty(responseBodyText))
        {
            HandleDifficultyRequestFailure();
            return;
        }

        try
        {
            var response = JsonUtility.FromJson<DifficultyServiceResponse>(responseBodyText);
            int resolvedDifficulty = ResolveDifficultyFromResponse(response);
            if (resolvedDifficulty >= DifficultyConfig.MinDifficulty)
            {
                response.difficulty = resolvedDifficulty;
                ApplyDifficultyResponse(response);
            }
            else
            {
                Debug.LogWarning($"[DifficultyManager] Difficulty response invalid: {responseBodyText}");
                HandleDifficultyRequestFailure();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DifficultyManager] Failed to parse difficulty response: {ex.Message}\\n{responseBodyText}");
            HandleDifficultyRequestFailure();
        }
    }

    private void ApplyDifficultyResponse(DifficultyServiceResponse response)
    {
        latestResponse = response;
        ApplyDifficulty(response.difficulty, true);
    }

    private int ResolveDifficultyFromResponse(DifficultyServiceResponse response)
    {
        if (response == null)
            return -1;

        if (response.difficulty >= DifficultyConfig.MinDifficulty)
            return Mathf.Clamp(response.difficulty, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);

        var label = !string.IsNullOrEmpty(response.nextRecommendedDifficulty)
            ? response.nextRecommendedDifficulty
            : (!string.IsNullOrEmpty(response.modelDifficulty)
                ? response.modelDifficulty
                : response.heuristicDifficulty);

        if (string.IsNullOrEmpty(label))
            return -1;

        return ConvertLegacyLabelToDifficulty(label);
    }

    private int ConvertLegacyLabelToDifficulty(string label)
    {
        int resolved;
        switch (label)
        {
            case DifficultyLabels.VeryEasy:
                resolved = DifficultyConfig.MinDifficulty;
                break;
            case DifficultyLabels.Easy:
                resolved = DifficultyConfig.MinDifficulty + 2;
                break;
            case DifficultyLabels.Normal:
                resolved = DifficultyConfig.MinDifficulty + 4;
                break;
            case DifficultyLabels.Hard:
                resolved = DifficultyConfig.MaxDifficulty - 2;
                break;
            case DifficultyLabels.VeryHard:
                resolved = DifficultyConfig.MaxDifficulty;
                break;
            default:
                return -1;
        }

        return Mathf.Clamp(resolved, DifficultyConfig.MinDifficulty, DifficultyConfig.MaxDifficulty);
    }

    private string ResolvePlayerId()
    {
        var session = LoginManager.Instance?.CurrentSession;
        if (session != null && !string.IsNullOrEmpty(session.UserId))
            return session.UserId;

        if (string.IsNullOrEmpty(cachedOfflinePlayerId))
        {
            cachedOfflinePlayerId = PlayerPrefs.GetString(LocalPlayerPrefsKey);
            if (string.IsNullOrEmpty(cachedOfflinePlayerId))
            {
                cachedOfflinePlayerId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(LocalPlayerPrefsKey, cachedOfflinePlayerId);
                PlayerPrefs.Save();
            }
        }

        return cachedOfflinePlayerId;
    }
}

[Serializable]
public class DifficultyTrackingSnapshot
{
    public string playerId;
    public string levelId;
    public float attempts;
    public float stars;
    public float powerupsUsed;
    public float usedMovePercentage;
    public float timeSpent;
}

[Serializable]
public class DifficultyServiceResponse
{
    public int difficulty;
    public string requestId;
    public string heuristicDifficulty;
    public float heuristicScore;
    public string modelDifficulty;
    public string nextRecommendedDifficulty;
}

public static class DifficultyLabels
{
    public const string VeryEasy = "Very_Easy";
    public const string Easy = "Easy";
    public const string Normal = "Normal";
    public const string Hard = "Hard";
    public const string VeryHard = "Very_Hard";
}

[Serializable]
public struct DifficultyConfig
{
    public const int MinDifficulty = 1;
    public const int MaxDifficulty = 10;

    public int difficulty;
    public Vector2Int boardSize;
    public int moveCount;
    public float spawnAccuracy;
    public float iceCoverage;
    public float iceStartRowRatio;
    public int iceClusterSize;

    public static DifficultyConfig CreateDefault()
    {
        return FromDifficulty(MinDifficulty);
    }

    public static DifficultyConfig FromDifficulty(int difficulty)
    {
        int clamped = Mathf.Clamp(difficulty, MinDifficulty, MaxDifficulty);
        float t = clamped / 10f;

        int board = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(9f, 6f, t)), 6, 9);
        int moves = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(35f, 15f, t)), 15, 35);
        float spawnAccuracy = Mathf.Clamp(Mathf.Lerp(0.45f, 0.15f, t), 0.15f, 0.45f);
        float iceCoverage = Mathf.Clamp(Mathf.Lerp(0.05f, 0.25f, t), 0f, 0.25f);
        float iceStartRowRatio = Mathf.Clamp01(Mathf.Lerp(0.85f, 0.60f, t));
        int iceClusterSize = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(1f, 4f, t)));

        if (clamped < 3)
        {
            iceCoverage = 0f;
            iceClusterSize = 0;
        }

        return new DifficultyConfig
        {
            difficulty = clamped,
            boardSize = new Vector2Int(board, board),
            moveCount = moves,
            spawnAccuracy = spawnAccuracy,
            iceCoverage = iceCoverage,
            iceStartRowRatio = iceStartRowRatio,
            iceClusterSize = iceClusterSize
        };
    }
}
