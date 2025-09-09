using UnityEngine;
using TMPro;
using System;

public class UserInformationUI : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI daysPlayedText; // Inspector'dan atayın

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        UpdateUserInfo();
        PlayerDataManager.OnUserDataChanged += UpdateUserInfo;
    }

    void OnDisable()
    {
        PlayerDataManager.OnUserDataChanged -= UpdateUserInfo;
    }

    private void UpdateUserInfo()
    {
        var user = PlayerDataManager.Instance.CurrentUser;
        if (user == null)
        {
            usernameText.text = "";
            emailText.text = "";
            currentLevelText.text = "";
            livesText.text = "";
            coinsText.text = "";
            if (daysPlayedText != null) daysPlayedText.text = "";
            return;
        }

        usernameText.text = user.Username;
        emailText.text = user.Email;
        currentLevelText.text = user.Stats != null ? user.Stats.CurrentLevel.ToString() : "0";
        livesText.text = user.Stats != null ? user.Stats.Lives.ToString() : "0";
        coinsText.text = user.Stats != null ? user.Stats.Coins.ToString() : "0";

        if (daysPlayedText != null)
        {
            DateTime createdAt = user.CreatedAt;
            int days = (int)(DateTime.UtcNow - createdAt).TotalDays;
            daysPlayedText.text = $"Congrats! You've been playing for {days + 1} days!"; // +1 to include the current day
        }
    }
}
