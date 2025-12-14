using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MoonPulse : MonoBehaviour
{
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float minAlpha = 140f;
    [SerializeField] private float maxAlpha = 255f;

    private Image image;
    private Tween pulseTween;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartPulse();
    }

    private void OnDisable()
    {
        pulseTween?.Kill();
        pulseTween = null;
    }

    private void StartPulse()
    {
        if (image == null)
            return;

        float min = Mathf.Clamp01(minAlpha / 255f);
        float max = Mathf.Clamp01(maxAlpha / 255f);

        var color = image.color;
        color.a = min;
        image.color = color;

        pulseTween = image.DOFade(max, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
