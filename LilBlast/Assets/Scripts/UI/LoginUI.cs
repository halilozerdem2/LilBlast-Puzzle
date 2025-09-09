using System;
using UnityEngine;
using UnityEngine.Rendering;

public class LoginUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject userInfoPanel;
    public GameObject logoutButton; // Inspector'dan atayın

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OpenLoginOrInfoPanel()
    {
        if (PlayerDataManager.Instance.IsLoggedIn)
        {
            loginPanel.SetActive(false);
            userInfoPanel.SetActive(true);
        }
        else
        {
            userInfoPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
    }
    public void Logout()
    {
        PlayerDataManager.Instance.Logout();
    }


}
