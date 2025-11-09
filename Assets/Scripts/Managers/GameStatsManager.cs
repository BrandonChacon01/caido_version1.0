using UnityEngine;
using System;

/// <summary>
/// Gestor de estadísticas del juego
/// Registra tiempos, saltos, y otras métricas durante la partida
/// </summary>
public class GameStatsManager : MonoBehaviour
{
    private static GameStatsManager _instance;
    public static GameStatsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameStatsManager");
                _instance = go.AddComponent<GameStatsManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Estadísticas Actuales")]
    [SerializeField] private GameStats currentStats;

    private float sessionStartTime;
    private bool isTracking = false;

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
    /// Inicia el registro de estadísticas (llamar al comenzar una partida)
    /// </summary>
    public void StartTracking()
    {
        currentStats = new GameStats();
        sessionStartTime = Time.time;
        isTracking = true;
        UnityEngine.Debug.Log("[GameStatsManager] Iniciando registro de estadísticas...");
    }

    /// <summary>
    /// Detiene el registro de estadísticas
    /// </summary>
    public void StopTracking()
    {
        if (isTracking)
        {
            currentStats.totalPlayTime = Time.time - sessionStartTime;
            isTracking = false;
            UnityEngine.Debug.Log($"[GameStatsManager] Estadísticas finales registradas. Tiempo: {FormatTime(currentStats.totalPlayTime)}");
        }
    }

    /// <summary>
    /// Resetea todas las estadísticas
    /// </summary>
    public void ResetStats()
    {
        currentStats = new GameStats();
        isTracking = false;
        UnityEngine.Debug.Log("[GameStatsManager] Estadísticas reseteadas");
    }

    /// <summary>
    /// Obtiene las estadísticas actuales
    /// </summary>
    public GameStats GetCurrentStats()
    {
        if (isTracking)
        {
            currentStats.totalPlayTime = Time.time - sessionStartTime;
        }
        return currentStats;
    }

    // ==================== MÉTODOS PARA REGISTRAR EVENTOS ====================

    /// <summary>
    /// Registra un salto
    /// </summary>
    public void RegisterJump()
    {
        currentStats.totalJumps++;
    }

    /// <summary>
    /// Registra la finalización de un nivel
    /// </summary>
    public void RegisterLevelCompleted(int levelNumber)
    {
        currentStats.levelsCompleted++;
        UnityEngine.Debug.Log($"[GameStatsManager] Nivel {levelNumber} completado. Total: {currentStats.levelsCompleted}");
    }

    // 🔹 AGREGA MÁS MÉTODOS SEGÚN NECESITES EN EL FUTURO
    // Ejemplos:
    // public void RegisterDeath() { currentStats.totalDeaths++; }
    // public void RegisterCollectible() { currentStats.collectiblesFound++; }
    // public void RegisterDash() { currentStats.totalDashes++; }

    // ==================== UTILIDADES ====================

    /// <summary>
    /// Formatea el tiempo en formato MM:SS
    /// </summary>
    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

#if UNITY_EDITOR
    [ContextMenu("Debug - Mostrar Estadísticas")]
    private void DebugShowStats()
    {
        GameStats stats = GetCurrentStats();
        UnityEngine.Debug.Log("=== ESTADÍSTICAS ACTUALES ===");
        UnityEngine.Debug.Log($"Tiempo Total: {FormatTime(stats.totalPlayTime)}");
        UnityEngine.Debug.Log($"Saltos: {stats.totalJumps}");
        UnityEngine.Debug.Log($"Niveles Completados: {stats.levelsCompleted}");
        UnityEngine.Debug.Log("============================");
    }

    [ContextMenu("Debug - Simular Datos")]
    private void DebugSimulateData()
    {
        StartTracking();
        currentStats.totalJumps = 247;
        currentStats.levelsCompleted = 5;
        currentStats.totalPlayTime = 542f; // 9 minutos, 2 segundos
        UnityEngine.Debug.Log("[GameStatsManager] Datos simulados cargados");
    }
#endif
}

/// <summary>
/// Estructura de datos para las estadísticas del juego
/// </summary>
[Serializable]
public class GameStats
{
    [Header("Tiempo")]
    public float totalPlayTime = 0f;

    [Header("Progreso")]
    public int levelsCompleted = 0;

    [Header("Acciones")]
    public int totalJumps = 0;

    // 🔹 AGREGA MÁS CAMPOS SEGÚN NECESITES EN EL FUTURO
    // public int totalDeaths = 0;
    // public int totalDashes = 0;
    // public int collectiblesFound = 0;
    // public float damageTaken = 0f;

    public GameStats()
    {
        totalPlayTime = 0f;
        levelsCompleted = 0;
        totalJumps = 0;
    }
}