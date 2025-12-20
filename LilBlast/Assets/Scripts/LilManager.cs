using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LilManager : MonoBehaviour
{
    public static LilManager Instance;

    [Header("Lil Settings")]
    [Tooltip("Manipülasyonlar arasında minimum süre (saniye)")]
    public float minManipulationTime = 7f;
    [Tooltip("Manipülasyonlar arasında maksimum süre (saniye)")]
    public float maxManipulationTime = 15f;

    public Transform playSpawnPoint,menuSpawnPoint;
    private float manipulationTime;    // Hedef süre
    private float countdown;           // Geçen süre
    private bool isManipulating = false;
    private bool isPaused = false;

    private CharacterAnimationController lilcontroller;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        lilcontroller = GetComponent<CharacterAnimationController>();
        GameManager.OnStateChanged += OnGameStateChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ResetTimer();
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnGameStateChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        if (isPaused || isManipulating) return;

        // Sadece oyun WaitingInput state’indeyken süreyi say
        if (GameManager.Instance._state == GameManager.GameState.WaitingInput)
        {
            countdown += Time.deltaTime;

            if (countdown >= manipulationTime)
            {
                if (ShouldLilIntervene())
                {
                    // Süre doldu, manipülasyon başlat
                    StartCoroutine(DoManipulation());
                }
                else
                {
                    // Bu turda Lil devreye girmiyor, süreyi sıfırla
                    ResetTimer();
                }
            }
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        // Eğer oyun resetlenirse sayacı başa al
        if (state == GameManager.GameState.Menu || state == GameManager.GameState.Play)
        {
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        countdown = 0f;
        manipulationTime = Random.Range(minManipulationTime, maxManipulationTime);
    }

    private bool ShouldLilIntervene()
    {
        var difficultyManager = DifficultyManager.Instance;
        float involvementChance = difficultyManager != null
            ? Mathf.Clamp01(difficultyManager.CurrentSettings.lilInvolvingPercentage)
            : 0.5f;
        return Random.value <= involvementChance;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            SetToMenuSpwnPoint();
        }
    }

    private IEnumerator DoManipulation()
    {
        isManipulating = true;
        GameManager.Instance.ChangeState(GameManager.GameState.Manipulating);

        int choice = Random.Range(0, 5);
        switch (choice)
        {
            case 0:  ReduceMoves(2); break;
            case 1: BlockRandomTiles(); break;
            case 2: DisablePowerUps(); break;
            case 3: TransformTargetBlocks(); break;
            case 4: DestroySpecialBlocks(); break;
        }

        yield return new WaitForSeconds(2f);

        BlockManager.Instance.SetAllBlocksInteractable(true);
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);

        // Timer resetle
        ResetTimer();
        isManipulating = false;
    }

    public void PauseManipulations()
    {
        if (isPaused)
            return;
        isPaused = true;
        countdown = 0f;
        if (isManipulating)
        {
            StopAllCoroutines();
            isManipulating = false;
        }
    }

    public void ResumeManipulations()
    {
        if (!isPaused)
            return;
        ResetTimer();
        isPaused = false;
    }

    // --- Manipülasyon fonksiyonları ---
    public void ReduceMoves(int amount = 1)
    {
        AudioManager.Instance.PlayLilVoice(0);
        GameOverHandler.Instance.DecreaseMove(amount);
    }

    public void BlockRandomTiles(int count = 3)
    {
        AudioManager.Instance.PlayLilVoice(1);
        // BlockManager.Instance.BlockTiles(count); // 👈 gerçek logic buraya
    }

    public void DisablePowerUps(int duration = 3)
    {
        AudioManager.Instance.PlayLilVoice(2);
        // PowerUpManager.Instance.Disable(duration); // 👈 gerçek logic buraya
    }

    public void TransformTargetBlocks()
    {
        AudioManager.Instance.PlayLilVoice(3);
        // Handler / BlockManager üzerinden hedef bloklara müdahale edebilirsin
    }

    public void DestroySpecialBlocks(int count = 2)
    {
        AudioManager.Instance.PlayLilVoice(4);
        // BlockManager.Instance.DestroySpecial(count); // 👈 gerçek logic buraya
    }

    public void SpawnLilAt(Transform aSpawnPoint)
    {
        transform.position = aSpawnPoint.position;
        transform.rotation = aSpawnPoint.rotation;
        transform.localScale = aSpawnPoint.localScale;
    }
    
    public void SetToPlaySpawn()
    {
        lilcontroller.OnPlayStateEnter();
        SpawnLilAt(playSpawnPoint);
    }

    public void SetToMenuSpwnPoint()
    {
        lilcontroller.SetMenuAnimations();
        SpawnLilAt(menuSpawnPoint);
    }
}
