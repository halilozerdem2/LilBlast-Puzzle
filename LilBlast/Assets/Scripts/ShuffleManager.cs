using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShuffleManager : MonoBehaviour
{
    public void HandleShuffle()
    {
        Debug.Log("Shuffle Başladı  |: boş hücre sayısı : " + GridManager.Instance.freeNodes.Count);
        //GridManager.Instance.freeNodes.Clear();
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            GridManager.Instance.freeNodes.Add(node);

        }
        Debug.Log("Grid boşaltıldı |: boş hücre sayısı : " + GridManager.Instance.freeNodes.Count);

        //GridManager.Instance.UpdateFreeNodes(); // Güncellenmiş boş node listesini al
        List<Node> availableNodes = new List<Node>(GridManager.Instance.freeNodes); // Shuffle için boş düğümleri listeye al

        if (availableNodes.Count < BlockManager.Instance._blocks.Count)
        {
            Debug.LogError("Shuffle için yeterli boş node bulunamadı!");
            return;
        }

        foreach (var block in BlockManager.Instance._blocks)
        {
            Node newNode = AssignNewPosition(block.blockType, availableNodes);
            if (newNode == null)
            {
                Debug.LogWarning("Geçerli bir shuffle pozisyonu bulunamadı, fallback olarak rastgele atama yapılacak.");
                newNode = availableNodes[Random.Range(0, availableNodes.Count)];
            }

            block.transform.DOMove(newNode.Pos, 2f).SetEase(Ease.InOutQuad);
            block.SetBlock(newNode);
            
            availableNodes.Remove(newNode);
            GridManager.Instance.freeNodes.Remove(newNode);// Seçilen node artık dolu olduğu için listeden çıkar
        }

        Debug.Log("KARIŞTIRMA BİTTİ |: boş hücre sayısı : " + GridManager.Instance.freeNodes.Count);

        GridManager.Instance.UpdateFreeNodes();
        BlockManager.Instance.FindAllNeighbours();

    }

    private Node AssignNewPosition(int type, List<Node> availableNodes)
    {
        float percentage = Random.Range(0f, 1f);
        Node newNode = null;

        if (percentage > 1f) // %10 ihtimalle belirli bir sütuna göre atama
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
        else // %70 ihtimalle tamamen rastgele atama

        {
            int selectedColumn = BlockType.Instance.SelectColumn(type);
            List<Node> columnNodes = availableNodes.FindAll(n => n.gridPosition.x == selectedColumn);

            if (columnNodes.Count > 0)
            {
                newNode = columnNodes[Random.Range(0, columnNodes.Count)];
            }
        }

        return newNode;
    }

}
