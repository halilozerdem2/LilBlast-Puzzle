using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BombBlock : Block
{
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
        int radius = 3; // 3x3 için 2, 4x4 için 3 yapabilirsiniz.
        Vector2Int center = node.gridPosition;
        Vector2Int target = block.node.gridPosition;

        return Mathf.Abs(target.x - center.x) < radius && Mathf.Abs(target.y - center.y) < radius;
    }

}
