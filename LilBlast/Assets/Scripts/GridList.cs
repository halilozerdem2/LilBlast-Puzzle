using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridList : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();
    public int Width;
    public int Height; 

    private void Awake()
    {
        nodes.Clear();

        foreach (Transform child in transform)
        {
            Node node = child.GetComponent<Node>();
            if (node != null)
            {
                nodes.Add(node);
            }
        }
        CalculateGridSize();
    }
    
    private void CalculateGridSize()
    {
        if (nodes.Count == 0)
        {
            Width = 0;
            Height = 0;
            return;
        }

        int maxX = nodes.Max(n => n.gridPosition.x);
        int maxY = nodes.Max(n => n.gridPosition.y);

        Width = maxX + 1;
        Height = maxY + 1;
    }
    
  
}