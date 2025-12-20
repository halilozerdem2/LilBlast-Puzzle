using System;
using System.Collections;
using System.Collections.Generic;
using LilBlast.Backend;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles every authentication flow by talking to the backend AuthController.
/// Bootstraps a guest session on start and exposes helpers for email/password and OAuth logins.
/// </summary>
public class LoginManager : MonoBehaviour
{
    private const string TokenKey = "LilGames.AuthToken";
    private const string UserKey = "LilGames.UserId";
    private const string ProviderKey = "LilGames.AuthProvider";
    private const string UsernameKey = "LilGames.Username";
    private const string OfflineUserKey = "LilGames.OfflineUserId";

    public static LoginManager Instance { get; private set; }

    [Header("Backend")]
    [SerializeField] private string baseApiUrl = "https://api.lilgamelabs.com";
    [SerializeField] private bool autoGuestLogin = true;
    [SerializeField] private bool offlineMode = false;

    [Header("UI Hooks")]
    [SerializeField] private UnityEvent onGuestLoginCompleted;
    [SerializeField] private UnityEvent onLoginSuccess;
    [SerializeField] private UnityEvent onRequireRegistration;
    [SerializeField] private UnityEvent onLogout;
    [SerializeField] private UnityEvent<string> onAuthError;

    [Header("UI Controls")]
    [SerializeField] private GameObject loginButtonLabel;
    [SerializeField] private LoginPanel loginPanel;

    public event Action<AuthSession> SessionChanged;
    public event Action<PlayerInventoryState> InventoryChanged;
    public event Action<PlayerStatsState> StatsChanged;
    public event Action<string> AuthError;
    public event Action<string> AuthFlowDiagnostic;

    public AuthSession CurrentSession => currentSession;
    public bool HasAuthenticatedUser => currentSession != null && !currentSession.IsGuest;
    public bool IsBusy => isBusy;
    public string BaseApiUrl => baseApiUrl;
    public bool IsOfflineMode => offlineMode;

