using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using LevelSystem;

/// <summary>
/// Gestor para guardar sesiones de juego en Supabase
/// </summary>
public class SupabaseGameSessionManager : MonoBehaviour
{
    private static SupabaseGameSessionManager _instance;
    public static SupabaseGameSessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SupabaseGameSessionManager");
                _instance = go.AddComponent<SupabaseGameSessionManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Supabase Configuration")]
    [SerializeField] private string supabaseUrl = "https://bkbhlkoafdvxrhmwpypp.supabase.co";
    [SerializeField] private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJrYmhsa29hZmR2eHJobXdweXBwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkwODUyNzgsImV4cCI6MjA3NDY2MTI3OH0.kYtonEdSaWC4nxa4jI1Pgb4rgKUE6lEdcGbikZFglrQ";

    // Eventos
    public event System.Action OnSaveSuccess;
    public event System.Action<string> OnSaveError;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Guarda una sesión de juego completada en Supabase
    /// </summary>
    /// <param name="userId">UID del usuario de Supabase</param>
    /// <param name="seed">Seed de la run (ej: 1A2B3A4B5)</param>
    /// <param name="durationInSeconds">Duración total en segundos</param>
    public void SaveGameSession(string userId, string seed, int durationInSeconds, System.Action onSuccess = null, System.Action<string> onError = null)
    {
        // Validaciones
        if (string.IsNullOrEmpty(userId))
        {
            UnityEngine.Debug.LogError("[SupabaseGameSession] Error: userId está vacío");
            onError?.Invoke("Usuario no válido");
            OnSaveError?.Invoke("Usuario no válido");
            return;
        }

        if (string.IsNullOrEmpty(seed))
        {
            UnityEngine.Debug.LogError("[SupabaseGameSession] Error: seed está vacía");
            onError?.Invoke("Seed no válida");
            OnSaveError?.Invoke("Seed no válida");
            return;
        }

        if (durationInSeconds <= 0)
        {
            UnityEngine.Debug.LogError("[SupabaseGameSession] Error: duración inválida");
            onError?.Invoke("Duración inválida");
            OnSaveError?.Invoke("Duración inválida");
            return;
        }

        UnityEngine.Debug.Log($"[SupabaseGameSession] Guardando sesión: User={userId}, Seed={seed}, Duration={durationInSeconds}s");

        StartCoroutine(SaveGameSessionCoroutine(userId, seed, durationInSeconds, onSuccess, onError));
    }

    private IEnumerator SaveGameSessionCoroutine(string userId, string seed, int durationInSeconds, System.Action onSuccess, System.Action<string> onError)
    {
        // Construir la URL del endpoint
        string url = $"{supabaseUrl}/rest/v1/game_sessions";

        // Obtener el access token del usuario
        string accessToken = "";
        if (SupabaseAuthManager.Instance != null)
        {
            accessToken = SupabaseAuthManager.Instance.AccessToken;
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            UnityEngine.Debug.LogError("[SupabaseGameSession] No hay access token disponible");
            onError?.Invoke("No hay sesión de usuario válida");
            OnSaveError?.Invoke("No hay sesión de usuario válida");
            yield break;
        }

        // Crear el objeto con los datos
        // completed_at se genera automáticamente con NOW() en Supabase si lo dejas como timestamp default
        // Pero lo enviaremos manualmente en formato ISO 8601
        string completedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        GameSessionData sessionData = new GameSessionData
        {
            user_id = userId,
            seed = seed,
            duration = durationInSeconds,
            completed_at = completedAt
        };

        string jsonData = JsonUtility.ToJson(sessionData);
        UnityEngine.Debug.Log($"[SupabaseGameSession] JSON a enviar: {jsonData}");

        // Crear la petición POST
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Headers requeridos
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Prefer", "return=minimal"); // No necesitamos que devuelva datos

        // Enviar petición
        yield return request.SendWebRequest();

        // Verificar resultado
        if (request.result == UnityWebRequest.Result.Success)
        {
            UnityEngine.Debug.Log("[SupabaseGameSession] ✅ Sesión guardada exitosamente en Supabase");
            onSuccess?.Invoke();
            OnSaveSuccess?.Invoke();
        }
        else
        {
            string errorMessage = request.downloadHandler.text;
            UnityEngine.Debug.LogError($"[SupabaseGameSession] ❌ Error al guardar sesión: {errorMessage}");

            // Intentar parsear el error
            try
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(errorMessage);
                string friendlyError = error.message ?? "Error al guardar la sesión";
                onError?.Invoke(friendlyError);
                OnSaveError?.Invoke(friendlyError);
            }
            catch
            {
                onError?.Invoke("Error al guardar la sesión en el servidor");
                OnSaveError?.Invoke("Error al guardar la sesión en el servidor");
            }
        }

        request.Dispose();
    }

    // ==================== CLASES PARA JSON ====================

    [System.Serializable]
    private class GameSessionData
    {
        public string user_id;
        public string seed;
        public int duration;
        public string completed_at;
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string message;
        public string hint;
        public string details;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug - Test Save Session")]
    private void DebugTestSaveSession()
    {
        if (!Application.isPlaying)
        {
            UnityEngine.Debug.LogWarning("[SupabaseGameSession] Solo funciona en modo Play");
            return;
        }

        // Test con datos ficticios
        string testUserId = SupabaseAuthManager.Instance != null && SupabaseAuthManager.Instance.IsLoggedIn
            ? SupabaseAuthManager.Instance.CurrentUserId
            : "test-user-id";

        SaveGameSession(
            testUserId,
            "1A2B3A4B5",
            300, // 5 minutos
            () => UnityEngine.Debug.Log("[Test] Guardado exitoso!"),
            (error) => UnityEngine.Debug.LogError($"[Test] Error: {error}")
        );
    }
#endif
}