using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Presents the list of available avatar sprites and lets authenticated users pick one as their profile picture.
/// Sprites are loaded from Resources/Avatars by default so adding a new texture to that folder automatically
/// surfaces it in the UI. Selection is persisted per user via PlayerPrefs.
/// </summary>
public class AvatarSelectionPanel : MonoBehaviour
{
    private const string AvatarPrefsKeyPrefix = "LilGames.Avatar.";

    [Header("Dependencies")]
    [SerializeField] private LoginManager loginManager;
    [SerializeField] private Transform avatarListRoot;
    [SerializeField] private AvatarButtonUI avatarButtonPrefab;
    [SerializeField] private Image selectedAvatarPreview;
    [SerializeField] private AvatarButtonUI loginButtonAvatar;

    [Header("Behaviour")]
    [SerializeField] private string avatarResourceFolder = "UI/AVATAR";
    [SerializeField] private GameObject loginRequiredNotice;
    [SerializeField] private GameObject avatarPanelRoot;
    [SerializeField] private LoginMethodPanel loginMethodPanel;
    [SerializeField] private GameObject loginMethodPanelRoot;
    [SerializeField] private MenuPanelController menuPanelController;
    [SerializeField] private LowerPanelButtonHandler lowerPanelButtonHandler;
    [SerializeField] private string statsButtonName = "Stats Button";
    [SerializeField] private int statsPanelIndex = 4;
    [SerializeField] private int logoutPanelIndex = 2;
    [SerializeField] private Sprite defaultAvatarSprite;
    [SerializeField] private bool closePanelAfterSelection = true;
    [SerializeField] private UnityEvent<Sprite> onAvatarSelected;

    [Header("Animation")]
    [SerializeField] private bool animatePanel = true;
    [SerializeField] private float animateDuration = 0.35f;
    [SerializeField] private Vector2 animateOffset = new Vector2(0, 200f);
    [SerializeField] private Vector3 animateStartScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Ease animateEase = Ease.OutBack;

    private readonly List<AvatarButtonUI> spawnedButtons = new List<AvatarButtonUI>();
    private readonly List<Sprite> buttonSprites = new List<Sprite>();
    private Sprite[] availableAvatars = Array.Empty<Sprite>();
    private int selectedIndex = -1;
    private bool isInitialized;
    private bool sessionSubscribed;
    private RectTransform avatarPanelRect;
    private Vector2 panelOriginalPos;
    private Tween panelTween;

    private void Awake()
    {
        EnsureInitialized();
        SubscribeToSessionChanges();
        HandleSessionChanged(loginManager != null ? loginManager.CurrentSession : null);

    }

    private void OnEnable()
    {
        EnsureInitialized();
        SubscribeToSessionChanges();
        HandleSessionChanged(loginManager != null ? loginManager.CurrentSession : null);
    }

    private void OnDisable()
    {
        //UnsubscribeFromSessionChanges();
        ResetPanelAnimation();
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
            return;

        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();
        if (loginMethodPanel == null)
            loginMethodPanel = FindObjectOfType<LoginMethodPanel>();
        if (loginMethodPanelRoot == null && loginMethodPanel != null)
            loginMethodPanelRoot = loginMethodPanel.gameObject;

        LoadAvatarsFromResources();
        BuildAvatarButtons();
        if (avatarPanelRoot != null)
        {
            avatarPanelRoot.SetActive(false);
            avatarPanelRect = avatarPanelRoot.GetComponent<RectTransform>();
            if (avatarPanelRect != null)
                panelOriginalPos = avatarPanelRect.anchoredPosition;
        }
        UpdateLoginButtonAvatar(defaultAvatarSprite);
        isInitialized = true;
    }

    private void SubscribeToSessionChanges()
    {
        if (loginManager == null || sessionSubscribed)
            return;

        loginManager.SessionChanged += HandleSessionChanged;
        sessionSubscribed = true;
    }

    private void UnsubscribeFromSessionChanges()
    {
        if (loginManager == null || !sessionSubscribed)
            return;

        loginManager.SessionChanged -= HandleSessionChanged;
        sessionSubscribed = false;
    }