    private AuthApiClient authClient;
    private AuthSession currentSession;
    private bool isBusy;
    private Coroutine playerDataRoutine;
    private Coroutine loginPanelHideRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (offlineMode)
        {
            authClient = null;
            InitializeOfflineSession();
        }
        else
        {
            authClient = new AuthApiClient(baseApiUrl);
            LoadSessionFromPrefs();
        }
        UpdateLoginUiState(HasAuthenticatedUser);
        EnsurePlayerDataController();
    }

    private void Start()
    {
        if (offlineMode)
        {
            EmitFlowDiagnostic("Offline mode active. Skipping backend login flow.");
            SessionChanged?.Invoke(currentSession);
            onGuestLoginCompleted?.Invoke();
            return;
        }

        if (!autoGuestLogin)
            return;

        if (currentSession == null || string.IsNullOrEmpty(currentSession.AuthToken))
        {
            StartCoroutine(BootstrapGuestSession());
        }
        else
        {
            onLoginSuccess?.Invoke();
            SessionChanged?.Invoke(currentSession);
            BeginPlayerDataRefresh();
            UpdateLoginUiState(HasAuthenticatedUser);
        }
    }

    public void LoginWithEmail(string identifier, string password)
    {
        var trimmedIdentifier = string.IsNullOrEmpty(identifier) ? string.Empty : identifier.Trim();
        var looksLikeEmail = trimmedIdentifier.Contains("@");
        var email = looksLikeEmail ? trimmedIdentifier : string.Empty;
        var username = looksLikeEmail ? string.Empty : trimmedIdentifier;

        LoginWithEmail(email, username, password);
    }

    /// <summary>
    /// Convenience wrapper for gameplay scripts that expect username/password login naming.
    /// </summary>
    public void LoginViaUserNameAndPassword(string username, string password)
    {
        LoginWithEmail(username, password);
    }

    public void LoginWithEmail(string email, string username, string password)
    {
        if (BlockedByOfflineMode("username/password login", true))
            return;

        if (isBusy)
            return;

        if (!CanStartNewLogin("username/password login"))
            return;

        var normalizedEmail = string.IsNullOrEmpty(email) ? string.Empty : email.Trim();
        var normalizedUsername = string.IsNullOrEmpty(username) ? string.Empty : username.Trim();
        var displayIdentifier = !string.IsNullOrEmpty(normalizedUsername) ? normalizedUsername : normalizedEmail;

        EmitFlowDiagnostic($"Starting username/password login for '{displayIdentifier}'.");
        var request = new LoginRequest
        {
            email = normalizedEmail,
            username = normalizedUsername,
            password = password
        };

        StartCoroutine(ExecuteAuthRequest(() => authClient.Login(request, response => HandleAuthResponse(response, AuthProvider.EMAIL_PASSWORD), HandleLoginError)));
    }

    public void RegisterWithEmail(string email, string username, string password)
    {
        if (BlockedByOfflineMode("sign-in", true))
            return;

        if (isBusy)
            return;

        if (!CanStartNewLogin("sign-in"))
            return;

        EmitFlowDiagnostic($"Starting sign-in (register) for email '{email}' and username '{username}'.");
        var request = new RegisterRequest { email = email, username = username, password = password };
        StartCoroutine(ExecuteAuthRequest(() => authClient.Register(request, response => HandleAuthResponse(response, AuthProvider.EMAIL_PASSWORD), HandleGenericError)));
    }

    /// <summary>
    /// Exposed alias so UI can call a SignIn method without caring about backend naming.
    /// </summary>
    public void SignIn(string email, string username, string password)
    {
        RegisterWithEmail(email, username, password);
    }

    public void LoginWithGoogle(string oauthToken)
    {
        EmitFlowDiagnostic("Google login requested.");
        LoginWithOAuth(AuthProvider.GOOGLE, oauthToken);
    }

    public void LoginWithFacebook(string oauthToken)
    {
        EmitFlowDiagnostic("Facebook login requested.");
        LoginWithOAuth(AuthProvider.FACEBOOK, oauthToken);
    }

    public void UpgradeGuestAccount(string email, string username, string password)
    {
        if (BlockedByOfflineMode("guest upgrade", true))
            return;

        if (currentSession == null || string.IsNullOrEmpty(currentSession.AuthToken))
        {
            EmitError("Guest session missing. Please relaunch the game.");
            return;
        }

        if (!currentSession.IsGuest)
        {
            EmitError("Only guest accounts can be upgraded. Please logout if you need to link another account.");
            return;
        }

        EmitFlowDiagnostic($"Upgrading guest user '{currentSession.UserId}' with email '{email}'.");
        var request = new UpgradeGuestRequest
        {
            userId = currentSession.UserId,
            email = email,
            username = username,
            password = password
        };

        StartCoroutine(ExecuteAuthRequest(() => authClient.UpgradeGuest(request, currentSession.AuthToken, response => HandleAuthResponse(response, AuthProvider.EMAIL_PASSWORD), HandleGenericError)));
    }

    public void Logout()
    {
        EmitFlowDiagnostic("Clearing session and logging out.");
        currentSession = null;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UserKey);
        PlayerPrefs.DeleteKey(ProviderKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.Save();
        onLogout?.Invoke();
        SessionChanged?.Invoke(null);
        InventoryChanged?.Invoke(null);
        StatsChanged?.Invoke(null);
        UpdateLoginUiState(false);

        if (playerDataRoutine != null)
        {
            StopCoroutine(playerDataRoutine);
            playerDataRoutine = null;
        }

        if (offlineMode)
        {
            InitializeOfflineSession();
            SessionChanged?.Invoke(currentSession);
            onGuestLoginCompleted?.Invoke();
            return;
        }

        if (autoGuestLogin && isActiveAndEnabled)
            StartCoroutine(BootstrapGuestSession());
    }

    private void LoginWithOAuth(AuthProvider provider, string token)
    {
        if (BlockedByOfflineMode($"{provider} OAuth login", true))
            return;

        if (isBusy)
            return;

        if (!CanStartNewLogin($"{provider} OAuth login"))
            return;

        if (string.IsNullOrEmpty(token))
        {
            EmitError("OAuth token missing.");
            return;
        }

        EmitFlowDiagnostic($"Starting {provider} OAuth login.");
        var request = new OAuthRequest { provider = provider, oauthToken = token };
        StartCoroutine(ExecuteAuthRequest(() => authClient.LoginWithOAuth(request, response => HandleAuthResponse(response, provider), HandleLoginError)));
    }

    private IEnumerator BootstrapGuestSession()
    {
        if (BlockedByOfflineMode("guest session bootstrap"))
            yield break;

        EmitFlowDiagnostic("Bootstrapping guest session.");
        var request = new GuestAuthRequest
        {
            deviceId = string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier)
                ? Guid.NewGuid().ToString()
                : SystemInfo.deviceUniqueIdentifier
        };

        yield return ExecuteAuthRequest(() => authClient.LoginAsGuest(request, response =>
        {
            HandleAuthResponse(response, AuthProvider.GUEST);
        }, HandleGenericError));
    }

    private IEnumerator ExecuteAuthRequest(Func<IEnumerator> routineFactory)
    {
        isBusy = true;
        if (routineFactory != null)
        {
            yield return routineFactory.Invoke();
        }
        else
        {
            Debug.LogWarning("LoginManager.ExecuteAuthRequest called without a routine.");
            yield return null;
        }
        isBusy = false;
    }

    private void HandleAuthResponse(AuthResponse response, AuthProvider provider)
    {
        if (response == null)
        {
            EmitError("Empty response from server.");
            return;
        }

        EmitFlowDiagnostic($"Received auth response from provider {provider} for user '{response.userId}'.");
        var resolvedUsername = !string.IsNullOrEmpty(response.username)
            ? response.username
            : response.userId;
        currentSession = new AuthSession(response.userId, response.authToken, resolvedUsername, provider);
        SaveSession();

        if (provider == AuthProvider.GUEST)
            onGuestLoginCompleted?.Invoke();
        else
            onLoginSuccess?.Invoke();

        SessionChanged?.Invoke(currentSession);
        InventoryChanged?.Invoke(currentSession.Inventory);
        StatsChanged?.Invoke(currentSession.Stats);
        BeginPlayerDataRefresh();
        UpdateLoginUiState(!currentSession.IsGuest);
    }

    private void HandleLoginError(BackendError error)
    {
        if (error == null)
            return;

        Debug.LogError($"[LoginManager] Login failed ({error.StatusCode}). Message: {error.Message}\nRaw: {error.RawBody}");
        EmitFlowDiagnostic($"Login error ({error.StatusCode}): {error.Message}");
        if (error.StatusCode == 404 || error.StatusCode == 401)
        {
            EmitError("Account not found. Please sign up.", error);
            onRequireRegistration?.Invoke();
        }
        else
        {
            EmitError($"Login failed: {error.Message}", error);
        }
    }

    private void HandleGenericError(BackendError error)
    {
        if (error == null)
            return;

        Debug.LogError($"[LoginManager] Backend error ({error.StatusCode}). Message: {error.Message}\nRaw: {error.RawBody}");
        EmitFlowDiagnostic($"Backend error ({error?.StatusCode}): {error?.Message}");
        EmitError(string.IsNullOrEmpty(error.Message) ? "Unexpected server error." : error.Message, error);
    }

    private void EmitError(string message, BackendError backendError = null)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (backendError != null)
        {
            Debug.LogWarning($"LoginManager: {message} (status: {backendError.StatusCode})\nRaw: {backendError.RawBody}");
            AuthFlowDiagnostic?.Invoke($"Error {backendError.StatusCode}: {message}");
        }
        else
        {
            Debug.LogWarning($"LoginManager: {message}");
            AuthFlowDiagnostic?.Invoke($"Error: {message}");
        }
        onAuthError?.Invoke(message);
        AuthError?.Invoke(message);
    }

    private void EmitFlowDiagnostic(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        AuthFlowDiagnostic?.Invoke(message);
    }

    private void LoadSessionFromPrefs()
    {
        if (!PlayerPrefs.HasKey(TokenKey))
            return;

        var token = PlayerPrefs.GetString(TokenKey, string.Empty);
        var userId = PlayerPrefs.GetString(UserKey, string.Empty);
        var providerString = PlayerPrefs.GetString(ProviderKey, AuthProvider.GUEST.ToString());
        var username = PlayerPrefs.GetString(UsernameKey, string.Empty);

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            return;

        if (!Enum.TryParse(providerString, out AuthProvider provider))
        {
            provider = AuthProvider.GUEST;
        }

        currentSession = new AuthSession(userId, token, username, provider);
    }

    private void SaveSession()
    {
        if (currentSession == null)
            return;

        PlayerPrefs.SetString(TokenKey, currentSession.AuthToken ?? string.Empty);
        PlayerPrefs.SetString(UserKey, currentSession.UserId ?? string.Empty);
        PlayerPrefs.SetString(ProviderKey, currentSession.Provider.ToString());
        PlayerPrefs.SetString(UsernameKey, currentSession.Username ?? string.Empty);
        PlayerPrefs.Save();
    }

    public void UpdateInventory(PlayerInventoryState snapshot)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("LoginManager.UpdateInventory called without an active session.");
            return;
        }

        currentSession.SetInventory(snapshot);
        InventoryChanged?.Invoke(currentSession.Inventory);
    }

    public void UpdateStats(PlayerStatsState snapshot)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("LoginManager.UpdateStats called without an active session.");
            return;
        }

        currentSession.SetStats(snapshot);
        StatsChanged?.Invoke(currentSession.Stats);
    }

    public void ReportPowerupUsage(PowerUpUsageSnapshot usage)
    {
        if (usage == null || !usage.HasUsage)
            return;

        if (BlockedByOfflineMode("powerup usage sync"))
            return;

        if (!HasAuthenticatedUser)
            return;

        if (authClient == null)
            return;

        if (Application.internetReachability == NetworkReachability.NotReachable)
            return;

        StartCoroutine(SendPowerupUsageCoroutine(new PowerUpUsageSnapshot(usage)));
    }

    private bool BlockedByOfflineMode(string actionDescription, bool notifyUser = false)
    {
        if (!offlineMode)
            return false;

        if (!string.IsNullOrEmpty(actionDescription))
            EmitFlowDiagnostic($"Skipped {actionDescription} because offline mode is active.");

        if (notifyUser)
            EmitError("Online features are disabled in this offline build.");

        return true;
    }

    private bool CanStartNewLogin(string description)
    {
        if (currentSession != null && !currentSession.IsGuest)
        {
            var display = string.IsNullOrEmpty(currentSession.Username) ? currentSession.UserId : currentSession.Username;
            EmitFlowDiagnostic($"Blocked {description} because '{display}' is already authenticated.");
            EmitError("You are already logged in. Please logout before switching accounts.");
            return false;
        }

        return true;
    }

    private void BeginPlayerDataRefresh()
    {
        if (!isActiveAndEnabled)
            return;

        if (BlockedByOfflineMode("player data refresh"))
            return;

        if (currentSession == null || string.IsNullOrEmpty(currentSession.UserId) || string.IsNullOrEmpty(currentSession.AuthToken))
            return;

        if (authClient == null)
            return;

        if (playerDataRoutine != null)
        {
            StopCoroutine(playerDataRoutine);
        }

        playerDataRoutine = StartCoroutine(FetchPlayerDataCoroutine(currentSession.UserId, currentSession.AuthToken));
    }

    private IEnumerator FetchPlayerDataCoroutine(string userId, string authToken)
    {
        if (BlockedByOfflineMode("player data fetch"))
        {
            playerDataRoutine = null;
            yield break;
        }

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(authToken))
        {
            playerDataRoutine = null;
            yield break;
        }

        yield return authClient.GetInventory(userId, authToken, response =>
        {
            if (!IsSessionStillActive(userId))
                return;

            var snapshot = new PlayerInventoryState
            {
                Coins = response?.coins ?? PlayerInventoryState.DefaultCoins,
                Lives = response?.lives ?? PlayerInventoryState.DefaultLives,
                Shuffle = response?.shuffleCount ?? PlayerInventoryState.DefaultPowerupCount,
                PowerShuffle = response?.powerShuffleCount ?? PlayerInventoryState.DefaultPowerupCount,
                Manipulate = response?.manipulateCount ?? PlayerInventoryState.DefaultPowerupCount,
                Destroy = response?.destroyCount ?? PlayerInventoryState.DefaultPowerupCount
            };
            UpdateInventory(snapshot);
        }, error => HandleDataFetchError("inventory", error));

        yield return authClient.GetProfile(userId, authToken, response =>
        {
            if (!IsSessionStillActive(userId))
                return;

            var snapshot = new PlayerStatsState
            {
                TotalAttempts = response?.totalAttempts ?? 0,
                TotalWins = response?.totalWins ?? 0,
                ThreeStarCount = response?.threeStarCount ?? 0,
                TotalPowerupsUsed = response?.totalPowerupsUsed ?? 0,
                TotalScore = response?.totalScore ?? 0,
                LastLevelReached = response?.lastLevelReached ?? 0
            };
            UpdateStats(snapshot);
        }, error => HandleDataFetchError("profile", error));

        playerDataRoutine = null;
    }

    private IEnumerator SendPowerupUsageCoroutine(PowerUpUsageSnapshot usage)
    {
        if (BlockedByOfflineMode("powerup usage sync"))
            yield break;

        if (!IsSessionStillActive(currentSession?.UserId))
            yield break;

        var token = currentSession.AuthToken;
        var userId = currentSession.UserId;
        var requests = BuildPowerupUsageRequests(userId, usage);

        foreach (var payload in requests)
        {
            yield return authClient.ConsumeInventory(payload, token, _ => { }, error => HandleDataFetchError("inventory-consume", error));
        }
    }

    private InventoryAdjustmentRequest[] BuildPowerupUsageRequests(string userId, PowerUpUsageSnapshot usage)
    {
        var list = new System.Collections.Generic.List<InventoryAdjustmentRequest>();

        void MaybeAdd(int amount, InventoryItemType type)
        {
            if (amount <= 0)
                return;

            list.Add(new InventoryAdjustmentRequest
            {
                userId = userId,
                itemType = type,
                amount = amount
            });
        }

        MaybeAdd(usage.Shuffle, InventoryItemType.SHUFFLE);
        MaybeAdd(usage.PowerShuffle, InventoryItemType.POWERSHUFFLE);
        MaybeAdd(usage.Manipulate, InventoryItemType.MANIPULATE);
        MaybeAdd(usage.Destroy, InventoryItemType.DESTROY);

        return list.ToArray();
    }

    private bool IsSessionStillActive(string expectedUserId)
    {
        return currentSession != null && currentSession.UserId == expectedUserId;
    }

    private void HandleDataFetchError(string domain, BackendError error)
    {
        if (error == null)
            return;

        Debug.LogWarning($"LoginManager: Failed to load {domain}: {error.Message} (status: {error.StatusCode})");
        AuthFlowDiagnostic?.Invoke($"Failed to load {domain}: {error.Message}");
    }

    private void EnsurePlayerDataController()
    {
        if (PlayerDataController.Instance != null)
            return;

        var go = new GameObject("PlayerDataController");
        go.AddComponent<PlayerDataController>();
    }

    private void InitializeOfflineSession()
    {
        EmitFlowDiagnostic("Offline mode enabled. Backend calls will be skipped.");
        var offlineUserId = ResolveOfflineUserId();
        currentSession = new AuthSession(offlineUserId, string.Empty, "Guest", AuthProvider.GUEST);
    }

    private string ResolveOfflineUserId()
    {
        var offlineUserId = PlayerPrefs.GetString(OfflineUserKey, string.Empty);
        if (!string.IsNullOrEmpty(offlineUserId))
            return offlineUserId;

        var deviceId = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrEmpty(deviceId))
            deviceId = Guid.NewGuid().ToString("N");

        offlineUserId = $"offline-{deviceId}";
        PlayerPrefs.SetString(OfflineUserKey, offlineUserId);
        PlayerPrefs.Save();
        return offlineUserId;
    }

    private void UpdateLoginUiState(bool hasAuthenticatedUser)
    {
        if (loginButtonLabel != null)
            loginButtonLabel.SetActive(!hasAuthenticatedUser);

        if (loginPanelHideRoutine != null)
        {
            StopCoroutine(loginPanelHideRoutine);
            loginPanelHideRoutine = null;
        }

        if (hasAuthenticatedUser && loginPanel != null)
        {
            loginPanelHideRoutine = StartCoroutine(HideLoginPanelAfterDelay(1f));
        }
    }

    private IEnumerator HideLoginPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loginPanel != null)
            loginPanel.HidePanelWithAnimation();
        loginPanelHideRoutine = null;
    }
}

