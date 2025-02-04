using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneEventManager : MonoBehaviour
{
    public static event Action<int> OnSceneLoaded; // Build index parametre olarak gelecek

    private void Awake()
    {
        // Sahne değiştiğinde bu event çalışacak
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int buildIndex = scene.buildIndex;
        Debug.Log($"Sahne Yüklendi: {scene.name} (Index: {buildIndex})");

        // Eventi tetikle
        OnSceneLoaded?.Invoke(buildIndex);
    }
}
