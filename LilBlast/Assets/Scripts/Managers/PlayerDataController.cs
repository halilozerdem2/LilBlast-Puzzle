using System;
using UnityEngine;

/// <summary>
/// Central authority for local player data (inventory + stats). Handles player-pref persistence
/// and mirrors LoginManager events so UI can listen from a single place.
/// Guests stay fully local; authenticated players sync with the backend when data arrives.
/// </summary>
public class PlayerDataController : MonoBehaviour
{
    public static PlayerDataController Instance { get; private set; }
    public static event Action<PlayerDataController> InstanceChanged;

    [SerializeField] private LoginManager loginManager;

    public PlayerInventoryState Inventory => new PlayerInventoryState(inventorySnapshot);
    public PlayerStatsState Stats => new PlayerStatsState(statsSnapshot);

    public event Action<PlayerInventoryState> InventoryUpdated;
    public event Action<PlayerStatsState> StatsUpdated;

    private const string PlayerDataKeyPrefix = "LilGames.PlayerData.";
    private const string AuthUserPrefsKey = "LilGames.UserId";
    private const string OfflineUserPrefsKey = "LilGames.OfflineUserId";
    private const string GuestStorageKey = "guest";

    private PlayerInventoryState inventorySnapshot = new PlayerInventoryState();
    private PlayerStatsState statsSnapshot = new PlayerStatsState();
    private string currentStorageKey = GuestStorageKey;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InstanceChanged?.Invoke(this);
        if (loginManager == null)
            loginManager = LoginManager.Instance;
        currentStorageKey = ResolveStorageKey(loginManager?.CurrentSession);
        LoadFromPrefs();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            loginManager = LoginManager.Instance;

        if (loginManager == null)
            return;

