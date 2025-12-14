using DG.Tweening;
using UnityEngine;

public class CloudUIScroller : MonoBehaviour
{
    [SerializeField] private float speed = 50f; // pixels per second
    [SerializeField] private float resetPositionX = -800f;
    [SerializeField] private float exitPositionX = 800f;

    private RectTransform rectTransform;
    private Tween scrollTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StartScroll();
    }

    private void OnDisable()
    {
        scrollTween?.Kill();
        scrollTween = null;
    }

    private void StartScroll()
    {
        if (rectTransform == null)
            return;

        var position = rectTransform.anchoredPosition;
        position.x = resetPositionX;
        rectTransform.anchoredPosition = position;

        float distance = Mathf.Abs(exitPositionX - resetPositionX);
        float duration = distance / Mathf.Max(1f, Mathf.Abs(speed));

        scrollTween = rectTransform.DOAnchorPosX(exitPositionX, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }
}
