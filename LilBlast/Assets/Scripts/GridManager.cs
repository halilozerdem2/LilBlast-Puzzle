using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using static GameManager;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] public int _width = 5;
    [SerializeField] public int _height = 7;

    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private Node _nodePrefab;

    public Dictionary<Vector2Int, Node> _nodes;
    public HashSet<Node> freeNodes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _nodes = new Dictionary<Vector2Int, Node>();
        freeNodes = new HashSet<Node>();

    }

    public void GenerateGrid()
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
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }

    public void UpdateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            int emptyY = -1; // Boş bir hücre bulunana kadar -1 olarak kalacak

            for (int y = 0; y < _height; y++)
            {
                Node currentNode = _nodes[new Vector2Int(x, y)];

                if (currentNode.OccupiedBlock == null)
                {
                    if (emptyY == -1) emptyY = y; // İlk boş hücreyi kaydet
                    continue;
                }

                if (emptyY != -1) // Eğer yukarıda boş bir hücre varsa
                {
                    Node emptyNode = _nodes[new Vector2Int(x, emptyY)];
                    Block blockToMove = currentNode.OccupiedBlock;

                    blockToMove.SetBlock(emptyNode);
                    blockToMove.transform.DOMove(emptyNode.Pos, 0.3f).SetEase(Ease.OutBounce);

                    currentNode.OccupiedBlock = null;
                    emptyNode.OccupiedBlock = blockToMove;

                    emptyY++; // Bir sonraki boş yere geç
                }
            }
        }

        UpdateFreeNodes();
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }


    public void UpdateFreeNodes()
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
}

