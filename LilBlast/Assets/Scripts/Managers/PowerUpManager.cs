using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] ShuffleManager shuffle;
    [SerializeField] GameObject bombanimation;

    // Buton referansları
    public Button shuffleButton;
    public Button powerShuffleButton;
    public Button modifyButton;
    public Button destroyButton;

    private int shuffleCount
    {
        get
        {
            if (PlayerDataManager.Instance == null) return 0;
            var user = PlayerDataManager.Instance.CurrentUser;
            return user?.PowerUps?.ShuffleCount ?? 0;
        }
    }
    private int powerShuffleCount
    {
        get
        {
            if (PlayerDataManager.Instance == null) return 0;
            var user = PlayerDataManager.Instance.CurrentUser;
            return user?.PowerUps?.PowerShuffleCount ?? 0;
        }
    }
    private int modifyCount
    {
        get
        {
            if (PlayerDataManager.Instance == null) return 0;
            var user = PlayerDataManager.Instance.CurrentUser;
            return user?.PowerUps?.ModifyCount ?? 0;
        }
    }
    private int destroyCount
    {
        get
        {
            if (PlayerDataManager.Instance == null) return 0;
            var user = PlayerDataManager.Instance.CurrentUser;
            return user?.PowerUps?.DestroyCount ?? 0;
        }
    }

    private void OnEnable()
    {
        UpdateButtons();
        PlayerDataManager.OnUserDataChanged += UpdateButtons;
    }

    private void OnDisable()
    {
        PlayerDataManager.OnUserDataChanged -= UpdateButtons;
    }

    private void Start()
    {
        UpdateButtons();
    }

    // --- PowerUp Kullanımları ---
    public void UseShuffle()
    {
        if (shuffleCount > 0)
        {
            PlayerDataManager.Instance.UpdatePowerUp("Shuffle", -1);
            shuffle.HandleShuffle();
        }
        //UpdateButtons();
    }

    public void UsePowerShuffle()
    {
        if (powerShuffleCount > 0)
        {
            PlayerDataManager.Instance.UpdatePowerUp("PowerShuffle", -1);
            shuffle.HandleShuffle(true);
        }
        UpdateButtons();
    }

    public void UseModify()
    {
        if (modifyCount > 0)
        {
            PlayerDataManager.Instance.UpdatePowerUp("Modify", -1);
            Debug.Log("Modify kullanıldı. Kalan: " + (modifyCount - 1));
            // Buraya senin modify algoritmanı çağır
        }
        UpdateButtons();
    }

    public void UseDestroy()
    {
        if (destroyCount > 0)
        {
            PlayerDataManager.Instance.UpdatePowerUp("Destroy", -1);
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, Camera.main.nearClipPlane + 5f);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
            Instantiate(bombanimation, worldPos, Quaternion.identity, this.transform);
            BlockManager.Instance.BlastAllBlocks();
        }
        UpdateButtons();
    }

    // --- Buton Güncelleme ---
    private void UpdateButtons()
    {
        // Eğer PlayerDataManager veya kullanıcı veya powerup verisi yoksa tüm butonları kapat
        if (PlayerDataManager.Instance == null ||
            PlayerDataManager.Instance.CurrentUser == null ||
            PlayerDataManager.Instance.CurrentUser.PowerUps == null)
        {
            if (shuffleButton) shuffleButton.interactable = false;
            if (powerShuffleButton) powerShuffleButton.interactable = false;
            if (modifyButton) modifyButton.interactable = false;
            if (destroyButton) destroyButton.interactable = false;
            return;
        }

        if (shuffleButton) shuffleButton.interactable = shuffleCount > 0;
        if (powerShuffleButton) powerShuffleButton.interactable = powerShuffleCount > 0;
        if (modifyButton) modifyButton.interactable = modifyCount > 0;
        if (destroyButton) destroyButton.interactable = destroyCount > 0;
    }
}
