using TMPro;
using UnityEngine;

/// <summary>
/// Centralizes password visibility toggling and input clearing for the authentication panel.
/// Wire it to the LoginPanel and hook UI buttons/toggles to the public methods.
/// </summary>
public class AuthInputFieldController : MonoBehaviour
{
    [Header("Login Inputs")]
    [SerializeField] private TMP_InputField[] loginInputs;
    [SerializeField] private TMP_InputField[] loginPasswordInputs;

    [Header("Sign-In Inputs")]
    [SerializeField] private TMP_InputField[] signInInputs;
    [SerializeField] private TMP_InputField[] signInPasswordInputs;

    [Header("Behaviour")]
    [SerializeField] private bool passwordsHiddenByDefault = true;
    [SerializeField] private char passwordMaskCharacter = '\u25CF'; // •

    private bool passwordsHidden;

    private void Awake()
    {
        passwordsHidden = passwordsHiddenByDefault;
        ApplyVisibilityToGroup(loginPasswordInputs, passwordsHidden);
        ApplyVisibilityToGroup(signInPasswordInputs, passwordsHidden);
    }

    public void TogglePasswordVisibility()
    {
        passwordsHidden = !passwordsHidden;
        ApplyVisibilityToGroup(loginPasswordInputs, passwordsHidden);
        ApplyVisibilityToGroup(signInPasswordInputs, passwordsHidden);
    }

    public void ClearLoginFields()
    {
        ClearGroup(loginInputs);
        ClearGroup(loginPasswordInputs);
    }

    public void ClearSignInFields()
    {
        ClearGroup(signInInputs);
        ClearGroup(signInPasswordInputs);
    }

    public void ClearAllFields()
    {
        ClearLoginFields();
        ClearSignInFields();
    }

    private void ClearGroup(TMP_InputField[] fields)
    {
        if (fields == null)
            return;

        foreach (var field in fields)
        {
            if (field == null)
                continue;
            field.text = string.Empty;
            field.ForceLabelUpdate();
        }
    }

    private void ApplyVisibilityToGroup(TMP_InputField[] fields, bool hide)
    {
        if (fields == null)
            return;

        foreach (var field in fields)
        {
            if (field == null)
                continue;

            field.contentType = hide ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            field.asteriskChar = passwordMaskCharacter;
            field.ForceLabelUpdate();
        }
    }
}
