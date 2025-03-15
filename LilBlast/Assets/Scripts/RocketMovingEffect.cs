using UnityEngine;
using DG.Tweening;

public class RocketMovingEffect : MonoBehaviour
{
    [SerializeField] private GameObject leftRocket;
    [SerializeField] private GameObject rightRocket;

    [SerializeField] private float moveDistance = 5f;  // Daha uzun mesafe
    [SerializeField] private float animationDuration = 0.5f;  // Daha hızlı animasyon

    private void OnEnable()
    {
        // Sol roketi sola hareket ettir
        leftRocket.transform.DOLocalMoveX(leftRocket.transform.localPosition.x - moveDistance, animationDuration)
            .SetEase(Ease.OutExpo);

        // Sağ roketi sağa hareket ettir
        rightRocket.transform.DOLocalMoveX(rightRocket.transform.localPosition.x + moveDistance, animationDuration)
            .SetEase(Ease.OutExpo);
    }

    private void OnDisable()
    {
        // Animasyonları durdur
        leftRocket.transform.DOKill(true);
        rightRocket.transform.DOKill(true);

        // Roketleri başlangıç pozisyonlarına sıfırla
        leftRocket.transform.localPosition = Vector3.zero;
        rightRocket.transform.localPosition = Vector3.zero;
    }
}
