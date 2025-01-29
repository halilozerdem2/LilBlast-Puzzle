using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq; // _nodes.Where()
using DG.Tweening; //dotween
using System;  // ArgumentOutOfRangeException 
using Random = UnityEngine.Random; // Random.value
using Unity.VisualScripting;
using TreeEditor;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 7;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;

    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private Block[] blockTypes= new Block[5];

    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;


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
        {   //O(1)
            Block randomBlock = blockTypes[Random.Range(0, blockTypes.Length)];
            var block= Instantiate(randomBlock, node.Pos, Quaternion.identity);
            block.transform.SetParent(node.transform);
        }
        ChangeState(GameState.WaitingInput);
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
