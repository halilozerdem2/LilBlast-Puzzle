using UnityEngine;

/// <summary>
/// Simple helper that subscribes to LoginManager events and logs backend responses/errors.
/// Attach it anywhere in the scene while wiring the LoginManager reference.
/// </summary>
public class LoginDebugLogger : MonoBehaviour
{
    [SerializeField] private LoginManager loginManager;

    private void Reset()
    {
        if (loginManager == null)
            loginManager = FindObjectOfType<LoginManager>();
    }

    private void Awake()
    {
        if (loginManager == null)
            loginManager = FindObjectOfType<LoginManager>();
    }

    private void OnEnable()
    {
        if (loginManager == null)
            return;

        loginManager.SessionChanged += HandleSessionChanged;
        loginManager.AuthError += HandleAuthError;
        loginManager.AuthFlowDiagnostic += HandleDiagnostic;
    }

    private void OnDisable()
    {
        if (loginManager == null)
            return;

        loginManager.SessionChanged -= HandleSessionChanged;
        loginManager.AuthError -= HandleAuthError;
        loginManager.AuthFlowDiagnostic -= HandleDiagnostic;
    }

    private void HandleSessionChanged(AuthSession session)
    {
        if (session == null)
        {
            Debug.Log("[LoginDebugLogger] Session cleared.");
            return;
        }

        var label = !string.IsNullOrEmpty(session.Username) ? session.Username : session.UserId;
        Debug.Log($"[LoginDebugLogger] Logged in. User: {label} (Id: {session.UserId}), Provider: {session.Provider}, Guest: {session.IsGuest}");
    }

    private void HandleAuthError(string message)
    {
        Debug.LogWarning($"[LoginDebugLogger] Backend error: {message}");
    }

    private void HandleDiagnostic(string message)
    {
        Debug.Log($"[LoginDebugLogger] Flow: {message}");
    }
}
