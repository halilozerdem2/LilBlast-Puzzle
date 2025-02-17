using UnityEngine;

public class Node : MonoBehaviour
{
    public Block OccupiedBlock;
    public Vector2 Pos => transform.position;
    public Vector2Int gridPosition;

    public Node GetNodeAt(Vector2Int aGridPosition)
    {
        if (gridPosition == aGridPosition)
            return this;
        else return null;
    }
}
