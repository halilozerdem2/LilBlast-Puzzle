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
    
    public int CalculateStarCount(int score)
    {
        if (score >= 7000)
            return 3;
        else if (score >= 5000)
            return 2;
        else 
            return 1;
    }
}