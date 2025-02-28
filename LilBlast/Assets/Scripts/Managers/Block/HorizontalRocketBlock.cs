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
            if (gridNode.gridPosition.y == row && gridNode.OccupiedBlock != null)
            {
                group.Add(gridNode.OccupiedBlock);
            }
        }

        return group;
    }


}
