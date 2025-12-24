using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the visuals of a single level selection button based on completion state.
/// Handles icon swapping, lock indicator visibility, button interactability and star display.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButtonController : MonoBehaviour
{
    public enum LevelStatus
    {
        Locked,
        Current,
        Completed
    }

    [SerializeField] [Min(LevelManager.FirstGameplayLevelBuildIndex)] private int levelIndex = 1;
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelNumberLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockIndicator;
    [SerializeField] private GameObject starsContainer;
    [SerializeField] private Image[] starImages;

    [Header("Sprites")]
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite completedSprite;

    [Header("Star Colors")]
    [SerializeField] private Color earnedStarColor = Color.white;
    [SerializeField] private Color missingStarColor = new Color(1f, 1f, 1f, 0.2f);

    public LevelStatus Status { get; private set; } = LevelStatus.Locked;
    public bool IsCompleted { get; private set; }
    public bool IsLastLevelReached { get; private set; }

    private LoginManager loginManager;
    private bool sessionSubscribed;

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        SubscribeToSessionChanges();
        LevelManager.LevelProgressUpdated += HandleBackendProgressChanged;
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromSessionChanges();
        LevelManager.LevelProgressUpdated -= HandleBackendProgressChanged;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheComponents();
        levelIndex = Mathf.Max(levelIndex, LevelManager.FirstGameplayLevelBuildIndex);
        Refresh();
    }
#endif

    public void Refresh()
    {
        levelIndex = Mathf.Clamp(levelIndex, LevelManager.FirstGameplayLevelBuildIndex, LevelManager.LastGameplayLevelBuildIndex);
        int lastCompleted = LevelManager.GetLastCompletedLevel();
        int nextPlayable = Mathf.Clamp(lastCompleted + 1, LevelManager.FirstGameplayLevelBuildIndex, LevelManager.LastGameplayLevelBuildIndex);

        IsCompleted = levelIndex <= lastCompleted;
        IsLastLevelReached = !IsCompleted && levelIndex == nextPlayable;

        if (IsCompleted)
            Status = LevelStatus.Completed;
        else if (IsLastLevelReached)
            Status = LevelStatus.Current;
        else
            Status = LevelStatus.Locked;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (levelNumberLabel != null)
            levelNumberLabel.text = levelIndex.ToString();

        switch (Status)
        {
            case LevelStatus.Completed:
                SetIconSprite(completedSprite);
                ToggleLock(false);
                SetButtonInteractable(true);
                ShowStars(LevelManager.GetLevelStarCount(levelIndex));
                break;
            case LevelStatus.Current:
                SetIconSprite(currentSprite != null ? currentSprite : lockedSprite);
                ToggleLock(false);
                SetButtonInteractable(true);
                ShowStars(0);
                break;
            case LevelStatus.Locked:
            default:
                SetIconSprite(lockedSprite);
                ToggleLock(true);
                SetButtonInteractable(false);
                ShowStars(0);
                break;
        }
    }

    private void SetIconSprite(Sprite sprite)
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (iconImage != null)
            iconImage.sprite = sprite;
    }

    private void ToggleLock(bool showLock)
    {
        if (lockIndicator != null)
            lockIndicator.SetActive(showLock);
    }

    private void SetButtonInteractable(bool interactable)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.interactable = interactable;
    }

    private void ShowStars(int starCount)
    {
        bool show = Status == LevelStatus.Completed;
        if (starsContainer != null)
            starsContainer.SetActive(show);

        if (starImages == null)
            return;

        starCount = Mathf.Clamp(starCount, 0, starImages.Length);
        for (int i = 0; i < starImages.Length; i++)
        {
            var star = starImages[i];
            if (star == null)
                continue;

            bool earned = i < starCount;
            star.gameObject.SetActive(show);
            star.color = earned ? earnedStarColor : missingStarColor;
        }
    }

    private void CacheComponents()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (iconImage == null)
            iconImage = GetComponent<Image>();
    }

    private void SubscribeToSessionChanges()
    {
        if (sessionSubscribed)
            return;

        if (loginManager == null)
            loginManager = LoginManager.Instance ?? FindObjectOfType<LoginManager>();

        if (loginManager == null)
            return;

        loginManager.SessionChanged += HandleSessionChanged;
        sessionSubscribed = true;
    }

    private void UnsubscribeFromSessionChanges()
    {
        if (!sessionSubscribed || loginManager == null)
            return;

        loginManager.SessionChanged -= HandleSessionChanged;
        sessionSubscribed = false;
    }

    private void HandleSessionChanged(AuthSession session)
    {
        Refresh();
    }

    private void HandleBackendProgressChanged()
    {
        Refresh();
    }

    public void LoadAssignedLevel()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("LevelButtonController: LevelManager.Instance missing, cannot load level.");
            return;
        }

        LevelManager.Instance.LoadLevel(levelIndex, levelIndex);
    }
}
