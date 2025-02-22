using System.Collections.Generic;
using UnityEngine;

public class VerticalRocketBlock : Block
{
    public override HashSet<Block> DetermineGroup()
    {
        HashSet<Block> group = new HashSet<Block>();
        int column = this.node.gridPosition.x;

        foreach (var gridNode in GridManager.Instance._nodes.Values)
        {
            if (gridNode.gridPosition.x == column && gridNode.OccupiedBlock != null)
            {
                group.Add(gridNode.OccupiedBlock);
            }
        }

        return group;
    }

}
