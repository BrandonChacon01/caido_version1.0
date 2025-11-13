using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;
using System.Text;

/// <summary>
/// Gestor de autenticación con Supabase usando HTTP directo
/// Maneja login, logout y persistencia de sesión
/// </summary>
public class SupabaseAuthManager : MonoBehaviour
{
    private static SupabaseAuthManager _instance;
    public static SupabaseAuthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SupabaseAuthManager");
                _instance = go.AddComponent<SupabaseAuthManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Supabase Configuration")]
    [SerializeField] private string supabaseUrl = "https://bkbhlkoafdvxrhmwpypp.supabase.co";
    [SerializeField] private string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJrYmhsa29hZmR2eHJobXdweXBwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkwODUyNzgsImV4cCI6MjA3NDY2MTI3OH0.kYtonEdSaWC4nxa4jI1Pgb4rgKUE6lEdcGbikZFglrQ";

    [Header("User State")]
    [SerializeField] private bool isLoggedIn = false;
    [SerializeField] private string currentUserEmail = "";
    [SerializeField] private string currentUserDisplayName = "";
    [SerializeField] private string currentUserId = "";
    [SerializeField] private string accessToken = "";

    // Eventos para notificar cambios de estado
    public event System.Action<bool> OnAuthStateChanged;
    public event System.Action<string> OnLoginSuccess;
    public event System.Action<string> OnLoginError;
    public event System.Action OnLogoutSuccess;

