using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class User
{
    public int UserID;
    public string Username;
    public string PasswordHash;
    public string Email;
    public DateTime CreatedAt;
    public PlayerStats Stats;
    public PowerUps PowerUps;
    public List<LevelProgress> LevelProgresses;
}

[Serializable]
public class PlayerStats
{
    public int PlayerStatsId;
    public int UserID;
    public int Coins;          // ✅ PascalCase
    public int Lives;
    public int CurrentLevel;
    public User User { get; set; }
}

[Serializable]
public class PowerUps
{
    public int PowerUpsId;
    public int UserID;
    public int ShuffleCount;       // ✅ PascalCase
    public int PowerShuffleCount;
    public int ModifyCount;
    public int DestroyCount;
    public User User { get; set; }
}

[Serializable]
public class LevelProgress
{
    public int ProgressID;
    public int UserID;
    public int LevelNumber;        // ✅ PascalCase
    public int Stars;
    public int CompletionTime;     // ✅ backend CompletionTime ile eşleşiyor
    public DateTime CompletedAt;
}

[Serializable]
public class LoginRequest
{
    public string Username;
    public string Password;
}

[Serializable]
public class RegisterRequest
{
    public string Username;
    public string Password;
    public string Email;
}
public class UpdateUserDto
{
    public PlayerStats stats { get; set; }

    public PowerUps powerUps { get; set; }

   public List<LevelProgress> levelProgresses { get; set; }
}
