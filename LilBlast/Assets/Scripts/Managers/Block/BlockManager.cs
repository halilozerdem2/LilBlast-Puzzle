using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using static GameManager;
using DG.Tweening.Core.Easing;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class BlockManager : MonoBehaviour
{
    public static BlockManager Instance;
    public static event Action<Block> OnBlockBlasted;

    [SerializeField] private Block[] blockTypes;

    public List<Block> blocks;
    public List<Block> blastedBlocks;
    public List<RegularBlock> regularBlocks;
    public Queue<Block>  specialBlocks;

    [SerializeField] private GameOverHandler handler;
    [SerializeField] private ScoreManager score; 
    public GameObject bomb ,vRocket, hRocket,colorBomb;

    private int minBlastableBlockGroupSize = 2;

    private void Awake()
    {
        Instance = this;
        blocks = new List<Block>();
        specialBlocks = new Queue<Block>();
        blastedBlocks = new List<Block>();
        regularBlocks = new List<RegularBlock>();
    }

    public void InitializeBlockManager()
    {
        blocks.Clear();
        specialBlocks.Clear();
        blastedBlocks.Clear();
        regularBlocks.Clear();
        Debug.Log("BlockManager yeniden başlatıldı.");
    }


    public void SpawnBlocks()
    {
        //if (GameManager.Instance._state != GameState.SpawningBlocks) return;
        List<Node> nodesToFill = GridManager.freeNodes.ToList();
        int counter = 0, spawnIndex;
        int targetBlockType = handler.targetBlockType;
        
        foreach (var node in nodesToFill)
        {
            int deadlockIndex = counter % 5; // deadlock için
            int randomIndex = Random.Range(0, blockTypes.Length);
            
            float spawnAccuricyPercentage = Random.Range(0f, 1f);
            if (spawnAccuricyPercentage <= 0.15f)
                spawnIndex = targetBlockType;
            else
                spawnIndex = randomIndex;

            GridManager.freeNodes.Remove(node);

            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            Block randomBlock = Instantiate(blockTypes[spawnIndex], spawnPos, Quaternion.identity, node.transform);
            //Block randomBlock =  ObjectPool.Instance.GetBlockFromPool(spawnIndex, spawnPos, Quaternion.identity);
            counter++; // deadlock için
            randomBlock.SetBlock(node);
            
            blocks.Add(randomBlock);
            regularBlocks.Add(randomBlock as RegularBlock);
            
            randomBlock.transform.DOMove(node.Pos, 0.5f).SetEase(Ease.OutBounce);
           // Debug.Log("hücreler doluyor| boş hücre sayısı" + GridManager.freeNodes.Count);
        }
        StartCoroutine(CheckValidMoves());
    }

    IEnumerator CheckValidMoves()
    {
        yield return new WaitForSeconds(0.51f);
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
        if (block is RegularBlock)
        {
            if (group.Count >= minBlastableBlockGroupSize)
            {
                handler.DecreaseMove();
                handler.UpdateTarget(block, group.Count);
                GameManager.Instance.ChangeState(GameState.Blasting);
                BlastRegularBlocks(group);

                switch (group.Count)
                {
                    case 1: break;
                    case 2: break;
                    case 3: CreateSpecialBlock(vRocket, block.node); break;
                    case 4: CreateSpecialBlock(hRocket, block.node); break;
                    case 5: CreateSpecialBlock(bomb, block.node); break;
                    default: CreateSpecialBlock(colorBomb, block.node,block.blockType); break;
                }

                GameManager.Instance.ChangeState(GameState.Falling);
            }
        }
        else
        {
            handler.DecreaseMove();
            GameManager.Instance.ChangeState(GameState.Blasting);
            specialBlocks.Enqueue(block);
            BlastSpecialBlocks();
        }

        
        block.Shake(0.3f, 0.1f);
        //ObjectPool.Instance.PlaySound(5);
    }


    private void CreateSpecialBlock(GameObject specialBlock, Node aNode, int blockType = -1)
    {
        var specialBlockObject = Instantiate(specialBlock, aNode.Pos, Quaternion.identity, aNode.transform);
        Block b = specialBlockObject.GetComponent<Block>();

        // Eğer bu block bir ColorBombBlock ise targetColorType ata
        ColorBombBlock colorBombBlock = b as ColorBombBlock;
        if (colorBombBlock != null && blockType != -1)
        {
            colorBombBlock.targetColorType = blockType;
        
            // İstersen ColorBomb’un rengini de değiştir
            SpriteRenderer sr = colorBombBlock.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // blockType'a göre bir renk tablosu kullanabilirsin
                //sr.color = BlockTypeToColor(blockType);
            }
        }

        b.SetBlock(aNode);
        blocks.Add(b);

        Vector3 initialSize = b.transform.localScale;
        specialBlockObject.transform.localScale = Vector3.zero;
        specialBlockObject.transform.DOScale(initialSize, 0.7f).SetEase(Ease.OutBack);
    }


    private void BlastRegularBlocks(HashSet<Block> aBlockGroup)
    {
        foreach (var b in aBlockGroup)
        {
            if (b.node != null)
            {
                if (b is RegularBlock)
                {
                    BlastBlock(b);
                    regularBlocks.Remove(b as RegularBlock);
                }

            }
        }
    }


    private void BlastSpecialBlocks()
    {
        StartCoroutine(ChangeStateDelayed(0.31f, GameState.Falling));
        while (specialBlocks.Count > 0)
        {
            Block specialBlock = specialBlocks.Dequeue();
            
            specialBlock.isBeingDestroyed = true;
            HashSet<Block> blockGroup = specialBlock.DetermineGroup();

            foreach (var item in blockGroup)
            {
                if (item is RegularBlock)
                    regularBlocks.Remove(item as RegularBlock);
                else
                {
                    if (item != blockGroup.First() && !item.isBeingDestroyed)
                    {
                        item.isBeingDestroyed = true;
                        specialBlocks.Enqueue(item);
                    }
                }
                if (item.blockType == handler.targetBlockType)
                    handler.UpdateTarget(item, 1);

                StartCoroutine(DelayedBlastBlock(item, 0.3f));
            }
            //HighlightGroupBeforeDestroy(blockGroup);
        }
        

    }
    IEnumerator ChangeStateDelayed(float aDelay, GameState aState)
    {
        yield return new WaitForSeconds(aDelay);
       // Debug.Log("a");
        GameManager.Instance.ChangeState(aState);

    }

    private IEnumerator DelayedBlastBlock(Block b, float delay)
    {
        if(b is BombBlock) b.Shake(delay, 0.2f);
        yield return new WaitForSeconds(delay);
        BlastBlock(b);

    }
    private void BlastBlock(Block b)
    {
        GridManager.freeNodes.Add(b.node);
        blastedBlocks.Add(b);
        b.node.OccupiedBlock = null;
        OnBlockBlasted?.Invoke(b);
        blocks.Remove(b);
        score.UpdateScore(b);
        Destroy(b.gameObject);
        //ObjectPool.Instance.ReturnToBlockPool(b.blockType, b.gameObject);
        ObjectPool.Instance.GetParticleFromPool(b.blockType, b.node.Pos, Quaternion.identity);

    }
    


    IEnumerator DestroyDelayed(GameObject aGameObject)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(aGameObject);
    }
    
}

