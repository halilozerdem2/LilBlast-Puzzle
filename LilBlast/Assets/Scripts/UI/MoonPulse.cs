using UnityEngine;
using UnityEngine.UI;

public class MoonPulse : MonoBehaviour
{
    public float speed = 1f;        // Parlama hızını ayarlar
    public float minAlpha = 140f;   // Minimum alpha değeri (0–255 arası)
    public float maxAlpha = 255f;   // Maksimum alpha değeri (0–255 arası)

    private Image image;
    private float alpha;
    private bool increasing = true;

    private void Awake()
    {
        image = GetComponent<Image>();
        alpha = minAlpha / 255f;
    }

    private void Update()
    {
        float alphaDelta = speed * Time.deltaTime;

        if (increasing)
        {
            alpha += alphaDelta;
            if (alpha >= maxAlpha / 255f)
            {
                alpha = maxAlpha / 255f;
                increasing = false;
            }
        }
        else
        {
            alpha -= alphaDelta;
            if (alpha <= minAlpha / 255f)
            {
                alpha = minAlpha / 255f;
                increasing = true;
            }
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}