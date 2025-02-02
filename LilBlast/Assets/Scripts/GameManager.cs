using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static event Action OnBlockSpawned;

    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;

    //public List<Block> _blocks;

    [SerializeField] private GameObject blastEffect;

    private GameState _state;

    private void Awake()
    {
        Instance = this;
        //_blocks = new List<Block>();
    }

    private void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    public void ChangeState(GameState newState)
    {
        _state = newState;
        switch (newState)
        {
            case GameState.GenerateLevel:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                BlockManager.Instance.SpawnBlocks();
                OnBlockSpawned?.Invoke();
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Blasting:
                GridManager.Instance.UpdateGrid();
                break;
            case GameState.Deadlock:
                StartCoroutine(ShuffleBoard());
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    
    private IEnumerator ShuffleBoard()
    {
        Debug.Log("Shuffling started...");

        List<Block> allBlocks = BlockManager.Instance._blocks.ToList();

        for (int i = 0; i < allBlocks.Count; i++)
        {
            int randomIndex = Random.Range(0, allBlocks.Count);

            if (i != randomIndex)
            {
                yield return StartCoroutine(SwapBlocksAnimated(allBlocks[i], allBlocks[randomIndex]));
            }
        }

        yield return new WaitForSeconds(0.5f); // Swap işlemlerinin bitmesini bekle

        BlockManager.Instance.FindAllNeighbours(); // Yeni komşulukları güncelle

        if (!BlockManager.Instance.HasValidMoves())
        {
            Debug.LogWarning("No valid moves found after shuffle. Retrying...");
            yield return StartCoroutine(ShuffleBoard()); // Yeniden shuffle et
        }
        else
        {
            Debug.Log("Shuffle successful!");
            ChangeState(GameState.WaitingInput);
        }
    }


    private IEnumerator SwapBlocksAnimated(Block blockA, Block blockB)
    {
        if (blockA == null || blockB == null)
        {
            Debug.LogWarning("Trying to swap a null block! Skipping swap.");
            yield break;
        }

        Vector3 startA = blockA.transform.position;
        Vector3 startB = blockB.transform.position;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            blockA.transform.position = Vector3.Lerp(startA, startB, t);
            blockB.transform.position = Vector3.Lerp(startB, startA, t);

            yield return null;
        }

        // Son pozisyonları tam olarak ayarla
        blockA.transform.position = startB;
        blockB.transform.position = startA;

        // Blokların bağlı olduğu node'ları değiştir
        Node tempNode = blockA.node;
        blockA.SetBlock(blockB.node);
        blockB.SetBlock(tempNode);
    }


    public enum GameState
    {
        GenerateLevel,
        SpawningBlocks,
        WaitingInput,
        Blasting,
        Deadlock,
        Win,
        Lose,
        Pause
    }
}