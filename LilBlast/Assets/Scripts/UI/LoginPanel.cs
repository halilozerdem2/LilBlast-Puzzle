using System;
using LilBlast.Backend;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles all login/sign-in UI interactions and forwards them to the LoginManager.
/// Supports email sign-in, username login, and OAuth (Google/Facebook) flows via UnityEvents.
/// </summary>
public class LoginPanel : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LoginManager loginManager;

    [Header("Login Fields")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;

    [Header("Sign-In Fields")]
    [SerializeField] private TMP_InputField signInEmailInput;
    [SerializeField] private TMP_InputField signInUsernameInput;
    [SerializeField] private TMP_InputField signInPasswordInput;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackLabel;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private CanvasGroup interactableCanvas;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color successColor = Color.white;

    [Header("OAuth Hooks")]
    [SerializeField] private UnityEvent onGoogleSignInRequested;
    [SerializeField] private UnityEvent onFacebookSignInRequested;

    private bool isWaitingForExternalOAuth;
    private AuthProvider pendingOAuthProvider = AuthProvider.GUEST;

    private void Awake()
    {
        if (loginManager == null)
            loginManager = FindObjectOfType<LoginManager>();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            return;

        loginManager.AuthError += HandleAuthError;
        loginManager.SessionChanged += HandleSessionChanged;
    }

    private void OnDisable()
    {
        if (loginManager == null)
            return;

        loginManager.AuthError -= HandleAuthError;
        loginManager.SessionChanged -= HandleSessionChanged;
    }

    public void SubmitLogin()
    {
        if (loginManager == null)
        {
            ShowFeedback("Login system not ready.", true);
            return;
        }

        if (loginManager.HasAuthenticatedUser)
        {
            ShowFeedback("Already logged in. Logout to switch accounts.", true);
            return;
        }

        var username = loginUsernameInput != null ? loginUsernameInput.text.Trim() : string.Empty;
        var password = loginPasswordInput != null ? loginPasswordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Enter username and password.", true);
            return;
        }

        SetBusy(true);
        loginManager.LoginWithEmail(username, password);
    }

    public void SubmitSignIn()
    {
        if (loginManager == null)
        {
            ShowFeedback("Login system not ready.", true);
            return;
        }

        if (loginManager.HasAuthenticatedUser)
        {
            ShowFeedback("Already logged in. Logout to switch accounts.", true);
            return;
        }

        var email = signInEmailInput != null ? signInEmailInput.text.Trim() : string.Empty;
        var username = signInUsernameInput != null ? signInUsernameInput.text.Trim() : string.Empty;
        var password = signInPasswordInput != null ? signInPasswordInput.text : string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Fill in email, username and password.", true);
            return;
        }

        SetBusy(true);
        loginManager.RegisterWithEmail(email, username, password);
    }

    public void BeginGoogleLogin()
    {
        BeginOAuthFlow(AuthProvider.GOOGLE, onGoogleSignInRequested);
    }

    public void BeginFacebookLogin()
    {
        BeginOAuthFlow(AuthProvider.FACEBOOK, onFacebookSignInRequested);
    }

    public void OnGoogleOAuthTokenReceived(string token)
    {
        CompleteOAuthLogin(AuthProvider.GOOGLE, token);
    }

    public void OnFacebookOAuthTokenReceived(string token)
    {
        CompleteOAuthLogin(AuthProvider.FACEBOOK, token);
    }

    public void OnGoogleOAuthFailed(string message)
    {
        HandleOAuthFailure(AuthProvider.GOOGLE, message);
    }

    public void OnFacebookOAuthFailed(string message)
    {
        HandleOAuthFailure(AuthProvider.FACEBOOK, message);
    }

    private void BeginOAuthFlow(AuthProvider provider, UnityEvent launchEvent)
    {
        if (loginManager == null)
        {
            ShowFeedback("Login system not ready.", true);
            return;
        }

        if (loginManager.HasAuthenticatedUser)
        {
            ShowFeedback("Already logged in. Logout to switch accounts.", true);
            return;
        }

        if (launchEvent == null)
        {
            ShowFeedback($"{provider} sign-in not configured.", true);
            return;
        }

        pendingOAuthProvider = provider;
        isWaitingForExternalOAuth = true;
        SetBusy(true);

        launchEvent.Invoke();
    }

    private void CompleteOAuthLogin(AuthProvider provider, string token)
    {
        if (!isWaitingForExternalOAuth || pendingOAuthProvider != provider)
            return;

        if (string.IsNullOrEmpty(token))
        {
            HandleOAuthFailure(provider, "Missing auth token.");
            return;
        }

        switch (provider)
        {
            case AuthProvider.GOOGLE:
                loginManager?.LoginWithGoogle(token);
                break;
            case AuthProvider.FACEBOOK:
                loginManager?.LoginWithFacebook(token);
                break;
        }
    }

    private void HandleOAuthFailure(AuthProvider provider, string message)
    {
        if (pendingOAuthProvider == provider)
        {
            pendingOAuthProvider = AuthProvider.GUEST;
            isWaitingForExternalOAuth = false;
        }

        SetBusy(false);
        ShowFeedback(string.IsNullOrEmpty(message) ? $"{provider} sign-in failed." : message, true);
    }

    private void HandleSessionChanged(AuthSession session)
    {
        pendingOAuthProvider = AuthProvider.GUEST;
        isWaitingForExternalOAuth = false;
        SetBusy(false);

        if (session != null)
        {
            ShowFeedback("Authentication successful!", false);
        }
    }

    private void HandleAuthError(string message)
    {
        pendingOAuthProvider = AuthProvider.GUEST;
        isWaitingForExternalOAuth = false;
        SetBusy(false);
        ShowFeedback(message, true);
    }

    private void SetBusy(bool busy)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(busy);

        if (interactableCanvas != null)
        {
            interactableCanvas.interactable = !busy;
            interactableCanvas.blocksRaycasts = !busy;
            interactableCanvas.alpha = busy ? 0.5f : 1f;
        }
    }

    private void ShowFeedback(string message, bool isError)
    {
        if (feedbackLabel == null)
            return;

        feedbackLabel.text = string.IsNullOrEmpty(message) ? string.Empty : message;
        feedbackLabel.color = isError ? errorColor : successColor;
    }
}
