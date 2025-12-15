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
    [SerializeField] MenuCanvasManager menuCanvas;
    [SerializeField] LevelCanvasManager levelCanvas;
    [SerializeField] ScoreManager score;
    [SerializeField] GameOverHandler handler;

   
    public GameState _state;
    private Coroutine winSequenceRoutine;
    private Coroutine losePanelRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;


        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        RefreshSceneBoundReferences();
        ChangeState(GameState.Menu);
        if(LevelManager.GetLastCompletedLevel()>=4)
            LevelManager.ResetProgress();
        
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneBoundReferences();
    }

    private void RefreshSceneBoundReferences()
    {
        

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            menuCanvas = FindObjectOfType<MenuCanvasManager>();
            levelCanvas = null;
            handler = null;
            score = null;
            powerUpManager = null;
        }
        else
        {
            shuffle = FindObjectOfType<ShuffleManager>();
            levelCanvas = FindObjectOfType<LevelCanvasManager>();
            menuCanvas = null;
            handler = FindObjectOfType<GameOverHandler>();
            score = FindObjectOfType<ScoreManager>();
            powerUpManager = FindObjectOfType<PowerUpManager>();
        }
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
                if (SceneManager.GetActiveScene().buildIndex != 0)
                {
                    SceneManager.LoadScene(0);
                    return;
                }
                LilManager.Instance?.SetToMenuSpwnPoint();
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
                shuffle?.HandleShuffle();
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
                const float loseDelay = 1f;
                if (losePanelRoutine != null)
                    StopCoroutine(losePanelRoutine);
                losePanelRoutine = StartCoroutine(ShowLosePanelWithDelay(loseDelay));
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
        levelCanvas?.ActivateWinPanel();
        AudioManager.Instance.isVictoryMode=false;
        winSequenceRoutine = null;
    }

    private IEnumerator ShowLosePanelWithDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        PauseGame();
        levelCanvas?.ActivateLostPanel();
        losePanelRoutine = null;
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
