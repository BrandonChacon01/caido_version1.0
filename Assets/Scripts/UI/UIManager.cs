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

    [Header("Scene Names")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu";

    private LevelTimer levelTimer;
    private PauseMenuController pauseMenuController;

    private void Start()
    {
        // Buscar el LevelTimer en la escena
        levelTimer = FindFirstObjectByType<LevelTimer>();

        if (levelTimer == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No se encontró LevelTimer en la escena");
        }

        // Buscar el PauseMenuController
        pauseMenuController = FindFirstObjectByType<PauseMenuController>();
    }

    /// <summary>
    /// Muestra el panel de Game Over y pausa el timer y el juego
    /// </summary>
    public void MostrarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            if (HealthBar != null) HealthBar.SetActive(false);
            if (HeatBar != null) HeatBar.SetActive(false);

            // 🔹 NUEVO: Pausar el juego completamente
            PauseManager.Instance.Pause(PauseReason.GameOver);

            // Notificar al PauseMenuController que estamos en Game Over
            if (pauseMenuController != null)
            {
                pauseMenuController.SetGameOverState(true);
            }

            // Pausar el timer
            if (levelTimer != null)
            {
                levelTimer.PauseTimer();

                // Mostrar el tiempo total acumulado de la run
                float currentLevelTime = levelTimer.TimeElapsed;
                float totalRunTime = 0f;

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
        // 🔹 NUEVO: Forzar reanudación antes de recargar
        PauseManager.Instance.ForceResume();

        // Reactivar elementos del HUD
        if (HealthBar != null) HealthBar.SetActive(true);
        if (HeatBar != null) HeatBar.SetActive(true);

        // Recargar la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Vuelve al menú principal (método principal con logs de prueba)
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("[UIManager] GoToMainMenu() llamado DESDE Game Over");

        // Forzar reanudación antes de cambiar de escena
        PauseManager.Instance.ForceResume();
        Debug.Log("[UIManager] ForceResume() OK, cargando escena " + mainMenuSceneName);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Vuelve al menú principal (alias para compatibilidad con referencias antiguas)
    /// </summary>
    public void VolverAlMenuPrincipal()
    {
        GoToMainMenu();
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

            // 🔹 NUEVO: Reanudar el juego
            PauseManager.Instance.Resume();

            // Notificar al PauseMenuController
            if (pauseMenuController != null)
            {
                pauseMenuController.SetGameOverState(false);
            }

            // Reanudar el timer
            if (levelTimer != null)
            {
                levelTimer.ResumeTimer();
            }
        }
    }
}