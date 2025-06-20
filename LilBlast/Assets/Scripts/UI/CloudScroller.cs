using UnityEngine;

public class CloudUIScroller : MonoBehaviour
{
    public float speed = 50f;                 // UI için genelde piksel/saniye hız
    public float resetPositionX = -800f;      // Bulutun ekran dışına çıkacağı sol nokta
    public float exitPositionX = 800f;        // Bulutun ekran dışına çıkacağı sağ nokta

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        pos.x += speed * Time.deltaTime;
        rectTransform.anchoredPosition = pos;

        if (pos.x > exitPositionX)
        {
            pos.x = resetPositionX;
            rectTransform.anchoredPosition = pos;
        }
    }
}