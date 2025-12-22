using UnityEngine;

public class UIScaleFlip : MonoBehaviour
{
    public RectTransform target;
    public float speed = 2f;

    Vector3 _defaultScale;
    bool _hasDefaultScale;

    void OnEnable()
    {
        if (target == null) return;

        _defaultScale = target.localScale;
        _hasDefaultScale = true;
    }

    void OnDisable()
    {
        RestoreDefaultScale();
    }

    void OnDestroy()
    {
        RestoreDefaultScale();
    }

    void Update()
    {
        if (target == null) return;
        EnsureDefaultScale();

        float amplitude = _hasDefaultScale ? Mathf.Abs(_defaultScale.x) : 1f;
        float t = Mathf.PingPong(Time.time * speed, 1f);       // 0–1 arası oscillation
        float x = Mathf.Lerp(-amplitude, amplitude, t);        // default değerin +/- aralığında kal

        Vector3 scale = target.localScale;
        scale.x = x;
        target.localScale = scale;
    }

    void EnsureDefaultScale()
    {
        if (_hasDefaultScale || target == null) return;

        _defaultScale = target.localScale;
        _hasDefaultScale = true;
    }

    void RestoreDefaultScale()
    {
        if (target == null || !_hasDefaultScale) return;

        target.localScale = _defaultScale;
    }
}
