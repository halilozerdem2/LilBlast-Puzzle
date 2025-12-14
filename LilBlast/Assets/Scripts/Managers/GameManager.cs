using UnityEngine;
using System;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnGridReady;
    public static event Action<GameState> OnStateChanged;


    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    
    [SerializeField] ShuffleManager shuffle;
   [SerializeField] PowerUpManager powerUpManager;
    [SerializeField] CanvasManager canvas;
    [SerializeField] ScoreManager score;
    [SerializeField] GameOverHandler handler;

   
    public GameState _state;
    private Coroutine winSequenceRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 120; // FPS'i 60'a sabitle
        QualitySettings.vSyncCount = 0;   // VSync'i kapat
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.Menu);
        if(LevelManager.GetLastCompletedLevel()>=4)
            LevelManager.ResetProgress();
        
    }

    public void ChangeState(GameState newState)
    {
        if(_state==newState) return;
        
        Debug.Log("State changing from " + _state + " to " + newState);
        _state = newState;
        
        OnStateChanged?.Invoke(_state);

        switch (newState)
        {
            case GameState.Menu:
                LilManager.Instance?.SetToMenuSpwnPoint();
                PauseGameplaySystems();
                canvas.ActivateMainMenu();
                Time.timeScale = 1;
                AudioManager.Instance.PlayMainMenuMusic();
                break;
                
            case GameState.Play:
                LilManager.Instance?.SetToPlaySpawn();
                LilManager.Instance?.ResumeManipulations();
                AudioManager.Instance.PlayGameSceneMusic();
                handler.AssignTarget();
                break;

            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                break;

            case GameState.WaitingInput:
                OnGridReady?.Invoke();

                if (GameOverHandler.Instance.pendingWin)
                {
                    GameOverHandler.Instance.pendingWin = false;
                    ChangeState(GameState.Win);
                }
                break;

            case GameState.Blasting:
                break;

            case GameState.Falling:
                GridManager.Instance.UpdateGrid();
                break;
            case GameState.Pause:
                PauseGame();
                break;
            case GameState.Shuffling:
                shuffle.HandleShuffle();
                OnGridReady?.Invoke();  
                break;

            case GameState.Win:
                var usageSnapshot = powerUpManager != null ? powerUpManager.ConsumeUsageSnapshot() : new PowerUpUsageSnapshot();
                LevelManager.Instance.CompleteLevel(score.currentScore, handler.moves, powerUpManager.CalculateSpentPowerUpAmount(), usageSnapshot);
                AudioManager.Instance.PlayVictorySound();
                if (winSequenceRoutine != null)
                    StopCoroutine(winSequenceRoutine);
                winSequenceRoutine = StartCoroutine(PlayWinSequence());
                break;


            case GameState.Lose:
                LevelManager.Instance.FailLevel();
                AudioManager.Instance.PlayLoseSequence();
                StartCoroutine(ShowLosePanelWithDelay(AudioManager.Instance.loseSFX.length));
                break;
            case GameState.Manipulating:
                BlockManager.Instance.SetAllBlocksInteractable(false);
                break;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        BlockManager.Instance.SetAllBlocksInteractable(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        BlockManager.Instance.SetAllBlocksInteractable(true);
        Instance.ChangeState(GameManager.GameState.WaitingInput);
    }
    public void Reset()
    {
        Debug.Log("Reset");
        // Grid ve blokları temizle
        for (int i = BlockManager.Instance.blocks.Count - 1; i >= 0; i--)
        {
            var block = BlockManager.Instance.blocks[i];
            if (block == null) continue;
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnBlockToPool(block);
            else
                Destroy(block.gameObject);
        }
        GridManager.Instance.ResetGrid();

        if (winSequenceRoutine != null)
        {
            StopCoroutine(winSequenceRoutine);
            winSequenceRoutine = null;
        }

        BlockManager.Instance.blocks.Clear();
        handler.collectedBlocks.Clear();
        shuffle.availableNodes.Clear();
        score.ResetScore();
        handler.pendingWin = false;
        handler.AssignTarget();
        BlockManager.Instance.AllowRefills();
    }

    private void PauseGameplaySystems()
    {
        BlockManager.Instance?.StopAllCoroutines();
        BlockManager.Instance?.SetAllBlocksInteractable(false);
        BlockManager.Instance?.AllowRefills();
        shuffle?.StopAllCoroutines();
        LilManager.Instance?.PauseManipulations();
    }
    
    private IEnumerator PlayWinSequence()
    {
        yield return new WaitForSeconds(1f);
        BlockManager.Instance.BlastAllBlocks(false);
        yield return new WaitForSeconds(5.0f);
        canvas.ActivateWinPanel();
        Reset();
        AudioManager.Instance.isVictoryMode=false;
        winSequenceRoutine = null;
    }

    private IEnumerator ShowLosePanelWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Instance.PauseGame();
        canvas.ActivateLostPanel();
    }
    
    public enum GameState
    {
        Menu,
         Pause,
        Play,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        Falling,
        Shuffling,
        Win,
        Lose,
        Manipulating,
    }
}