    // Propiedades públicas
    public bool IsLoggedIn => isLoggedIn;
    public string CurrentUserEmail => currentUserEmail;
    public string CurrentUserDisplayName => currentUserDisplayName;
    public string CurrentUserId => currentUserId;
    public string AccessToken => accessToken;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Intentar restaurar sesión al iniciar
        LoadSavedSession();
    }

    /// <summary>
    /// Inicia sesión con email y contraseña
    /// </summary>
    public void Login(string email, string password)
    {
        StartCoroutine(LoginCoroutine(email, password));
    }

    private IEnumerator LoginCoroutine(string email, string password)
    {
        UnityEngine.Debug.Log($"[SupabaseAuth] Intentando login con: {email}");

        // Construir la URL del endpoint de auth
        string url = $"{supabaseUrl}/auth/v1/token?grant_type=password";

        // Crear el JSON del body
        string jsonData = JsonUtility.ToJson(new LoginRequest
        {
            email = email,
            password = password
        });

        // Crear la petición
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Headers requeridos por Supabase
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseAnonKey);

        // Enviar petición
        yield return request.SendWebRequest();

        // Verificar resultado
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                // Parsear respuesta
                string responseText = request.downloadHandler.text;
                UnityEngine.Debug.Log($"[SupabaseAuth] Respuesta recibida: {responseText}");

                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);

                // Guardar datos del usuario
                currentUserEmail = response.user.email;
                currentUserDisplayName = string.IsNullOrEmpty(response.user.user_metadata.display_name)
                    ? response.user.email.Split('@')[0]
                    : response.user.user_metadata.display_name;
                currentUserId = response.user.id;
                accessToken = response.access_token;
                isLoggedIn = true;

                // Guardar sesión
                SaveSession();

                // Notificar éxito
                OnLoginSuccess?.Invoke(currentUserDisplayName);
                OnAuthStateChanged?.Invoke(true);

                UnityEngine.Debug.Log($"[SupabaseAuth] ✅ Login exitoso: {currentUserDisplayName}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SupabaseAuth] Error al parsear respuesta: {e.Message}");
                OnLoginError?.Invoke("Error al procesar la respuesta del servidor");
            }
        }
        else
        {
            string errorMessage = request.downloadHandler.text;
            UnityEngine.Debug.LogError($"[SupabaseAuth] ❌ Error en login: {errorMessage}");

            // Intentar parsear el error de Supabase
            try
            {
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(errorMessage);
                OnLoginError?.Invoke(error.error_description ?? error.msg ?? "Error de autenticación");
            }
            catch
            {
                OnLoginError?.Invoke("Error al iniciar sesión. Verifica tus credenciales.");
            }
        }

        request.Dispose();
    }

    /// <summary>
    /// Cierra la sesión actual
    /// </summary>
    public void Logout()
    {
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        UnityEngine.Debug.Log("[SupabaseAuth] Cerrando sesión...");

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Construir URL del endpoint de logout
            string url = $"{supabaseUrl}/auth/v1/logout";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();

            // Headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("[SupabaseAuth] Logout exitoso en servidor");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[SupabaseAuth] Error en logout del servidor: {request.error}");
            }

            request.Dispose();
        }

        // Limpiar datos de sesión localmente (siempre, aunque falle el logout en servidor)
        currentUserEmail = "";
        currentUserDisplayName = "";
        currentUserId = "";
        accessToken = "";
        isLoggedIn = false;

        // Borrar sesión guardada
        ClearSavedSession();

        // Notificar
        OnLogoutSuccess?.Invoke();
        OnAuthStateChanged?.Invoke(false);

        UnityEngine.Debug.Log("[SupabaseAuth] ✅ Sesión cerrada localmente");
    }

    /// <summary>
    /// Guarda la sesión en PlayerPrefs
    /// </summary>
    private void SaveSession()
    {
        PlayerPrefs.SetString("SupabaseAuth_Email", currentUserEmail);
        PlayerPrefs.SetString("SupabaseAuth_DisplayName", currentUserDisplayName);
        PlayerPrefs.SetString("SupabaseAuth_UserId", currentUserId);
        PlayerPrefs.SetString("SupabaseAuth_AccessToken", accessToken);
        PlayerPrefs.SetInt("SupabaseAuth_IsLoggedIn", isLoggedIn ? 1 : 0);
        PlayerPrefs.Save();

        UnityEngine.Debug.Log("[SupabaseAuth] Sesión guardada localmente");
    }

    /// <summary>
    /// Carga la sesión guardada desde PlayerPrefs
    /// </summary>
    private void LoadSavedSession()
    {
        if (PlayerPrefs.HasKey("SupabaseAuth_IsLoggedIn"))
        {
            isLoggedIn = PlayerPrefs.GetInt("SupabaseAuth_IsLoggedIn") == 1;

            if (isLoggedIn)
            {
                currentUserEmail = PlayerPrefs.GetString("SupabaseAuth_Email", "");
                currentUserDisplayName = PlayerPrefs.GetString("SupabaseAuth_DisplayName", "");
                currentUserId = PlayerPrefs.GetString("SupabaseAuth_UserId", "");
                accessToken = PlayerPrefs.GetString("SupabaseAuth_AccessToken", "");

                UnityEngine.Debug.Log($"[SupabaseAuth] Sesión restaurada: {currentUserDisplayName}");
                OnAuthStateChanged?.Invoke(true);
            }
        }
    }

    /// <summary>
    /// Elimina la sesión guardada
    /// </summary>
    private void ClearSavedSession()
    {
        PlayerPrefs.DeleteKey("SupabaseAuth_Email");
        PlayerPrefs.DeleteKey("SupabaseAuth_DisplayName");
        PlayerPrefs.DeleteKey("SupabaseAuth_UserId");
        PlayerPrefs.DeleteKey("SupabaseAuth_AccessToken");
        PlayerPrefs.DeleteKey("SupabaseAuth_IsLoggedIn");
        PlayerPrefs.Save();

        UnityEngine.Debug.Log("[SupabaseAuth] Sesión local eliminada");
    }

    // ==================== CLASES PARA JSON ====================

    [System.Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public User user;
    }

    [System.Serializable]
    private class User
    {
        public string id;
        public string email;
        public UserMetadata user_metadata;
    }

    [System.Serializable]
    private class UserMetadata
    {
        public string display_name;
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string error;
        public string error_description;
        public string msg;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug - Mostrar Estado")]
    private void DebugShowState()
    {
        UnityEngine.Debug.Log("=== ESTADO DE AUTENTICACIÓN ===");
        UnityEngine.Debug.Log($"Logged In: {isLoggedIn}");
        UnityEngine.Debug.Log($"Email: {currentUserEmail}");
        UnityEngine.Debug.Log($"Display Name: {currentUserDisplayName}");
        UnityEngine.Debug.Log($"User ID: {currentUserId}");
        UnityEngine.Debug.Log($"Has Token: {!string.IsNullOrEmpty(accessToken)}");
        UnityEngine.Debug.Log("================================");
    }

    [ContextMenu("Debug - Limpiar Sesión Local")]
    private void DebugClearSession()
    {
        ClearSavedSession();
        currentUserEmail = "";
        currentUserDisplayName = "";
        currentUserId = "";
        accessToken = "";
        isLoggedIn = false;
        UnityEngine.Debug.Log("[SupabaseAuth] Sesión limpiada manualmente");
    }
#endif
}