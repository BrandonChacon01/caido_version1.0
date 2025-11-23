using LevelSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "SampleScene";   // Tu escena de juego
    [SerializeField] private string loadingSceneName = "Loading";    // La escena de carga (opcional)
    [SerializeField] private string videoIntroSceneName = "VideoIntro"; // Escena con el video

    [Header("Intro Settings")]
    [SerializeField] private bool useVideoIntro = true; // Toggle para usar video o ir directo
    [SerializeField] private bool useLevelSystem = true; // Usar el sistema de niveles

    [Header("Audio")]
    [Tooltip("AudioSource para la música de fondo del menú")]
    [SerializeField] private AudioSource backgroundMusicSource;

    [Tooltip("Clip de audio que se reproducirá como música de fondo")]
    [SerializeField] private AudioClip backgroundMusicClip;

    [Tooltip("Volumen de la música de fondo (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Tooltip("¿Reproducir la música en loop?")]
    [SerializeField] private bool loopMusic = true;

    [Tooltip("Duración del fade in de la música (segundos)")]
    [SerializeField] private float musicFadeInDuration = 1.5f;

    [Tooltip("Duración del fade out al salir del menú (segundos)")]
    [SerializeField] private float musicFadeOutDuration = 1f;

    [Header("Panels")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;

    [Header("Buttons")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnBackFromOptions;

    private bool isTransitioning = false;

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

        // Verificar si hay sesión iniciada
        CheckAuthenticationStatus();

        // Iniciar música de fondo
        InitializeBackgroundMusic();
    }

    /// <summary>
    /// Inicializa y reproduce la música de fondo del menú
    /// </summary>
    private void InitializeBackgroundMusic()
    {
        if (backgroundMusicSource == null)
        {
            UnityEngine.Debug.LogWarning("[MainMenu] No hay AudioSource asignado para la música de fondo");
            return;
        }

        if (backgroundMusicClip == null)
        {
            UnityEngine.Debug.LogWarning("[MainMenu] No hay AudioClip asignado para la música de fondo");
            return;
        }

        // Configurar el AudioSource
        backgroundMusicSource.clip = backgroundMusicClip;
        backgroundMusicSource.loop = loopMusic;
        backgroundMusicSource.volume = 0f; // Empezar en volumen 0 para el fade in

        // Reproducir música
        backgroundMusicSource.Play();

        // Iniciar fade in
        StartCoroutine(FadeInMusic(backgroundMusicSource, musicVolume, musicFadeInDuration));

        UnityEngine.Debug.Log($"[MainMenu] Música de fondo iniciada: {backgroundMusicClip.name}");
    }

    /// <summary>
    /// Fade in de la música
    /// </summary>
    private IEnumerator FadeInMusic(AudioSource audioSource, float targetVolume, float duration)
    {
        float elapsedTime = 0f;
        float startVolume = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    /// <summary>
    /// Fade out de la música
    /// </summary>
    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float elapsedTime = 0f;
        float startVolume = audioSource.volume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    /// <summary>
    /// Detiene la música con fade out
    /// </summary>
    private void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(backgroundMusicSource, musicFadeOutDuration));
        }
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
        if (isTransitioning) return;
        isTransitioning = true;

        // Verificar y guardar estado de autenticación al dar Play
        CheckAuthenticationStatus();

        // Iniciar tracking de estadísticas
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
            GameStatsManager.Instance.StartTracking();
            UnityEngine.Debug.Log("[MainMenu] Tracking de estadísticas iniciado");
        }

        // Detener música con fade out antes de cambiar de escena
        StartCoroutine(FadeOutAndLoadGame());
    }

    /// <summary>
    /// Hace fade out de la música y luego carga el juego
    /// </summary>
    private IEnumerator FadeOutAndLoadGame()
    {
        // Hacer fade out de la música
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(backgroundMusicSource, musicFadeOutDuration));
        }

        // Cargar la siguiente escena
        LoadGameScene();
    }

    /// <summary>
    /// Lógica para cargar la escena del juego
    /// </summary>
    private void LoadGameScene()
    {
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
        if (isTransitioning) return;
        isTransitioning = true;

        // Fade out antes de cerrar
        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>
    /// Hace fade out y luego cierra la aplicación
    /// </summary>
    private IEnumerator FadeOutAndQuit()
    {
        // Hacer fade out de la música
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(backgroundMusicSource, musicFadeOutDuration));
        }

#if UNITY_EDITOR
        UnityEngine.Debug.Log("[MainMenu] Salir (en Editor no se cierra).");
        isTransitioning = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (panelOptions && panelOptions.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ShowOptions();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test - Detener Música")]
    private void TestStopMusic()
    {
        if (Application.isPlaying)
        {
            StopBackgroundMusic();
        }
    }

    [ContextMenu("Test - Reiniciar Música")]
    private void TestRestartMusic()
    {
        if (Application.isPlaying)
        {
            StopBackgroundMusic();
            Invoke(nameof(InitializeBackgroundMusic), musicFadeOutDuration);
        }
    }
#endif
}

// Contenedor para pasar datos entre escenas (ahora incluye datos de autenticación)
public static class LoadingPayload
{
    public static string NextScene;
    public static bool UseLevelSystem;

    // Datos de autenticación
    public static bool IsUserLoggedIn = false;
    public static string UserDisplayName = "Invitado";
    public static string UserId = "";
}