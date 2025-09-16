using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LilManager : MonoBehaviour
{
    public static LilManager Instance;

    [Header("Lil Settings")]
    public float actionDelay = 1.0f; // Lil’in bir hamleyi uygulamadan önce bekleme süresi
    public bool isActive = true;     // Lil devrede mi?

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Debug.Log("LilManager hazır!");
    }

    // --- 1) Hamle azaltma
    public void ReduceMoves(int amount = 1)
    {
        if (!isActive) return;
        Debug.Log($"Lil oyuncunun {amount} hamlesini azalttı!");
        // TODO: handler.moves -= amount; gibi bir işlem ekleyebilirsin
    }

    // --- 2) Tahtada rastgele blok engelleme
    public void BlockRandomTiles(int count = 3)
    {
        if (!isActive) return;
        Debug.Log($"Lil tahtada {count} rastgele bloğu engelledi!");
        // TODO: GridManager üzerinden rastgele node seçip kilitle
    }

    // --- 3) Power up kullanımını 3 hamle boyunca engelleme
    public void DisablePowerUps(int duration = 3)
    {
        if (!isActive) return;
        Debug.Log($"Lil power-up kullanımını {duration} hamle boyunca engelledi!");
        // TODO: PowerUpManager üzerinde flag koyabilirsin
    }

    // --- 4) Hedef blokları farklı renklere dönüştürme
    public void TransformTargetBlocks()
    {
        if (!isActive) return;
        Debug.Log("Lil tüm hedef blokları farklı renklere dönüştürdü!");
        // TODO: handler.collectedBlocks listesine bak, uygun blokları değiştir
    }

    // --- 5) Tahtadaki özel blokların bir kısmını yok etme
    public void DestroySpecialBlocks(int count = 2)
    {
        if (!isActive) return;
        Debug.Log($"Lil {count} özel bloğu yok etti!");
        // TODO: BlockManager.Instance içinden özel blokları bul ve yok et
    }

    // Basit test için Lil’in aksiyonlarını sırayla çalıştıran coroutine
    public IEnumerator TestLilActions()
    {
        yield return new WaitForSeconds(actionDelay);
        ReduceMoves();

        yield return new WaitForSeconds(actionDelay);
        BlockRandomTiles();

        yield return new WaitForSeconds(actionDelay);
        DisablePowerUps();

        yield return new WaitForSeconds(actionDelay);
        TransformTargetBlocks();

        yield return new WaitForSeconds(actionDelay);
        DestroySpecialBlocks();
    }
}
