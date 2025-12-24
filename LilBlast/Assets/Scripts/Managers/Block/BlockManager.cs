using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
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
    public event Action BoardCleared;

    [SerializeField] private Block[] blockTypes;

    public List<Block> blocks;
    public List<Block> blastedBlocks;
    public List<RegularBlock> regularBlocks;
    private bool suppressRefills;
    private readonly Dictionary<GameObject, int> prefabPoolIndices = new Dictionary<GameObject, int>();
    private int[] blockTypePoolIndices;
    private int nextPoolIndex;

    [SerializeField] private GameOverHandler handler;
    [SerializeField] private ScoreManager score;
    public GameObject bomb, vRocket, hRocket, colorBomb;

    private int minBlastableBlockGroupSize = 2;
    public bool isModifyActive = false;
    public int modifyTargetType = -1; // Hedef tip (örn. handler.targetBlockType)

    public bool SuppressRefills => suppressRefills;
    public bool IsClearingBoard => isClearingBoard;

    private bool isClearingBoard;

    private void Awake()
    {
        Instance = this;
        blocks = new List<Block>();
        blastedBlocks = new List<Block>();
        regularBlocks = new List<RegularBlock>();
        blockTypePoolIndices = new int[blockTypes.Length];
        RegisterBlockPrefabs();
        suppressRefills = false;
    }

    public void InitializeBlockManager()
    {
        StopAllCoroutines();
        blocks.Clear();
        blastedBlocks.Clear();
        regularBlocks.Clear();
        prefabPoolIndices.Clear();
        RegisterBlockPrefabs();
        suppressRefills = false;
    }

    private void RegisterBlockPrefabs()
    {
        nextPoolIndex = 0;
        for (int i = 0; i < blockTypes.Length; i++)
        {
            var blockPrefab = blockTypes[i];
            blockTypePoolIndices[i] = EnsurePoolIndex(blockPrefab != null ? blockPrefab.gameObject : null);
        }
        RegisterSpecialPrefab(bomb);
        RegisterSpecialPrefab(vRocket);
        RegisterSpecialPrefab(hRocket);
        RegisterSpecialPrefab(colorBomb);
    }

    private void RegisterSpecialPrefab(GameObject prefab)
    {
        EnsurePoolIndex(prefab);
    }

    private int EnsurePoolIndex(GameObject prefab)
    {
        if (prefab == null)
            return -1;

        if (prefabPoolIndices.TryGetValue(prefab, out var existing))
            return existing;

        var poolIndex = nextPoolIndex++;
        prefabPoolIndices[prefab] = poolIndex;
        return poolIndex;
    }


    public void SpawnBlocks()
    {
        //if (GameManager.Instance._state != GameState.SpawningBlocks) return;
        if (suppressRefills)
            return;
        GridManager.Instance.UpdateOccupiedBlock();
        var dedupedNodes = new List<Node>();
        var seenNodes = new HashSet<Node>();
        foreach (var node in GridManager.freeNodes)
        {
            if (node == null || node.OccupiedBlock != null || node.HasBlocker)
                continue;

            if (seenNodes.Add(node))
                dedupedNodes.Add(node);
        }

        GridManager.freeNodes.Clear();
        GridManager.freeNodes.AddRange(dedupedNodes);

        List<Node> nodesToFill = dedupedNodes;
        int counter = 0, spawnIndex;
        int targetBlockType = handler.targetBlockType;
        var difficultyManager = DifficultyManager.Instance;
        float difficultySpawnChance = Mathf.Clamp01(
            difficultyManager != null
                ? difficultyManager.CurrentConfig.spawnAccuracy
                : 0.15f);

        foreach (var node in nodesToFill)
        {
            int deadlockIndex = counter % 5; // deadlock için
            int randomIndex = Random.Range(0, blockTypes.Length);

            float spawnAccuricyPercentage = Random.Range(0f, 1f);
            if (spawnAccuricyPercentage <= difficultySpawnChance)
                spawnIndex = targetBlockType;
            else
                spawnIndex = randomIndex;

            GridManager.freeNodes.Remove(node);

            Vector3 spawnPos = new Vector3(node.Pos.x, GridManager.Instance._height + 1, 0);
            var blockPrefab = blockTypes[spawnIndex];
            var poolIndex = blockTypePoolIndices[spawnIndex];
            Block randomBlock = SpawnBlockFromPool(poolIndex, blockPrefab != null ? blockPrefab.gameObject : null, spawnPos, node.transform);
            if (randomBlock == null)
                continue;
            counter++; // deadlock için
            randomBlock.SetBlock(node);

            blocks.Add(randomBlock);
            if (randomBlock is RegularBlock regularBlock)
                regularBlocks.Add(regularBlock);

            randomBlock.transform.DOMove(node.Pos, 0.5f).SetEase(Ease.OutBounce);
            // Debug.Log("hücreler doluyor| boş hücre sayısı" + GridManager.freeNodes.Count);
        }
        StartCoroutine(CheckValidMoves());
    }

    IEnumerator CheckValidMoves()
    {
        yield return new WaitForSeconds(0.51f);
        // Debug.Log("Block spawnland |: boş hücre sayısı : " + GridManager.freeNodes.Count);
        //if(GameManager.Instance._state==GameState.Win)
        if (HasValidMoves())
            GameManager.Instance.ChangeState(GameState.WaitingInput);
        else
            GameManager.Instance.ChangeState(GameState.Shuffling);
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
                        if (isModifyActive && neighborNode.OccupiedBlock.blockType == handler.targetBlockType)
                            continue;
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
                var specialBlockNode = block.node;
                var specialBlockType = block.blockType;
                var targetAnimations = new List<(Sprite sprite, Color color, Vector3 position)>();
                foreach (var member in group)
                {
                    if (member != null && member.blockType == handler.targetBlockType)
                    {
                        var renderer = member.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                            targetAnimations.Add((renderer.sprite, renderer.color, member.transform.position));
                    }
                }
                handler.DecreaseMove();
                TriggerHapticFeedback();
                handler.UpdateTarget(block, group.Count);
                GameManager.Instance.ChangeState(GameState.Blasting);
                if (block.gameObject.activeInHierarchy)
                    block.ForceShake(0.3f, 0.1f);
                BlastRegularBlocks(group);

                foreach (var info in targetAnimations)
                    handler.PlayTargetCollectAnimation(info.sprite, info.color, info.position, 4f);

                switch (group.Count)
                {
                    case 1: break;
                    case 2: break;
                    case 3: CreateSpecialBlock(vRocket, specialBlockNode); break;
                    case 4: CreateSpecialBlock(hRocket, specialBlockNode); break;
                    case 5: CreateSpecialBlock(bomb, specialBlockNode); break;
                    default: CreateSpecialBlock(colorBomb, specialBlockNode, specialBlockType); break;
                }

                GameManager.Instance.ChangeState(GameState.Falling);
            }
        }
        else if (GameManager.Instance._state == GameState.Manipulating)
        {
            TryModifyBlock(block);
            return;
        }
        else
        {
            handler.DecreaseMove();
            TriggerHapticFeedback();
            GameManager.Instance.ChangeState(GameState.Blasting);
            if (block.gameObject.activeInHierarchy)
                block.ForceShake(0.3f, 0.1f);
            StartCoroutine(ProcessSpecialBlocks(block));
            return;
        }
        //ObjectPool.Instance.PlaySound(5);
    }


    private void CreateSpecialBlock(GameObject specialBlock, Node aNode, int blockType = -1)
    {
        if (specialBlock == null || aNode == null)
        {
            Debug.LogError("BlockManager: Special block prefab or target node missing.");
            return;
        }

        GridManager.freeNodes.Remove(aNode);

        int poolIndex = EnsurePoolIndex(specialBlock);
        Block b = SpawnBlockFromPool(poolIndex, specialBlock, aNode.Pos, aNode.transform);
        if (b == null)
            return;

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
        b.transform.localScale = Vector3.zero;
        b.transform.DOScale(initialSize, 0.7f).SetEase(Ease.OutBack);
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


    private IEnumerator ProcessSpecialBlocks(Block initialBlock)
    {
        var queue = new Queue<Block>();
        var visitedSpecials = new HashSet<Block>();
        queue.Enqueue(initialBlock);
        visitedSpecials.Add(initialBlock);

        while (queue.Count > 0)
        {
            var specialBlock = queue.Dequeue();
            if (specialBlock == null)
                continue;

            specialBlock.isBeingDestroyed = true;
            var blockGroup = specialBlock.DetermineGroup();
            NodeBlocker.HandleSpecialBlastArea(specialBlock);

            var regularsToBlast = new HashSet<Block>();

            foreach (var item in blockGroup)
            {
                if (item == null)
                    continue;

                if (item.gameObject.activeInHierarchy)
                    item.ForceShake(0.4f, 0.15f);

                if (item.blockType == handler.targetBlockType)
                {
                    handler.UpdateTarget(item, 1);
                    handler.PlayTargetCollectAnimation(
                        item.GetComponent<SpriteRenderer>()?.sprite,
                        item.GetComponent<SpriteRenderer>()?.color ?? Color.white,
                        item.transform.position,
                        4f);
                }

                if (item is RegularBlock regular)
                {
                    regularBlocks.Remove(regular);
                    regularsToBlast.Add(item);
                }
                else if (item != specialBlock && visitedSpecials.Add(item))
                {
                    queue.Enqueue(item);
                }
            }

            yield return new WaitForSeconds(0.4f);

            foreach (var block in regularsToBlast)
                BlastBlock(block);

            BlastBlock(specialBlock);
        }

        GameManager.Instance.ChangeState(GameState.Falling);
    }
    IEnumerator ChangeStateDelayed(float aDelay, GameState aState)
    {
        yield return new WaitForSeconds(aDelay);
        // Debug.Log("a");
        GameManager.Instance.ChangeState(aState);

    }

    private IEnumerator DelayedBlastBlock(Block b, float delay)
    {
        if (b is BombBlock) b.Shake(delay, 0.2f);
        yield return new WaitForSeconds(delay);
        BlastBlock(b);

    }
    private void BlastBlock(Block b)
    {
        if (b == null)
            return;

        var currentNode = b.node;
        var blastPosition = currentNode != null ? (Vector3)currentNode.Pos : b.transform.position;
        if (currentNode != null)
        {
            if (!currentNode.HasBlocker)
                GridManager.freeNodes.Add(currentNode);
            currentNode.OccupiedBlock = null;
        }
        blastedBlocks.Add(b);
        OnBlockBlasted?.Invoke(b);
        blocks.Remove(b);
        score.UpdateScore(b);
        b.transform.DOKill();

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnBlockToPool(b);
        }
        else
        {
            Destroy(b.gameObject);
        }

        ObjectPool.Instance?.GetParticleFromPool(b.blockType, blastPosition, Quaternion.identity);

    }


    public void BlastAllBlocks(bool simultaneous = true)
    {
        if (GameManager.Instance._state != GameState.WaitingInput && GameManager.Instance._state != GameState.Win)
            return;

        if (blocks.Count == 0)
        {
            isClearingBoard = true;
            NotifyBoardCleared();
            return;
        }

        isClearingBoard = true;
        handler.DecreaseMove();
        GameManager.Instance.ChangeState(GameState.Blasting);

        StartCoroutine(ClearBoardUsingGrid(0.02f));
    }

    public void DestroyAllBlocksInstant()
    {
        if (blocks.Count == 0)
            return;

        GameManager.Instance.ChangeState(GameState.Blasting);
        suppressRefills = false;
        isClearingBoard = true;
        ClearBoardInstant();
        GameManager.Instance.ChangeState(GameState.Falling);
        NotifyBoardCleared();
    }


    public void SetAllBlocksInteractable(bool interactable)
    {
        foreach (var block in blocks)
        {
            if (block != null)
            {
                Collider2D col = block.GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = interactable;
            }
        }
    }

    public void ToggleModifyMode()
    {
        isModifyActive = !isModifyActive;
        modifyTargetType = handler.targetBlockType; // ya da UI'den seçilecek tip
    }

    public void AllowRefills()
    {
        suppressRefills = false;
    }

    public void TryModifyBlock(Block block)
    {
        if (!isModifyActive) return;
        if (block == null || block.blockType == handler.targetBlockType || !(block is RegularBlock))
            return;

        HashSet<Block> group = block.DetermineGroup();
        var targetAnimations = new List<(Sprite sprite, Color color, Vector3 position)>();

        foreach (var b in group)
        {
            Node node = b.node;

            if (b.blockType == handler.targetBlockType)
            {
                var renderer = b.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    targetAnimations.Add((renderer.sprite, renderer.color, b.transform.position));
            }

            BlockType.Instance.RemoveBlock(b.blockType, node.gridPosition);
            blocks.Remove(b);
            if (b is RegularBlock rb)
                regularBlocks.Remove(rb);

            ObjectPool.Instance.ReturnBlockToPool(b);

            var newPrefab = blockTypes[handler.targetBlockType];
            int poolIndex = blockTypePoolIndices[handler.targetBlockType];
            Block newBlock = SpawnBlockFromPool(poolIndex, newPrefab != null ? newPrefab.gameObject : null, node.transform.position, transform);
            if (newBlock == null)
                continue;
            newBlock.blockType = handler.targetBlockType;

            blocks.Add(newBlock);
            if (newBlock is RegularBlock newRegular)
                regularBlocks.Add(newRegular);
            BlockType.Instance.AddBlock(handler.targetBlockType, node.gridPosition);

            newBlock.SetBlock(node);
        }

        isModifyActive = false;
        GameManager.Instance.ChangeState(GameManager.GameState.WaitingInput);
    }

    private Block SpawnBlockFromPool(int poolIndex, GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null)
            return null;

        Block blockInstance = null;
        if (ObjectPool.Instance != null && poolIndex >= 0)
        {
            blockInstance = ObjectPool.Instance.GetBlockFromPool(poolIndex, prefab, position, Quaternion.identity, parent);
        }
        else
        {
            var obj = Instantiate(prefab, position, Quaternion.identity, parent);
            blockInstance = obj.GetComponent<Block>();
            if (blockInstance != null)
                blockInstance.poolIndex = poolIndex;
        }

        return blockInstance;
    }

    private IEnumerator ClearBoardUsingGrid(float delayBetweenClears)
    {
        suppressRefills = true;

        var grid = GridManager.Instance;
        if (grid != null)
        {
            var specialBuffer = CollectSpecialBlocks(grid);
            foreach (var special in specialBuffer)
            {
                if (special == null)
                    continue;

                yield return ClearBlockFromNode(special, true);
            }

            yield return grid.ClearBoardSequentially(this, delayBetweenClears, false);
        }
        else
        {
            var snapshot = new List<Block>(blocks);
            foreach (var block in snapshot)
            {
                if (block == null || block is RegularBlock)
                    continue;

                yield return ClearBlockFromNode(block, true);
            }

            foreach (var block in snapshot)
            {
                if (block == null || !(block is RegularBlock))
                    continue;

                yield return ClearBlockFromNode(block, false);

                if (delayBetweenClears > 0f)
                    yield return new WaitForSeconds(delayBetweenClears);
            }
        }

        GameManager.Instance.ChangeState(GameState.Falling);
        NotifyBoardCleared();
    }

    internal IEnumerator ClearBlockFromNode(Block block, bool processSpecials)
    {
        if (block == null)
            yield break;

        block.isBeingDestroyed = true;

        if (processSpecials && !(block is RegularBlock))
        {
            if (block.gameObject.activeInHierarchy)
                block.ForceShake(0.2f, 0.12f);

            yield return ProcessSpecialBlocks(block);
            yield return new WaitForSeconds(0.05f);
        }
        else
        {
            if (block.gameObject.activeInHierarchy)
                block.ForceShake(0.15f, 0.12f);
            yield return new WaitForSeconds(0.02f);

            if (block.blockType == handler.targetBlockType)
            {
                handler.UpdateTarget(block, 1);
                var renderer = block.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    handler.PlayTargetCollectAnimation(renderer.sprite, renderer.color, block.transform.position, 4f);
            }

            BlastBlock(block);
        }
    }

    private void ClearBoardInstant()
    {
        var grid = GridManager.Instance;
        if (grid != null)
        {
            for (int y = grid._height - 1; y >= 0; y--)
            {
                for (int x = 0; x < grid._width; x++)
                {
                    var key = new Vector2Int(x, y);
                    if (!grid._nodes.TryGetValue(key, out var node) || node == null)
                        continue;

                    var block = node.OccupiedBlock;
                    RemoveBlockInstant(block);
                }
            }
        }
        else
        {
            foreach (var block in new List<Block>(blocks))
                RemoveBlockInstant(block);
        }
    }

    private void RemoveBlockInstant(Block block)
    {
        if (block == null || block.isBeingDestroyed)
            return;

        block.isBeingDestroyed = true;

        if (block.blockType == handler.targetBlockType)
        {
            handler.UpdateTarget(block, 1);
            var renderer = block.GetComponent<SpriteRenderer>();
            if (renderer != null)
                handler.PlayTargetCollectAnimation(renderer.sprite, renderer.color, block.transform.position, 4f);
        }

        BlastBlock(block);
    }

    private void TriggerHapticFeedback()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    private List<Block> CollectSpecialBlocks(GridManager grid)
    {
        var specials = new List<Block>();
        if (grid == null || grid._nodes == null || grid._nodes.Count == 0)
            return specials;

        var seen = new HashSet<Block>();
        for (int y = grid._height - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid._width; x++)
            {
                var key = new Vector2Int(x, y);
                if (!grid._nodes.TryGetValue(key, out var node) || node == null)
                    continue;

                var block = node.OccupiedBlock;
                if (block == null || block is RegularBlock)
                    continue;

                if (seen.Add(block))
                    specials.Add(block);
            }
        }

        return specials;
    }

    private void NotifyBoardCleared()
    {
        if (!isClearingBoard)
            return;

        isClearingBoard = false;
        BoardCleared?.Invoke();
    }

}
