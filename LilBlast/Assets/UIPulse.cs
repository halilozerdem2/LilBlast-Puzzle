using UnityEngine;
using DG.Tweening;

public class UIPulse : MonoBehaviour
{
    public RectTransform target;
    public float scaleAmount = 1.1f;   // Ne kadar büyüsün
    public float duration = 0.6f;      // Animasyon süresi

    void Start()
    {
        if (target == null) target = GetComponent<RectTransform>();

        Vector3 originalScale = target.localScale;

        // Hafif büyüyüp küçülerek loop yapar
        target.DOScale(originalScale * scaleAmount, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}