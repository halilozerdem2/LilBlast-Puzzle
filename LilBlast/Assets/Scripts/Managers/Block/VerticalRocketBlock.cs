using System.Collections.Generic;
using UnityEngine;

public class VerticalRocketBlock : Block
{
    public override int scoreEffect { get; set; } = 25;

    public override HashSet<Block> DetermineGroup()
    {
        HashSet<Block> group = new HashSet<Block>();
        int column = this.node.gridPosition.x;

        foreach (var gridNode in GridManager.Instance._nodes.Values)
        {
            if (gridNode == null)
                continue;

            if (gridNode.gridPosition.x != column)
                continue;

            var block = gridNode.OccupiedBlock;
            if (block == null || block.node != gridNode || !block.gameObject.activeInHierarchy)
                continue;

            group.Add(block);
        }

        return group;
    }

}
