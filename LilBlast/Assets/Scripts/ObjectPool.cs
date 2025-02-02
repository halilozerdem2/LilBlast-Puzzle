using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    public GameObject[] particlePrefabs;
    public GameObject[] blockPrefabs;
    public AudioClip[] audioClips; // Ses efektleri için
    public int poolSize = 10;

    private Dictionary<int, Queue<GameObject>> particlePools = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, Queue<GameObject>> blockPools = new Dictionary<int, Queue<GameObject>>();
    private AudioSource audioSource;

    void Awake()
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

        audioSource = gameObject.AddComponent<AudioSource>();
        InitializePools();
    }

    void InitializePools()
    {
        if (particlePrefabs.Length == 0 || blockPrefabs.Length == 0)
        {
            Debug.LogError("ObjectPool: Prefabs are missing!");
            return;
        }

        for (int i = 0; i < particlePrefabs.Length; i++)
        {
            particlePools[i] = CreatePool(particlePrefabs[i]);
        }

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            blockPools[i] = CreatePool(blockPrefabs[i]);
        }
    }

    private Queue<GameObject> CreatePool(GameObject prefab)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        for (int j = 0; j < poolSize; j++)
        {
            GameObject obj = Instantiate(prefab, transform);
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

        PlaySound(type); // Ses efekti çal
        return obj;  
    }

    
    private GameObject GetObjectFromPool(Queue<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            Debug.LogWarning("Object pool is empty! Creating a new instance.");
            obj = Instantiate(prefab, transform);
        }

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

    public GameObject GetBlockFromPool(int type, Vector3 position, Quaternion rotation)
    {
        if (blockPools.ContainsKey(type) && blockPools[type].Count > 0)
        {
            GameObject obj = blockPools[type].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            Debug.LogWarning("Block pool is empty! Consider increasing pool size.");
            return null;
        }
    }

}
