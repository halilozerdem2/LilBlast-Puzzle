using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBlock : Block
{
    [SerializeField] private int blastRadius = 3;
    public override int scoreEffect { get; set; } = 50;
    public int BlastRadius => Mathf.Max(1, blastRadius);

    public override HashSet<Block> DetermineGroup()
    {
        HashSet<Block> group = new HashSet<Block>();
        Queue<Block> queue = new Queue<Block>();
        queue.Enqueue(this);
        group.Add(this);

        while (queue.Count > 0)
        {
            Block current = queue.Dequeue();

            foreach (Block neighbor in current.FindNeighbours())
            {
                if (!group.Contains(neighbor))
                {
                    // Yalnızca belirli bir menzil içindeyse ekle
                    if (IsWithinBlastRadius(neighbor))
                    {
                        group.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return group;
    }

    // 3x3 veya 4x4 alan içinde olup olmadığını kontrol eden fonksiyon
    private bool IsWithinBlastRadius(Block block)
    {
        int radius = BlastRadius;
        Vector2Int center = node.gridPosition;
        Vector2Int target = block.node.gridPosition;

        return Mathf.Abs(target.x - center.x) < radius && Mathf.Abs(target.y - center.y) < radius;
    }

}
