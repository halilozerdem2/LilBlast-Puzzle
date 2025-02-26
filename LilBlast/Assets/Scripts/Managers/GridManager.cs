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
    public static List<Node> freeNodes;

    private void Awake()
    {
        Instance = this;

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
    board.transform.localScale = new Vector3(_width+0.25f, _height+0.25f, 1);
        CenterCamera(center);
    
}
    private void CenterCamera(Vector2 center)
    {
        Camera.main.transform.position = new Vector3(center.x, center.y + 2f, -10); // Kamera ortalama

        // Kamera boyutunu grid büyüklüğüne göre ayarla
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = _height / 2f + .5f; // +1 biraz kenar boşluğu eklemek için
        float horizontalSize = (_width / 2f + .5f) / aspectRatio;
        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

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


    public void UpdateFreeNodes()
    {
        freeNodes.Clear();
        foreach (var node in _nodes.Values)
        {
            if (node.OccupiedBlock == null)
            {
                freeNodes.Add(node);
                node.OccupiedBlock.SetBlock(node);
            }

        }
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