[Serializable]
public class AuthSession
{
    public string UserId => userId;
    public string AuthToken => authToken;
    public string Username => username;
    public AuthProvider Provider => provider;
    public bool IsGuest => provider == AuthProvider.GUEST;
    public PlayerInventoryState Inventory => inventory;
    public PlayerStatsState Stats => stats;

    [SerializeField] private string userId;
    [SerializeField] private string authToken;
    [SerializeField] private string username;
    [SerializeField] private AuthProvider provider;
    [SerializeField] private PlayerInventoryState inventory = new PlayerInventoryState();
    [SerializeField] private PlayerStatsState stats = new PlayerStatsState();

    public AuthSession(string userId, string authToken, string username, AuthProvider provider)
    {
        this.userId = userId;
        this.authToken = authToken;
        this.username = username;
        this.provider = provider;
    }

    public void SetInventory(PlayerInventoryState snapshot)
    {
        inventory = snapshot != null ? new PlayerInventoryState(snapshot) : new PlayerInventoryState();
    }

    public void SetStats(PlayerStatsState snapshot)
    {
        stats = snapshot != null ? new PlayerStatsState(snapshot) : new PlayerStatsState();
    }
}

[Serializable]
public class PlayerInventoryState
{
    public const long DefaultCoins = 100L;
    public const int DefaultLives = 5;
    public const int DefaultPowerupCount = 10;

