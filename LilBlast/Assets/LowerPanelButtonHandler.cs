using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
public class LowerPanelButtonHandler : MonoBehaviour
{
    [System.Serializable]
    public class MenuButton
    {
        public string name;
        public Button button;
        [HideInInspector] public Image image;
        [HideInInspector] public RectTransform rectTransform;
    }

    [Header("Butonlar")]
    [SerializeField] private List<MenuButton> menuButtons;

    [Header("Animasyon Ayarları")]
    [SerializeField] private float activeScale = 1.0f;
    [SerializeField] private float inactiveScale = 0.7f;
    [SerializeField] private float scaleDuration = 0.25f;

    [Header("Renk Ayarları")]
    [SerializeField] private string activeHexColor = "#FFD700"; // Örnek: Altın rengi
    [SerializeField] private Color inactiveColor = Color.white;

    private MenuButton activeButton;
    private Color activeColor;

    private void Awake()
    {
        // Aktif renk hex'ten çevrilir
        if (!ColorUtility.TryParseHtmlString(activeHexColor, out activeColor))
        {
            Debug.LogWarning("Geçersiz hex rengi! Varsayılan olarak sarı kullanılacak.");
            activeColor = Color.yellow;
        }

        // Image ve RectTransform bileşenlerini al
        foreach (var menuButton in menuButtons)
        {
            if (menuButton.button != null)
            {
                menuButton.image = menuButton.button.GetComponent<Image>();
                menuButton.rectTransform = menuButton.button.GetComponent<RectTransform>();
            }
        }
    }

    private void Start()
    {
        foreach (var menuButton in menuButtons)
        {
            menuButton.button.onClick.AddListener(() => OnButtonClicked(menuButton));
        }

        // Varsayılan ilk buton aktif olsun
        if (menuButtons.Count > 0)
        {
            OnButtonClicked(menuButtons[2]);
        }
    }

    private void OnButtonClicked(MenuButton clickedButton)
    {
        foreach (var menuButton in menuButtons)
        {
            bool isActive = (menuButton == clickedButton);

            // DOTween ile animasyonlu scale
            if (menuButton.rectTransform != null)
            {
                float targetScale = isActive ? activeScale : inactiveScale;
                menuButton.rectTransform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack);
            }

            // Renk değişimi
            if (menuButton.image != null)
            {
                menuButton.image.color = isActive ? activeColor : inactiveColor;
            }
        }

        activeButton = clickedButton;
    }

    public string GetActiveButtonName()
    {
        return activeButton != null ? activeButton.name : string.Empty;
    }

    public void SetActiveButton(string buttonName)
    {
        activeButton.name = buttonName;
    }
    void Update()
    {
        Debug.Log(activeButton.name);
    }


}

