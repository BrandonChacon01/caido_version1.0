using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Controlador SIMPLE para el video splash inicial que carga antes del MainMenu
/// </summary>
public class VideoSplashController : MonoBehaviour
{
    [Header("Video Configuration")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Next Scene")]
    [Tooltip("Escena que se carga después del video (MainMenu)")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Skip Options")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private KeyCode escapeKey = KeyCode.Escape;
    [SerializeField] private KeyCode anyKey = KeyCode.Return; // Enter también

    private bool videoFinished = false;
    private bool isLoadingNextScene = false;

    private void Start()
    {
        // Buscar el VideoPlayer si no está asignado
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            UnityEngine.Debug.LogError("[VideoSplashController] No se encontró VideoPlayer. Cargando MainMenu directamente...");
            LoadNextScene();
            return;
        }

        // Configurar eventos del VideoPlayer
        videoPlayer.loopPointReached += OnVideoFinished;

        // Iniciar reproducción
        videoPlayer.Play();
        UnityEngine.Debug.Log("[VideoSplashController] 🎬 Reproduciendo video splash inicial...");
    }

    private void Update()
    {
        // Permitir saltar el video si está habilitado
        if (allowSkip && !isLoadingNextScene && !videoFinished)
        {
            // Cualquier tecla salta el video
            if (Input.GetKeyDown(skipKey) ||
                Input.GetKeyDown(escapeKey) ||
                Input.GetKeyDown(anyKey) ||
                Input.anyKeyDown)
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
        UnityEngine.Debug.Log("[VideoSplashController] ✅ Video splash terminado. Cargando MainMenu...");
        LoadNextScene();
    }

    /// <summary>
    /// Salta el video y carga la siguiente escena
    /// </summary>
    public void SkipVideo()
    {
        if (isLoadingNextScene) return;

        UnityEngine.Debug.Log("[VideoSplashController] ⏭️ Video splash saltado por el usuario.");

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        LoadNextScene();
    }

    /// <summary>
    /// Carga el MainMenu
    /// </summary>
    private void LoadNextScene()
    {
        if (isLoadingNextScene) return;

        isLoadingNextScene = true;

        UnityEngine.Debug.Log($"[VideoSplashController] 🎯 Cargando escena: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        // Limpiar el evento para evitar errores
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}