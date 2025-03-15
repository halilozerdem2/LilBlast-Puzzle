using UnityEngine;
using DG.Tweening;

public class VerticalRocketEffect : MonoBehaviour
{
    [SerializeField] private GameObject topRocket;
    [SerializeField] private GameObject bottomRocket;

    [SerializeField] private float moveDistance = 5f;  // Daha uzun mesafe
    [SerializeField] private float animationDuration = 0.5f;  // Daha hızlı animasyon

    private void OnEnable()
    {
        // Üst roketi yukarı hareket ettir
        topRocket.transform.DOLocalMoveY(topRocket.transform.localPosition.y + moveDistance, animationDuration)
            .SetEase(Ease.OutExpo);

        // Alt roketi aşağı hareket ettir
        bottomRocket.transform.DOLocalMoveY(bottomRocket.transform.localPosition.y - moveDistance, animationDuration)
            .SetEase(Ease.OutExpo);
    }

    private void OnDisable()
    {
        // Animasyonları durdur
        topRocket.transform.DOKill(true);
        bottomRocket.transform.DOKill(true);

        // Roketleri başlangıç pozisyonlarına sıfırla
        topRocket.transform.localPosition = Vector3.zero;
        bottomRocket.transform.localPosition = Vector3.zero;
    }
}
