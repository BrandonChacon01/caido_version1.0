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
    // 🔹 Agrega más textos aquí según lo que implementes en el futuro

    [Header("Configuración")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Efectos")]
    public bool animateTitle = true;
    public float titleScalePulse = 1.1f;
    public float pulseSpeed = 1f;
    public float statsRevealDelay = 0.5f;
    public float statsRevealInterval = 0.3f;

    private GameStats finalStats;

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

        // 🔹 Aquí puedes agregar más estadísticas en el futuro

        UnityEngine.Debug.Log("[VictoryScreenController] Estadísticas reveladas");
    }

    /// <summary>
    /// Vuelve al menú principal
    /// </summary>
    public void ReturnToMainMenu()
    {
        UnityEngine.Debug.Log("[VictoryScreenController] Volviendo al menú principal...");

        // Resetear estadísticas para la próxima partida
        if (GameStatsManager.Instance != null)
        {
            GameStatsManager.Instance.ResetStats();
        }

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