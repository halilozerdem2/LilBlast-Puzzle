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
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        InitializePools();
    }

    private void InitializePools()
    {
        if (particlePrefabs.Length == 0)
        {
            Debug.LogError("ObjectPool: Particle prefabs missing!");
            return;
        }

        for (int i = 0; i < particlePrefabs.Length; i++)
        {
            particlePools[i] = CreatePool(particlePrefabs[i]);
        }
    }

    private Queue<GameObject> CreatePool(GameObject prefab)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        for (int j = 0; j < poolSize; j++)
        {
            GameObject obj = Instantiate(prefab, pools.transform);
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
        GameObject obj = (pool.Count > 0) ? pool.Dequeue() : Instantiate(prefab, pools.transform);

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
}
