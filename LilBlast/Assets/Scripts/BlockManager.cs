using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance { get; private set; }
    

    [SerializeField] private Block[] blockTypes;
    public List<Block> _blocks;

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
        _blocks = new List<Block>();

    }
    public void SpawnBlocks()
    {
        List<Node> nodesToFill = GridManager.Instance.freeNodes.ToList();
        foreach (var node in nodesToFill)
        {
            int randomIndex = Random.Range(0, blockTypes.Length);
            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[randomIndex], spawnPos, Quaternion.identity);

            randomBlock.SetBlock(node);
            _blocks.Add(randomBlock);

            GridManager.Instance.freeNodes.Remove(node);

            randomBlock.transform.DOMove(node.Pos, 0.7f).SetEase(Ease.OutBounce);
        }

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
                    b.node.OccupiedBlock = null;
                    GridManager.Instance.freeNodes.Add(b.node); // Boşalan düğümü freeNodes'a ekle
                }
                Destroy(b.gameObject);
                ObjectPool.Instance.GetParticleFromPool(b.blockType, b.node.Pos, Quaternion.identity);
            }

            GridManager.Instance.UpdateFreeNodes();
            BlockManager.Instance.FindAllNeighbours();
            GameManager.Instance.ChangeState(GameState.Blasting);
        }
        block.Shake();
        ObjectPool.Instance.PlaySound(5);
    }

}
