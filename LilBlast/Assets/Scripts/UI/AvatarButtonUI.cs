using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple helper component used by AvatarSelectionPanel to render an avatar option.
/// Handles icon assignment and selection highlighting while exposing the cached Button reference.
/// </summary>
[RequireComponent(typeof(Button))]
public class AvatarButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color deselectedColor = Color.white;

    private Button cachedButton;

    public Button Button => cachedButton != null ? cachedButton : (cachedButton = GetComponent<Button>());

    public Sprite Sprite => icon != null ? icon.sprite : null;

    public void SetIcon(Sprite sprite)
    {
        if (icon == null)
            icon = GetComponent<Image>();

        if (icon == null)
            return;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
            selectionHighlight.SetActive(isSelected);

        if (icon != null)
            icon.color = isSelected ? selectedColor : deselectedColor;
    }
}
