using LevelSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";   // Tu escena de juego
    [SerializeField] private string loadingSceneName = "Loading";    // La escena de carga (opcional)
    [SerializeField] private string videoIntroSceneName = "VideoIntro"; // Escena con el video

    [Header("Intro Settings")]
    [SerializeField] private bool useVideoIntro = true; // Toggle para usar video o ir directo
    [SerializeField] private bool useLevelSystem = true; // Usar el sistema de niveles

    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;

    [Header("Buttons")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnBackFromOptions;

    private void Awake()
    {
        if (btnPlay) btnPlay.onClick.AddListener(OnPlay);
        if (btnOptions) btnOptions.onClick.AddListener(ShowOptions);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuit);
        if (btnBackFromOptions) btnBackFromOptions.onClick.AddListener(ShowMain);

        ShowMain();
    }

    // Resetear el progreso al cargar el menú
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetProgress();
            UnityEngine.Debug.Log("[MainMenu] Progreso del LevelManager reseteado");
        }

        // Resetear estadísticas al volver al menú
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
            UnityEngine.Debug.Log("[MainMenu] Estadísticas reseteadas");
        }

        // 🔹 NUEVO: Verificar si hay sesión iniciada
        CheckAuthenticationStatus();
    }

    /// <summary>
    /// Verifica si el usuario está logueado y guarda sus datos
    /// </summary>
    private void CheckAuthenticationStatus()
    {
        if (SupabaseAuthManager.Instance != null)
        {
            bool isLoggedIn = SupabaseAuthManager.Instance.IsLoggedIn;

            if (isLoggedIn)
            {
                string displayName = SupabaseAuthManager.Instance.CurrentUserDisplayName;
                string userId = SupabaseAuthManager.Instance.CurrentUserId;

                UnityEngine.Debug.Log($"[MainMenu] Usuario logueado: {displayName} (ID: {userId})");

                // Guardar los datos en LoadingPayload para usarlos después
                LoadingPayload.UserDisplayName = displayName;
                LoadingPayload.UserId = userId;
                LoadingPayload.IsUserLoggedIn = true;
            }
            else
            {
                UnityEngine.Debug.Log("[MainMenu] Usuario no logueado (modo invitado)");
                LoadingPayload.UserDisplayName = "Invitado";
                LoadingPayload.UserId = "";
                LoadingPayload.IsUserLoggedIn = false;
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("[MainMenu] SupabaseAuthManager no encontrado");
            LoadingPayload.IsUserLoggedIn = false;
        }
    }

    private void ShowMain()
    {
        if (panelMain) panelMain.SetActive(true);
        if (panelOptions) panelOptions.SetActive(false);
        if (btnPlay) btnPlay.Select();
    }

    private void ShowOptions()
    {
        if (panelMain) panelMain.SetActive(false);
        if (panelOptions) panelOptions.SetActive(true);
        if (btnBackFromOptions) btnBackFromOptions.Select();
    }

    private void OnPlay()
    {
        // 🔹 MODIFICADO: Verificar y guardar estado de autenticación al dar Play
        CheckAuthenticationStatus();

        // Iniciar tracking de estadísticas
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
            GameStatsManager.Instance.StartTracking();
            UnityEngine.Debug.Log("[MainMenu] Tracking de estadísticas iniciado");
        }

        // Configurar el sistema de niveles
        if (useLevelSystem && LevelManager.Instance != null)
        {
            // Marcar que queremos usar el sistema de niveles
            LoadingPayload.UseLevelSystem = true;

            // Si hay video intro, cargarlo primero
            if (useVideoIntro && !string.IsNullOrEmpty(videoIntroSceneName))
            {
                UnityEngine.Debug.Log("[MainMenu] Cargando video intro (luego iniciará el sistema de niveles)...");
                LoadingPayload.NextScene = ""; // El video llamará al LevelManager
                SceneManager.LoadScene(videoIntroSceneName);
            }
            else
            {
                // Sin video, ir directo al Nivel 1
                UnityEngine.Debug.Log("[MainMenu] Iniciando sistema de niveles directamente...");
                LevelManager.Instance.StartGame();
            }
            return;
        }

        // Código original como fallback (sin sistema de niveles)
        LoadingPayload.UseLevelSystem = false;
        LoadingPayload.NextScene = gameSceneName;

        if (useVideoIntro && !string.IsNullOrEmpty(videoIntroSceneName))
        {
            UnityEngine.Debug.Log("[MainMenu] Cargando video intro...");
            SceneManager.LoadScene(videoIntroSceneName);
        }
        else if (!string.IsNullOrEmpty(loadingSceneName))
        {
            UnityEngine.Debug.Log("[MainMenu] Cargando pantalla de carga...");
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            UnityEngine.Debug.Log("[MainMenu] Cargando juego directamente...");
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[MainMenu] Salir (en Editor no se cierra).");
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (panelOptions && panelOptions.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMain();
        }
    }
}

// 🔹 MODIFICADO: Contenedor para pasar datos entre escenas (ahora incluye datos de autenticación)
public static class LoadingPayload
{
    public static string NextScene;
    public static bool UseLevelSystem;

    // Datos de autenticación
    public static bool IsUserLoggedIn = false;
    public static string UserDisplayName = "Invitado";
    public static string UserId = "";
}