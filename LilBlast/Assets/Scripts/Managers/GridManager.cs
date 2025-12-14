using System;
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

                if (currentNode.OccupiedBlock == null)
                {
                    if (emptyY == -1) emptyY = y; // İlk boş hücreyi bul
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
                        freeNodes.Add(originNode);
                    }

                    // Eski boş hücre listesine ekle
                    // (origin node already added above)

                    // Blok yeni yerine taşındı, bu hücre artık boş değil
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
        Debug.Log("Grid tamamen sıfırlandı.");
    }

    private void HandleBlockSettled()
    {
        pendingFallAnimations = Mathf.Max(0, pendingFallAnimations - 1);
        if (pendingFallAnimations == 0)
            GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }



}
