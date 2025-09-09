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

    [Header("Register UI")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerEmailInput;
    public Button registerButton;

    public AuthWarningManager authWarningManager; // Inspector'dan atayın

    // Username kontrolü için örnek fonksiyon (backend ile async kontrol önerilir)
    private bool IsUsernameTaken(string username)
    {
        // Burada backend sorgusu ile kontrol etmelisiniz.
        // Şimdilik false dönelim (kullanıcı adı alınmamış gibi)
        return false;
    }

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    public void OnLoginClicked()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (!authWarningManager.ValidateLogin(username, password))
            return;

        // Buraya kadar geldiyse giriş kuralları sağlanmıştır, login işlemini başlatabilirsiniz
        StartCoroutine(ApiManager.Instance.PostRequest<LoginRequest, User>(
            "/api/auth/login",
            new LoginRequest { Username = username, Password = password },
            user =>
            {
                PlayerDataManager.Instance.SetUser(user);
                PlayerUIManager.Instance.UpdateUI();
                StartCoroutine(Deactive());
                
            },
            error =>
            {
                authWarningManager.ShowLoginFailed();
            }
        ));
    }

    IEnumerator Deactive()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
    public void OnRegisterClicked()
    {
        string username = registerUsernameInput.text;
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;

        if (!authWarningManager.ValidateRegister(username, email, password, IsUsernameTaken))
            return;

        // Buraya kadar geldiyse kayıt kuralları sağlanmıştır, register işlemini başlatabilirsiniz
        StartCoroutine(ApiManager.Instance.PostRequest<RegisterRequest, User>(
            "/api/auth/register",
            new RegisterRequest { Username = username, Password = password, Email = email },
            user =>
            {
                PlayerDataManager.Instance.SetUser(user);
                PlayerUIManager.Instance.UpdateUI();
                StartCoroutine(Deactive());
            },
            error =>
            {
                authWarningManager.ShowWarning("Register failed: " + error);
            }
        ));
    }
}