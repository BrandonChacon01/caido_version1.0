using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using LevelSystem;

/// <summary>
/// Controlador para reproducir un video introductorio antes de iniciar el juego
/// </summary>
public class VideoIntroController : MonoBehaviour
{
    [Header("Video Configuration")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextSceneName = "SampleScene"; // Escena del juego (fallback)

    [Header("Skip Options")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private KeyCode escapeKey = KeyCode.Escape;

    private bool videoFinished = false;
    private bool isLoadingNextScene = false;
    private bool levelsGenerated = false; // 🔹 NUEVO

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            UnityEngine.Debug.LogError("[VideoIntroController] No se encontró VideoPlayer. Cargando escena directamente...");
            LoadNextScene();
            return;
        }

        // Obtener la escena de destino desde LoadingPayload si existe
        if (!string.IsNullOrEmpty(LoadingPayload.NextScene))
        {
            nextSceneName = LoadingPayload.NextScene;
        }

        // 🔹 NUEVO: Generar la selección aleatoria de niveles al inicio del video
        GenerateLevelSequence();

        // Configurar eventos del VideoPlayer
        videoPlayer.loopPointReached += OnVideoFinished;

        // Iniciar reproducción
        videoPlayer.Play();
        UnityEngine.Debug.Log("[VideoIntroController] Reproduciendo video intro...");
    }

    private void Update()
    {
        // Permitir saltar el video si está habilitado
        if (allowSkip && !isLoadingNextScene && !videoFinished)
        {
            if (Input.GetKeyDown(skipKey) || Input.GetKeyDown(escapeKey) || Input.anyKeyDown)
            {
                SkipVideo();
            }
        }
    }

    /// <summary>
    /// Genera la secuencia aleatoria de niveles
    /// </summary>
    private void GenerateLevelSequence()
    {
        if (levelsGenerated)
        {
            UnityEngine.Debug.LogWarning("[VideoIntroController] Los niveles ya fueron generados");
            return;
        }

        // Solo generar si vamos a usar el sistema de niveles
        if (LoadingPayload.UseLevelSystem)
        {
            UnityEngine.Debug.Log("[VideoIntroController] Generando secuencia aleatoria de niveles...");
            LevelRandomizer.Instance.GenerateRandomLevelSequence();
            levelsGenerated = true;

            UnityEngine.Debug.Log($"[VideoIntroController] ✅ Secuencia generada. Semilla: {LevelRandomizer.Instance.GeneratedSeed}");
        }
        else
        {
            UnityEngine.Debug.Log("[VideoIntroController] Sistema de niveles desactivado, usando flujo tradicional");
        }
    }

    /// <summary>
    /// Se llama cuando el video termina de reproducirse
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        UnityEngine.Debug.Log("[VideoIntroController] Video terminado. Cargando juego...");
        LoadNextScene();
    }

    /// <summary>
    /// Salta el video y carga la siguiente escena
    /// </summary>
    public void SkipVideo()
    {
        if (isLoadingNextScene) return;

        UnityEngine.Debug.Log("[VideoIntroController] Video saltado por el usuario.");
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        LoadNextScene();
    }

    /// <summary>
    /// Carga la siguiente escena (el juego o el primer nivel)
    /// </summary>
    private void LoadNextScene()
    {
        if (isLoadingNextScene) return;

        isLoadingNextScene = true;

        // Verificar si debemos usar el sistema de niveles
        if (LoadingPayload.UseLevelSystem && LevelManager.Instance != null)
        {
            UnityEngine.Debug.Log("[VideoIntroController] Iniciando sistema de niveles...");
            LevelManager.Instance.StartGame();
        }
        else
        {
            // Fallback: cargar escena tradicional
            UnityEngine.Debug.Log($"[VideoIntroController] Cargando escena: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnDestroy()
    {
        // Limpiar el evento
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}