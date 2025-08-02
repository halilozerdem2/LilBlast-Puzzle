using System;
using UnityEngine;
using DG.Tweening;

public class BombAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease easeType = Ease.InBack;

    private void OnEnable()
    {
        PlayShrinkAnimation();
    }

    public void PlayShrinkAnimation()
    {
        transform.DOScale(Vector3.zero, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}