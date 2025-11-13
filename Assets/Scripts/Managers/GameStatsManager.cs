using UnityEngine;
using System;
using System.Collections.Generic;

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

    [Header("Tiempos por Nivel")]
    [SerializeField] private List<float> levelTimes = new List<float>();

    [Header("Semilla de Niveles")]
    [SerializeField] private string levelSeed = ""; // Ej: "1A2B3A4B5"

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
        levelTimes.Clear();
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
            currentStats.totalPlayTime = GetTotalAccumulatedTime();
            isTracking = false;
            UnityEngine.Debug.Log($"[GameStatsManager] Estadísticas finales registradas. Tiempo total: {FormatTime(currentStats.totalPlayTime)}");
        }
    }

    /// <summary>
    /// Resetea todas las estadísticas
    /// </summary>
    public void ResetStats()
    {
        currentStats = new GameStats();
        levelTimes.Clear();
        levelSeed = "";
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
            currentStats.totalPlayTime = GetTotalAccumulatedTime();
        }

        // 🔹 AGREGADO: Incluir la seed en las estadísticas
        currentStats.levelSeed = levelSeed;

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

    /// <summary>
    /// Registra el tiempo usado en un nivel específico
    /// </summary>
    /// <param name="timeUsed">Tiempo que se usó en el nivel (en segundos)</param>
    public void RegisterLevelTime(float timeUsed)
    {
        levelTimes.Add(timeUsed);
        UnityEngine.Debug.Log($"[GameStatsManager] Tiempo de nivel registrado: {FormatTime(timeUsed)} (Total de niveles: {levelTimes.Count})");
    }

    /// <summary>
    /// Establece la semilla de niveles generada
    /// </summary>
    /// <param name="seed">Semilla en formato "1A2B3A4B5"</param>
    public void SetLevelSeed(string seed)
    {
        levelSeed = seed;
        UnityEngine.Debug.Log($"[GameStatsManager] Semilla de niveles establecida: {levelSeed}");
    }

    /// <summary>
    /// Obtiene la semilla de niveles
    /// </summary>
    public string GetLevelSeed()
    {
        return levelSeed;
    }

    /// <summary>
    /// Obtiene el tiempo total acumulado de todos los niveles
    /// </summary>
    public float GetTotalAccumulatedTime()
    {
        float total = 0f;
        foreach (float time in levelTimes)
        {
            total += time;
        }
        return total;
    }

    /// <summary>
    /// Obtiene los tiempos individuales de cada nivel
    /// </summary>
    public List<float> GetLevelTimes()
    {
        return new List<float>(levelTimes); // Devolver una copia
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
        UnityEngine.Debug.Log($"Semilla de Niveles: {levelSeed}");
        UnityEngine.Debug.Log($"Tiempo Total Acumulado: {FormatTime(GetTotalAccumulatedTime())}");
        UnityEngine.Debug.Log($"Saltos: {stats.totalJumps}");
        UnityEngine.Debug.Log($"Niveles Completados: {stats.levelsCompleted}");
        
        if (levelTimes.Count > 0)
        {
            UnityEngine.Debug.Log("--- Tiempos por Nivel ---");
            for (int i = 0; i < levelTimes.Count; i++)
            {
                UnityEngine.Debug.Log($"  Nivel {i + 1}: {FormatTime(levelTimes[i])}");
            }
        }
        UnityEngine.Debug.Log("============================");
    }

    [ContextMenu("Debug - Simular Datos")]
    private void DebugSimulateData()
    {
        StartTracking();
        levelSeed = "1A2B3A4B5";
        currentStats.totalJumps = 247;
        currentStats.levelsCompleted = 5;
        
        // Simular tiempos de niveles
        levelTimes.Clear();
        levelTimes.Add(95.5f);   // Nivel 1: 1:35
        levelTimes.Add(110.2f);  // Nivel 2: 1:50
        levelTimes.Add(88.7f);   // Nivel 3: 1:28
        levelTimes.Add(105.3f);  // Nivel 4: 1:45
        levelTimes.Add(142.1f);  // Nivel 5: 2:22
        
        currentStats.totalPlayTime = GetTotalAccumulatedTime();
        
        UnityEngine.Debug.Log("[GameStatsManager] Datos simulados cargados");
        DebugShowStats();
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

    [Header("Semilla")]
    public string levelSeed = ""; // 🔹 AGREGADO: Seed de la run (ej: "1A2B3A4B5")

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
        levelSeed = ""; // 🔹 AGREGADO
    }
}