using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static GameManager;

public abstract class Block : MonoBehaviour
{
    public Node node;
    public bool isBlastable = false;
    public List<Block> group = new List<Block>();
    public int blockType;
    public int poolIndex = -1;
    private BoxCollider2D boxCollider2D;
    private Tween shakeTween;
    private Transform[] visualTransforms;
    private Vector3[] defaultLocalScales;
    private Quaternion[] defaultLocalRotations;
    public bool isBeingDestroyed;
    public abstract int scoreEffect { get; set; }

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        visualTransforms = GetComponentsInChildren<Transform>(true);
        int count = visualTransforms.Length;
        defaultLocalScales = new Vector3[count];
        defaultLocalRotations = new Quaternion[count];
        for (int i = 0; i < count; i++)
        {
            defaultLocalScales[i] = visualTransforms[i].localScale;
            defaultLocalRotations[i] = visualTransforms[i].localRotation;
        }
    }

    public void SetBlock(Node aNode)
    {
        if (node != null)
        {
            var previous = node;
            previous.OccupiedBlock = null;
        }
        node = aNode;
        node.OccupiedBlock = this;
        transform.SetParent(node.transform);
        ResetVisualState();
    }

    public abstract HashSet<Block> DetermineGroup();

 private void OnMouseDown()
{
    if (GameManager.Instance._state != GameState.WaitingInput) return;

    if (BlockManager.Instance.isModifyActive)
        BlockManager.Instance.TryModifyBlock(this);
    else
        BlockManager.Instance.TryBlastBlock(this);
}
    public List<Block> FindNeighbours()
    {
        List<Block> neighbours = new List<Block>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            var lookupPosition = node.gridPosition + dir;
            if (GridManager.Instance._nodes.TryGetValue(lookupPosition, out var neighbourNode))
            {
                var neighbourBlock = neighbourNode.OccupiedBlock;
                if (neighbourBlock != null && neighbourBlock.node == neighbourNode && neighbourBlock.gameObject.activeInHierarchy)
                    neighbours.Add(neighbourBlock);
            }
        }
        return neighbours;
    }


    public void Shake(float duration, float magnitude)
    {
        var startLocalPosition = transform.localPosition;
        shakeTween?.Kill();
        shakeTween = transform.DOShakePosition(duration, new Vector3(magnitude, magnitude, 0f))
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                shakeTween = null;
                transform.localPosition = startLocalPosition;
            });
    }

    public void ForceShake(float duration, float magnitude)
    {
        shakeTween?.Kill();
        shakeTween = null;
        Shake(duration, magnitude);
    }

    public void SetBlocksInteractable(bool interactable)
    {
        boxCollider2D.enabled = interactable;
    }

    public void ResetVisualState()
    {
        if (visualTransforms == null)
            return;

        for (int i = 0; i < visualTransforms.Length; i++)
        {
            var t = visualTransforms[i];
            if (t == null)
                continue;

            t.DOKill();
            t.localRotation = defaultLocalRotations[i];
            t.localScale = defaultLocalScales[i];
        }

    }
}
