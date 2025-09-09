using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    public User CurrentUser { get; private set; }

    public static event System.Action OnUserDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUser(User user)
    {
        CurrentUser = user;
        OnUserDataChanged?.Invoke();
    }

    public void UpdateCoins(int amount)
    {
        if (CurrentUser?.Stats == null) return;
        CurrentUser.Stats.Coins += amount;
        SyncUserData();
    }

    public void UpdateLives(int amount)
    {
        if (CurrentUser?.Stats == null) return;
        CurrentUser.Stats.Lives += amount;
        SyncUserData();
    }

    public void UpdatePowerUp(string type, int amount)
    {
        if (CurrentUser?.PowerUps == null) return;
        switch (type)
        {
            case "Shuffle":
                {
                    CurrentUser.PowerUps.ShuffleCount += amount;
                    Debug.Log("Shuffle hakkın azaldı"); } break;
            case "PowerShuffle": CurrentUser.PowerUps.PowerShuffleCount += amount; break;
            case "Modify": CurrentUser.PowerUps.ModifyCount += amount; break;
            case "Destroy": CurrentUser.PowerUps.DestroyCount += amount; break;
        }
        SyncUserData();
        //OnUserDataChanged?.Invoke(); // <-- Event'i hemen tetikle
    }

    public void UpdateLevelProgress(LevelProgress progress)
    {
        if (CurrentUser?.LevelProgresses == null) return;
        var existing = CurrentUser.LevelProgresses.Find(lp => lp.LevelNumber == progress.LevelNumber);
        if (existing != null)
        {
            existing.Stars = progress.Stars;
            existing.CompletionTime = progress.CompletionTime; // Unity'de CompletionTimeMinutes, backend'de CompletionTime
            existing.CompletedAt = progress.CompletedAt;
        }
        else
        {
            CurrentUser.LevelProgresses.Add(progress);
        }
        SyncUserData();
    }

    public void SyncUserData()
    {
        if (CurrentUser == null) return;
        Debug.Log("Syncing user data to backend...");
        // Sadece backend'in beklediği alanları, ID'siz olarak gönderiyoruz!
        var updateDto = new
        {
            stats = new
            {
                coins = CurrentUser.Stats.Coins,
                lives = CurrentUser.Stats.Lives,
                currentLevel = CurrentUser.Stats.CurrentLevel
            },
            powerUps = new
            {
                shuffleCount = CurrentUser.PowerUps.ShuffleCount,
                powerShuffleCount = CurrentUser.PowerUps.PowerShuffleCount,
                modifyCount = CurrentUser.PowerUps.ModifyCount,
                destroyCount = CurrentUser.PowerUps.DestroyCount
            },
            levelProgresses = CurrentUser.LevelProgresses.Select(lp => new
            {
                levelNumber = lp.LevelNumber,
                stars = lp.Stars,
                completionTime = lp.CompletionTime,
                completedAt = lp.CompletedAt
            }).ToList()
        };

        string jsonBody = JsonConvert.SerializeObject(
            updateDto,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            });

        StartCoroutine(ApiManager.Instance.PutRequestRaw<User>(
            $"/api/auth/update/{CurrentUser.UserID}",
            jsonBody,
            user =>
            {
                CurrentUser = user;
                OnUserDataChanged?.Invoke();
            },
            error =>
            {
                
                Debug.LogError("Sync failed: " + error);
                Debug.LogError("Gönderilen veri: " + jsonBody);
            }
        ));
    }
}