using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Scene-bound controller that keeps all main-menu UI widgets in sync with backend data and login state.
/// Attach it in the MainMenu scene and wire the serialized references to the relevant Texts / GameObjects.
/// </summary>
public class MainMenuUIController : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private GameObject loginPanelRoot;
    [SerializeField] private GameObject loginButtonLabel;
    [SerializeField] private string loggedOutText = "Guest";

    [Header("Inventory Labels")]
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text livesLabel;
    [SerializeField] private TMP_Text shuffleLabel;
    [SerializeField] private TMP_Text powerShuffleLabel;
    [SerializeField] private TMP_Text manipulateLabel;
    [SerializeField] private TMP_Text destroyLabel;

    [Header("Stats Labels")]
    [SerializeField] private TMP_Text totalScoreLabel;
    [SerializeField] private TMP_Text lastLevelLabel;
    [SerializeField] private TMP_Text threeStarLabel;
    [SerializeField] private TMP_Text totalPowerupsLabel;
    [SerializeField] private TMP_Text attemptsWinRatioLabel;

    private LoginManager loginManager;
    private PlayerDataController playerDataController;
    private Coroutine controllerWatcher;

    private void Awake()
    {
        loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();
        playerDataController = PlayerDataController.Instance ?? FindObjectOfType<PlayerDataController>();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();

        if (loginManager != null)
        {
            loginManager.SessionChanged += HandleSessionChanged;
            HandleSessionChanged(loginManager.CurrentSession);
        }

        PlayerDataController.InstanceChanged += HandlePlayerDataControllerChanged;
        AttachPlayerDataController(playerDataController ?? PlayerDataController.Instance ?? FindObjectOfType<PlayerDataController>());
        if (playerDataController == null && controllerWatcher == null)
            controllerWatcher = StartCoroutine(WaitForPlayerDataController());
    }

    private void OnDisable()
    {
        if (loginManager != null)
            loginManager.SessionChanged -= HandleSessionChanged;

        PlayerDataController.InstanceChanged -= HandlePlayerDataControllerChanged;
        DetachPlayerDataController();

        if (controllerWatcher != null)
        {
            StopCoroutine(controllerWatcher);
            controllerWatcher = null;
        }
    }

    private void HandleSessionChanged(AuthSession session)
    {
        if (usernameLabel != null)
        {
            if (session == null)
                usernameLabel.text = loggedOutText;
            else if (!string.IsNullOrEmpty(session.Username))
                usernameLabel.text = session.Username;
            else
                usernameLabel.text = !string.IsNullOrEmpty(session.UserId) ? session.UserId : loggedOutText;
        }

        bool hasAuthUser = session != null && !session.IsGuest;
        if (loginPanelRoot != null && loginPanelRoot.activeSelf != hasAuthUser)
            loginPanelRoot.SetActive(hasAuthUser);
        if (loginButtonLabel != null && loginButtonLabel.activeSelf == hasAuthUser)
            loginButtonLabel.SetActive(!hasAuthUser);
    }

    private void HandleInventoryChanged(PlayerInventoryState inventory)
    {
        SetNumber(coinsLabel, inventory?.Coins);
        SetNumber(livesLabel, inventory?.Lives);
        SetNumber(shuffleLabel, inventory?.Shuffle);
        SetNumber(powerShuffleLabel, inventory?.PowerShuffle);
        SetNumber(manipulateLabel, inventory?.Manipulate);
        SetNumber(destroyLabel, inventory?.Destroy);
    }

    private void HandleStatsChanged(PlayerStatsState stats)
    {
        SetNumber(totalScoreLabel, stats?.TotalScore);
        SetNumber(lastLevelLabel, stats?.LastLevelReached);
        SetNumber(threeStarLabel, stats?.ThreeStarCount);
        SetNumber(totalPowerupsLabel, stats?.TotalPowerupsUsed);
        SetRatio(attemptsWinRatioLabel, stats?.TotalWins, stats?.TotalAttempts);
    }

    private void AttachPlayerDataController(PlayerDataController controller)
    {
        DetachPlayerDataController();
        playerDataController = controller;
        if (playerDataController == null)
            return;

        playerDataController.InventoryUpdated += HandleInventoryChanged;
        playerDataController.StatsUpdated += HandleStatsChanged;
        HandleInventoryChanged(playerDataController.Inventory);
        HandleStatsChanged(playerDataController.Stats);
    }

    private void DetachPlayerDataController()
    {
        if (playerDataController == null)
            return;

        playerDataController.InventoryUpdated -= HandleInventoryChanged;
        playerDataController.StatsUpdated -= HandleStatsChanged;
        playerDataController = null;
    }

    private IEnumerator WaitForPlayerDataController()
    {
        while (playerDataController == null)
        {
            var instance = PlayerDataController.Instance;
            if (instance != null)
            {
                AttachPlayerDataController(instance);
                break;
            }

            yield return null;
        }

        controllerWatcher = null;
    }

    private void HandlePlayerDataControllerChanged(PlayerDataController controller)
    {
        AttachPlayerDataController(controller);
        if (controller != null && controllerWatcher != null)
        {
            StopCoroutine(controllerWatcher);
            controllerWatcher = null;
        }
        else if (controller == null && controllerWatcher == null)
        {
            controllerWatcher = StartCoroutine(WaitForPlayerDataController());
        }
    }

    private void SetNumber(TMP_Text label, long? value)
    {
        if (label == null)
            return;

        label.text = value.HasValue ? value.Value.ToString() : "0";
    }

    private void SetNumber(TMP_Text label, int? value)
    {
        if (label == null)
            return;

        label.text = value.HasValue ? value.Value.ToString() : "0";
    }

    private void SetRatio(TMP_Text label, long? wins, long? attempts)
    {
        if (label == null)
            return;

        if (!wins.HasValue || !attempts.HasValue || attempts.Value <= 0)
        {
            label.text = "0%";
            return;
        }

        float ratio = Mathf.Clamp01((float)wins.Value / Mathf.Max(1f, attempts.Value));
        label.text = $"{ratio * 100f:0.#}%";
    }
}
