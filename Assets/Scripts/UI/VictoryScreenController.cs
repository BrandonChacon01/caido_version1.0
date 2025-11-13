using UnityEngine;
using UnityEngine.SceneManagement;
using LevelSystem;
using TMPro;

public class VictoryScreenController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI jumpsText;

    [Header("Configuración")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Efectos")]
    public bool animateTitle = true;
    public float titleScalePulse = 1.1f;
    public float pulseSpeed = 1f;
    public float statsRevealDelay = 0.5f;
    public float statsRevealInterval = 0.3f;

    [Header("Save Feedback")]
    [Tooltip("Texto para mostrar el estado del guardado (opcional)")]
    public TextMeshProUGUI saveStatusText;

    private GameStats finalStats;
    private bool isSavingData = false;

    private void Start()
    {
        UnityEngine.Debug.Log("[VictoryScreenController] Pantalla de victoria cargada");

        // Detener el tracking de estadísticas
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.StopTracking();
            finalStats = GameStatsManager.Instance.GetCurrentStats();
        }
        else
        {
            UnityEngine.Debug.LogWarning("[VictoryScreenController] No se encontró GameStatsManager. Usando datos por defecto.");
            finalStats = new GameStats();
        }

        // Resetear el progreso del LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetProgress();
        }

        // Mostrar estadísticas
        StartCoroutine(RevealStatsSequence());

        // Animación del título
        if (animateTitle && titleText != null)
        {
            StartCoroutine(PulseTitleCoroutine());
        }

        // Ocultar texto de estado de guardado si existe
        if (saveStatusText != null)
        {
            saveStatusText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Secuencia animada de revelación de estadísticas
    /// </summary>
    private System.Collections.IEnumerator RevealStatsSequence()
    {
        // Ocultar todo inicialmente
        if (timeText) timeText.gameObject.SetActive(false);
        if (jumpsText) jumpsText.gameObject.SetActive(false);

        yield return new WaitForSeconds(statsRevealDelay);

        // Revelar tiempo
        if (timeText)
        {
            timeText.gameObject.SetActive(true);
            timeText.text = $"Tiempo Total: {GameStatsManager.FormatTime(finalStats.totalPlayTime)}";
            yield return new WaitForSeconds(statsRevealInterval);
        }

        // Revelar saltos
        if (jumpsText)
        {
            jumpsText.gameObject.SetActive(true);
            jumpsText.text = $"Saltos Realizados: {finalStats.totalJumps}";
            yield return new WaitForSeconds(statsRevealInterval);
        }

        UnityEngine.Debug.Log("[VictoryScreenController] Estadísticas reveladas");
    }

    /// <summary>
    /// Vuelve al menú principal (con guardado de sesión si está logueado)
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Evitar múltiples clics
        if (isSavingData)
        {
            UnityEngine.Debug.LogWarning("[VictoryScreenController] Ya se está procesando el guardado...");
            return;
        }

        UnityEngine.Debug.Log("[VictoryScreenController] Preparando para volver al menú principal...");

        // 🔹 NUEVO: Verificar si el usuario está logueado
        if (LoadingPayload.IsUserLoggedIn && !string.IsNullOrEmpty(LoadingPayload.UserId))
        {
            // Usuario logueado - guardar la sesión antes de volver al menú
            UnityEngine.Debug.Log("[VictoryScreenController] Usuario logueado detectado. Guardando sesión...");
            StartCoroutine(SaveSessionAndReturnToMenu());
        }
        else
        {
            // Usuario no logueado (invitado) - volver directo al menú
            UnityEngine.Debug.Log("[VictoryScreenController] Usuario invitado. Volviendo al menú sin guardar.");
            GoToMainMenu();
        }
    }

    /// <summary>
    /// Guarda la sesión en Supabase y luego vuelve al menú
    /// </summary>
    private System.Collections.IEnumerator SaveSessionAndReturnToMenu()
    {
        isSavingData = true;

        // Mostrar mensaje de guardado si hay texto configurado
        if (saveStatusText != null)
        {
            saveStatusText.gameObject.SetActive(true);
            saveStatusText.text = "Guardando sesión...";
            saveStatusText.color = Color.yellow;
        }

        // Obtener los datos necesarios
        string userId = LoadingPayload.UserId;
        string seed = GetCurrentSeed();
        int durationInSeconds = ConvertTimeToSeconds(finalStats.totalPlayTime);

        UnityEngine.Debug.Log($"[VictoryScreenController] Datos a guardar:");
        UnityEngine.Debug.Log($"  - User ID: {userId}");
        UnityEngine.Debug.Log($"  - Seed: {seed}");
        UnityEngine.Debug.Log($"  - Duration: {durationInSeconds} segundos ({GameStatsManager.FormatTime(finalStats.totalPlayTime)})");

        // Validar que tenemos todos los datos necesarios
        if (string.IsNullOrEmpty(seed))
        {
            UnityEngine.Debug.LogError("[VictoryScreenController] No se pudo obtener la seed. Continuando sin guardar.");
            ShowSaveError("No se pudo obtener la seed de la partida");
            yield return new WaitForSeconds(2f);
            GoToMainMenu();
            yield break;
        }

        // Variables para controlar el resultado
        bool saveCompleted = false;
        bool saveSuccess = false;
        string errorMessage = "";

        // Guardar la sesión
        SupabaseGameSessionManager.Instance.SaveGameSession(
            userId,
            seed,
            durationInSeconds,
            onSuccess: () =>
            {
                saveCompleted = true;
                saveSuccess = true;
            },
            onError: (error) =>
            {
                saveCompleted = true;
                saveSuccess = false;
                errorMessage = error;
            }
        );

        // Esperar a que termine el guardado (timeout de 10 segundos)
        float timeout = 10f;
        float elapsed = 0f;

        while (!saveCompleted && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Verificar resultado
        if (saveSuccess)
        {
            UnityEngine.Debug.Log("[VictoryScreenController] ✅ Sesión guardada exitosamente");

            if (saveStatusText != null)
            {
                saveStatusText.text = "¡Sesión guardada!";
                saveStatusText.color = Color.green;
            }

            yield return new WaitForSeconds(1f);
        }
        else
        {
            if (elapsed >= timeout)
            {
                UnityEngine.Debug.LogError("[VictoryScreenController] ⏱️ Timeout al guardar sesión");
                ShowSaveError("Tiempo de espera agotado");
            }
            else
            {
                UnityEngine.Debug.LogError($"[VictoryScreenController] ❌ Error al guardar sesión: {errorMessage}");
                ShowSaveError($"Error: {errorMessage}");
            }

            yield return new WaitForSeconds(2f);
        }

        // Volver al menú
        GoToMainMenu();
    }

    /// <summary>
    /// Obtiene la seed actual del LevelRandomizer
    /// </summary>
    private string GetCurrentSeed()
    {
        if (LevelRandomizer.Instance != null && LevelRandomizer.Instance.IsGenerated)
        {
            return LevelRandomizer.Instance.GeneratedSeed;
        }

        // Fallback: intentar obtenerla del GameStatsManager
        if (GameStatsManager.Instance != null)
        {
            GameStats stats = GameStatsManager.Instance.GetCurrentStats();
            if (!string.IsNullOrEmpty(stats.levelSeed))
            {
                return stats.levelSeed;
            }
        }

        UnityEngine.Debug.LogWarning("[VictoryScreenController] No se pudo obtener la seed de ninguna fuente");
        return null;
    }

    /// <summary>
    /// Convierte el tiempo en float (segundos) a int
    /// </summary>
    private int ConvertTimeToSeconds(float timeInSeconds)
    {
        return Mathf.RoundToInt(timeInSeconds);
    }

    /// <summary>
    /// Muestra un mensaje de error en el guardado
    /// </summary>
    private void ShowSaveError(string message)
    {
        if (saveStatusText != null)
        {
            saveStatusText.text = message;
            saveStatusText.color = Color.red;
        }
    }

    /// <summary>
    /// Navega al menú principal
    /// </summary>
    private void GoToMainMenu()
    {
        UnityEngine.Debug.Log("[VictoryScreenController] Volviendo al menú principal...");

        // Resetear estadísticas para la próxima partida
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
        }

        // Cargar escena
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Animación de pulso del título
    /// </summary>
    private System.Collections.IEnumerator PulseTitleCoroutine()
    {
        if (titleText == null) yield break;

        Vector3 originalScale = titleText.transform.localScale;

        while (true)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(1f, titleScalePulse, t);
                titleText.transform.localScale = originalScale * scale;
                yield return null;
            }

            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * pulseSpeed;
                float scale = Mathf.Lerp(titleScalePulse, 1f, t);
                titleText.transform.localScale = originalScale * scale;
                yield return null;
            }
        }
    }
}