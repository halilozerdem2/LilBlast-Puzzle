using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the login button visuals in sync with the current auth session.
/// Toggling the logged-in / logged-out UI states happens automatically when LoginManager fires SessionChanged.
/// </summary>
public class LoginButtonController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LoginManager loginManager;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;

    [Header("State Containers")]
    [SerializeField] private GameObject[] showWhenLoggedIn;
    [SerializeField] private GameObject[] showWhenLoggedOut;

    [Header("Labels")]
    [SerializeField] private TMP_Text usernameLabel;
    [SerializeField] private string loggedOutUsername = "Guest";
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private string loggedInStatusText = "Tap to edit";
    [SerializeField] private string loggedOutStatusText = "Tap to log in";

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();

        if (loginManager != null)
        {
            loginManager.SessionChanged += HandleSessionChanged;
            HandleSessionChanged(loginManager.CurrentSession);
        }
        else
        {
            HandleSessionChanged(null);
        }
    }

    private void OnDisable()
    {
        if (loginManager != null)
            loginManager.SessionChanged -= HandleSessionChanged;
    }

    private void HandleSessionChanged(AuthSession session)
    {
        UpdateUi(session);
    }

    private void UpdateUi(AuthSession session)
    {
        bool hasAuthenticatedUser = session != null && !session.IsGuest;
        ToggleGroup(showWhenLoggedIn, hasAuthenticatedUser);
        ToggleGroup(showWhenLoggedOut, !hasAuthenticatedUser);

        if (loginButton != null)
            loginButton.interactable = !hasAuthenticatedUser;

        if (logoutButton != null)
            logoutButton.interactable = hasAuthenticatedUser;

        if (usernameLabel != null)
            usernameLabel.text = ResolveDisplayName(session, hasAuthenticatedUser);

        if (statusLabel != null)
            statusLabel.text = hasAuthenticatedUser ? loggedInStatusText : loggedOutStatusText;
    }

    private string ResolveDisplayName(AuthSession session, bool hasAuthenticatedUser)
    {
        if (!hasAuthenticatedUser || session == null)
            return loggedOutUsername;

        if (!string.IsNullOrEmpty(session.Username))
            return session.Username;

        return string.IsNullOrEmpty(session.UserId) ? loggedOutUsername : session.UserId;
    }

    private void ToggleGroup(GameObject[] targets, bool state)
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            var target = targets[i];
            if (target != null)
                target.SetActive(state);
        }
    }

    private void CacheReferences()
    {
        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();

        if (loginButton == null)
            loginButton = GetComponent<Button>();
    }
}
