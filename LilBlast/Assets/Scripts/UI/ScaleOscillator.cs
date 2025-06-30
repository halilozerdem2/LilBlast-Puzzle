using UnityEngine;
using System.Collections;

public class ScaleOscillator : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        XYZ
    }

    [Header("Scale Animation Settings")]
    public Axis scaleAxis = Axis.X;
    public float speed = 2f;         // Hız (cycle per second)
    public float amplitude = 0.2f;   // Ne kadar küçülsün? (örneğin %20)
    public bool invert = false;      // İster ters dalga yap

    private Vector3 initialScale;
    private Coroutine scaleCoroutine;

    void OnEnable()
    {
        initialScale = transform.localScale;
        scaleCoroutine = StartCoroutine(AnimateScale());
    }

    void OnDisable()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
    }

    private IEnumerator AnimateScale()
    {
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime * speed;

            // Sinüs pozitif dalga → 0 ile 1 arası gidip gelir
            float wave = Mathf.Abs(Mathf.Sin(time));
            float scaleOffset = 1f - (wave * amplitude); // Örn: 1 - 0.2 → %80'e kadar iner

            if (invert)
                scaleOffset = 1f + (wave * amplitude); // Büyüyerek salınma

            Vector3 newScale = initialScale;

            switch (scaleAxis)
            {
                case Axis.X:
                    newScale.x = initialScale.x * scaleOffset;
                    break;
                case Axis.Y:
                    newScale.y = initialScale.y * scaleOffset;
                    break;
                case Axis.XYZ:
                    newScale = initialScale * scaleOffset;
                    break;
            }

            transform.localScale = newScale;

            yield return null;
        }
    }
}