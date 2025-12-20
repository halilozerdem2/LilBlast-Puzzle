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
            return;
        }
    }

    private void HandleAuthError(string message)
    {
    }

    private void HandleDiagnostic(string message)
    {
    }
}
