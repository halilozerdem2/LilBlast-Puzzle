using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Music Toggle")]
    public ToggleSwitchAnimator musicSwitchAnimator;

    [Header("SFX Toggle")]
    public ToggleSwitchAnimator sfxSwitchAnimator;

    private bool isMusicOn;
    private bool isSFXOn;

    private void Start()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        AudioManager.Instance.ToggleMusic(isMusicOn);
        AudioManager.Instance.ToggleSFX(isSFXOn);

        UpdateUI();
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        AudioManager.Instance.ToggleMusic(isMusicOn);
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        musicSwitchAnimator.Toggle();
        UpdateUI();
    }

    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        AudioManager.Instance.ToggleSFX(isSFXOn);
        PlayerPrefs.SetInt("SFXOn", isSFXOn ? 1 : 0);
        sfxSwitchAnimator.Toggle();
        UpdateUI();
    }

    private void UpdateUI()
    {
        musicSwitchAnimator.SetState(isMusicOn);
        sfxSwitchAnimator.SetState(isSFXOn);
    }
}