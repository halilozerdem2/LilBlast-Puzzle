using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NodeBlocker : MonoBehaviour
{
    [SerializeField] private int strength = 3;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fullBlockSprite;
    [SerializeField] private Sprite halfBlockSprite;
    [SerializeField] private Sprite quarterBlockSprite;

    public Node Node { get; private set; }
    public int Strength => strength;

    private Sprite defaultSprite;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            defaultSprite = spriteRenderer.sprite;

        CacheNode();
    }

    private void OnEnable()
    {
        CacheNode();
        Register();
        UpdateVisual();
        BlockManager.OnBlockBlasted += HandleBlockBlasted;
    }

    private void OnDisable()
    {
        BlockManager.OnBlockBlasted -= HandleBlockBlasted;
        Unregister();
    }

    private void CacheNode()
    {
        if (Node != null)
            return;

        Node = GetComponentInParent<Node>();
        if (Node == null)
            Debug.LogWarning($"{name}: NodeBlocker requires a parent Node to function.");
    }

    private void Register()
    {
        if (Node == null)
            return;

        Node.AttachBlocker(this);
    }

    private void Unregister()
    {
        if (Node == null)
            return;

        Node.DetachBlocker(this);
    }

    private void HandleBlockBlasted(Block blastedBlock)
    {
        if (!isActiveAndEnabled || Node == null || blastedBlock == null || blastedBlock.node == null)
            return;

        Vector2Int delta = blastedBlock.node.gridPosition - Node.gridPosition;
        int absX = Mathf.Abs(delta.x);
        int absY = Mathf.Abs(delta.y);
        if ((absX == 1 && absY == 0) || (absY == 1 && absX == 0))
            ReduceStrength(1);
    }

    public void ReduceStrength(int amount = 1)
    {
        if (amount <= 0 || strength <= 0)
            return;

        strength = Mathf.Max(0, strength - amount);
        UpdateVisual();

        if (strength <= 0)
            ReleaseNode();
    }

    private void ReleaseNode()
    {
        Unregister();
        Destroy(gameObject);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        if (strength <= 0)
        {
            spriteRenderer.enabled = false;
            return;
        }

        Sprite target = null;
        int strengthState = Mathf.Clamp(strength, 1, 3);
        switch (strengthState)
        {
            case 3:
                target = fullBlockSprite != null ? fullBlockSprite : defaultSprite;
                break;
            case 2:
                if (halfBlockSprite != null)
                    target = halfBlockSprite;
                else if (fullBlockSprite != null)
                    target = fullBlockSprite;
                else
                    target = defaultSprite;
                break;
            case 1:
                if (quarterBlockSprite != null)
                    target = quarterBlockSprite;
                else if (halfBlockSprite != null)
                    target = halfBlockSprite;
                else
                    target = defaultSprite;
                break;
        }

        spriteRenderer.enabled = true;
        if (target != null)
            spriteRenderer.sprite = target;
    }

    public static void HandleSpecialBlastArea(Block specialBlock)
    {
        if (specialBlock == null)
            return;

        foreach (var node in EnumerateAffectedNodes(specialBlock))
        {
            if (node != null && node.Blocker != null)
                node.Blocker.ReduceStrength();
        }
    }

    private static IEnumerable<Node> EnumerateAffectedNodes(Block specialBlock)
    {
        var grid = GridManager.Instance;
        if (grid == null || grid._nodes == null || specialBlock.node == null)
            yield break;

        if (specialBlock is HorizontalRocketBlock)
        {
            int row = specialBlock.node.gridPosition.y;
            foreach (var node in grid._nodes.Values)
            {
                if (node != null && node.gridPosition.y == row)
                    yield return node;
            }
        }
        else if (specialBlock is VerticalRocketBlock)
        {
            int column = specialBlock.node.gridPosition.x;
            foreach (var node in grid._nodes.Values)
            {
                if (node != null && node.gridPosition.x == column)
                    yield return node;
            }
        }
        else if (specialBlock is BombBlock bomb)
        {
            Vector2Int center = specialBlock.node.gridPosition;
            int radius = bomb.BlastRadius;
            foreach (var node in grid._nodes.Values)
            {
                if (node == null)
                    continue;

                Vector2Int position = node.gridPosition;
                if (Mathf.Abs(position.x - center.x) < radius && Mathf.Abs(position.y - center.y) < radius)
                    yield return node;
            }
        }
        else
        {
            yield return specialBlock.node;
        }
    }
}
