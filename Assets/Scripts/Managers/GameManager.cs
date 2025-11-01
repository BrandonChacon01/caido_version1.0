using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager - Singleton que controla el estado global del juego
/// Mantiene información del nivel actual y configuración entre escenas
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    [Header("Configuración de Niveles")]
    [Tooltip("Nivel actual del jugador (1-5)")]
    public int CurrentLevel = 1;

    [Tooltip("Número total de niveles en el juego")]
    public int TotalLevels = 5;

    [Header("Configuración del Jugador")]
    [Tooltip("Prefab del jugador que se spawneará en cada nivel")]
    public GameObject PlayerPrefab;

    [Tooltip("Posición de spawn inicial del jugador")]
    public Vector3 PlayerSpawnPosition = new Vector3(-8f, 0f, 0f);

    [Header("Estado del Juego")]
    public bool IsGamePaused = false;

    // Eventos para comunicación entre sistemas
    public delegate void LevelCompleted(int levelNumber);
    public static event LevelCompleted OnLevelCompleted;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Inicialización
        Debug.Log($"[GameManager] Iniciado - Nivel Actual: {CurrentLevel}");
    }

    /// <summary>
    /// Carga el siguiente nivel de manera procedural
    /// </summary>
    public void LoadNextLevel()
    {
        CurrentLevel++;

        if (CurrentLevel > TotalLevels)
        {
            // Juego completado - volver al menú principal
            Debug.Log("[GameManager] ¡Juego Completado! Volviendo al menú principal...");
            CurrentLevel = 1; // Reiniciar para próxima partida
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Cargar el siguiente nivel
            Debug.Log($"[GameManager] Cargando Nivel {CurrentLevel}...");
            string sceneName = $"Level{CurrentLevel}";
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Inicia el juego desde el nivel 1
    /// Llamado desde el MainMenu
    /// </summary>
    public void StartNewGame()
    {
        CurrentLevel = 1;
        Debug.Log("[GameManager] Nueva partida iniciada");
        LoadLevel(1);
    }

    /// <summary>
    /// Carga un nivel específico
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > TotalLevels)
        {
            Debug.LogError($"[GameManager] Nivel {levelNumber} fuera de rango (1-{TotalLevels})");
            return;
        }

        CurrentLevel = levelNumber;
        string sceneName = $"Level{levelNumber}";
        Debug.Log($"[GameManager] Cargando {sceneName}...");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Marca el nivel actual como completado
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log($"[GameManager] ¡Nivel {CurrentLevel} completado!");
        OnLevelCompleted?.Invoke(CurrentLevel);

        // Pequeña pausa antes de cargar el siguiente nivel
        Invoke(nameof(LoadNextLevel), 2f);
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log($"[GameManager] Reiniciando Nivel {CurrentLevel}...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Vuelve al menú principal
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[GameManager] Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Pausa/Despausa el juego
    /// </summary>
    public void TogglePause()
    {
        IsGamePaused = !IsGamePaused;
        Time.timeScale = IsGamePaused ? 0f : 1f;
        Debug.Log($"[GameManager] Juego {(IsGamePaused ? "Pausado" : "Reanudado")}");
    }

    private void OnDestroy()
    {
        // Asegurar que el tiempo vuelva a la normalidad al destruirse
        Time.timeScale = 1f;
    }
}