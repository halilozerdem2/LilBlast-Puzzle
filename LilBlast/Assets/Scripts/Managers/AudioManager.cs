using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] gameSceneMusic;
    [SerializeField] AudioClip victoryMusic;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private int currentTrackIndex = 0;
    public bool isVictoryMode = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAudioSources();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMainMenuMusic();
    }

    private void Update()
    {
        if (isVictoryMode) return;

        if (!musicSource.isPlaying && gameSceneMusic.Length > 0)
        {
            PlayNextGameTrack();
        }
    }


    private void InitializeAudioSources()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = false;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    private void LoadSettings()
    {
        bool musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        ToggleMusic(musicOn);
        ToggleSFX(sfxOn);
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayGameSceneMusic()
    {
        currentTrackIndex = 0;
        PlayNextGameTrack();
    }

    private void PlayNextGameTrack()
    {
        if (gameSceneMusic.Length == 0) return;

        musicSource.clip = gameSceneMusic[currentTrackIndex];
        musicSource.loop = false;
        musicSource.Play();

        currentTrackIndex = (currentTrackIndex + 1) % gameSceneMusic.Length;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
    }

    public void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;
    }

    public bool IsMusicOn() => !musicSource.mute;

    public bool IsSFXOn() => !sfxSource.mute;

    // 🔉 Harici Clip çalmak istersen:
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && !IsSFXOn()) return;
        sfxSource.PlayOneShot(clip);
    }

    // 🎯 ObjectPool üzerinden ses efektini tip ID ile çal
    public void PlaySFX(int type)
    {
        if (!IsSFXOn()) return;
        ObjectPool.Instance?.PlaySound(type);
    }
    
    public void PlayVictorySound()
    {
        StopMusic();
        isVictoryMode = true;
        sfxSource.PlayOneShot(victoryMusic);
        Debug.Log("Victory sound played!");
    }

}
