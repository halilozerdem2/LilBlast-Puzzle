using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private Vector2 dragStartPos;

    public System.Action<float> OnDragEnded;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var delta = eventData.position.x - dragStartPos.x;

        OnDragEnded?.Invoke(delta);
    }
}
