using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnGridReady;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;

    [SerializeField] private GameObject blastEffect;
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
    }

    private void Start()
    {
        canvas.ActivateMainMenu();
    }

    public void ChangeState(GameState newState)
    {
        if(_state==newState) return;
        
        Debug.Log("State changing from " + _state + " to " + newState);
        _state = newState;

        switch (newState)
        {
            case GameState.Menu:
                AudioManager.Instance.PlayMainMenuMusic();
                Reset();
                break;
                
            case GameState.Play:
                AudioManager.Instance.PlayGameSceneMusic();
                handler.AssignTarget();
                GridManager.Instance.GenerateGrid();
                break;

            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                break;

            case GameState.WaitingInput:
                BlockManager.Instance.FindAllNeighbours();
                OnGridReady?.Invoke();
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
                AudioManager.Instance.StopMusic();
                canvas.ActivateWinPanel();
                break;

            case GameState.Lose:
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