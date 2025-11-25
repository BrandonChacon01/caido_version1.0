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

    [Header("Game Over UI Buttons")]
    [Tooltip("Botón para iniciar una nueva run directamente desde el Game Over")]
    public UnityEngine.UI.Button btnStartNewRun;

    private void Start()
    {
        levelTimer = FindFirstObjectByType<LevelTimer>();

        if (levelTimer == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No se encontró LevelTimer en la escena");
        }
        else
        {
            levelTimer.OnTimeUp += OnTimerExpired;
            UnityEngine.Debug.Log("[UIManager] ✅ Suscrito al evento OnTimeUp del LevelTimer");
        }

        pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        if (btnStartNewRun != null)
            btnStartNewRun.onClick.AddListener(IniciarNuevaRun);
    }

    private void OnTimerExpired()
    {
        UnityEngine.Debug.Log("[UIManager] ⏰ ¡Tiempo agotado! Mostrando Game Over");
        MostrarPanelGameOver();
    }

    private void OnDestroy()
    {
        if (levelTimer != null)
        {
            levelTimer.OnTimeUp -= OnTimerExpired;
            UnityEngine.Debug.Log("[UIManager] Desuscrito del evento OnTimeUp");
        }
    }

    public void MostrarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            if (HealthBar != null) HealthBar.SetActive(false);
            if (HeatBar != null) HeatBar.SetActive(false);

            PauseManager.Instance.Pause(PauseReason.GameOver);

            if (pauseMenuController != null)
            {
                pauseMenuController.SetGameOverState(true);
            }

            if (levelTimer != null)
            {
                levelTimer.PauseTimer();

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

    public void ReiniciarNivel()
    {
        PauseManager.Instance.ForceResume();

        if (HealthBar != null) HealthBar.SetActive(true);
        if (HeatBar != null) HeatBar.SetActive(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        UnityEngine.Debug.Log("[UIManager] GoToMainMenu() llamado DESDE Game Over");

        PauseManager.Instance.ForceResume();
        UnityEngine.Debug.Log("[UIManager] ForceResume() OK, cargando escena " + mainMenuSceneName);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void VolverAlMenuPrincipal()
    {
        GoToMainMenu();
    }

    public void OcultarPanelGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);

            if (HealthBar != null) HealthBar.SetActive(true);
            if (HeatBar != null) HeatBar.SetActive(true);

            PauseManager.Instance.Resume();

            if (pauseMenuController != null)
            {
                pauseMenuController.SetGameOverState(false);
            }

            if (levelTimer != null)
            {
                levelTimer.ResumeTimer();
            }
        }
    }

    /// <summary>
    /// Inicia una nueva run (como si presionaras Jugar en MainMenu) directamente desde Game Over.
    /// No regresa a MainMenu.
    /// </summary>
    public void IniciarNuevaRun()
    {
        // 1. OCULTAR Panel GameOver y HUD
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (HealthBar != null) HealthBar.SetActive(true);
        if (HeatBar != null) HeatBar.SetActive(true);

        // 2. REANUDAR EL JUEGO (IMPORTANTE)
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ForceResume();
        }
        Time.timeScale = 1f;

        // 3. RESETEAR progreso e iniciar tracking estadístico igual que MainMenu
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetProgress();
            UnityEngine.Debug.Log("[UIManager][NuevaRun] Progreso del LevelManager reseteado");
        }

        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
            UnityEngine.Debug.Log("[UIManager][NuevaRun] Estadísticas reseteadas");
            GameStatsManager.Instance.StartTracking();
            UnityEngine.Debug.Log("[UIManager][NuevaRun] Tracking de estadísticas iniciado");
        }

        if (SupabaseAuthManager.Instance != null)
        {
            // Aquí puedes poner tu lógica de autenticación si la requieres
        }

        LoadingPayload.UseLevelSystem = true;
        LevelRandomizer.Instance.ResetSelection();
        LevelRandomizer.Instance.GenerateRandomLevelSequence();

        SceneManager.LoadScene("VideoIntro");
    }
}