using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverHandler : MonoBehaviour
{
    public static GameOverHandler Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI movesCountText;
    [SerializeField] private TextMeshProUGUI collectedBlockCountText;
    [SerializeField] private RawImage targetBlock;
    [SerializeField] private Sprite[] blockIcons;

    public int moves;
    private int targetBlockCount;
    public int targetBlockType;
    private int currentTarget;
    public bool pendingWin = false;

    public List<Block> collectedBlocks;
    public List<Block> blastedBlocks;

    private void Awake()
    {
        // Singleton ayarı
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        collectedBlocks = new List<Block>();
        AssignTarget();
    }

    public void AssignTarget()
    {
        pendingWin = false;
        collectedBlocks.Clear();
        blastedBlocks.Clear(); 
        targetBlockType = Random.Range(0, blockIcons.Length);
        targetBlockCount = Random.Range(50, 80);
        currentTarget = targetBlockCount;
        moves = Random.Range(40, 55);
       //moves = 2;
        UpdateUI();
    }

    private void OnEnable()
    {
        BlockManager.OnBlockBlasted += CheckCollectedBlocks;
    }

    private void OnDisable()
    {
        BlockManager.OnBlockBlasted -= CheckCollectedBlocks;
    }

    public void CheckCollectedBlocks(Block aTargetBlock)
    {
        if (aTargetBlock.blockType == targetBlockType)
        {
            collectedBlocks.Add(aTargetBlock);
            UpdateUI();
        }

        if (collectedBlocks.Count >= targetBlockCount)
        {
            pendingWin = true;
            UpdateUI(true);
        }
        else if (moves <= 0)
        {
            moves = 0;
            UpdateUI();
            GameManager.Instance.ChangeState(GameManager.GameState.Lose);
        }
    }

    private void UpdateUI(bool isWin = false)
    {
        movesCountText.text = moves.ToString();
        collectedBlockCountText.text = currentTarget.ToString();

        if (blockIcons != null && blockIcons.Length > targetBlockType && targetBlock != null)
        {
            targetBlock.texture = blockIcons[targetBlockType].texture;
        }
    }

    public void DecreaseMove(int aAmount=1)
    {
        moves--;
        UpdateUI();
    }

    public void UpdateTarget(Block b, int a)
    {
        if (b.blockType == targetBlockType)
        {
            currentTarget -= a;
            if (currentTarget <= 0) currentTarget = 0;
            UpdateUI();
        }
    }

    public void IncreaseMoves()
    {
        moves += 5;
        UpdateUI();
    }


}
