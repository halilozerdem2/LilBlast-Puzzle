using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static GameManager;

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
    public CameraFitter cameraFitter;

    private void Awake()
    {
        Instance = this;

        _nodes = new Dictionary<Vector2Int, Node>();
        freeNodes = new List<Node>();

        // Sadece sahne 1 yüklendiğinde grid oluştur
        LevelManager.OnLevelOneLoaded += InitializeGrid;
        
    }

    private void OnDestroy()
    {
        LevelManager.OnLevelOneLoaded -= InitializeGrid;
    }

    private void InitializeGrid()
    {
        _gridParent = FindAnyObjectByType<GridList>();
        Debug.Log("GridManager: Grid sahne 1 için oluşturuluyor...");
        GetGridListFromScene();
        _width = _gridParent.Width;
        _height = _gridParent.Height;
    }

    public void GetGridListFromScene()
    {
        foreach (var node in _gridParent.nodes)
        {
            freeNodes.Add(node);
            _nodes.Add(node.gridPosition, node);
            Debug.Log("Eklendi");
            
        }
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }

    public void UpdateGrid()
    {
        //if (GameManager.Instance._state != GameManager.GameState.Falling) return;
        freeNodes.Clear(); // Önce freeNodes listesini temizle, en güncel haliyle ekleyelim

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
                    //Debug.Log("11111");
                    continue;
                }

                if (emptyY != -1) // Eğer yukarıda boş hücre varsa
                {
                    Node emptyNode = _nodes[new Vector2Int(x, emptyY)];
                    Block blockToMove = currentNode.OccupiedBlock;

                    // Eski konumu boşalt
                    blockToMove.node.OccupiedBlock = null;

                    // Eski boş hücre listesine ekle
                    freeNodes.Add(blockToMove.node);

                    // Blok yeni yerine taşındı, bu hücre artık boş değil
                    freeNodes.Remove(emptyNode);

                    blockToMove.SetBlock(emptyNode);
                    blockToMove.transform.DOMove(emptyNode.Pos, 0.4f).SetEase(Ease.OutBounce)
                        .OnComplete(() => {
                            GameManager.Instance.ChangeState(GameState.SpawningBlocks);
                        }); ;

                    emptyY++; // Bir sonraki boş hücreye geç
                }
            }
        }
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);

    }

    
    public void UpdateOccupiedBlock()
    {
        foreach (var node in _nodes.Values)
        {
            if (node.OccupiedBlock == null) // Sadece boş düğümler kontrol edilecek
            {
                foreach (var block in BlockManager.Instance.blocks)
                {
                    if (block.node == node) // Eğer blok bu düğüme aitse
                    {
                        node.OccupiedBlock = block;
                        break; // Gereksiz tekrarları önlemek için döngüyü kır
                    }
                }
            }
        }
    }

    public void ResetGrid()
    {
        _nodes.Clear();
        freeNodes.Clear();
        Debug.Log("Grid tamamen sıfırlandı.");
    }



}