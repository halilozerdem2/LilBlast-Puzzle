using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

using static GameManager;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    public static event Action<Block> OnBlockBlasted;

    [SerializeField] private Block[] blockTypes;
    public List<Block> _blocks;
    public List<Block> blastedBlocks;

    public GameOverHandler handler;

    int minBlastableBlockGroupSize = 2;

    private void Awake()
    {
        Instance = this;
        _blocks = new List<Block>();
        blastedBlocks = new List<Block>();

    }

    public void SpawnBlocks()
    {
        //if (GameManager.Instance._state != GameState.SpawningBlocks) return;
        List<Node> nodesToFill = GridManager.freeNodes.ToList();
        int counter = 0;
        foreach (var node in nodesToFill)
        {
            GridManager.freeNodes.Remove(node);

            int deadlockIndex = counter % 5;
            int randomIndex = Random.Range(0, blockTypes.Length);

            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[randomIndex], spawnPos, Quaternion.identity);

            counter++;
            randomBlock.SetBlock(node);
            _blocks.Add(randomBlock);


            randomBlock.transform.DOMove(node.Pos, 0.7f).SetEase(Ease.OutBounce);
            //Debug.Log("hücreler doluyor| boş hücre sayısı" + GridManager.freeNodes.Count);
        }
       // Debug.Log("Block spawnland |: boş hücre sayısı : " + GridManager.freeNodes.Count);
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
        if (GameManager.Instance._state != GameState.WaitingInput) return;
        HashSet<Block> group = block.FloodFill();
        if (group.Count >= minBlastableBlockGroupSize)
        {
            handler.UpdateTarget(block, group.Count);

            GameManager.Instance.ChangeState(GameState.Blasting);
            BlastBlocks(group);
            handler.DecreaseMove();
            //if (GameManager.Instance._state != GameManager.GameState.WaitingInput) return;

     
            //GridManager.Instance.UpdateFreeNodes();
           // Debug.Log("Bloklar patlatıldı : boş hücre sayısı : " + GridManager.freeNodes.Count);
            FindAllNeighbours();

            GameManager.Instance.ChangeState(GameState.Falling);
        }
        block.Shake(0.3f, 0.1f); // Wrong Move
        ObjectPool.Instance.PlaySound(5);

    }

    private void BlastBlocks(HashSet<Block> aBlockGroup)
    {
        foreach (var b in aBlockGroup)
        {
            if (b.node != null)
            {
                //Debug.Log(GameManager.Instance._state);
                blastedBlocks.Add(b);
                OnBlockBlasted?.Invoke(b);
                //BlockType.Instance.RemoveBlock(b.blockType, b.node.gridPosition);
                GridManager.freeNodes.Add(b.node); // Boşalan düğümü freeNodes'a ekle
                b.node.OccupiedBlock = null;
                _blocks.Remove(b);
            }
            Destroy(b.gameObject);
            //ObjectPool.Instance.ReturnToBlockPool(b.blockType, b.gameObject);
            ObjectPool.Instance.GetParticleFromPool(b.blockType, b.node.Pos, Quaternion.identity);
        }
    }
}

