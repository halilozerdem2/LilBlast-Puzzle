using UnityEngine;
using UnityEngine.UI;

public class MenuPanelController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int totalPanels = 5;
    [SerializeField] private float snapSpeed = 10f;

    private int currentPanelIndex = 0;
    private bool isSnapping = false;
    private float targetPosition;

    void Update()
    {
        if (isSnapping)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                targetPosition,
                Time.deltaTime * snapSpeed
            );

            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetPosition) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    public void GoToPanel(int panelIndex)
    {
        currentPanelIndex = Mathf.Clamp(panelIndex, 0, totalPanels - 1);
        targetPosition = PanelIndexToPosition(currentPanelIndex);
        isSnapping = true;
    }

    private float PanelIndexToPosition(int panelIndex)
{
    float contentWidth = scrollRect.content.rect.width;
    float viewportWidth = scrollRect.viewport.rect.width;

    float panelWidth = contentWidth / totalPanels;
    float maxScroll = contentWidth - viewportWidth;

    float targetCenterX = (panelIndex * panelWidth) + (panelWidth / 2f) - (viewportWidth / 2f);

    return Mathf.Clamp01(targetCenterX / maxScroll);
}

}
