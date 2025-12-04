using UnityEngine;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;

public class AuthWarningManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    public void ShowWarning(string message)
    {
        if (warningPanel == null || warningText == null) return;

        warningPanel.SetActive(true);
        warningText.text = message;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideWarningAfterDelay(2f));
    }

    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningPanel.SetActive(false);
    }

    // --- Validation Methods ---

    public bool ValidateRegister(string username, string email, string password, System.Func<string, bool> isUsernameTaken)
    {
        // Empty fields
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowWarning("Please fill all fields.");
            return false;
        }

        // Username: no special chars, no space
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
        {
            ShowWarning("Username can't contain special characters or spaces.");
            return false;
        }

        // Username: unique
        if (isUsernameTaken != null && isUsernameTaken(username))
        {
            ShowWarning("This username is already taken.");
            return false;
        }

        // Email: basic domain check
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowWarning("Please enter a valid email address.");
            return false;
        }

        // Password: min 6 chars
        if (password.Length < 6)
        {
            ShowWarning("Password must be at least 6 characters.");
            return false;
        }
        ShowWarning("Welcome " + username+ "!");

        return true;
    }

    public bool ValidateLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowWarning("Please fill all fields.");
            return false;
        }
        return true;
    }

    public void ShowLoginFailed()
    {
        ShowWarning("Wrong username or password.");
    }
}