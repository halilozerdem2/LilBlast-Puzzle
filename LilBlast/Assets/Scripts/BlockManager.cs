using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

using static GameManager;
using TreeEditor;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    public static event Action<Block> OnBlockBlasted;

    [SerializeField] private Block[] blockTypes;
    public HashSet<Block> _blocks;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _blocks = new HashSet<Block>();

    }
    public void SpawnBlocks()
    {
        List<Node> nodesToFill = GridManager.Instance.freeNodes.ToList();
        int counter = 0;
        foreach (var node in nodesToFill)
        {
            GridManager.Instance.freeNodes.Remove(node);
            int randomIndex = Random.Range(0, blockTypes.Length);
            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[counter%5], spawnPos, Quaternion.identity);
            //BlockType.Instance.AddBlock(randomBlock.blockType, node.gridPosition);
            counter++;
            randomBlock.SetBlock(node);
            _blocks.Add(randomBlock);


            randomBlock.transform.DOMove(node.Pos, 0.7f).SetEase(Ease.OutBounce);
            Debug.Log("hücreler doluyor| boş hücre sayısı" + GridManager.Instance.freeNodes.Count);
        }
        Debug.Log("Block spawnland |: boş hücre sayısı : " + GridManager.Instance.freeNodes.Count);
        FindAllNeighbours();
        if (HasValidMoves())
            GameManager.Instance.ChangeState(GameState.WaitingInput);
        else
            GameManager.Instance.ChangeState(GameState.Deadlock);
    }

    public void FindAllNeighbours()
    {
        foreach (var block in _blocks)
        {
            if (block != null)
            {
                block.FindNeighbours(GridManager.Instance._nodes.Values.ToList());
            }
        }
    }

    public bool HasValidMoves() // Deadlock Tespiti
    {
        foreach (var node in GridManager.Instance._nodes.Values)
        {
            if (node.OccupiedBlock == null) continue;

            Block currentBlock = node.OccupiedBlock;
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                if (GridManager.Instance._nodes.TryGetValue(node.gridPosition + dir, out Node neighborNode))
                {
                    if (neighborNode.OccupiedBlock != null && neighborNode.OccupiedBlock.blockType == currentBlock.blockType)
                    {
                        return true; // En az bir geçerli hamle var, shuffle yapmaya gerek yok
                    }
                }
            }
        }
        return false; // Deadlock durumu var, shuffle yapılmalı
    }

    public void TryBlastBlock(Block block)
    {
        HashSet<Block> group = block.FloodFill();
        if (group.Count >= 2)
        {
            foreach (var b in group)
            {
                if (b.node != null)
                {
                    //BlockType.Instance.RemoveBlock(b.blockType, b.node.gridPosition);
                    GridManager.Instance.freeNodes.Add(b.node); // Boşalan düğümü freeNodes'a ekle
                    b.node.OccupiedBlock = null;
                    _blocks.Remove(b);
                }
                Destroy(b.gameObject);
                ObjectPool.Instance.GetParticleFromPool(b.blockType, b.node.Pos, Quaternion.identity);
            }

            //GridManager.Instance.UpdateFreeNodes();
            Debug.Log("Bloklar patlatıldı : boş hücre sayısı : " + GridManager.Instance.freeNodes.Count);
            FindAllNeighbours();
            GameManager.Instance.ChangeState(GameState.Blasting);
        }
        block.Shake(0.3f, 0.1f);
        ObjectPool.Instance.PlaySound(5);
    }
}