    public long Coins = DefaultCoins;
    public int Lives = DefaultLives;
    public int Shuffle = DefaultPowerupCount;
    public int PowerShuffle = DefaultPowerupCount;
    public int Manipulate = DefaultPowerupCount;
    public int Destroy = DefaultPowerupCount;

    public PlayerInventoryState()
    {
    }

    public PlayerInventoryState(PlayerInventoryState other)
    {
        if (other == null)
            return;

        Coins = other.Coins;
        Lives = other.Lives;
        Shuffle = other.Shuffle;
        PowerShuffle = other.PowerShuffle;
        Manipulate = other.Manipulate;
        Destroy = other.Destroy;
    }
}

[Serializable]
public class PlayerStatsState
{
    public long TotalAttempts;
    public long TotalWins;
    public int ThreeStarCount;
    public long TotalPowerupsUsed;
    public long TotalScore;
    public int LastLevelReached;

    public PlayerStatsState()
    {
    }

    public PlayerStatsState(PlayerStatsState other)
    {
        if (other == null)
            return;

        TotalAttempts = other.TotalAttempts;
        TotalWins = other.TotalWins;
        ThreeStarCount = other.ThreeStarCount;
        TotalPowerupsUsed = other.TotalPowerupsUsed;
        TotalScore = other.TotalScore;
        LastLevelReached = other.LastLevelReached;
    }
}
