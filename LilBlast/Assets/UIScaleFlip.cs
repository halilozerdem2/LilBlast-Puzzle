using UnityEngine;

public class UIScaleFlip : MonoBehaviour
{
    public RectTransform target;
    public float speed = 2f;

    void Update()
    {
        if (target == null) return;

        float t = Mathf.PingPong(Time.time * speed, 1f);   // 0–1 arası oscillation
        float x = Mathf.Lerp(-1f, 1f, t);                  // -1 ile 1 arası geçiş

        Vector3 scale = target.localScale;
        scale.x = x;
        target.localScale = scale;
    }
}
