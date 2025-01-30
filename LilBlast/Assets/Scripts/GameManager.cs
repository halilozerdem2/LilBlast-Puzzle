using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 7;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block[] blockTypes;
    [SerializeField] private SpriteRenderer _boardPrefab;

    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    void GenerateGrid()
    {
        _nodes = new List<Node>();
        _blocks = new List<Block>();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector3(x, y), Quaternion.identity);
                node.gridPosition = new Vector2Int(x, y);
                _nodes.Add(node);
            }
        }

        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(_width, _height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeState(GameState.SpawningBlocks);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;
        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks();
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Blasting:
                break;
            case GameState.NoMoreMove:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void SpawnBlocks()
    {
        foreach (var node in _nodes)
        {
            Block randomBlock = Instantiate(blockTypes[Random.Range(0, blockTypes.Length)], node.Pos, Quaternion.identity);
            randomBlock.SetBlock(node);
            _blocks.Add(randomBlock);
        }
        FindAllNeighbours();
        ChangeState(GameState.WaitingInput);
    }

    private void FindAllNeighbours()
    {
        foreach (var block in _blocks)
        {
            block.FindNeighbors(_nodes);
        }
    }

    public void TryBlastBlock(Block block)
    {
        HashSet<Block> group = block.FloodFill();
        if (group.Count >= 2)  // En az 2 blok olmalı
        {
            foreach (var b in group)
            {
                Destroy(b.gameObject);
            }
            ChangeState(GameState.Blasting);
        }
    }

    public enum GameState
    {
        GenerateLevel,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        NoMoreMove,
        Win,
        Lose,
        Pause
    }
}
