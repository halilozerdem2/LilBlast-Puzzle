using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movesCountText;
    [SerializeField] private TextMeshProUGUI collectedBlockCountText;
    [SerializeField] private RawImage targetBlock;
    [SerializeField] private Sprite[] blockIcons;

    private int moves;
    private int targetBlockCount;
    private int targetBlockType;

    public List<Block> collectedBlocks;

    private void Awake()
    {
        collectedBlocks = new List<Block>();
        
        AssignTarget();
    }

    public void AssignTarget()
    {
        collectedBlocks.Clear();
        targetBlockType = Random.Range(0, blockIcons.Length);
        targetBlockCount = Random.Range(20, 30);
        moves = Random.Range(35, 55);
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
        }

        if (collectedBlocks.Count >= targetBlockCount)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Win);
        }
        else if (moves <= 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Lose);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        movesCountText.text = moves.ToString();
        collectedBlockCountText.text = $"{collectedBlocks.Count} / {targetBlockCount}";

        if (blockIcons != null && blockIcons.Length > targetBlockType && targetBlock != null)
        {
            targetBlock.texture = blockIcons[targetBlockType].texture;
        }
    }

    public void DecreaseMove()
    {
        moves--;
        UpdateUI();
    }
}
