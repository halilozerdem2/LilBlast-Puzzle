using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 7;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block[] blockTypes;
    [SerializeField] private SpriteRenderer _boardPrefab;

    private Dictionary<Vector2Int, Node> _nodes;
    private List<Block> _blocks;
    private HashSet<Node> freeNodes; // Boş olan düğümleri takip eden liste

    private GameState _state;

    private void Awake()
    {
        Instance = this;
        _nodes = new Dictionary<Vector2Int, Node>();
        _blocks = new List<Block>();
        freeNodes = new HashSet<Node>(); // Boş düğümler burada saklanacak
    }

    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector3(x, y), Quaternion.identity);
                node.gridPosition = new Vector2Int(x, y);
                _nodes[node.gridPosition] = node;

                freeNodes.Add(node); // Başlangıçta tüm düğümler boş olacak
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
                UpdateGrid();
                break;
            case GameState.Deadlock:
                StartCoroutine(ShuffleBoard());
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
        StartCoroutine(SpawnBlocksCoroutine());
    }
    private IEnumerator SpawnBlocksCoroutine()
    {
        yield return new WaitForSeconds(0.2f); // Bloklar düştükten sonra 0.2 saniye bekle

        UpdateFreeNodes();
        List<Node> nodesToFill = freeNodes.ToList(); // Şu anda boş olan düğümleri listeye al

        foreach (var node in nodesToFill)
        {
            // Blokları üstten (örneğin _height + 1 seviyesinden) spawn et
            Vector3 spawnPos = new Vector3(node.Pos.x, _height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[Random.Range(0, blockTypes.Length)], spawnPos, Quaternion.identity);

            randomBlock.SetBlock(node);
            _blocks.Add(randomBlock);
            freeNodes.Remove(node); // Artık dolu, freeNodes listesinden çıkar

            // Animasyonlu düşme efekti
            randomBlock.transform.DOMove(node.Pos, 0.5f).SetEase(Ease.OutBounce);
        }

        FindAllNeighbours();
        if (HasValidMoves())
            ChangeState(GameState.WaitingInput);
        else
            ChangeState(GameState.Deadlock);
    }

    private void FindAllNeighbours()
    {
        foreach (var block in _blocks)
        {
            if (block != null)
            {
                block.FindNeighbours(_nodes.Values.ToList());
            }
        }
    }

    private bool HasValidMoves() // Deadlock Tespiti
    {
        foreach (var node in _nodes.Values)
        {
            if (node.OccupiedBlock == null) continue;

            Block currentBlock = node.OccupiedBlock;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                if (_nodes.TryGetValue(node.gridPosition + dir, out Node neighborNode))
                {
                    if (neighborNode.OccupiedBlock != null && neighborNode.OccupiedBlock.blockType == currentBlock.blockType)
                    {
                        return true; // En az bir geçerli hamle var, shuffle yapmaya gerek yok
                    }
                }
            }
        }
        return false; // Deadlock durumu var, shuffle yapılmalı
    }

    public void TryBlastBlock(Block block)
    {
        HashSet<Block> group = block.FloodFill();
        if (group.Count >= 2)
        {
            foreach (var b in group)
            {
                if (b.node != null)
                {
                    b.node.OccupiedBlock = null;
                    freeNodes.Add(b.node); // Boşalan düğümü freeNodes'a ekle
                }

                Destroy(b.gameObject);
            }

            UpdateFreeNodes();
            FindAllNeighbours();
            ChangeState(GameState.Blasting);
        }
        block.Shake();
    }


    public void UpdateGrid() // O(n^3) karmaşıklığındaki yapı dicitonary kullanılarak               
    {                        // O(n^2 Log N) seviyesine düşürülecek
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Node currentNode = _nodes.FirstOrDefault(n => n.Key.x == x && n.Key.y == y).Value;
                if (currentNode == null || currentNode.OccupiedBlock != null) continue;

                for (int k = y + 1; k < _height; k++)
                {
                    Node upperNode = _nodes.FirstOrDefault(n => n.Key.x == x && n.Key.y == k).Value;
                    if (upperNode == null) continue;
                    if (upperNode.OccupiedBlock != null)
                    {
                        // Swap 
                        Block blockToMove = upperNode.OccupiedBlock;
                        blockToMove.SetBlock(currentNode);
                        blockToMove.transform.DOMove(currentNode.Pos, 0.3f).SetEase(Ease.OutBounce);

                        upperNode.OccupiedBlock = null;
                        freeNodes.Add(upperNode);
                        freeNodes.Remove(currentNode);
                        break;
                    }
                }
            }
        }
        SpawnBlocks();
    }

    private IEnumerator ShuffleBoard()
    {
        Debug.Log("Shuffling started...");

        List<Block> allBlocks = _blocks.ToList();

        for (int i = 0; i < allBlocks.Count; i++)
        {
            int randomIndex = Random.Range(0, allBlocks.Count);

            if (i != randomIndex)
            {
                yield return StartCoroutine(SwapBlocksAnimated(allBlocks[i], allBlocks[randomIndex]));
            }
        }

        yield return new WaitForSeconds(0.5f); // Swap işlemlerinin bitmesini bekle

        FindAllNeighbours(); // Yeni komşulukları güncelle

        if (!HasValidMoves())
        {
            Debug.LogWarning("No valid moves found after shuffle. Retrying...");
            yield return StartCoroutine(ShuffleBoard()); // Yeniden shuffle et
        }
        else
        {
            Debug.Log("Shuffle successful!");
            ChangeState(GameState.WaitingInput);
        }
    }


    private IEnumerator SwapBlocksAnimated(Block blockA, Block blockB)
    {
        if (blockA == null || blockB == null)
        {
            Debug.LogWarning("Trying to swap a null block! Skipping swap.");
            yield break;
        }

        Vector3 startA = blockA.transform.position;
        Vector3 startB = blockB.transform.position;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            blockA.transform.position = Vector3.Lerp(startA, startB, t);
            blockB.transform.position = Vector3.Lerp(startB, startA, t);

            yield return null;
        }

        // Son pozisyonları tam olarak ayarla
        blockA.transform.position = startB;
        blockB.transform.position = startA;

        // Blokların bağlı olduğu node'ları değiştir
        Node tempNode = blockA.node;
        blockA.SetBlock(blockB.node);
        blockB.SetBlock(tempNode);
    }






    private void UpdateFreeNodes()
    {
        freeNodes.Clear();
        foreach (var node in _nodes.Values)
        {
            if (node.OccupiedBlock == null)
            {
                freeNodes.Add(node);
            }
        }
    }


    public enum GameState
    {
        GenerateLevel,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        Deadlock,
        Win,
        Lose,
        Pause
    }
}