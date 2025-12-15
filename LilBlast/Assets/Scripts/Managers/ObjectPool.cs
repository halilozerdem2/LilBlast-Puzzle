using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [Header("Prefabs")]
    public GameObject[] particlePrefabs;
    public GameObject[] blockPrefabs;

    [Header("Audio")]
    public AudioClip[] audioClips;
    public int poolSize = 10;
    public Transform pools;

    private Dictionary<int, Queue<GameObject>> particlePools = new Dictionary<int, Queue<GameObject>>();
    private readonly Dictionary<int, Queue<Block>> blockPools = new Dictionary<int, Queue<Block>>();
    private readonly Dictionary<int, GameObject> blockPrefabLookup = new Dictionary<int, GameObject>();
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (pools == null)
            pools = transform;
        audioSource = gameObject.AddComponent<AudioSource>();
        InitializePools();
    }

    private void InitializePools()
    {
        if (particlePrefabs.Length == 0)
        {
            Debug.LogError("ObjectPool: Particle prefabs missing!");
        }
        else
        {
            for (int i = 0; i < particlePrefabs.Length; i++)
            {
                particlePools[i] = CreatePool(particlePrefabs[i]);
            }
        }

        if (blockPrefabs != null)
        {
            for (int i = 0; i < blockPrefabs.Length; i++)
            {
                if (blockPrefabs[i] == null) continue;
                RegisterBlockPrefab(i, blockPrefabs[i]);
            }
        }
    }

    private Queue<GameObject> CreatePool(GameObject prefab)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        var parent = pools != null ? pools : transform;
        for (int j = 0; j < poolSize; j++)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        return pool;
    }

    public GameObject GetParticleFromPool(int type, Vector3 position, Quaternion rotation)
    {
        if (!particlePools.ContainsKey(type))
        {
            Debug.LogWarning($"Particle pool of type {type} does not exist!");
            return null;
        }

        GameObject obj = GetObjectFromPool(particlePools[type], particlePrefabs[type], position, rotation);
        if (obj == null) return null;

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPool(obj, particlePools[type], 1f));
        }

        // 🔉 Ses efekti çal
        AudioManager.Instance.PlaySFX(type);
        return obj;
    }

    private GameObject GetObjectFromPool(Queue<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;
        while (pool.Count > 0 && obj == null)
            obj = pool.Dequeue();

        var parent = pools != null ? pools : transform;
        if (obj == null)
            obj = Instantiate(prefab, parent);
        else
            obj.transform.SetParent(parent);

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    public void PlaySound(int type)
    {
        if (audioClips != null && type < audioClips.Length && audioClips[type] != null)
        {
            audioSource.PlayOneShot(audioClips[type]);
        }
    }

    private System.Collections.IEnumerator ReturnToPool(GameObject obj, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public void RegisterBlockPrefab(int index, GameObject prefab)
    {
        if (prefab == null)
            return;

        blockPrefabLookup[index] = prefab;
        if (!blockPools.ContainsKey(index))
            blockPools[index] = new Queue<Block>();
    }

    public Block GetBlockFromPool(int index, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool: Requested block prefab is null.");
            return null;
        }

        RegisterBlockPrefab(index, prefab);

        var pool = blockPools[index];
        Block blockInstance = null;

        while (pool.Count > 0 && blockInstance == null)
        {
            blockInstance = pool.Dequeue();
        }

        if (blockInstance == null)
        {
            var obj = Instantiate(prefab, position, rotation, parent);
            blockInstance = obj.GetComponent<Block>();
        }

        PrepareBlockInstance(blockInstance, index, position, rotation, parent);
        return blockInstance;
    }

    public void ReturnBlockToPool(Block block)
    {
        if (block == null)
            return;

        var poolIndex = block.poolIndex;
        if (poolIndex < 0 || !blockPools.TryGetValue(poolIndex, out var pool))
        {
            Destroy(block.gameObject);
            return;
        }

        if (block.node != null)
        {
            if (block.node.OccupiedBlock == block)
                block.node.OccupiedBlock = null;
            block.node = null;
        }

        block.StopAllCoroutines();
        block.ResetVisualState();
        block.SetBlocksInteractable(false);
        block.gameObject.SetActive(false);
        block.transform.SetParent(pools != null ? pools : transform);
        pool.Enqueue(block);
    }

    private void PrepareBlockInstance(Block blockInstance, int index, Vector3 position, Quaternion rotation, Transform parent)
    {
        blockInstance.poolIndex = index;
        blockInstance.node = null; // ensure no stale node reference from previous scene
        blockInstance.isBeingDestroyed = false;
        blockInstance.isBlastable = false;
        blockInstance.group?.Clear();
        blockInstance.SetBlocksInteractable(true);
        blockInstance.ResetVisualState();

        var t = blockInstance.transform;
        t.SetParent(parent);
        t.position = position;
        t.rotation = rotation;
        blockInstance.gameObject.SetActive(true);
    }
}
