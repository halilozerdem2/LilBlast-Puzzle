using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple helper that reflects the currently logged in username and mirrors inventory/stat snapshots on the UI.
/// </summary>
public class UIUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private string loggedOutText = "Not logged in";
    [SerializeField] private PlayerDataController playerDataController;

    [Header("Inventory Labels")]
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text livesLabel;
    [SerializeField] private TMP_Text shuffleLabel;
    [SerializeField] private TMP_Text powerShuffleLabel;
    [SerializeField] private TMP_Text manipulateLabel;
    [SerializeField] private TMP_Text destroyLabel;

    [Header("Stats Labels")]
    [SerializeField] private TMP_Text totalAttemptsLabel;
    [SerializeField] private TMP_Text totalWinsLabel;
    [SerializeField] private TMP_Text totalScoreLabel;
    [SerializeField] private TMP_Text lastLevelLabel;
    [SerializeField] private TMP_Text threeStarLabel;
    [SerializeField] private TMP_Text totalPowerupsLabel;

    [Header("Profile")]
    [SerializeField] private AvatarSelectionPanel avatarSelectionPanel;
    [SerializeField] private Image avatarImage;
    private LoginManager loginManager;

    private void Awake()
    {
        if (loginManager == null)
            loginManager = FindObjectOfType<LoginManager>();
        if (playerDataController == null)
            playerDataController = FindObjectOfType<PlayerDataController>();
        if (avatarSelectionPanel == null)
            avatarSelectionPanel = FindObjectOfType<AvatarSelectionPanel>();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            return;

        loginManager.SessionChanged += HandleSessionChanged;
        UpdateLabel(loginManager.CurrentSession);
        UpdateAvatar(loginManager.CurrentSession);

        if (playerDataController != null)
        {
            playerDataController.InventoryUpdated += HandleInventoryChanged;
            playerDataController.StatsUpdated += HandleStatsChanged;
            HandleInventoryChanged(playerDataController.Inventory);
            HandleStatsChanged(playerDataController.Stats);
        }
    }

    private void OnDisable()
    {
        if (loginManager == null)
            return;

        loginManager.SessionChanged -= HandleSessionChanged;

        if (playerDataController != null)
        {
            playerDataController.InventoryUpdated -= HandleInventoryChanged;
            playerDataController.StatsUpdated -= HandleStatsChanged;
        }
    }

    private void HandleSessionChanged(AuthSession session)
    {
        UpdateLabel(session);
        UpdateAvatar(session);
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
        SetNumber(totalAttemptsLabel, stats?.TotalAttempts);
        SetNumber(totalWinsLabel, stats?.TotalWins);
        SetNumber(totalScoreLabel, stats?.TotalScore);
        SetNumber(lastLevelLabel, stats?.LastLevelReached);
        SetNumber(threeStarLabel, stats?.ThreeStarCount);
        SetNumber(totalPowerupsLabel, stats?.TotalPowerupsUsed);
    }

    private void UpdateLabel(AuthSession session)
    {
        if (usernameLabel == null)
        {
            Debug.LogWarning("UIUpdater: Username label reference missing.");
            return;
        }

        if (session == null)
        {
            usernameLabel.text = loggedOutText;
            return;
        }

        var displayName = !string.IsNullOrEmpty(session.Username)
            ? session.Username
            : (!string.IsNullOrEmpty(session.UserId) ? session.UserId : loggedOutText);

        usernameLabel.text = displayName;
    }

    private void UpdateAvatar(AuthSession session)
    {
        if (avatarImage == null)
            return;

        if (avatarSelectionPanel == null)
        {
            avatarImage.enabled = false;
            return;
        }

        var sprite = avatarSelectionPanel.GetSpriteForSession(session);
        avatarImage.sprite = sprite;
        avatarImage.enabled = sprite != null;
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
}
