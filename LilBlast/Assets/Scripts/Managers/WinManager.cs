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
        moveUsagePercentage = Mathf.Clamp01(moveUsagePercentage);

        float moveEfficiency = 1f - moveUsagePercentage; // 1 = hiç hamle kullanmadı, 0 = tüm hamleleri tüketti
        float scoreNormalized = Mathf.InverseLerp(1000f, 4000f, Mathf.Max(0, score));

        float combined = (moveEfficiency * 0.7f) + (scoreNormalized * 0.3f);

        if (combined >= 0.5f)
            return 3;
        if (combined >= 0.25f)
            return 2;

        return 1;
    }
}
