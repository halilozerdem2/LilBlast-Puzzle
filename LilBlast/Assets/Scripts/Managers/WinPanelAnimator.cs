using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;


public class WinPanelAnimator : MonoBehaviour
{
    [Header("Panel & Yıldızlar")]
    public RectTransform panelTransform;
    public TMP_Text scoreText;
    public RectTransform[] starRects;

    [Header("Animasyon Ayarları")]
    public float panelDuration = 0.5f;
    public float starDelay = 0.3f;
    public Vector2 hiddenPos = new Vector2(-1000f, 0f);
    public Vector2 visiblePos = new Vector2(0f, 0f);

    private void OnEnable()
    {
        // Başlangıç durumlarını ayarla
        panelTransform.anchoredPosition = hiddenPos;

        foreach (var star in starRects)
        {
            star.gameObject.SetActive(false);
        }

        // Skor ve yıldız sayısını al
        int score = ScoreManager.Instance.currentScore;
        float moveUsagePercent = 1f;
        var handler = GameOverHandler.Instance;
        if (handler != null && handler.TotalMovesGranted > 0)
            moveUsagePercent = Mathf.Clamp01((float)handler.MovesUsed / handler.TotalMovesGranted);

        float completionMinutes = 0f;
        var levelManager = LevelManager.Instance;
        if (levelManager != null && levelManager.CurrentLevelProgress != null)
            completionMinutes = levelManager.CurrentLevelProgress.CompletionTime / 60f;

        int starCount = 0;
        if (levelManager != null && levelManager.CurrentLevelProgress != null)
            starCount = levelManager.CurrentLevelProgress.Stars;
        else
            starCount = WinManager.Instance.CalculateStarCount(score, moveUsagePercent, completionMinutes);

        // Panel animasyonu başlat
        scoreText.text = score.ToString();

        panelTransform.DOAnchorPos(visiblePos, panelDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => ShowStars(starCount));
    }

    private void ShowStars(int starCount)
    {
        for (int i = 0; i < starCount && i < starRects.Length; i++)
        {
            RectTransform star = starRects[i];
            star.gameObject.SetActive(true);

            // Konum sabit, sadece scale 0.1'den 1'e büyüsün
            star.localScale = Vector3.one * 0.1f;

            star.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetDelay(i * starDelay)
                .SetUpdate(true);
        }
    }



}
