using TMPro;
using UnityEngine;

/// <summary>
/// Mirrors the player's inventory (coins, lives, power-ups) on the in-game HUD
/// by listening to PlayerDataController inventory events.
/// </summary>
public class InventoryHUDUpdater : MonoBehaviour
{
    [Header("Currency & Lives")]
    [SerializeField] private TMP_Text coinsLabel;
    [SerializeField] private TMP_Text livesLabel;

    [Header("Power-Ups")]
    [SerializeField] private TMP_Text shuffleLabel;
    [SerializeField] private TMP_Text powerShuffleLabel;
    [SerializeField] private TMP_Text manipulateLabel;
    [SerializeField] private TMP_Text destroyLabel;
    [SerializeField] private PlayerDataController playerDataController;

    private void Awake()
    {
        if (playerDataController == null)
            playerDataController = FindObjectOfType<PlayerDataController>();
    }

    private void OnEnable()
    {
        if (playerDataController == null)
            return;

        playerDataController.InventoryUpdated += HandleInventoryUpdated;
        HandleInventoryUpdated(playerDataController.Inventory);
    }

    private void OnDisable()
    {
        if (playerDataController == null)
            return;

        playerDataController.InventoryUpdated -= HandleInventoryUpdated;
    }

    private void HandleInventoryUpdated(PlayerInventoryState state)
    {
        SetNumber(coinsLabel, state?.Coins);
        SetNumber(livesLabel, state?.Lives);
        SetNumber(shuffleLabel, state?.Shuffle);
        SetNumber(powerShuffleLabel, state?.PowerShuffle);
        SetNumber(manipulateLabel, state?.Manipulate);
        SetNumber(destroyLabel, state?.Destroy);
    }

    private void SetNumber(TMP_Text label, int? value)
    {
        if (label == null)
            return;

        label.text = value.HasValue ? value.Value.ToString() : "0";
    }

    private void SetNumber(TMP_Text label, long? value)
    {
        if (label == null)
            return;

        label.text = value.HasValue ? value.Value.ToString() : "0";
    }
}
