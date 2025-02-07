using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using Random = UnityEngine.Random;

public class ShuffleManager : MonoBehaviour
{
    public  List<Node> availableNodes;
    public List<Block> blocks;

    public void HandleShuffle()
    {
        //.Log("Shuffle Başladı  |:blok sayısı : " + BlockManager.Instance._blocks.Count);
        //GridManager.freeNodes.Clear();
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            GridManager.freeNodes.Add(node);
            node.OccupiedBlock = null;

        }
        //Debug.Log("Grid boşaltıldı |: boş hücre sayısı : " + GridManager.freeNodes.Count);

        //GridManager.Instance.UpdateFreeNodes(); // Güncellenmiş boş node listesini al
        availableNodes = new List<Node>(GridManager.Instance._nodes.Values); // Shuffle için boş düğümleri listeye al
        blocks = new List<Block>(BlockManager.Instance._blocks);

        if (availableNodes.Count < blocks.Count)
        {
            Debug.LogError("Shuffle için yeterli boş node bulunamadı!");
            return;
        }
            Debug.Log(availableNodes.Count);

        foreach (var block in blocks)
        {
            block.Shake(0.2f, 0.1f);
            Node targetNode = AssignNewPosition(block.blockType, availableNodes);

            if (targetNode == null)
            {
                Debug.LogWarning("Geçerli bir shuffle pozisyonu bulunamadı, fallback olarak rastgele atama yapılacak.");
                targetNode = availableNodes[Random.Range(0, availableNodes.Count)];
            }

            block.SetBlock(targetNode);
            //Debug.Log("grid pozisyonu : "  + targetNode.gridPosition+"Blok : " +targetNode.OccupiedBlock);
            GridManager.freeNodes.Remove(targetNode);// Seçilen node artık dolu olduğu için listeden çıkar
            //Debug.Log("grid doluyor |: boş hücre sayısı : " + GridManager.freeNodes.Count);
            block.transform.DOMove(targetNode.Pos, 1f).SetEase(Ease.InOutQuad)
             .OnComplete(() => {
                 Instance.ChangeState(GameState.WaitingInput);
             }); ; ;


            availableNodes.Remove(targetNode);
            
        }
        GridManager.Instance.UpdateOccupiedBlock();
        BlockManager.Instance.FindAllNeighbours();

    }

    private Node AssignNewPosition(int type, List<Node> availableNodes)
    {
        float percentage = Random.Range(0f, 1f);
        Node newNode = null;

        if (percentage >= .2f) // %70 tamamen rastgele
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
        else if(percentage>=.1f) // %10 belirlenen sütuna atama
        {
            int selectedColumn = BlockType.Instance.SelectColumn(type);
            List<Node> columnNodes = availableNodes.FindAll(n => n.gridPosition.x == selectedColumn);

            if (columnNodes.Count > 0)
            {
                newNode = columnNodes[Random.Range(0, columnNodes.Count)];
            }
        }
        else
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
