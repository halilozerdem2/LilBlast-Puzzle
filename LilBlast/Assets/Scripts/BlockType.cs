using System.Collections.Generic;
using UnityEngine;

public class BlockType : MonoBehaviour
{
    public static BlockType Instance;
    //Blok tiplerine göre oluşturulan, konumları barındıran anlık liste
    public Dictionary<int, HashSet<Vector2Int>> blockPositions;
    private void Awake()
    {
        Instance = this;
        blockPositions = new Dictionary<int, HashSet<Vector2Int>>();
    }
    public void AddBlock(int type, Vector2Int position)
    {
        if (!blockPositions.ContainsKey(type))
        {
            blockPositions[type] = new HashSet<Vector2Int>();
        }
        blockPositions[type].Add(position);
    }

    public void RemoveBlock(int type, Vector2Int position)
    {
        if (blockPositions.ContainsKey(type))
        {
            blockPositions[type].Remove(position);

            if (blockPositions[type].Count == 0)
            {
                blockPositions.Remove(type);
            }
        }
    }

    public int SelectColumn(int blockType)
    {
        int columndIndex = (blockType + 3) % GridManager.Instance._width;
        //blok tipine göre sırasız sütun seçme
        return columndIndex;
    }


}
