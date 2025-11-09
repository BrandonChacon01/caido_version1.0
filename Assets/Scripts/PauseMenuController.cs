using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controlador del menú de pausa
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel del menú de pausa")]
    public GameObject pauseMenuPanel;

    [Header("Settings")]
    [Tooltip("Tecla para pausar/despausar")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Scene Names")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu";

    private bool isInGameOver = false;
    private bool isLevelComplete = false;

    private void Start()
    {
        // Asegurarse de que el menú de pausa esté oculto al inicio
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Solo permitir pausar con ESC si no estamos en Game Over ni nivel completado
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isInGameOver && !isLevelComplete)
            {
                TogglePause();
            }
        }
    }

    /// <summary>
    /// Alterna entre pausar y despausar
    /// </summary>
    public void TogglePause()
    {
        if (PauseManager.Instance.IsPausedBy(PauseReason.PauseMenu))
        {
            Resume();
        }
        else if (!PauseManager.Instance.IsPaused)
        {
            Pause();
        }
    }

    /// <summary>
    /// Pausa el juego y muestra el menú
    /// </summary>
    public void Pause()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        PauseManager.Instance.Pause(PauseReason.PauseMenu);
        UnityEngine.Debug.Log("[PauseMenuController] Menú de pausa activado");
    }

    /// <summary>
    /// Reanuda el juego y oculta el menú
    /// </summary>
    public void Resume()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        PauseManager.Instance.Resume();
        UnityEngine.Debug.Log("[PauseMenuController] Juego reanudado desde menú de pausa");
    }

    /// <summary>
    /// Vuelve al menú principal
    /// </summary>
    public void GoToMainMenu()
    {
        // Forzar reanudación antes de cambiar de escena
        PauseManager.Instance.ForceResume();

        UnityEngine.Debug.Log("[PauseMenuController] Volviendo al menú principal...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Marca que estamos en Game Over (para deshabilitar pausa con ESC)
    /// </summary>
    public void SetGameOverState(bool state)
    {
        isInGameOver = state;
    }

    /// <summary>
    /// Marca que el nivel está completo (para deshabilitar pausa con ESC)
    /// </summary>
    public void SetLevelCompleteState(bool state)
    {
        isLevelComplete = state;
    }
}