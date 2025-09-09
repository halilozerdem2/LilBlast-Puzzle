using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private const string BASE_URL = "http://localhost:5113";

    public IEnumerator PostRequest<TRequest, TResponse>(string endpoint, TRequest data, Action<TResponse> onSuccess, Action<string> onError, Action<string> onWelcome = null)
    {
        string url = $"{BASE_URL}{endpoint}";
        string jsonData = JsonConvert.SerializeObject(data);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
            }
            else
            {
                try
                {
                    TResponse response = JsonConvert.DeserializeObject<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                    // If the response is a User, show welcome message
                    if (response is User user && onWelcome != null)
                    {
                        onWelcome.Invoke($"Welcome {user.Username}!");
                    }
                }
                catch (Exception ex)
                {
                    onError?.Invoke("Deserialization error: " + ex.Message);
                }
            }
        }
    }

    public IEnumerator PutRequestRaw<TResponse>(string endpoint, string jsonBody, Action<TResponse> onSuccess, Action<string> onError)
    {
        string url = $"{BASE_URL}{endpoint}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"UnityWebRequest Error: {request.error}, ResponseCode: {request.responseCode}, Text: {request.downloadHandler.text}");

                onError?.Invoke(request.error);
            }
            else
            {
                try
                {
                    Debug.Log("başarılı");
                    TResponse response = JsonConvert.DeserializeObject<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.Log("başarısız");
                    onError?.Invoke("Deserialization error: " + ex.Message);
                }
            }
        }
    
    }

    public IEnumerator GetRequest<TResponse>(string endpoint, Action<TResponse> onSuccess, Action<string> onError)
    {
        string url = $"{BASE_URL}{endpoint}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
            }
            else
            {
                try
                {
                    TResponse response = JsonConvert.DeserializeObject<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    onError?.Invoke("Deserialization error: " + ex.Message);
                }
            }
        }
    }
}