    private void LoadAvatarsFromResources()
    {
        availableAvatars = Resources.LoadAll<Sprite>(avatarResourceFolder);
        if (availableAvatars == null)
            availableAvatars = Array.Empty<Sprite>();

        Array.Sort(availableAvatars, (a, b) => string.CompareOrdinal(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty));

        if (defaultAvatarSprite == null && availableAvatars.Length > 0)
            defaultAvatarSprite = availableAvatars[0];
    }

    private void BuildAvatarButtons()
    {
        if (avatarListRoot == null || avatarButtonPrefab == null)
        {
            Debug.LogWarning("AvatarSelectionPanel: Missing list root or button prefab reference.");
            return;
        }

        foreach (Transform child in avatarListRoot)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();
        buttonSprites.Clear();

        for (var i = 0; i < availableAvatars.Length; i++)
        {
            var sprite = availableAvatars[i];
            if (sprite == null)
                continue;

            var buttonInstance = Instantiate(avatarButtonPrefab, avatarListRoot);
            buttonInstance.SetIcon(sprite);
            buttonSprites.Add(sprite);
            var index = buttonSprites.Count - 1;
            buttonInstance.Button.onClick.AddListener(() => HandleAvatarClicked(index));
            buttonInstance.SetSelected(false);
            spawnedButtons.Add(buttonInstance);
        }
    }

    private void HandleAvatarClicked(int index)
    {
        if (!CanModifySelection())
        {
            return;
        }

        ApplySelection(index, true);
    }

    public void OnLoginButtonClicked()
    {
        EnsureInitialized();

        if (loginManager == null || !loginManager.HasAuthenticatedUser)
        {
            SetAvatarPanelVisible(false);
            ShowLoginChooser();
            return;
        }

        ToggleAvatarPanel();
    }

    public void OnLogoutButtonPressed()
    {
        EnsureInitialized();
        SetAvatarPanelVisible(false);
        loginManager?.Logout();
        ShowLoginChooser();
        if (menuPanelController != null)
        {
            var index = Mathf.Max(0, logoutPanelIndex);
            menuPanelController.GoToPanel(index);
        }
        if (lowerPanelButtonHandler != null)
            lowerPanelButtonHandler.SetActiveButton("Play Button");
    }

    public void OnStatsButtonPressed()
    {
        EnsureInitialized();
        SetAvatarPanelVisible(false);
        if (menuPanelController != null)
        {
            var index = Mathf.Max(0, statsPanelIndex);
            menuPanelController.GoToPanel(index);
        }

        if (lowerPanelButtonHandler != null && !string.IsNullOrEmpty(statsButtonName))
            lowerPanelButtonHandler.SetActiveButton(statsButtonName);
    }

    private void ToggleAvatarPanel()
    {
        EnsureInitialized();

        if (avatarPanelRoot == null)
            return;

        var nextState = !avatarPanelRoot.activeSelf;
        SetAvatarPanelVisible(nextState);
    }

    private void SetAvatarPanelVisible(bool visible)
    {
        if (avatarPanelRoot == null)
            return;

        avatarPanelRoot.SetActive(visible);
        if (visible && animatePanel)
            PlayPanelAnimation();
        else if (!visible)
            ResetPanelAnimation();
    }

    private void ShowLoginChooser()
    {
        EnsureInitialized();

        if (loginMethodPanelRoot == null && loginMethodPanel != null)
            loginMethodPanelRoot = loginMethodPanel.gameObject;

        if (loginMethodPanelRoot != null)
            loginMethodPanelRoot.SetActive(true);

        loginMethodPanel?.ShowChooser();
    }

    private void HandleSessionChanged(AuthSession session)
    {
        var hasAuthenticatedUser = session != null && !session.IsGuest;
        UpdateInteractableState(hasAuthenticatedUser);

        if (!hasAuthenticatedUser)
        {
            ApplySelection(-1, false);
            UpdateLoginButtonAvatar(defaultAvatarSprite);
            SetAvatarPanelVisible(false);
            return;
        }
        var savedIndex = LoadSavedSelection(session.UserId);
        if (savedIndex >= 0 && savedIndex < buttonSprites.Count)
            ApplySelection(savedIndex, false);
        else
            ApplySelection(-1, false);
    }

