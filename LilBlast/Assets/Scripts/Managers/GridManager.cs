using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static GameManager;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] public int _width = 5;
    [SerializeField] public int _height = 7;

    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private GridList _gridParent; 
    [SerializeField] private NodeBlocker bottomRowBlockerPrefab;
    [SerializeField] [Min(0)] private int initialBlockedRowCount = 1;

    public Dictionary<Vector2Int, Node> _nodes;
    public static List<Node> freeNodes;
    public GameObject gridPrefab;
    public CameraFitter cameraFitter;
    private int pendingFallAnimations;

    private void Awake()
    {
        Instance = this;

        _nodes = new Dictionary<Vector2Int, Node>();
        freeNodes = new List<Node>();

    }
    
    public void InitializeGrid()
    {
        ResetGrid();
        var difficultyManager = DifficultyManager.Instance;
        bool generatedFromDifficulty = false;
        if (difficultyManager != null)
        {
            var config = difficultyManager.CurrentConfig;
            var boardSize = config.boardSize;
            _width = Mathf.Max(5, boardSize.x);
            _height = Mathf.Max(5, boardSize.y);
            GenerateGrid();
            ApplyNodeBlockersFromConfig(config);
            generatedFromDifficulty = true;
        }

        if (!generatedFromDifficulty)
        {
            _gridParent = null;
            _gridParent = FindAnyObjectByType<GridList>();
            if (_gridParent != null)
            {
                _width = _gridParent.Width;
                _height = _gridParent.Height;
                GetGridListFromScene(_gridParent);
            }
            else
            {
                GenerateGrid();
            }

            EnsureBottomRowBlockers(initialBlockedRowCount);
        }
        BlockManager.Instance.InitializeBlockManager();
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);
        
    }

    public void GetGridListFromScene(GridList aGridList)
    {
        foreach (var node in aGridList.nodes)
        {
            freeNodes.Add(node);
            _nodes.Add(node.gridPosition, node);
        }
    }
    public List<Node> GenerateGrid()
    {
       GameObject gridGameObject=Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
       
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector3(x, y), Quaternion.identity,gridGameObject.transform);
                node.gridPosition = new Vector2Int(x, y);
                _nodes[node.gridPosition] = node;

                freeNodes.Add(node);
            }
        }

        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.transform.SetParent(gridGameObject.transform);
        board.transform.localScale = new Vector3(_width+0.25f, _height+0.25f, 1);
        cameraFitter.FitCameraToGrid(_width, _height, gridGameObject.transform);
        return freeNodes;
    }

    public void UpdateGrid()
    {
        if (GameManager.Instance._state != GameState.Falling) return;
        if (BlockManager.Instance != null && BlockManager.Instance.SuppressRefills)
            return;
        UpdateOccupiedBlock();
        pendingFallAnimations = 0;
        bool blocksMoved = false;
        freeNodes.Clear();
        for (int x = 0; x < _width; x++)
        {
            int emptyY = -1; // İlk boş hücreyi saklayacak değişken

            for (int y = 0; y < _height; y++)
            {
                Node currentNode = _nodes[new Vector2Int(x, y)];

                if (currentNode.HasBlocker)
                {
                    emptyY = -1;
                    continue;
                }

                if (currentNode.OccupiedBlock == null)
                {
                    if (emptyY == -1) emptyY = y; // İlk boş hücreyi bul
                    if (!currentNode.HasBlocker)
                        freeNodes.Add(currentNode); // Boş hücreyi freeNodes listesine ekle
                    continue;
                }

                if (emptyY != -1) // Eğer yukarıda boş hücre varsa
                {
                    Node emptyNode = _nodes[new Vector2Int(x, emptyY)];
                    Block blockToMove = currentNode.OccupiedBlock;

                    // Eski konumu boşalt
                    var originNode = blockToMove.node;
                    if (originNode != null)
                    {
                        originNode.OccupiedBlock = null;
                        if (!originNode.HasBlocker)
                            freeNodes.Add(originNode);
                    }

                    // Eski boş hücre listesine ekle
                    // (origin node already added above)

                    // Blok yeni yerine taşındı, bu hücre artık boş değil
                    if (!emptyNode.HasBlocker)
                        freeNodes.Remove(emptyNode);

                    blockToMove.SetBlock(emptyNode);
                    pendingFallAnimations++;
                    blocksMoved = true;
                    blockToMove.transform.DOMove(emptyNode.Pos, 0.35f).SetEase(Ease.OutBounce)
                        .OnComplete(HandleBlockSettled);

                    emptyY++; // Bir sonraki boş hücreye geç
                }
            }
        }
        if (!blocksMoved)
            GameManager.Instance.ChangeState(GameState.SpawningBlocks);

    }

    
    public void UpdateOccupiedBlock()
    {
        foreach (var node in _nodes.Values)
            node.OccupiedBlock = null;

        foreach (var block in BlockManager.Instance.blocks)
        {
            if (block == null)
                continue;

            var blockNode = block.node;
            if (blockNode == null)
                continue;

            if (!_nodes.TryGetValue(blockNode.gridPosition, out var gridNode) || gridNode != blockNode)
                continue;

            gridNode.OccupiedBlock = block;
        }
    }

    public void ResetGrid()
    {
        foreach (var node in _nodes.Values)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        _nodes.Clear();
        freeNodes.Clear();
        pendingFallAnimations = 0;
    }

    public IEnumerator ClearBoardSequentially(BlockManager blockManager, float delayBetweenClears = 0.02f, bool processSpecials = true)
    {
        if (blockManager == null)
            yield break;

        for (int y = _height - 1; y >= 0; y--)
        {
            for (int x = 0; x < _width; x++)
            {
                var key = new Vector2Int(x, y);
                if (!_nodes.TryGetValue(key, out var node) || node == null)
                    continue;

                var block = node.OccupiedBlock;
                if (block == null)
                    continue;

                yield return blockManager.ClearBlockFromNode(block, processSpecials);

                if (delayBetweenClears > 0f)
                    yield return new WaitForSeconds(delayBetweenClears);
            }
        }
    }

    private void HandleBlockSettled()
    {
        pendingFallAnimations = Mathf.Max(0, pendingFallAnimations - 1);
        if (pendingFallAnimations == 0)
            GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }


    private void ApplyNodeBlockersFromConfig(DifficultyConfig config)
    {
        if (bottomRowBlockerPrefab == null || _nodes == null || _nodes.Count == 0)
            return;

        if (config.iceCoverage <= 0f || config.iceClusterSize <= 0)
            return;

        int totalNodes = Mathf.Max(1, _width * _height);
        int targetBlockers = Mathf.Clamp(Mathf.RoundToInt(totalNodes * Mathf.Clamp01(config.iceCoverage)), 0, totalNodes);
        if (targetBlockers <= 0)
            return;

        int cappedRowStart = Mathf.Clamp(Mathf.CeilToInt(_height * Mathf.Clamp01(1f - config.iceStartRowRatio)), 1, _height);
        if (cappedRowStart <= 0)
            return;

        var candidateNodes = new List<Node>();
        foreach (var node in _nodes.Values)
        {
            if (node == null)
                continue;

            if (node.gridPosition.y < cappedRowStart)
                candidateNodes.Add(node);
        }

        if (candidateNodes.Count == 0)
            return;

        targetBlockers = Mathf.Clamp(targetBlockers, 0, candidateNodes.Count);
        if (targetBlockers <= 0)
            return;

        var blockedNodes = new HashSet<Node>();
        var rowCounts = new int[_height];
        var columnCounts = new int[_width];
        int clusterSize = Mathf.Max(1, config.iceClusterSize);
        int safety = candidateNodes.Count * 4;

        while (blockedNodes.Count < targetBlockers && safety-- > 0)
        {
            var seed = candidateNodes[UnityEngine.Random.Range(0, candidateNodes.Count)];
            if (!TrySpawnBlockerAtNode(seed, blockedNodes, rowCounts, columnCounts))
                continue;

            int remainingClusterSpots = Mathf.Min(clusterSize - 1, targetBlockers - blockedNodes.Count);
            if (remainingClusterSpots > 0)
                GrowBlockerCluster(seed, remainingClusterSpots, targetBlockers, cappedRowStart, blockedNodes, rowCounts, columnCounts);
        }
    }

    private void GrowBlockerCluster(Node seed, int remainingClusterSpots, int targetBlockers, int cappedRowStart, HashSet<Node> blockedNodes, int[] rowCounts, int[] columnCounts)
    {
        if (seed == null || remainingClusterSpots <= 0)
            return;

        var queue = new Queue<Node>();
        queue.Enqueue(seed);
        Vector2Int[] neighborOffsets =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0 && remainingClusterSpots > 0 && blockedNodes.Count < targetBlockers)
        {
            var current = queue.Dequeue();
            foreach (var offset in neighborOffsets)
            {
                if (remainingClusterSpots <= 0 || blockedNodes.Count >= targetBlockers)
                    break;

                var neighborPos = current.gridPosition + offset;
                if (neighborPos.x < 0 || neighborPos.x >= _width)
                    continue;
                if (neighborPos.y < 0 || neighborPos.y >= cappedRowStart)
                    continue;

                if (!_nodes.TryGetValue(neighborPos, out var neighbor) || neighbor == null)
                    continue;

                if (TrySpawnBlockerAtNode(neighbor, blockedNodes, rowCounts, columnCounts))
                {
                    remainingClusterSpots--;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private bool TrySpawnBlockerAtNode(Node node, HashSet<Node> blockedNodes, int[] rowCounts, int[] columnCounts)
    {
        if (node == null || node.HasBlocker || blockedNodes.Contains(node))
            return false;

        int row = node.gridPosition.y;
        int column = node.gridPosition.x;
        if (row < 0 || row >= _height || column < 0 || column >= _width)
            return false;

        if (rowCounts[row] >= _width - 1)
            return false;

        if (columnCounts[column] >= _height - 1)
            return false;

        var blocker = Instantiate(bottomRowBlockerPrefab, node.transform);
        blocker.transform.localPosition = Vector3.zero;
        blocker.transform.localRotation = Quaternion.identity;
        blockedNodes.Add(node);
        rowCounts[row]++;
        columnCounts[column]++;
        return true;
    }


    private void EnsureBottomRowBlockers(int requestedRowCount)
    {
        if (bottomRowBlockerPrefab == null || _nodes == null || _nodes.Count == 0)
            return;

        int maxRowsAllowed = Mathf.Min(_height, Mathf.Max(0, _width - 1));
        int rowsToBlock = Mathf.Clamp(requestedRowCount, 0, maxRowsAllowed);
        if (rowsToBlock <= 0)
            return;

        foreach (var node in _nodes.Values)
        {
            if (node == null)
                continue;

            int rowIndex = node.gridPosition.y;
            if (rowIndex >= rowsToBlock || node.HasBlocker)
                continue;

            var blocker = Instantiate(bottomRowBlockerPrefab, node.transform);
            blocker.transform.localPosition = Vector3.zero;
            blocker.transform.localRotation = Quaternion.identity;
        }
    }
}
