using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnBlockSpawned;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;

    [SerializeField] private GameObject blastEffect;
    ShuffleManager shuffle;

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
        ChangeState(GameState.Play);
    }

    public void ChangeState(GameState newState)
    {
        _state = newState;
        switch (newState)
        {
            case GameState.Menu:
                //
                break;
            case GameState.Play:
                //SceneManager.LoadScene(1);
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                OnBlockSpawned?.Invoke();
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Blasting:
                GridManager.Instance.UpdateGrid();
                break;
            case GameState.Deadlock:
                shuffle.HandleShuffle();
                break;
            case GameState.Win:
                SceneManager.LoadScene(2);
                break;
            case GameState.Lose:
                SceneManager.LoadScene(3);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    

    public enum GameState
    {
        Menu,
        Play,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        Deadlock,
        Win,
        Lose,
        Pause
    }
}