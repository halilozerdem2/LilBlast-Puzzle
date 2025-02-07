using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;
using static GameManager;

public class Block : MonoBehaviour
{
    public Node node;
    public bool isBlastable = false;
    public int neighboursCount = 0;
    public int blockType;
    public List<Block> neighbours = new List<Block>();

    private BoxCollider2D collider;


    Vector2 originalPosition;
    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
         // Event tetikleniyor
    }
    public void SetBlock(Node aNode)
    {
        if (node != null) node.OccupiedBlock = null;
        node = aNode;
        node.OccupiedBlock = this;
        transform.SetParent(node.transform);
       
    }
    
    public void FindNeighbours(List<Node> nodes)
    {
        neighbours.Clear();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Node neighbourNode = nodes.FirstOrDefault(n => n.gridPosition == node.gridPosition + dir);
            if (neighbourNode != null && neighbourNode.OccupiedBlock != null)
            {
                Block neighbourBlock = neighbourNode.OccupiedBlock;
                if (neighbourBlock.blockType == this.blockType) // Aynı tipte mi kontrol et
                {
                    neighbours.Add(neighbourBlock);
                    isBlastable = true;
                }
            }
        }
        neighboursCount = neighbours.Count;
        isBlastable = neighboursCount > 0;
    }

    public HashSet<Block> FloodFill()
    {
        HashSet<Block> visited = new HashSet<Block>();
        Stack<Block> stack = new Stack<Block>();

        stack.Push(this);
        visited.Add(this);

        while (stack.Count > 0)
        {
            Block current = stack.Pop();

            foreach (Block neighbour in current.neighbours)
            {
                if (!visited.Contains(neighbour))
                {
                    visited.Add(neighbour);
                    stack.Push(neighbour);
                }
            }
        }
        return visited;
    }


    public void Shake(float aShakeDuration, float aShakeMagnitude)
    {
        originalPosition = this.transform.position;
        //originalPosition = node.Pos;
        StartCoroutine(ShakeCoroutine(aShakeDuration,aShakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float aShakeDuration,float aShakeMagnitude)
    {
        float elapsedTime = 0f;

        while (elapsedTime < aShakeDuration)
        {
            float xShake = Random.Range(-aShakeMagnitude, aShakeMagnitude);
            float yShake = Random.Range(-aShakeMagnitude, aShakeMagnitude);

            transform.position = new Vector3(originalPosition.x + xShake, originalPosition.y + yShake);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }

    private void OnMouseDown()
    {
        if (Instance._state == GameState.WaitingInput)
        {
            BlockManager.Instance.TryBlastBlock(this);

        }
        else
            Debug.Log("Failed");
    }

    public void SetBlocksInteractable(bool interactable)
    {
        collider.enabled = interactable;
    }

}
