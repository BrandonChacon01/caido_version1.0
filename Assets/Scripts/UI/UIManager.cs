using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using LevelSystem;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelGameOver;

    [Header("HUD Elements")]
    public GameObject HealthBar;
    public GameObject HeatBar;

    [Header("Game Over UI")]
    [Tooltip("Texto que muestra el tiempo transcurrido en Game Over")]
    public TextMeshProUGUI runTimeText;

    private LevelTimer levelTimer;

    private void Start()
    {
        // Buscar el LevelTimer en la escena
        levelTimer = FindFirstObjectByType<LevelTimer>();

        if (levelTimer == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No se encontró LevelTimer en la escena");
        }
    }

    /// <summary>
    /// Muestra el panel de Game Over y pausa el timer
    /// </summary>
    public void MostrarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            if (HealthBar != null) HealthBar.SetActive(false);
            if (HeatBar != null) HeatBar.SetActive(false);

            // Pausar el timer
            if (levelTimer != null)
            {
                levelTimer.PauseTimer();

                // 🔹 MODIFICADO: Mostrar el tiempo total acumulado de la run
                float currentLevelTime = levelTimer.TimeElapsed;
                float totalRunTime = 0f;

                // Obtener el tiempo total acumulado + el tiempo del nivel actual
                if (GameStatsManager.Instance != null)
                {
                    totalRunTime = GameStatsManager.Instance.GetTotalAccumulatedTime() + currentLevelTime;
                }
                else
                {
                    totalRunTime = currentLevelTime;
                }

                if (runTimeText != null)
                {
                    runTimeText.text = $"Tiempo de run: {GameStatsManager.FormatTime(totalRunTime)}";
                }

                UnityEngine.Debug.Log($"[UIManager] Game Over - Tiempo nivel actual: {GameStatsManager.FormatTime(currentLevelTime)} | Tiempo total run: {GameStatsManager.FormatTime(totalRunTime)}");
            }
        }
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void ReiniciarNivel()
    {
        // Reactivar elementos del HUD
        if (HealthBar != null) HealthBar.SetActive(true);
        if (HeatBar != null) HeatBar.SetActive(true);

        // Recargar la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Vuelve al menú principal
    /// </summary>
    public void VolverAlMenuPrincipal()
    {
        // Nota: El LevelManager y GameStatsManager se resetearán al cargar el MainMenu
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Oculta el panel de Game Over y reanuda el timer
    /// </summary>
    public void OcultarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);

            if (HealthBar != null) HealthBar.SetActive(true);
            if (HeatBar != null) HeatBar.SetActive(true);

            // Reanudar el timer
            if (levelTimer != null)
            {
                levelTimer.ResumeTimer();
            }
        }
    }
}