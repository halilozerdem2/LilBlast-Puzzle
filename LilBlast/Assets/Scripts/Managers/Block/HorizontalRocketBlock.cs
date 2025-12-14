using System.Collections.Generic;
using UnityEngine;

public class HorizontalRocketBlock : Block
{
    public override int scoreEffect { get; set; } = 25;

    public override HashSet<Block> DetermineGroup()
    {
        HashSet<Block> group = new HashSet<Block>();

        // Mevcut bloğun olduğu satırın tamamını seç
        int row = this.node.gridPosition.y;

        foreach (var gridNode in GridManager.Instance._nodes.Values)
        {
            if (gridNode == null)
                continue;

            if (gridNode.gridPosition.y != row)
                continue;

            var block = gridNode.OccupiedBlock;
            if (block == null || block.node != gridNode || !block.gameObject.activeInHierarchy)
                continue;

            group.Add(block);
        }

        return group;
    }


}
