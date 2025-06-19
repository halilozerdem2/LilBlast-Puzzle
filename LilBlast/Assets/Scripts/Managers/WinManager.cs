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

    /// <summary>
    /// Skora göre yıldız sayısını döner.
    /// </summary>
    public int CalculateStarCount(int score)
    {
        if (score >= 8000)
            return 3;
        else if (score >= 6000)
            return 2;
        else 
            return 1;
    }
}