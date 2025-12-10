using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LilBlast.Backend
{
    [Serializable]
    public enum AuthProvider
    {
        GUEST,
        GOOGLE,
        FACEBOOK,
        EMAIL_PASSWORD
    }

    [Serializable]
    public class RegisterRequest
    {
        public string email;
        public string username;
        public string password;
    }

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string username;
        public string password;
    }

    [Serializable]
    public class GuestAuthRequest
    {
        public string deviceId;
    }

    [Serializable]
    public class OAuthRequest
    {
        public AuthProvider provider;
        public string oauthToken;
    }

    [Serializable]
    public class UpgradeGuestRequest
    {
        public string userId;
        public string email;
        public string username;
        public string password;
    }

    [Serializable]
    public class AuthResponse
    {
        public string userId;
        public string authToken;
        public string username;
    }

    [Serializable]
    public enum InventoryItemType
    {
        COINS,
        LIVES,
        SHUFFLE,
        POWERSHUFFLE,
        MANIPULATE,
        DESTROY
    }

    [Serializable]
    public class InventoryAdjustmentRequest
    {
        public string userId;
        public InventoryItemType itemType;
        public long amount;
    }

    [Serializable]
    public class InventoryResponse
    {
        public long coins;
        public int lives;
        public int shuffleCount;
        public int powerShuffleCount;
        public int manipulateCount;
        public int destroyCount;
    }

    [Serializable]
    public class ProfileResponse
    {
        public long totalAttempts;
        public long totalWins;
        public int threeStarCount;
        public long totalPowerupsUsed;
        public long totalScore;
        public int lastLevelReached;
    }

    [Serializable]
    public class SuccessResponse
    {
        public bool success;
    }

    [Serializable]
    internal class ErrorResponse
    {
        public string message;
        public string error;
    }

    public sealed class BackendError
    {
        public long StatusCode { get; }
        public string Message { get; }
        public string RawBody { get; }

        public BackendError(long statusCode, string message, string rawBody)
        {
            StatusCode = statusCode;
            Message = message;
            RawBody = rawBody;
        }
    }

    /// <summary>
    /// Lightweight HTTP helper for the Lil Games backend.
    /// </summary>
    public class AuthApiClient
    {
        private readonly string baseUrl;

        public AuthApiClient(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Debug.LogWarning("AuthApiClient baseUrl is empty. Requests will likely fail.");
                this.baseUrl = string.Empty;
            }
            else
            {
                this.baseUrl = baseUrl.TrimEnd('/');
            }
        }

        public IEnumerator Register(RegisterRequest payload, Action<AuthResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/auth/sign-in", payload, onSuccess, onError);
        }

        public IEnumerator Login(LoginRequest payload, Action<AuthResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/auth/login", payload, onSuccess, onError);
        }

        public IEnumerator LoginAsGuest(GuestAuthRequest payload, Action<AuthResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/auth/guest", payload, onSuccess, onError);
        }

        public IEnumerator LoginWithOAuth(OAuthRequest payload, Action<AuthResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/auth/oauth", payload, onSuccess, onError);
        }

        public IEnumerator UpgradeGuest(UpgradeGuestRequest payload, string authToken, Action<AuthResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/auth/upgrade-guest", payload, onSuccess, onError, authToken);
        }

        public IEnumerator ConsumeInventory(InventoryAdjustmentRequest payload, string authToken, Action<SuccessResponse> onSuccess, Action<BackendError> onError)
        {
            yield return Post("/inventory/consume", payload, onSuccess, onError, authToken);
        }

        public IEnumerator GetInventory(string userId, string authToken, Action<InventoryResponse> onSuccess, Action<BackendError> onError)
        {
            if (string.IsNullOrEmpty(userId))
            {
                onError?.Invoke(new BackendError(400, "Missing userId", string.Empty));
                yield break;
            }

            yield return Get($"/inventory/{userId}", onSuccess, onError, authToken);
        }

        public IEnumerator GetProfile(string userId, string authToken, Action<ProfileResponse> onSuccess, Action<BackendError> onError)
        {
            if (string.IsNullOrEmpty(userId))
            {
                onError?.Invoke(new BackendError(400, "Missing userId", string.Empty));
                yield break;
            }

            yield return Get($"/profile/{userId}", onSuccess, onError, authToken);
        }

        private IEnumerator Post<TPayload, TResponse>(string route, TPayload payload, Action<TResponse> onSuccess, Action<BackendError> onError, string authToken = null)
        {
            var url = BuildUrl(route);
            var json = payload != null ? JsonUtility.ToJson(payload) : "{}";

            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    var error = BuildError(request);
                    onError?.Invoke(error);
                    yield break;
                }

                try
                {
                    var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse backend response: {ex.Message}\n{request.downloadHandler.text}");
                    onError?.Invoke(new BackendError(request.responseCode, "Invalid server response", request.downloadHandler.text));
                }
            }
        }

        private IEnumerator Get<TResponse>(string route, Action<TResponse> onSuccess, Action<BackendError> onError, string authToken = null)
        {
            var url = BuildUrl(route);

            using (var request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    var error = BuildError(request);
                    onError?.Invoke(error);
                    yield break;
                }

                try
                {
                    var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse backend response: {ex.Message}\\n{request.downloadHandler.text}");
                    onError?.Invoke(new BackendError(request.responseCode, "Invalid server response", request.downloadHandler.text));
                }
            }
        }

        private string BuildUrl(string route)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return route;

            if (string.IsNullOrEmpty(route))
                return baseUrl;

            if (!route.StartsWith("/"))
                route = "/" + route;

            return baseUrl + route;
        }

        private BackendError BuildError(UnityWebRequest request)
        {
            var raw = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            var message = request.error;

            try
            {
                if (!string.IsNullOrEmpty(raw))
                {
                    var parsed = JsonUtility.FromJson<ErrorResponse>(raw);
                    if (parsed != null)
                    {
                        if (!string.IsNullOrEmpty(parsed.message))
                            message = parsed.message;
                        else if (!string.IsNullOrEmpty(parsed.error))
                            message = parsed.error;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return new BackendError(request.responseCode, message, raw);
        }
    }
}
