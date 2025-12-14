using UnityEngine;
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
        ResetTimer();
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnGameStateChanged;
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
                // Süre doldu, manipülasyon başlat
                StartCoroutine(DoManipulation());
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
        Debug.Log($"Lil oyuncunun {amount} hamlesini azalttı!");
    }

    public void BlockRandomTiles(int count = 3)
    {
        AudioManager.Instance.PlayLilVoice(1);
        Debug.Log($"Lil tahtada {count} rastgele bloğu engelledi!");
        // BlockManager.Instance.BlockTiles(count); // 👈 gerçek logic buraya
    }

    public void DisablePowerUps(int duration = 3)
    {
        AudioManager.Instance.PlayLilVoice(2);
        Debug.Log($"Lil power-up kullanımını {duration} hamle boyunca engelledi!");
        // PowerUpManager.Instance.Disable(duration); // 👈 gerçek logic buraya
    }

    public void TransformTargetBlocks()
    {
        AudioManager.Instance.PlayLilVoice(3);
        Debug.Log("Lil tüm hedef blokları farklı renklere dönüştürdü!");
        // Handler / BlockManager üzerinden hedef bloklara müdahale edebilirsin
    }

    public void DestroySpecialBlocks(int count = 2)
    {
        AudioManager.Instance.PlayLilVoice(4);
        Debug.Log($"Lil {count} özel bloğu yok etti!");
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
