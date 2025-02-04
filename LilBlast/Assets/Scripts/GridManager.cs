using System.Collections.Generic;
using DG.Tweening;
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
    public List<Node> freeNodes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _nodes = new Dictionary<Vector2Int, Node>();
        freeNodes = new List<Node>();

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

        Camera.main.transform.position = new Vector3(center.x, center.y + 1f, -10);
        Debug.Log("Grid oluşturuldu: boş hücre sayısı : " + freeNodes.Count);
        GameManager.Instance.ChangeState(GameState.SpawningBlocks);
    }

    public void UpdateGrid()
    {
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
                    blockToMove.transform.DOMove(emptyNode.Pos, 0.3f).SetEase(Ease.OutBounce);

                    emptyY++; // Bir sonraki boş hücreye geç
                }
            }
        }

        Debug.Log("Bloklar aşağı düştü | Boş hücre sayısı : " + freeNodes.Count);
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
               //Debug.Log("HATA: Node " + node.gridPosition + " boş olarak kaydedildi!");
            }
            else
            {
                //Debug.LogError("Node " + node.gridPosition + " dolu, Block: " + node.OccupiedBlock.name);
            }
        }
    }

}