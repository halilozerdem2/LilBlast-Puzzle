using UnityEngine;

public class ToggleSwitchAnimator : MonoBehaviour
{
    public RectTransform buttonHandle; // Kayacak buton
    public Vector2 onPosition;
    public Vector2 offPosition;
    public float moveSpeed = 10f;

    private bool isOn = true;
    private Vector2 targetPosition;

    private void Start()
    {
        // Başlangıç durumu
        targetPosition = isOn ? onPosition : offPosition;
        buttonHandle.anchoredPosition = targetPosition;
    }

    private void Update()
    {
        // Yumuşak hareket
        buttonHandle.anchoredPosition = Vector2.Lerp(buttonHandle.anchoredPosition, targetPosition, Time.unscaledDeltaTime * moveSpeed);
    }

    public void Toggle()
    {
        isOn = !isOn;
        targetPosition = isOn ? onPosition : offPosition;
    }

    public void SetState(bool value)
    {
        isOn = value;
        targetPosition = isOn ? onPosition : offPosition;
    }

    public bool IsOn() => isOn;
}