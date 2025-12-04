using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] gameSceneMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip loseMusic;
    [SerializeField] public AudioClip loseSFX;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Lil Audio Clips")]
    [SerializeField] private AudioSource lilSource;       
    [SerializeField] private AudioClip[] lilClips;     


    private int currentTrackIndex = 0;
    public bool isVictoryMode = false;
    public bool isLoseMode = false;


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
    // Eğer kazanma veya kaybetme modundaysak müzik değiştirme
    if (isVictoryMode || isLoseMode) return;

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
        if(lilSource == null)
            lilSource = gameObject.AddComponent<AudioSource>();
        
        lilSource.loop = false;
        lilSource.volume = 1.0f; 
        musicSource.playOnAwake = false;

        musicSource.loop = false;
        musicSource.volume = 0.7f;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.volume = 0.7f;
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
    }
    public void PlayLoseSequence()
    {
        StopMusic();
        isLoseMode = true;
        sfxSource.PlayOneShot(loseSFX);
        Instance.StartCoroutine(PlayLoseMusicAfterSFX());
    }
    private IEnumerator PlayLoseMusicAfterSFX()
    {
        if (loseSFX == null)
        {
            Debug.LogError("LoseSFX is missing!");
            yield break;
        }

        yield return new WaitForSeconds(loseSFX.length);

        if (!isLoseMode) yield break; // bu sırada başka state’e geçtiyse çalma

        if (loseMusic == null)
        {
            Debug.LogError("LoseMusic is missing!");
            yield break;
        }
        musicSource.clip = loseMusic;
        musicSource.loop = true;
        musicSource.Play();
        isLoseMode = false; // müzik başladıktan sonra kaybetme modunu kapat
}
    
    public void PlayLilVoice(int clipIndex)
    {
        if (lilClips == null || lilClips.Length == 0) return;

        if (clipIndex < 0 || clipIndex >= lilClips.Length)
            clipIndex = 0; // default olarak ilk ses çalınır

        lilSource.PlayOneShot(lilClips[clipIndex]);
    }

}