using TMPro;
using UnityEngine;

/// <summary>
/// Updates the in-game power-up count labels by listening to PlayerDataController inventory events.
/// Ensures HUD values always match the current player inventory, even mid-level.
/// </summary>
public class PowerUpHUDUpdater : MonoBehaviour
{
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
        SetLabel(shuffleLabel, state?.Shuffle);
        SetLabel(powerShuffleLabel, state?.PowerShuffle);
        SetLabel(manipulateLabel, state?.Manipulate);
        SetLabel(destroyLabel, state?.Destroy);
    }

    private void SetLabel(TMP_Text label, int? value)
    {
        if (label == null)
            return;

        label.text = value.HasValue ? value.Value.ToString() : "0";
    }
}
