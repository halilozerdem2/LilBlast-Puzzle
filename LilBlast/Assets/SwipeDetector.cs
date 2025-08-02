using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private Vector2 dragStartPos;

    public System.Action<float> OnDragEnded;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
        Debug.Log("Swipe başladı: " + dragStartPos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var delta = eventData.position.x - dragStartPos.x;
        Debug.Log("Swipe bitti. DeltaX: " + delta);

        OnDragEnded?.Invoke(delta);
    }
}
