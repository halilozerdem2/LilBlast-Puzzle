using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;

public class ShuffleManager : MonoBehaviour
{
    public List<Node> availableNodes;
    public List<Block> blocks;

    private readonly Dictionary<int, List<Node>> _columnBuckets = new Dictionary<int, List<Node>>();
    private readonly Dictionary<int, List<Node>> _rowBuckets = new Dictionary<int, List<Node>>();
    private readonly Dictionary<Node, int> _nodeIndices = new Dictionary<Node, int>();

    public void HandleShuffle(bool isOrdered=false)
    {
        Instance.ChangeState(GameState.Shuffling);
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            
            GridManager.freeNodes.Add(node);
            node.OccupiedBlock = null;
        }

        if (availableNodes == null)
            availableNodes = new List<Node>();
        else
            availableNodes.Clear();

        availableNodes.AddRange(GridManager.freeNodes); // Shuffle için boş düğümleri listeye al

        if (blocks == null)
            blocks = new List<Block>();
        else
            blocks.Clear();

        blocks.AddRange(BlockManager.Instance.blocks);

        if (availableNodes.Count < blocks.Count)
        {
            Debug.LogError("Shuffle için yeterli boş node bulunamadı!");
            return;
        }
        //Debug.Log(availableNodes.Count);

        ShuffleAvailableNodes();
        RefreshNodeIndices();
        BuildBuckets();
        
        foreach (var block in blocks)
        {
            block.Shake(0.2f, 0.1f);
            Node targetNode = AssignNewPosition(block.blockType, isOrdered);

            if (targetNode == null)
            {
                Debug.LogWarning("Geçerli bir shuffle pozisyonu bulunamadı, fallback olarak rastgele atama yapılacak.");
                targetNode = PickRandomAvailableNode();
            }

            Debug.Log("karışıyor");
            block.SetBlock(targetNode);

            ConsumeNode(targetNode);
            block.transform.DOMove(targetNode.Pos, 1.2f).SetEase(Ease.InOutQuad);
        }

        GridManager.Instance.UpdateOccupiedBlock();
        StartCoroutine(HandleStateDelayed());

    }

    IEnumerator HandleStateDelayed()
    {
        yield return new WaitForSeconds(1.21f);
        Instance.ChangeState(GameState.WaitingInput);
    }

    private void ShuffleAvailableNodes()
    {
        for (int i = availableNodes.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (availableNodes[i], availableNodes[j]) = (availableNodes[j], availableNodes[i]);
        }
    }

    private void RefreshNodeIndices()
    {
        _nodeIndices.Clear();
        for (int i = 0; i < availableNodes.Count; i++)
        {
            _nodeIndices[availableNodes[i]] = i;
        }
    }

    private void BuildBuckets()
    {
        foreach (var list in _columnBuckets.Values)
        {
            list.Clear();
        }

        foreach (var list in _rowBuckets.Values)
        {
            list.Clear();
        }

        foreach (var node in availableNodes)
        {
            if (!_columnBuckets.TryGetValue(node.gridPosition.x, out List<Node> columnList))
            {
                columnList = new List<Node>();
                _columnBuckets[node.gridPosition.x] = columnList;
            }
            columnList.Add(node);

            if (!_rowBuckets.TryGetValue(node.gridPosition.y, out List<Node> rowList))
            {
                rowList = new List<Node>();
                _rowBuckets[node.gridPosition.y] = rowList;
            }
            rowList.Add(node);
        }
    }

    private Node AssignNewPosition(int type, bool isOrdered = false)
    {
        float percentage = Random.Range(0f, 1f);

        if (isOrdered)
            percentage = Random.Range(0f, 0.1f);

        if (availableNodes.Count == 0)
            return null;

        if (percentage > 0.1f) // %90 tamamen rastgele
        {
            return PickRandomAvailableNode();
        }

        Node newNode;
        if (percentage >= 0.05f && percentage <= 0.1f) // sütuna atama
        {
            newNode = GetNodeFromColumn(type);
        }
        else // Satıra atama
        {
            newNode = GetNodeFromRow(type);
        }

        return newNode ?? PickRandomAvailableNode();
    }

    private Node GetNodeFromColumn(int type)
    {
        int selectedColumn = BlockType.Instance.SelectColumn(type);
        if (_columnBuckets.TryGetValue(selectedColumn, out List<Node> columnNodes) && columnNodes.Count > 0)
        {
            return columnNodes[Random.Range(0, columnNodes.Count)];
        }

        return null;
    }

    private Node GetNodeFromRow(int type)
    {
        int selectedRow = BlockType.Instance.SelectRow(type);
        if (_rowBuckets.TryGetValue(selectedRow, out List<Node> rowNodes) && rowNodes.Count > 0)
        {
            return rowNodes[Random.Range(0, rowNodes.Count)];
        }

        return null;
    }

    private Node PickRandomAvailableNode()
    {
        if (availableNodes == null || availableNodes.Count == 0)
            return null;

        return availableNodes[availableNodes.Count - 1];
    }

    private void ConsumeNode(Node node)
    {
        if (node == null)
            return;

        GridManager.freeNodes.Remove(node); // Seçilen node artık dolu olduğu için listeden çıkar

        if (_nodeIndices.TryGetValue(node, out int index))
        {
            int lastIndex = availableNodes.Count - 1;
            Node lastNode = availableNodes[lastIndex];
            availableNodes[index] = lastNode;
            availableNodes.RemoveAt(lastIndex);
            _nodeIndices[lastNode] = index;
            _nodeIndices.Remove(node);
        }
        else
        {
            availableNodes.Remove(node);
        }

        if (_columnBuckets.TryGetValue(node.gridPosition.x, out List<Node> columnNodes))
        {
            columnNodes.Remove(node);
        }

        if (_rowBuckets.TryGetValue(node.gridPosition.y, out List<Node> rowNodes))
        {
            rowNodes.Remove(node);
        }
    }

}
