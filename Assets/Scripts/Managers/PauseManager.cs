using UnityEngine;

/// <summary>
/// Gestor centralizado de pausa del juego
/// Maneja pausas por Game Over, Level Complete, y menú de pausa
/// </summary>
public class PauseManager : MonoBehaviour
{
    private static PauseManager _instance;
    public static PauseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PauseManager");
                _instance = go.AddComponent<PauseManager>();
            }
            return _instance;
        }
    }

    [Header("Estado de Pausa")]
    [SerializeField] private bool isPaused = false;
    [SerializeField] private PauseReason currentPauseReason = PauseReason.None;

    // Propiedades públicas
    public bool IsPaused => isPaused;
    public PauseReason CurrentPauseReason => currentPauseReason;

    private void Awake()
    {
        // Singleton pattern - mantener solo una instancia
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Pausa el juego
    /// </summary>
    public void Pause(PauseReason reason)
    {
        if (isPaused)
        {
            UnityEngine.Debug.LogWarning($"[PauseManager] El juego ya estaba pausado por: {currentPauseReason}");
            return;
        }

        isPaused = true;
        currentPauseReason = reason;
        Time.timeScale = 0f;

        UnityEngine.Debug.Log($"[PauseManager] Juego pausado. Razón: {reason}");
    }

    /// <summary>
    /// Reanuda el juego
    /// </summary>
    public void Resume()
    {
        if (!isPaused)
        {
            UnityEngine.Debug.LogWarning("[PauseManager] El juego no estaba pausado");
            return;
        }

        isPaused = false;
        PauseReason previousReason = currentPauseReason;
        currentPauseReason = PauseReason.None;
        Time.timeScale = 1f;

        UnityEngine.Debug.Log($"[PauseManager] Juego reanudado. Razón previa: {previousReason}");
    }

    /// <summary>
    /// Fuerza la reanudación sin importar el estado
    /// </summary>
    public void ForceResume()
    {
        isPaused = false;
        currentPauseReason = PauseReason.None;
        Time.timeScale = 1f;
        UnityEngine.Debug.Log("[PauseManager] Juego forzado a reanudar");
    }

    /// <summary>
    /// Verifica si el juego está pausado por una razón específica
    /// </summary>
    public bool IsPausedBy(PauseReason reason)
    {
        return isPaused && currentPauseReason == reason;
    }
}

/// <summary>
/// Razones por las cuales el juego puede estar pausado
/// </summary>
public enum PauseReason
{
    None,           // No está pausado
    GameOver,       // Pantalla de Game Over
    LevelComplete,  // Nivel completado
    PauseMenu       // Menú de pausa (ESC)
}