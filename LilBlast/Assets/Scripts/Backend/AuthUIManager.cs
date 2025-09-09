using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthUIManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public TextMeshProUGUI loginErrorText;

    [Header("Register UI")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerEmailInput;
    public Button registerButton;
    public TextMeshProUGUI registerErrorText;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    private void OnLoginClicked()
    {
        loginErrorText.text = "";
        var loginRequest = new LoginRequest
        {
            Username = loginUsernameInput.text,
            Password = loginPasswordInput.text
        };
        StartCoroutine(ApiManager.Instance.PostRequest<LoginRequest, User>(
            "/api/auth/login",
            loginRequest,
            user =>
            {
                PlayerDataManager.Instance.SetUser(user);
                PlayerUIManager.Instance.UpdateUI();
            },
            error => loginErrorText.text = "Login failed: " + error,
            welcomeMsg => loginErrorText.text = welcomeMsg
        ));
    }

    private void OnRegisterClicked()
    {
        registerErrorText.text = "";
        var registerRequest = new RegisterRequest
        {
            Username = registerUsernameInput.text,
            Password = registerPasswordInput.text,
            Email = registerEmailInput.text
        };
        StartCoroutine(ApiManager.Instance.PostRequest<RegisterRequest, User>(
            "/api/auth/register",
            registerRequest,
            user =>
            {
                PlayerDataManager.Instance.SetUser(user);
                PlayerUIManager.Instance.UpdateUI();
            },
            error => registerErrorText.text = "Register failed: " + error,
            welcomeMsg => registerErrorText.text = welcomeMsg
        ));
    }
}