using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnBlockSpawned;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;

    [SerializeField] private GameObject blastEffect;
    [SerializeField] ShuffleManager shuffle;
    [SerializeField] CanvasManager canvas;
    [SerializeField] GameOverHandler handler;
    public GameState _state;

    private void Awake()
    {
        Application.targetFrameRate = 60; // FPS'i 60'a sabitle
        QualitySettings.vSyncCount = 0;   // VSync'i kapat
        shuffle = GetComponentInChildren<ShuffleManager>();

        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Menu);
    }

    public void ChangeState(GameState newState)
    {
        Debug.Log("State changing from " + _state + " to " + newState);
        _state = newState;

        switch (newState)
        {
            case GameState.Menu:
                Reset();
                break;
                
            case GameState.Play:

                handler.AssignTarget();
                GridManager.Instance.GenerateGrid();
                //ChangeState(GameState.SpawningBlocks);
                break;

            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                OnBlockSpawned?.Invoke();
                break;

            case GameState.WaitingInput:
                break;

            case GameState.Blasting:
                break;
            case GameState.Falling:
                GridManager.Instance.UpdateGrid();
                break;

            case GameState.Deadlock:
                shuffle.HandleShuffle();
                break;

            case GameState.Win:
                canvas.ActivateWinPanel();
                break;

            case GameState.Lose:
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
        foreach (var block in BlockManager.Instance._blocks)
        {
            Destroy(block.gameObject);
        }
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            Destroy(node);
        }

        BlockManager.Instance._blocks.Clear();
        GridManager.freeNodes.Clear();
        GridManager.Instance._nodes.Clear();
        handler.collectedBlocks.Clear();
        shuffle.availableNodes.Clear();
        shuffle.availableNodes.Clear();
    }



    public enum GameState
    {
        Menu,
        Play,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        Falling,
        Deadlock,
        Win,
        Lose,
    }
}