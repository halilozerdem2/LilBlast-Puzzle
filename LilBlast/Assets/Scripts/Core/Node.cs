using UnityEngine;

public class Node : MonoBehaviour
{
    public Block OccupiedBlock;
    public Vector2 Pos => transform.position;
    public Vector2Int gridPosition;
    [SerializeField] private NodeBlocker blocker;

    public NodeBlocker Blocker => blocker;
    public bool HasBlocker => blocker != null;
    public bool IsAvailable => OccupiedBlock == null && !HasBlocker;

    public Node GetNodeAt(Vector2Int aGridPosition)
    {
        if (gridPosition == aGridPosition)
            return this;
        else return null;
    }

    internal void AttachBlocker(NodeBlocker newBlocker)
    {
        if (newBlocker == null)
            return;

        blocker = newBlocker;
        RemoveFromFreeList();
    }

    internal void DetachBlocker(NodeBlocker currentBlocker)
    {
        if (currentBlocker == null || blocker != currentBlocker)
            return;

        blocker = null;
        TryAddToFreeList();
    }

    private void RemoveFromFreeList()
    {
        if (GridManager.freeNodes == null)
            return;

        while (GridManager.freeNodes.Remove(this))
        {
        }
    }

    private void TryAddToFreeList()
    {
        if (GridManager.freeNodes == null || OccupiedBlock != null)
            return;

        if (!GridManager.freeNodes.Contains(this))
            GridManager.freeNodes.Add(this);
    }
}
