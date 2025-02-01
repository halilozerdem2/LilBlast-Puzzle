using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    public GameObject[] particlePrefabs; // 5 farklı particle için
    public GameObject[] blockPrefabs; // 5 farklı blok için
    public int poolSize = 20;

    private Dictionary<int, Queue<GameObject>> particlePools = new Dictionary<int, Queue<GameObject>>();
    private Dictionary<int, Queue<GameObject>> blockPools = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < particlePrefabs.Length; i++)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = Instantiate(particlePrefabs[i]);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            particlePools.Add(i, pool);
        }

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = Instantiate(blockPrefabs[i]);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            blockPools.Add(i, pool);
        }
    }

    public GameObject GetParticleFromPool(int type, Vector3 position, Quaternion rotation)
    {
        if (particlePools.ContainsKey(type) && particlePools[type].Count > 0)
        {
            GameObject obj = particlePools[type].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            StartCoroutine(ReturnToPool(obj, particlePools[type],1f));
            return obj;
        }
        else
        {
            Debug.LogWarning("Particle pool is empty! Consider increasing pool size.");
            return null;
        }
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

    private System.Collections.IEnumerator ReturnToPool(GameObject obj, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
