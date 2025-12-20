using UnityEngine;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public int CalculateStarCount(int score, float moveUsagePercentage, float completionTimeMinutes)
    {
        const int minimumScore = 1500;
        const int maximumScore = 5000;
        const float scoreWeight = 0.6f;
        const float efficiencyWeight = 0.25f;
        const float timeWeight = 0.15f;

        moveUsagePercentage = Mathf.Clamp01(moveUsagePercentage);
        float normalizedScore = Mathf.InverseLerp(minimumScore, maximumScore, score);
        float moveEfficiency = 1f - moveUsagePercentage;
        float timeEfficiency = CalculateTimeEfficiency(completionTimeMinutes);

        float combinedValue = (normalizedScore * scoreWeight)
                             + (moveEfficiency * efficiencyWeight)
                             + (timeEfficiency * timeWeight);

        if (combinedValue >= 0.6f)
            return 3;
        if (combinedValue >= 0.3f)
            return 2;

        return 1;
    }

    private float CalculateTimeEfficiency(float completionTimeMinutes)
    {
        if (completionTimeMinutes <= 1f)
            return 1f;

        if (completionTimeMinutes <= 2f)
            return Mathf.Lerp(1f, 0.5f, completionTimeMinutes - 1f);

        return 0.1f;
    }
}
