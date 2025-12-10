using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] ShuffleManager shuffle;
    [SerializeField] GameObject bombanimation;
    [SerializeField] private PlayerDataController playerDataController;

    // Buton referansları
    public Button shuffleButton;
    public Button powerShuffleButton;
    public Button modifyButton;
    public Button destroyButton;
    public int initialPowerUpCount;

    [Header("Available Charges")]
    [SerializeField] private int shuffleCount = 0;
    [SerializeField] private int powerShuffleCount = 0;
    [SerializeField] private int modifyCount = 0;
    [SerializeField] private int destroyCount = 0;

    private int usedShuffle;
    private int usedPowerShuffle;
    private int usedModify;
    private int usedDestroy;

    private void Awake()
    {
        if (playerDataController == null)
            playerDataController = FindObjectOfType<PlayerDataController>();
    }

    private void OnEnable()
    {
        UpdateButtons();
        LevelManager.OnLevelSceneLoaded += TotalPowerUps;
        if (playerDataController != null)
        {
            playerDataController.InventoryUpdated += HandleInventoryUpdated;
            HandleInventoryUpdated(playerDataController.Inventory);
        }

    }

    private void OnDisable()
    {
        LevelManager.OnLevelSceneLoaded -= TotalPowerUps;
        if (playerDataController != null)
            playerDataController.InventoryUpdated -= HandleInventoryUpdated;
    }

    private void Start()
    {
        UpdateButtons();
    }

    // --- PowerUp Kullanımları ---
    public void UseShuffle()
    {
        if (shuffleCount <= 0) return;

        shuffleCount--;
        usedShuffle++;
        shuffle.HandleShuffle();
        RegisterImmediateUsage(1, 0, 0, 0);
        UpdateButtons();
    }

    public void UsePowerShuffle()
    {
        if (powerShuffleCount <= 0) return;

        powerShuffleCount--;
        usedPowerShuffle++;
        shuffle.HandleShuffle(true);
        RegisterImmediateUsage(0, 1, 0, 0);
        UpdateButtons();
    }

    public void UseModify()
    {
        if (modifyCount <= 0) return;

        modifyCount--;
        usedModify++;
        Debug.Log("Modify kullanıldı. Kalan: " + modifyCount);
        // Buraya senin modify algoritmanı çağır
        RegisterImmediateUsage(0, 0, 1, 0);
        UpdateButtons();
    }

    public void UseDestroy()
    {
        if (destroyCount <= 0) return;

        destroyCount--;
        usedDestroy++;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, Camera.main.nearClipPlane + 5f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        Instantiate(bombanimation, worldPos, Quaternion.identity, this.transform);
        BlockManager.Instance.BlastAllBlocks();
        RegisterImmediateUsage(0, 0, 0, 1);
        UpdateButtons();
    }

    // --- Buton Güncelleme ---
    private void UpdateButtons()
    {
        if (shuffleButton) shuffleButton.interactable = shuffleCount > 0;
        if (powerShuffleButton) powerShuffleButton.interactable = powerShuffleCount > 0;
        if (modifyButton) modifyButton.interactable = modifyCount > 0;
        if (destroyButton) destroyButton.interactable = destroyCount > 0;
    }
    public void TotalPowerUps()
    {
        ResetUsageCounters();
        initialPowerUpCount = shuffleCount + powerShuffleCount + modifyCount + destroyCount;
        Debug.Log("Total Power-Ups after scene load: " + initialPowerUpCount);
    }
     public int CalculateSpentPowerUpAmount()
    {
        int remaining = shuffleCount + powerShuffleCount + modifyCount + destroyCount;
        return Mathf.Max(0, initialPowerUpCount - remaining);
    }

    public PowerUpUsageSnapshot ConsumeUsageSnapshot()
    {
        var snapshot = new PowerUpUsageSnapshot
        {
            Shuffle = usedShuffle,
            PowerShuffle = usedPowerShuffle,
            Manipulate = usedModify,
            Destroy = usedDestroy
        };

        ResetUsageCounters();

        return snapshot;
    }

    private void HandleInventoryUpdated(PlayerInventoryState inventory)
    {
        if (inventory == null)
            return;

        shuffleCount = Mathf.Max(0, inventory.Shuffle);
        powerShuffleCount = Mathf.Max(0, inventory.PowerShuffle);
        modifyCount = Mathf.Max(0, inventory.Manipulate);
        destroyCount = Mathf.Max(0, inventory.Destroy);
        UpdateButtons();
    }

    private void ResetUsageCounters()
    {
        usedShuffle = 0;
        usedPowerShuffle = 0;
        usedModify = 0;
        usedDestroy = 0;
    }

    private void RegisterImmediateUsage(int shuffleDelta, int powerShuffleDelta, int manipulateDelta, int destroyDelta)
    {
        var snapshot = new PowerUpUsageSnapshot
        {
            Shuffle = shuffleDelta,
            PowerShuffle = powerShuffleDelta,
            Manipulate = manipulateDelta,
            Destroy = destroyDelta
        };

        PlayerDataController.Instance?.ApplyPowerupUsage(snapshot);
    }
}
