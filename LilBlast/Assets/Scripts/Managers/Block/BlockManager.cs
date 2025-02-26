using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

using static GameManager;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    public static event Action<Block> OnBlockBlasted;

    [SerializeField] private Block[] blockTypes;

    public List<Block> blocks;
    public List<Block> blastedBlocks;
    public List<RegularBlock> regularBlocks;
    public Queue<Block> specialBlocks;

    public GameOverHandler handler;

    public GameObject bomb ,vRocket, hRocket;

    private int minBlastableBlockGroupSize = 2;

    private void Awake()
    {
        Instance = this;
        blocks = new List<Block>();
        specialBlocks = new Queue<Block>();
        blastedBlocks = new List<Block>();
        regularBlocks=new List<RegularBlock>();

    }
    public void SpawnBlocks()
    {
        //if (GameManager.Instance._state != GameState.SpawningBlocks) return;
        List<Node> nodesToFill = GridManager.freeNodes.ToList();
        int counter = 0, spawnIndex;
        int targetBlockType = handler.targetBlockType;
        
        foreach (var node in nodesToFill)
        {
            int deadlockIndex = counter % 5;
            int randomIndex = Random.Range(0, blockTypes.Length);
            
            float spawnAccuricyPercentage = Random.Range(0f, 1f);
            if (spawnAccuricyPercentage <= 0.15f)
                spawnIndex = targetBlockType;
            else
                spawnIndex = randomIndex;

            GridManager.freeNodes.Remove(node);

            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[spawnIndex], spawnPos, Quaternion.identity);
            //Block randomBlock =  ObjectPool.Instance.GetBlockFromPool(spawnIndex, spawnPos, Quaternion.identity);
            counter++;
            randomBlock.SetBlock(node);
            blocks.Add(randomBlock);
            regularBlocks.Add(randomBlock as RegularBlock);


            randomBlock.transform.DOMove(node.Pos, 0.5f).SetEase(Ease.OutBounce);
           // Debug.Log("hücreler doluyor| boş hücre sayısı" + GridManager.freeNodes.Count);
        }
        StartCoroutine(CheckValidMoves());
        
        //FindAllNeighbours();

    }

    IEnumerator CheckValidMoves()
    {
        yield return new WaitForSeconds(0.31f);
       // Debug.Log("Block spawnland |: boş hücre sayısı : " + GridManager.freeNodes.Count);
        if (HasValidMoves())
            GameManager.Instance.ChangeState(GameState.WaitingInput);
        else
            GameManager.Instance.ChangeState(GameState.Shuffling);
    }

    public void FindAllNeighbours()
    {
        foreach (var block in regularBlocks)
        {
            if (block != null)
            {
                block.FindNeighbours();
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
        HashSet<Block> group = block.DetermineGroup();

        if(block is RegularBlock)
        {
            if (group.Count >= minBlastableBlockGroupSize)
            {
                handler.DecreaseMove();
                handler.UpdateTarget(block, group.Count);
                GameManager.Instance.ChangeState(GameState.Blasting);
                BlastBlocks(group);

                 switch (group.Count)
                 {
                    case 3: CreateSpecialBlock(vRocket, block.node); break;
                    case 4: CreateSpecialBlock(bomb, block.node); break;
                    case 5: CreateSpecialBlock(hRocket, block.node); break;
                 }
            }
        }
        else
        {
            handler.DecreaseMove();
            foreach (var b in group)
            {
                if (b != block)
                    specialBlocks.Enqueue(b);
                if (b.blockType == handler.targetBlockType)
                    handler.UpdateTarget(b, 1);
            }
            BlastBlocks(group);
        }
        GameManager.Instance.ChangeState(GameState.Falling);
        block.Shake(0.3f, 0.1f);
        ObjectPool.Instance.PlaySound(5);
    }

    private void CreateSpecialBlock(GameObject specialBlock, Node aNode)
    {
        var specialBlockObject = Instantiate(specialBlock, aNode.Pos, Quaternion.identity);
        Block b = specialBlockObject.GetComponent<Block>();
        b.SetBlock(aNode);
        blocks.Add(b);
    }


    private void BlastBlocks(HashSet<Block> aBlockGroup)
    {
        foreach (var b in aBlockGroup)
        {
            if (b.node != null)
            {
                if(b is RegularBlock)
                {
                    BlastBlock(b);
                    regularBlocks.Remove(b as RegularBlock);
                }
                else
                {
                    specialBlocks.Enqueue(b);
                    Debug.Log(b);
                }
            }   
        }

        BlastSpecialBlocks();
    }


    private void BlastSpecialBlocks()
    {
        while (specialBlocks.Count > 0)
        {
            HashSet<Block> blockGroup = specialBlocks.Dequeue().DetermineGroup();

            foreach (var item in blockGroup)
            {
                if (item is RegularBlock)
                    regularBlocks.Remove(item as RegularBlock);
                else
                {
                    if (item != blockGroup.First())
                        specialBlocks.Enqueue(item);
                }

                BlastBlock(item);
            }
        }
    }


    private void BlastBlock(Block b)
    {
        GridManager.freeNodes.Add(b.node);
        blastedBlocks.Add(b);
        b.node.OccupiedBlock = null;
        OnBlockBlasted?.Invoke(b);
        blocks.Remove(b);
        Destroy(b.gameObject);
        //ObjectPool.Instance.ReturnToBlockPool(b.blockType, b.gameObject);
        ObjectPool.Instance.GetParticleFromPool(b.blockType, b.node.Pos, Quaternion.identity);

    }

    private IEnumerator MoveUpAndDestroy(Block b)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPos = b.transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 0.5f, 0); // Blok yukarı çıkacak

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            b.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        Destroy(b.gameObject);
    }


}

