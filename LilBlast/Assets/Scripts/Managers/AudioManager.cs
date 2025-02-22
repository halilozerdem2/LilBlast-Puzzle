using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] gameSceneMusic;

    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Awake()
    {
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.Stop();
    }
    private void Start()
    {
        PlayMainMenuMusic();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayNextGameTrack();
        }
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true;
            audioSource.Play();
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

        audioSource.clip = gameSceneMusic[currentTrackIndex];
        audioSource.loop = false;
        audioSource.Play();

        currentTrackIndex = (currentTrackIndex + 1) % gameSceneMusic.Length;
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