    private void UpdateInteractableState(bool enabled)
    {
        foreach (var button in spawnedButtons)
            button.Button.interactable = enabled;

        if (loginRequiredNotice != null)
            loginRequiredNotice.SetActive(!enabled);
    }

    private bool CanModifySelection()
    {
        return loginManager != null && loginManager.HasAuthenticatedUser;
    }

    private void ApplySelection(int index, bool save)
    {
        if (index == selectedIndex)
            return;

        if (selectedIndex >= 0 && selectedIndex < spawnedButtons.Count)
            spawnedButtons[selectedIndex].SetSelected(false);

        selectedIndex = index;

        if (selectedIndex >= 0 && selectedIndex < spawnedButtons.Count && selectedIndex < buttonSprites.Count)
        {
            spawnedButtons[selectedIndex].SetSelected(true);
            var sprite = buttonSprites[selectedIndex];
            UpdateSelectedPreview(sprite);
            if (save)
                SaveSelection(selectedIndex);
            onAvatarSelected?.Invoke(sprite);
            UpdateLoginButtonAvatar(sprite);
            if (closePanelAfterSelection)
                SetAvatarPanelVisible(false);
        }
        else
        {
            UpdateSelectedPreview(null);
            onAvatarSelected?.Invoke(null);
            UpdateLoginButtonAvatar(defaultAvatarSprite);
        }
    }

    public Sprite GetSpriteForSession(AuthSession session)
    {
        if (!isInitialized)
            EnsureInitialized();

        if (session == null || session.IsGuest)
            return defaultAvatarSprite;

        var index = LoadSavedSelection(session.UserId);
        if (index >= 0 && index < buttonSprites.Count)
            return buttonSprites[index];

        return defaultAvatarSprite;
    }

    private void UpdateSelectedPreview(Sprite sprite)
    {
        if (selectedAvatarPreview == null)
            return;

        selectedAvatarPreview.sprite = sprite;
        selectedAvatarPreview.enabled = sprite != null;
    }

    private void SaveSelection(int index)
    {
        if (loginManager?.CurrentSession == null)
            return;

        if (index < 0 || index >= buttonSprites.Count)
            return;

        var key = BuildAvatarPrefKey(loginManager.CurrentSession.UserId);
        PlayerPrefs.SetInt(key, index);
        PlayerPrefs.Save();
    }

    private int LoadSavedSelection(string userId)
    {
        var key = BuildAvatarPrefKey(userId);
        return PlayerPrefs.GetInt(key, -1);
    }

    private string BuildAvatarPrefKey(string userId)
    {
        return string.IsNullOrEmpty(userId) ? AvatarPrefsKeyPrefix + "guest" : AvatarPrefsKeyPrefix + userId;
    }

    private void UpdateLoginButtonAvatar(Sprite sprite)
    {
        if (loginButtonAvatar == null)
            return;

        var resolved = sprite != null ? sprite : defaultAvatarSprite;
        loginButtonAvatar.SetIcon(resolved);
        loginButtonAvatar.SetSelected(false);
    }

    private void PlayPanelAnimation()
    {
        if (avatarPanelRect == null)
            return;

        ResetPanelAnimation();
        var startScale = new Vector3(animateStartScale.x, Mathf.Max(animateStartScale.y, 0.001f), animateStartScale.z);
        avatarPanelRect.localScale = startScale;
        avatarPanelRect.anchoredPosition = panelOriginalPos + animateOffset;
        panelTween = DOTween.Sequence()
            .Join(avatarPanelRect.DOAnchorPos(panelOriginalPos, animateDuration).SetEase(animateEase))
            .Join(avatarPanelRect.DOScale(Vector3.one, animateDuration).SetEase(animateEase))
            .OnComplete(() => panelTween = null);
    }

    private void ResetPanelAnimation()
    {
        if (panelTween != null)
        {
            panelTween.Kill();
            panelTween = null;
        }

        if (avatarPanelRect != null)
        {
            avatarPanelRect.localScale = Vector3.one;
            avatarPanelRect.anchoredPosition = panelOriginalPos;
        }
    }
}
