using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverHandler : MonoBehaviour
{
    public static GameOverHandler Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI movesCountText;
    [SerializeField] private TextMeshProUGUI collectedBlockCountText;
    [SerializeField] private RawImage targetBlock;
    [SerializeField] private Sprite[] blockIcons;
    [SerializeField] private RectTransform targetCollectAnchor;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float collectAnimationDuration = 0.4f;

    public int moves;
    private int startingMoves;
    private int extraMovesGranted;
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
        collectedBlocks = new List<Block>();
        AssignTarget();
    }

    public void AssignTarget()
    {
        pendingWin = false;
         
        targetBlockType = Random.Range(0, blockIcons.Length);
        targetBlockCount = Random.Range(50, 80);
        currentTarget = targetBlockCount;
        moves = Random.Range(40, 55);
        startingMoves = moves;
        extraMovesGranted = 0;
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

    public int StartingMoves => startingMoves;

    public int TotalMovesGranted => startingMoves + extraMovesGranted;

    public int MovesUsed => Mathf.Max(0, TotalMovesGranted - moves);

    public void DecreaseMove(int aAmount=1)
    {
        moves--;
        UpdateUI();
    }

    public void AddMoves(int amount)
    {
        if (amount <= 0)
            return;

        moves += amount;
        extraMovesGranted += amount;
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
        AddMoves(5);
    }

    private void PlayTargetCollectAnimation(Block block)
    {
        if (block == null)
            return;

        var spriteRenderer = block.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        PlayTargetCollectAnimation(spriteRenderer.sprite, spriteRenderer.color, block.transform.position, 3f);
    }

    public void PlayTargetCollectAnimation(Sprite sprite, Color color, Vector3 worldPosition, float scaleMultiplier = 1f)
    {
        if (sprite == null)
            return;

        var canvas = targetCanvas != null ? targetCanvas : targetBlock != null ? targetBlock.canvas : null;
        if (canvas == null)
            return;

        var canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        var animObj = new GameObject("TargetCollectAnim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = animObj.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        var image = animObj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;

        var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint);
        rect.anchoredPosition = localPoint;
        rect.localScale = Vector3.one * scaleMultiplier;
        var startScale = rect.localScale;

        var targetRect = targetCollectAnchor != null ? targetCollectAnchor : targetBlock != null ? targetBlock.rectTransform : null;
        if (targetRect == null)
        {
            Object.Destroy(animObj);
            return;
        }

        var targetScreenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            targetRect.position);
        Vector2 targetLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            targetScreenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out targetLocal);

        var sequence = DOTween.Sequence();
        sequence.Join(rect.DOAnchorPos(targetLocal, collectAnimationDuration).SetEase(Ease.InCubic));
        sequence.Join(rect.DOScale(Mathf.Max(startScale.x * 0.3f, 0.3f), collectAnimationDuration).SetEase(Ease.OutCubic));
        sequence.OnComplete(() => Object.Destroy(animObj));
    }


}
