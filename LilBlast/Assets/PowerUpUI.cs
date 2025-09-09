using UnityEngine;
using TMPro;

public class PowerUpUI : MonoBehaviour
{
    [Header("Player UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI shuffleText;
    public TextMeshProUGUI powerShuffleText;
    public TextMeshProUGUI modifyText;
    public TextMeshProUGUI destroyText;

    void OnEnable()
    {
        UpdateUI();
        PlayerDataManager.OnUserDataChanged += UpdateUI;
    }
    void OnDisable()
    {
        PlayerDataManager.OnUserDataChanged -= UpdateUI;
    }
    void UpdateUI()
    {
        var user = PlayerDataManager.Instance.CurrentUser;
        if (user == null || user.Stats == null || user.PowerUps == null) return;

        coinsText.text = user.Stats.Coins.ToString();
        livesText.text = user.Stats.Lives.ToString();
        shuffleText.text = user.PowerUps.ShuffleCount.ToString();
        powerShuffleText.text = user.PowerUps.PowerShuffleCount.ToString();
        modifyText.text = user.PowerUps.ModifyCount.ToString();
        destroyText.text = user.PowerUps.DestroyCount.ToString();
    }
}
