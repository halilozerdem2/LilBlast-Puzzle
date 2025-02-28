using System.Collections.Generic;


public class RegularBlock : Block
{
    public override int scoreEffect { get; set; } = 10;
    
    public override HashSet<Block> DetermineGroup()
    {
        HashSet<Block> visited = new HashSet<Block>();
        Stack<Block> stack = new Stack<Block>();

        stack.Push(this);
        visited.Add(this);

        while (stack.Count > 0)
        {
            Block current = stack.Pop();
            var neighbours = current.FindNeighbours();

            foreach (Block neighbour in neighbours)
            {
                if (!visited.Contains(neighbour)&& neighbour.blockType==this.blockType)
                {
                    visited.Add(neighbour);
                    stack.Push(neighbour);
                }
            }
        }
        return visited;
    }

}
