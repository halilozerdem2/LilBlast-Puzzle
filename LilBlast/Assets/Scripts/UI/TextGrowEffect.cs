using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextGrowEffect : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    [SerializeField] private float startScale = 0.1f;

    private void OnEnable()
    {
        // Önce scale'ı küçült
        transform.localScale = Vector3.one * startScale;

        // Ardından DoTween ile normal scale'a getir
        transform.DOScale(Vector3.one*3.5f, duration).SetEase(easeType);
    }
}