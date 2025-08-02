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
    [SerializeField] CanvasManager canvas;
    [SerializeField] ScoreManager score;
    [SerializeField] GameOverHandler handler;

   
    public GameState _state;

    private void Awake()
    {
        Application.targetFrameRate = 60; // FPS'i 60'a sabitle
        QualitySettings.vSyncCount = 0;   // VSync'i kapat
        Instance = this;
        DontDestroyOnLoad(this);
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
                Reset();
                canvas.ActivateMainMenu();
                AudioManager.Instance.PlayMainMenuMusic();
                break;
                
            case GameState.Play:
                AudioManager.Instance.PlayGameSceneMusic();
                handler.AssignTarget();
                break;

            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                break;

            case GameState.WaitingInput:
                BlockManager.Instance.FindAllNeighbours();
                OnGridReady?.Invoke();

                if (handler.pendingWin)
                {
                    handler.pendingWin = false;
                    ChangeState(GameState.Win);
                }
                break;

            case GameState.Blasting:
                break;

            case GameState.Falling:
                GridManager.Instance.UpdateGrid();
                break;
            case GameState.Pause:
                break;
            case GameState.Shuffling:
                shuffle.HandleShuffle();
                OnGridReady?.Invoke();
                break;

            case GameState.Win:
                AudioManager.Instance.PlayVictorySound();
                StartCoroutine(PlayWinSequence());
                break;


            case GameState.Lose:
                Reset();
                AudioManager.Instance.StopMusic();
                canvas.ActivateLostPanel();
                break;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Reset()
    {
        Debug.Log("Reset");
        // Grid ve blokları temizle
        foreach (var block in BlockManager.Instance.blocks)
        {
            Destroy(block.gameObject);
        }
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            Destroy(node);
        }

        BlockManager.Instance.blocks.Clear();
        GridManager.freeNodes.Clear();
        GridManager.Instance._nodes.Clear();
        handler.collectedBlocks.Clear();
        shuffle.availableNodes.Clear();
        shuffle.availableNodes.Clear();
        score.ResetScore();
    }
    
    private IEnumerator PlayWinSequence()
    {
        LevelManager.SaveLevelProgress(SceneManager.GetActiveScene().buildIndex);
        yield return new WaitForSeconds(1f);
        BlockManager.Instance.BlastAllBlocks(false);
        Debug.Log("Win sequence started!");
        
        yield return new WaitForSeconds(3.0f);

        canvas.ActivateWinPanel();

        Reset();
        AudioManager.Instance.isVictoryMode=false;
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
    }
}