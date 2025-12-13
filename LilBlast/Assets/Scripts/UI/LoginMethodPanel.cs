using DG.Tweening;
using UnityEngine;

/// <summary>
/// Lightweight controller that shows the "choose your login method" panel before the full LoginPanel.
/// Buttons on this panel call the public methods below to pick a provider.
/// </summary>
public class LoginMethodPanel : MonoBehaviour
{
    [SerializeField] private GameObject loginPanelRoot;
    [SerializeField] private LoginPanel loginPanel;
    [SerializeField] private bool hideLoginPanelWhileChooserOpen = true;
    [Header("Animation")]
    [SerializeField] private bool animateOnShow = true;
    [SerializeField] private float slideDistance = 400f;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.OutBack;
    private LoginManager loginManager;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Tween slideTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
            originalAnchoredPosition = rectTransform.anchoredPosition;

        loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();
        if (loginPanel == null && loginPanelRoot != null)
            loginPanel = loginPanelRoot.GetComponent<LoginPanel>();
        if (loginPanelRoot == null && loginPanel != null)
            loginPanelRoot = loginPanel.gameObject;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();

        if (loginManager != null)
            loginManager.SessionChanged += HandleSessionChanged;

        if (loginManager != null)
            ShowOrHideBasedOnSession(loginManager.CurrentSession);
    }

    private void OnDisable()
    {
        if (loginManager != null)
            loginManager.SessionChanged -= HandleSessionChanged;

        StopSlideTween();
        ResetPosition();
    }

    private void HandleSessionChanged(AuthSession session)
    {
        ShowOrHideBasedOnSession(session);
    }

    private void ShowOrHideBasedOnSession(AuthSession session)
    {
        var hasAuthenticatedUser = session != null && !session.IsGuest;
        if (!hasAuthenticatedUser && gameObject.activeSelf)
            return;
    }

    public void ShowChooser()
    {
        Debug.Log("ShowChooser");
        if (hideLoginPanelWhileChooserOpen && loginPanelRoot != null && loginPanelRoot != gameObject)
        {
            loginPanelRoot.SetActive(false);
        }
        else if (loginPanelRoot == gameObject)
        {
            Debug.LogWarning("LoginMethodPanel: loginPanelRoot references this panel. Please assign the actual login panel GameObject instead.");
        }

        gameObject.SetActive(true);
        if (animateOnShow)
            PlaySlideIn();
        Debug.Log(gameObject.activeSelf ? "LoginMethodPanel: true" : "LoginMethodPanel: false");
    }

    public void HideChooser()
    {
        Debug.Log("HideChooser");
        StopSlideTween();
        ResetPosition();
        gameObject.SetActive(false);
    }

    public void ChooseUsernamePassword()
    {
        HideChooser();
        if (loginPanelRoot != null)
            loginPanelRoot.SetActive(true);
    }

    public void ChooseGoogleLogin()
    {
        HideChooser();
        loginPanel?.BeginGoogleLogin();
    }

    public void ChooseFacebookLogin()
    {
        HideChooser();
        loginPanel?.BeginFacebookLogin();
    }

    private void PlaySlideIn()
    {
        if (rectTransform == null)
            return;

        StopSlideTween();
        rectTransform.anchoredPosition = originalAnchoredPosition + Vector2.down * Mathf.Abs(slideDistance);
        slideTween = rectTransform.DOAnchorPos(originalAnchoredPosition, slideDuration)
            .SetEase(slideEase)
            .OnKill(() => slideTween = null);
    }

    private void StopSlideTween()
    {
        if (slideTween == null)
            return;

        slideTween.Kill();
        slideTween = null;
    }

    private void ResetPosition()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}
