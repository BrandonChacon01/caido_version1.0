using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using LevelSystem; // 🔹 AGREGADO

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

        // 🔹 NUEVO: Verificar si debemos usar el sistema de niveles
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