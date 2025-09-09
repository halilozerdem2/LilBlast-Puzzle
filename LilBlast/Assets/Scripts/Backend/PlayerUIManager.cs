using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [Header("Player UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI shuffleText;
    public TextMeshProUGUI powerShuffleText;
    public TextMeshProUGUI modifyText;
    public TextMeshProUGUI destroyText;
    public TextMeshProUGUI usernameText;

    private int displayedCoins;
    private int displayedLives;
    private int displayedShuffle;
    private int displayedPowerShuffle;
    private int displayedModify;
    private int displayedDestroy;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        PlayerDataManager.OnUserDataChanged += OnUserDataChanged;
        if (PlayerDataManager.Instance == null ||
            PlayerDataManager.Instance.CurrentUser == null ||
            PlayerDataManager.Instance.CurrentUser.Stats == null ||
            PlayerDataManager.Instance.CurrentUser.PowerUps == null)
        {
            ResetUI();
            return;
        }
        UpdateUI();
    }

    private void OnDisable()
    {
        PlayerDataManager.OnUserDataChanged -= OnUserDataChanged;
    }

    private void OnUserDataChanged()
    {
        var user = PlayerDataManager.Instance.CurrentUser;
        if (user == null || user.Stats == null || user.PowerUps == null)
            ResetUI();
        else
            UpdateUI();
    }

    public void ResetUI()
    {
        StopAllCoroutines();
        displayedCoins = displayedLives = displayedShuffle = displayedPowerShuffle = displayedModify = displayedDestroy = 0;
        if (coinsText != null) coinsText.text = "0";
        if (livesText != null) livesText.text = "0";
        if (shuffleText != null) shuffleText.text = "0";
        if (powerShuffleText != null) powerShuffleText.text = "0";
        if (modifyText != null) modifyText.text = "0";
        if (destroyText != null) destroyText.text = "0";
        if (usernameText != null) usernameText.text = "User";
    }

    public void UpdateUI()
    {
        var user = PlayerDataManager.Instance.CurrentUser;
        if (user == null || user.Stats == null || user.PowerUps == null) return;

        StopAllCoroutines();
        StartCoroutine(AnimateNumber(coinsText, displayedCoins, user.Stats.Coins, v => displayedCoins = v));
        StartCoroutine(AnimateNumber(livesText, displayedLives, user.Stats.Lives, v => displayedLives = v));
        StartCoroutine(AnimateNumber(shuffleText, displayedShuffle, user.PowerUps.ShuffleCount, v => displayedShuffle = v));
        StartCoroutine(AnimateNumber(powerShuffleText, displayedPowerShuffle, user.PowerUps.PowerShuffleCount, v => displayedPowerShuffle = v));
        StartCoroutine(AnimateNumber(modifyText, displayedModify, user.PowerUps.ModifyCount, v => displayedModify = v));
        StartCoroutine(AnimateNumber(destroyText, displayedDestroy, user.PowerUps.DestroyCount, v => displayedDestroy = v));
        usernameText.text = user.Username;
    }

    private IEnumerator AnimateNumber(TextMeshProUGUI text, int from, int to, System.Action<int> setValue, float duration = 0.5f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            text.text = value.ToString();
            setValue(value);
            yield return null;
        }
        text.text = to.ToString();
        setValue(to);
    }
}