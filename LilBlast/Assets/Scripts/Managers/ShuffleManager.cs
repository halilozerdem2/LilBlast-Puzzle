using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;

public class ShuffleManager : MonoBehaviour
{
    public  List<Node> availableNodes;
    public List<Block> blocks;
   // public GridList gridList;

    public void HandleShuffle(bool isOrdered=false)
    {
        Instance.ChangeState(GameState.Shuffling);
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            
            GridManager.freeNodes.Add(node);
            node.OccupiedBlock = null;
        }

        availableNodes = new List<Node>(GridManager.freeNodes); // Shuffle için boş düğümleri listeye al
        blocks = new List<Block>(BlockManager.Instance.blocks);

        if (availableNodes.Count < blocks.Count)
        {
            Debug.LogError("Shuffle için yeterli boş node bulunamadı!");
            return;
        }
            //Debug.Log(availableNodes.Count);
        
        foreach (var block in blocks)
        {
            block.Shake(0.2f, 0.1f);
            Node targetNode = AssignNewPosition(block.blockType, availableNodes,isOrdered);

            if (targetNode == null)
            {
                Debug.LogWarning("Geçerli bir shuffle pozisyonu bulunamadı, fallback olarak rastgele atama yapılacak.");
                targetNode = availableNodes[Random.Range(0, availableNodes.Count)];
            }

            Debug.Log("karışıyor");
            block.SetBlock(targetNode);
            
            GridManager.freeNodes.Remove(targetNode);// Seçilen node artık dolu olduğu için listeden çıkar
            block.transform.DOMove(targetNode.Pos, 1.2f).SetEase(Ease.InOutQuad);
            availableNodes.Remove(targetNode);
            
        }

        GridManager.Instance.UpdateOccupiedBlock();
        StartCoroutine(HandleStateDelayed());

    }

    IEnumerator HandleStateDelayed()
    {
        yield return new WaitForSeconds(1.21f);
        Instance.ChangeState(GameState.WaitingInput);
    }

    private Node AssignNewPosition(int type, List<Node> availableNodes, bool isOrdered = false)
    {
        Node newNode = null;
        float percentage = Random.Range(0f, 1f);

        if (isOrdered)
            percentage = Random.Range(0f, 0.1f);

        if (percentage > 0.1f) // %90 tamamen rastgele
        {
            // Fisher-Yates Shuffle uygulanarak rastgele seçme
            int n = availableNodes.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (availableNodes[i], availableNodes[j]) = (availableNodes[j], availableNodes[i]);
            }

            newNode = availableNodes[0]; // Fisher-Yates ile karıştırılan ilk düğümü seç
        }
        else if(percentage>=0.05f && percentage<=0.1f) // sütuna atama
        {
            int selectedColumn = BlockType.Instance.SelectColumn(type);
            List<Node> columnNodes = availableNodes.FindAll(n => n.gridPosition.x == selectedColumn);

            if (columnNodes.Count > 0)
            {
                newNode = columnNodes[Random.Range(0, columnNodes.Count)];
            }
        }
        else // Satıra atama
        {
            int selectedRow = BlockType.Instance.SelectRow(type);
            List<Node> rowNodes = availableNodes.FindAll(n => n.gridPosition.y == selectedRow);

            if (rowNodes.Count > 0)
            {
                newNode = rowNodes[Random.Range(0, rowNodes.Count)];
            }
        }

        return newNode;
    }

}
