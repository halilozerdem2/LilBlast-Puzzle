using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;
using static GameManager;

public abstract class Block : MonoBehaviour
{
    public Node node;
    public bool isBlastable = false;
    public List<Block> group = new List<Block>();
    public int blockType;
    private new BoxCollider2D collider;
    private bool isShaking;
 
    private Vector2 originalPosition;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    public void SetBlock(Node aNode)
    {
        if (node != null) node.OccupiedBlock = null;
        node = aNode;
        node.OccupiedBlock = this;
        transform.SetParent(node.transform);
    }

    public abstract HashSet<Block> DetermineGroup();

    private void OnMouseDown()
    {
        if (Instance._state == GameState.WaitingInput)
        {
            BlockManager.Instance.TryBlastBlock(this);
        }
        else
        {
            Debug.Log("Failed");
        }
    }
    public List<Block> FindNeighbours()
    {
        List<Block> neighbours = new List<Block>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Node neighbourNode = GridManager.Instance._nodes.Values.FirstOrDefault(n => n.gridPosition == node.gridPosition + dir);
            if (neighbourNode != null && neighbourNode.OccupiedBlock != null)
            {
                neighbours.Add(neighbourNode.OccupiedBlock);
            }
        }
        return neighbours;
    }


    public void Shake(float aShakeDuration, float aShakeMagnitude)
    {
        if (isShaking) return;
        isShaking = true;
        originalPosition = transform.position;
        StartCoroutine(ShakeCoroutine(aShakeDuration, aShakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float aShakeDuration, float aShakeMagnitude)
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
        isShaking = false;
    }

    public void SetBlocksInteractable(bool interactable)
    {
        collider.enabled = interactable;
    }
}