        loginManager.SessionChanged += HandleSessionChanged;
        loginManager.InventoryChanged += HandleInventoryChanged;
        loginManager.StatsChanged += HandleStatsChanged;
    }

    private void OnDisable()
    {
        if (loginManager == null)
            return;

        loginManager.SessionChanged -= HandleSessionChanged;
        loginManager.InventoryChanged -= HandleInventoryChanged;
        loginManager.StatsChanged -= HandleStatsChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            InstanceChanged?.Invoke(null);
        }
    }

    private void Start()
    {
        NotifyInventoryUpdated();
        NotifyStatsUpdated();
    }

    private void HandleSessionChanged(AuthSession session)
    {
        currentStorageKey = ResolveStorageKey(session);
        LoadFromPrefs();
        NotifyInventoryUpdated();
        NotifyStatsUpdated();
    }

    private void HandleInventoryChanged(PlayerInventoryState state)
    {
        if (state == null)
            return;

        inventorySnapshot = new PlayerInventoryState(state);
        SaveInventoryToPrefs();
        NotifyInventoryUpdated();
    }

    private void HandleStatsChanged(PlayerStatsState state)
    {
        if (state == null)
            return;

        statsSnapshot = new PlayerStatsState(state);
        SaveStatsToPrefs();
        NotifyStatsUpdated();
    }

    public void RecordLevelCompletion(LevelProgress progress, int score, PowerUpUsageSnapshot usage)
    {
        if (progress == null)
            return;

        statsSnapshot.TotalAttempts++;
        statsSnapshot.TotalWins++;
        statsSnapshot.TotalScore += score;
        statsSnapshot.TotalPowerupsUsed += usage?.TotalUsed ?? progress.PowerUpsUsed;
        if (progress.Stars >= 3)
            statsSnapshot.ThreeStarCount++;
        if (progress.LevelNumber > statsSnapshot.LastLevelReached)
            statsSnapshot.LastLevelReached = progress.LevelNumber;

        SaveStatsToPrefs();
        NotifyStatsUpdated();

        ApplyPowerupUsage(usage);
    }

    public void RecordLevelFailure()
    {
        statsSnapshot.TotalAttempts++;
        SaveStatsToPrefs();
        NotifyStatsUpdated();
    }

    public void ApplyPowerupUsage(PowerUpUsageSnapshot usage)
    {
        if (usage == null || usage.TotalUsed <= 0)
            return;

        inventorySnapshot.Shuffle = Mathf.Max(0, inventorySnapshot.Shuffle - usage.Shuffle);
        inventorySnapshot.PowerShuffle = Mathf.Max(0, inventorySnapshot.PowerShuffle - usage.PowerShuffle);
        inventorySnapshot.Manipulate = Mathf.Max(0, inventorySnapshot.Manipulate - usage.Manipulate);
        inventorySnapshot.Destroy = Mathf.Max(0, inventorySnapshot.Destroy - usage.Destroy);

        SaveInventoryToPrefs();
        NotifyInventoryUpdated();

        if (ShouldSyncWithServer())
        {
            LoginManager.Instance?.ReportPowerupUsage(usage);
        }
    }

    private bool ShouldSyncWithServer()
    {
        if (loginManager == null || loginManager.CurrentSession == null)
            return false;

        if (loginManager.CurrentSession.IsGuest)
            return false;

        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void LoadFromPrefs()
    {
        var inventoryJson = PlayerPrefs.GetString(BuildKey("Inventory"), string.Empty);
        if (!string.IsNullOrEmpty(inventoryJson))
        {
            try
            {
                inventorySnapshot = JsonUtility.FromJson<PlayerInventoryState>(inventoryJson);
            }
            catch
            {
                inventorySnapshot = new PlayerInventoryState();
            }
        }
        else
        {
            inventorySnapshot = new PlayerInventoryState();
        }

        var statsJson = PlayerPrefs.GetString(BuildKey("Stats"), string.Empty);
        if (!string.IsNullOrEmpty(statsJson))
        {
            try
            {
                statsSnapshot = JsonUtility.FromJson<PlayerStatsState>(statsJson);
            }
            catch
            {
                statsSnapshot = new PlayerStatsState();
            }
        }
        else
        {
            statsSnapshot = new PlayerStatsState();
        }
    }

    private void SaveInventoryToPrefs()
    {
        var json = JsonUtility.ToJson(inventorySnapshot);
        PlayerPrefs.SetString(BuildKey("Inventory"), json);
        PlayerPrefs.Save();
    }

    private void SaveStatsToPrefs()
    {
        var json = JsonUtility.ToJson(statsSnapshot);
        PlayerPrefs.SetString(BuildKey("Stats"), json);
        PlayerPrefs.Save();
    }

    private void NotifyInventoryUpdated()
    {
        InventoryUpdated?.Invoke(new PlayerInventoryState(inventorySnapshot));
    }

    private void NotifyStatsUpdated()
    {
        StatsUpdated?.Invoke(new PlayerStatsState(statsSnapshot));
    }

    public static void ResetLocalData()
    {
        Instance?.ResetSnapshots();

        ClearStoredDataForKey(GuestStorageKey);
        var lastAuthUser = PlayerPrefs.GetString(AuthUserPrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(lastAuthUser))
            ClearStoredDataForKey(lastAuthUser);
        var offlineUserId = PlayerPrefs.GetString(OfflineUserPrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(offlineUserId))
            ClearStoredDataForKey(offlineUserId);

        PlayerPrefs.Save();
    }

    private string BuildKey(string suffix)
    {
        return $"{PlayerDataKeyPrefix}{currentStorageKey}.{suffix}";
    }

    private string ResolveStorageKey(AuthSession session)
    {
        if (session == null)
            return GuestStorageKey;

        if (!string.IsNullOrEmpty(session.UserId))
            return session.UserId;

        return GuestStorageKey;
    }

    private void ResetSnapshots()
    {
        inventorySnapshot = new PlayerInventoryState();
        statsSnapshot = new PlayerStatsState();
        NotifyInventoryUpdated();
        NotifyStatsUpdated();
    }

    private static void ClearStoredDataForKey(string storageKey)
    {
        if (string.IsNullOrEmpty(storageKey))
            return;

        PlayerPrefs.DeleteKey($"{PlayerDataKeyPrefix}{storageKey}.Inventory");
        PlayerPrefs.DeleteKey($"{PlayerDataKeyPrefix}{storageKey}.Stats");
    }
}
